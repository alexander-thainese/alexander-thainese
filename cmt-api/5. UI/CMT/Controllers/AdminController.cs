using CMT.Attributes;
using CMT.BL.Core;
using CMT.BL.DataDistinctor;
using CMT.BL.S3;
using CMT.BL.Workers;
using CMT.BO;
using CMT.Handlers;
using CMT.PV.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CMT.Controllers
{


    [EnableCors("*", "Origin, Content-Type, Accept, Authorization",
                                               "GET, PUT, POST, DELETE, OPTIONS", SupportsCredentials = true)]
    public class AdminController : BaseApiController
    {
        /// <summary>
        /// Restarts the workers.
        /// </summary>
        /// <returns></returns>
        [CMTAuthorize]
        [HttpGet]
        [Route("api/Admin/RestartWorkers")]
        public bool RestartWorkers()
        {
            BaseWorker.RestartAllWorkers();
            return true;
        }


        [HttpGet]
        [Route("api/Admin/HealthCheck")]
        public string HealthCheck()
        {

            return "Application is up";
        }


        /// <summary>
        /// Generates the file extracts.
        /// </summary>
        /// <returns></returns>
        [CMTAuthorize]
        [HttpGet]
        [Route("api/Admin/GenerateFileExtracts")]
        public string GenerateFileExtracts()
        {
            new FileExtractorWorker(new CMTLogger()).Work();
            return "OK";
        }


        /// <summary>
        /// Gets the s3 files.
        /// </summary>
        /// <returns></returns>
        [CMTAuthorize]
        [HttpGet]
        [Route("api/Admin/GetS3Files")]
        public IEnumerable<S3FileBO> GetS3Files()
        {
            List<S3FileBO> result = S3Helper.GetFiles(ApplicationSettings.S3ImportFileSourceConfiguration);
            return result;
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <returns></returns>
        [CMTAuthorize]
        [HttpGet]
        [Route("api/Admin/version")]
        public DateTime GetVersion()
        {
            return System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Resets the application.
        /// </summary>
        [CMTAuthorize]
        [HttpGet]
        [Route("api/Admin/reset")]
        public void ResetApp()
        {
            HttpRuntime.UnloadAppDomain();
        }


        /// <summary>
        ///Manually trigger  uploads brands.
        /// </summary>
        /// <returns></returns>
        [CMTAuthorize]
        [HttpGet]
        [Route("api/Admin/uploadBrands")]
        public List<string> UploadBrands(int interval)
        {
            CMTLogger logger = new CMTLogger();
            try
            {

                logger.LogAction("Admin/uploadBrands started", GetType());
                BrandUploader brandUploader = new BrandUploader();
                List<string> fileKeyToUpload = brandUploader.GetFileNamesToImport(interval);

                foreach (string fileKey in fileKeyToUpload)
                {
                    try
                    {

                        brandUploader.ImportFile(fileKey);
                    }
                    catch (Exception fileException)
                    {
                        logger.LogAction("error in file:" +  fileKey, GetType());
                        logger.LogError(GetType(), fileException);
                    }
                }
                logger.LogAction("Admin/uploadBrands ended", GetType());
                return fileKeyToUpload;
            }
            catch (Exception exc)
            {

                logger.LogError(GetType(), exc);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NotAcceptable, "Error");
                throw new HttpResponseException(response);
            }
        }
        [CMTAuthorize]
        [HttpGet]
        [Route("api/Admin/uploadAttributes")]
        public List<string> UploadAttributes(int interval)
        {
            CMTLogger logger = new CMTLogger();
            try
            {

                logger.LogAction("Admin/uploadAttributes started", GetType());
                AttributeUploader uploader = new AttributeUploader();
                List<string> fileKeyToUpload = uploader.GetFileNamesToImport(interval);

                foreach (string fileKey in fileKeyToUpload)
                {
                    uploader.ImportFile(fileKey);
                }
                logger.LogAction("Admin/uploadAttributes ended", GetType());
                return fileKeyToUpload;
            }
            catch (Exception exc)
            {

                logger.LogError(GetType(), exc);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NotAcceptable, "Error");
                throw new HttpResponseException(response);
            }
        }

        [CMTAuthorize]
        [HttpGet]
        [Route("api/Admin/getPasswordHash")]
        public string GetPasswordHash(string password)
        {
            CMTLogger logger = new CMTLogger();
            try
            {
                ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                return userManager.PasswordHasher.HashPassword(password);

            }
            catch (Exception exc)
            {

                logger.LogError(GetType(), exc);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NotAcceptable, "Error");
                throw new HttpResponseException(response);
            }
        }

        [CMTAuthorize]
        [HttpGet]
        [Route("api/Admin/checkPasswordHash")]
        public bool CheckPasswordHash(string password, string hash)
        {
            CMTLogger logger = new CMTLogger();
            try
            {
                ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                return userManager.PasswordHasher.VerifyHashedPassword(hash, password) == PasswordVerificationResult.Success;

            }
            catch (Exception exc)
            {

                logger.LogError(GetType(), exc);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NotAcceptable, "Error");
                throw new HttpResponseException(response);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("api/Admin/SsoHeaders")]
        public object GetSsoHeaders()
        {
            // Celowo nie ujawniam nazw nagłówków.
            return Request.CreateResponse(
                new
                {
                    UserDomain = Request.Headers.Where(p => p.Key == "IAMPFIZERUSERDOMAIN").Select(p => p.Value).FirstOrDefault(),
                    UserLogin = Request.Headers.Where(p => p.Key == "IAMPFIZERUSERCN").Select(p => p.Value).FirstOrDefault(),
                    UserMail = Request.Headers.Where(p => p.Key == "IAMPFIZERUSERINTERNETEMAILADDRESS").Select(p => p.Value).FirstOrDefault(),
                    UserFirstName = Request.Headers.Where(p => p.Key == "IAMPFIZERUSERFIRSTNAME").Select(p => p.Value).FirstOrDefault(),
                    UserLastName = Request.Headers.Where(p => p.Key == "IAMPFIZERUSERLASTNAME").Select(p => p.Value).FirstOrDefault()
                });
        }
    }
}