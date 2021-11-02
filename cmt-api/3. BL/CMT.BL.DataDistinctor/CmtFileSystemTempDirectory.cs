using DDR.TemporaryFileStorage;
using System;
using System.IO;

namespace CMT.BL.DataDistinctor
{
    public class CmtFileSystemTempDirectory : TemporaryDirectory
    {
        public string WorkingDirectory { get; }

        public CmtFileSystemTempDirectory(string name)
            : base(name)
        {
            WorkingDirectory = name;
        }

        public override void Store(string filename, Stream stream, bool overwrite = false)
        {
            throw new NotImplementedException();
        }

        public override Stream Create(string filename)
        {
            throw new NotImplementedException();
        }

        public override Stream Get(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(filename));
            }

            return new FileStream(GetPath(filename), FileMode.Open);
        }

        public override void Delete(string filename)
        {
            File.Delete(GetPath(filename));
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override string GetPath(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(filename));
            }

            return Path.Combine(WorkingDirectory, filename);
        }
    }
}