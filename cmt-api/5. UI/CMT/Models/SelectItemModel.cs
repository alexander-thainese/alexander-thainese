using CMT.BO;
using System;
using System.Runtime.Serialization;

namespace CMT.Models
{
    [DataContract(Name = "SelectItem")]
    public class SelectItemModel
    {
        [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
        public Guid ObjectId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }

        public SelectItemModel()
        {
        }

        public SelectItemModel(Guid objectId, string name)
        {
            ObjectId = objectId;
            Name = name;
        }

        public SelectItemModel(Guid objectId, string name, string description)
        {
            ObjectId = objectId;
            Name = name;
            Description = description;
        }
    }
}