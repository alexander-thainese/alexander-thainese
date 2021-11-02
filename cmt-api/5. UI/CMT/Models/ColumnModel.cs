using CMT.BO;
using System;
using System.Runtime.Serialization;

namespace CMT.Models
{
    [DataContract(Name = "Column")]
    public class ColumnModel
    {
        [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
        public Guid ObjectId { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Tooltip { get; set; }
        [DataMember]
        public string Icon { get; set; }
        [DataMember]
        public string Caption { get; set; }
        [DataMember]
        public int? DistinctValueCount { get; set; }
        [DataMember]
        public bool IsMapped { get; set; }
        [DataMember]
        public Guid? SchemaElementId { get; set; }
        [DataMember]
        public string ElementName { get; set; }
        [DataMember]
        public bool IsIdSelected { get; set; }
        [DataMember]
        public bool IsId { get; set; }

        public ColumnModel(Guid objectId, string title, string tooltip = null, string icon = null, string caption = null, int? distinctValueCount = null)
        {
            ObjectId = objectId;
            Title = title;
            Tooltip = tooltip;
            Icon = icon;
            Caption = caption;
            DistinctValueCount = distinctValueCount;
        }
    }
}