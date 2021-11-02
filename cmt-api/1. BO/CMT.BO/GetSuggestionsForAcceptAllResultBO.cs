using System;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract(Name = "GetSuggestionsForAcceptAllResult")]
    public class GetSuggestionsForAcceptAllResultBO
    {
        [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
        public Guid ObjectId { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string ValueFromLoad { get; set; }
        [DataMember]
        public int Count { get; set; }
        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public long RowNumber { get; set; }

        public GetSuggestionsForAcceptAllResultBO(Guid objectId, string value, string name, string valueFromLoad, int count, int type, long rowNumber)
        {
            ObjectId = objectId;
            Value = value;
            Name = name;
            ValueFromLoad = valueFromLoad;
            Count = count;
            Type = type;
            RowNumber = rowNumber;
        }
    }
}
