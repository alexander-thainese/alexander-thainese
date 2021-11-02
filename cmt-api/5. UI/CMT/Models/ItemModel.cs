using System.Runtime.Serialization;

namespace CMT.Models
{
    [DataContract(Name = "Item")]
    public class ItemModel
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public object Value { get; set; }

        public ItemModel()
        {
        }

        public ItemModel(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}