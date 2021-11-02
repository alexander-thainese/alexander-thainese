using CMT.Common.Interfaces;
using System;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract]
    public class BaseObject : IAuditableObject
    {
        [DataMember]
        public string CreateUser { get; set; }
        [DataMember]
        public DateTime CreateDate { get; set; }
        [DataMember]
        public string ChangeUser { get; set; }
        [DataMember]
        public DateTime? ChangeDate { get; set; }
    }
}
