using CMT.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMT.BO.Admin
{
    [DataContract(Name = "TreeElement")]
    public class TreeElementBO : IBusinessObject
    {
        [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
        public Guid ObjectId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? LocalValues { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? AllValues { get; set; }

        [DataMember]
        public byte Type { get; set; }

        [DataMember]
        public List<TreeElementBO> Children { get; set; }

        [DataMember]
        public Guid? ParentId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Channel { get; set; }

        [DataMember(Name = "Status")]
        public bool? IsActive { get; set; }

        [DataMember]
        public string LocalValue { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? Level { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsLov { get; set; }

        [DataMember]
        public string LevelName { get; set; }

        [DataMember]
        public string DataType { get; set; }

        [DataMember]
        public bool SearchTermFound { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool Readonly { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string ActivatedBy { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime? ActivationDate { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string DefinedBy { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime? DefinitionDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DeactivatedBy { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime? DeactivationDate { get; set; }
        public Guid? RootId { get; set; }
        public string DisplayName
        {
            get
            {
                return ToString();
            }
        }

        [DataMember]
        public string LocalCode { get; set; }

        [DataMember]
        public string GlobalCode { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int Attributes { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public ListValue DefaultValue { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsRequired { get; set; }
        [DataMember]
        public Guid? ParentValueId { get; set; }

        [DataMember]
        public bool HasTags { get; set; }

        [DataMember]
        public bool HasChildren { get; set; }
    }
}
