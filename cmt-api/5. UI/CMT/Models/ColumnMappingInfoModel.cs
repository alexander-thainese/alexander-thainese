using System;
using System.Runtime.Serialization;

namespace CMT.Models
{
    [DataContract]
    public class ColumnMappingInfoModel
    {
        [DataMember]
        public Guid ImportId { get; set; }
        [DataMember]
        public Guid SchemaElementId { get; set; }
        [DataMember]
        public string ColumnName { get; set; }
        [DataMember]
        public byte? Level { get; set; }
        [DataMember]
        public Guid ColumnId { get; set; }
    }
}