using System.Runtime.Serialization;

namespace CMT.BO.Admin
{
    [DataContract(Name = "ParentValueAttribute")]
    public class ParentValueAttribute
    {
        [DataMember]
        public string ParentId { get; set; }
        [DataMember]
        public string ElementName { get; set; }
        [DataMember]
        public string AttributeValue { get; set; }
        [DataMember]
        public int ElementType { get; set; }
        [DataMember]
        public string Color { get; set; }
    }

    public enum AttributeElementType
    {
        BU = 8,
        SUBBU = 16,
        PORTFOLIO = 32,
        INDICATION = 64
    }
}
