namespace CMT.Models
{
    public class ResumableConfiguration
    {
        public int Chunks { get; set; }

        public string Identifier { get; set; }

        public string FileName { get; set; }

        public static ResumableConfiguration Create(string identifier, string filename, int chunks)
        {
            return new ResumableConfiguration()
            {
                Identifier = identifier,
                FileName = filename,
                Chunks = chunks
            };
        }
    }
}