using CMT.Attributes;
using CMT.BO;
using CMT.Handlers;
using CMT.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CMT.Controllers
{
    [CMTAuthorize]
    [RoutePrefix("api/file")]

    [EnableCors("*", "*", "GET, PUT, POST, DELETE, OPTIONS", SupportsCredentials = true)]
    public class FileUploadController : BaseApiController
    {
        //[Route("upload")]
        //[HttpOptions]
        //public HttpResponseMessage UploadFileOptions()
        //{
        //    return Request.CreateResponse(HttpStatusCode.OK);
        //}
        /// <summary>
        /// Method to support file uploading
        /// </summary>
        /// <param name="resumableChunkNumber"></param>
        /// <param name="resumableIdentifier"></param>
        /// <returns></returns>
        [Route("upload")]
        [HttpGet]
        public HttpResponseMessage Upload(int resumableChunkNumber, string resumableIdentifier)
        {
            return ChunkIsHere(resumableChunkNumber, resumableIdentifier) ? Request.CreateResponse(HttpStatusCode.OK) : Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Posts the file.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Route("upload")]
        [HttpPost]
        public async Task<object> PostFile()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            MultipartFormDataStreamProvider multipartProvider = new MultipartFormDataStreamProvider(ApplicationSettings.SourceFilesFolder);

            if (await ReadPart(multipartProvider))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                string message = DeleteInvalidChunkData(multipartProvider) ? "Cannot read multi part file data." : "Cannot delete temporary file chunk data.";
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, message);
            }
        }

        private async Task<bool> ReadPart(MultipartFormDataStreamProvider multipartProvider)
        {
            try
            {
                await Request.Content.ReadAsMultipartAsync(multipartProvider);
                ResumableConfiguration resumableConfiguration = GetUploadConfiguration(multipartProvider);
                int chunkNumber = GetChunkNumber(multipartProvider);

                MultipartFileData chunk = multipartProvider.FileData[0];
                RenameChunk(chunk, chunkNumber, resumableConfiguration.Identifier);
                TryAssembleFile(resumableConfiguration);

                return true;
            }
            catch (Exception e)
            {
                CMTLogger logger = new CMTLogger();
                logger.LogError(GetType(), e);
                return false;
            }
        }

        private bool DeleteInvalidChunkData(MultipartFormDataStreamProvider multipartProvider)
        {
            try
            {
                string localFileName = multipartProvider.FileData[0].LocalFileName;

                if (File.Exists(localFileName))
                {
                    File.Delete(localFileName);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        [NonAction]
        private ResumableConfiguration GetUploadConfiguration(MultipartFormDataStreamProvider multipartProvider)
        {
            return ResumableConfiguration.Create(GetId(multipartProvider), GetFileName(multipartProvider), GetTotalChunks(multipartProvider));
        }

        [NonAction]
        private string GetFileName(MultipartFormDataStreamProvider multipartProvider)
        {
            string filename = multipartProvider.FormData["resumableFilename"];
            return !string.IsNullOrEmpty(filename) ? filename : multipartProvider.FileData[0].Headers.ContentDisposition.FileName.Trim('\"');
        }

        [NonAction]
        private string GetId(MultipartFormDataStreamProvider multipartProvider)
        {
            string id = multipartProvider.FormData["resumableIdentifier"];
            return !string.IsNullOrEmpty(id) ? id : Guid.NewGuid().ToString();
        }

        [NonAction]
        private int GetTotalChunks(MultipartFormDataStreamProvider multipartProvider)
        {
            string total = multipartProvider.FormData["resumableTotalChunks"];
            return !string.IsNullOrEmpty(total) ? Convert.ToInt32(total) : 1;
        }

        [NonAction]
        private int GetChunkNumber(MultipartFormDataStreamProvider multipartProvider)
        {
            string chunk = multipartProvider.FormData["resumableChunkNumber"];
            return !string.IsNullOrEmpty(chunk) ? Convert.ToInt32(chunk) : 1;
        }

        [NonAction]
        private void RenameChunk(MultipartFileData chunk, int chunkNumber, string identifier)
        {
            string generatedFileName = chunk.LocalFileName;
            string chunkFileName = GetChunkFileName(chunkNumber, identifier);
            if (File.Exists(chunkFileName))
            {
                File.Delete(chunkFileName);
            }

            File.Move(generatedFileName, chunkFileName);
        }

        [NonAction]
        private string GetChunkFileName(int chunkNumber, string identifier)
        {
            return Path.Combine(ApplicationSettings.SourceFilesFolder, string.Format("{0}_{1}", identifier, chunkNumber.ToString()));
        }

        [NonAction]
        private void TryAssembleFile(ResumableConfiguration resumableConfiguration)
        {
            if (AllChunksAreHere(resumableConfiguration))
            {
                string path = ConsolidateFile(resumableConfiguration);
                RenameFile(path, Path.Combine(ApplicationSettings.SourceFilesFolder, resumableConfiguration.FileName));
                DeleteChunks(resumableConfiguration);
            }
        }

        [NonAction]
        private void DeleteChunks(ResumableConfiguration resumableConfiguration)
        {
            for (int chunkNumber = 1; chunkNumber <= resumableConfiguration.Chunks; chunkNumber++)
            {
                string chunkFileName = GetChunkFileName(chunkNumber, resumableConfiguration.Identifier);
                File.Delete(chunkFileName);
            }
        }

        [NonAction]
        private string ConsolidateFile(ResumableConfiguration resumableConfiguration)
        {
            string path = GetFilePath(resumableConfiguration);
            using (FileStream destStream = File.Create(path, 15000))
            {
                for (int chunkNumber = 1; chunkNumber <= resumableConfiguration.Chunks; chunkNumber++)
                {
                    string chunkFileName = GetChunkFileName(chunkNumber, resumableConfiguration.Identifier);
                    using (FileStream sourceStream = File.OpenRead(chunkFileName))
                    {
                        sourceStream.CopyTo(destStream);
                    }
                }
                destStream.Close();
            }

            return path;
        }


        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        [Route("DeleteFile/{fileName}")]
        [HttpPost]
        public bool DeleteFile(string fileName)
        {
            string[] files = Directory.GetFiles(ApplicationSettings.SourceFilesFolder, fileName + "*");
            foreach (string file in files)
            {
                File.Delete(file);
            }
            return true;
        }

        [NonAction]
        private string RenameFile(string sourceName, string targetName)
        {
            string targetExtension = Path.GetExtension(targetName);
            string fileName = Path.GetFileName(sourceName);
            string realFileName = Path.Combine(ApplicationSettings.SourceFilesFolder, Path.ChangeExtension(fileName, targetExtension));
            if (File.Exists(realFileName))
            {
                File.Delete(realFileName);
            }

            File.Move(sourceName, realFileName);
            return realFileName;
        }

        [NonAction]
        private string GetFilePath(ResumableConfiguration resumableConfiguration)
        {
            return Path.Combine(ApplicationSettings.SourceFilesFolder, resumableConfiguration.Identifier);
        }

        [NonAction]
        private bool ChunkIsHere(int chunkNumber, string identifier)
        {
            string fileName = GetChunkFileName(chunkNumber, identifier);
            return File.Exists(fileName);
        }

        [NonAction]
        private bool AllChunksAreHere(ResumableConfiguration resumableConfiguration)
        {
            for (int chunkNumber = 1; chunkNumber <= resumableConfiguration.Chunks; chunkNumber++)
            {
                if (!ChunkIsHere(chunkNumber, resumableConfiguration.Identifier))
                {
                    return false;
                }
            }

            return true;
        }
    }
}