using CMT.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract(Name = "Value")]
    public class ValueBO : BaseObject, IBusinessObject
    {
        [Required]
        [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
        public Guid ObjectId { get; set; }


        [DataMember(EmitDefaultValue = false)]
        public List<ValueBO> ChildValues { get; set; }

        [IgnoreDataMember]
        public Guid? ChildListId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ValueDetailBO> Details { get; set; }


        [DataMember(EmitDefaultValue = false)]
        public List<ValueTagBO> Tags { get; set; }


        [DataMember(EmitDefaultValue = false)]
        [MaxLength(200)]
        public string TextValue { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public byte Status { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [MaxLength(40)]
        public string ExternalId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid? ValueListId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid? ParentId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ValueBO ParentValue { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid? CountryId { get; set; }

        [DataMember]
        public string GlobalCode { get; set; }

        public string DisplayName
        {
            get
            {
                return GetType().Name;
            }
        }
    }
}

