using CMT.Common;
using CMT.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract(Name = "Element")]
    public class ElementBO : BaseObject, IBusinessObject
    {
        [Required]
        [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
        public Guid ObjectId { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "Type")]
        public string ElementTypeName { get; set; }

        [DataMember]
        public string Name { get; set; }



        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ValueBO> ValueList { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public byte Status { get; set; }

        public bool Readonly { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public ElementAttributes? Attributes { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid? ValueListId { get; set; }

        public string DisplayName
        {
            get
            {
                return ToString();
            }
        }

        [DataMember]
        public MappingStatus MappingStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool? IsDirect { get; set; }

        [IgnoreDataMember]
        public bool IsLov { get; set; }
        public string DefaultValue { get; set; }
        [DataMember]
        public virtual Guid TypeId { get; set; }
    }
}
