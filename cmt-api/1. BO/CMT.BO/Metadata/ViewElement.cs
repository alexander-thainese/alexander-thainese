using CMT.Common.Interfaces;
using System;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract(Name = "ViewElement")]
    public class ViewElementBO : IBusinessObject
    {
        public string DisplayName { get { return ToString(); } }

        [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
        public Guid ObjectId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public int SchemasUsage { get; set; }
        [DataMember]
        public string LocalValues { get; set; }
    }
}
