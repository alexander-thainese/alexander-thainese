using System;
using System.Linq;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract(Name = "S3File")]
    public class S3FileBO
    {
        [DataMember]
        public string FullName { get; set; }
        [DataMember]
        public DateTime Date { get; set; }
        [DataMember]
        public bool IsImported { get; set; }

        [DataMember]
        public string FileName
        {
            get
            {
                string[] splitted = FullName.Split('/');
                return splitted.Last();
            }
        }
    }
}
