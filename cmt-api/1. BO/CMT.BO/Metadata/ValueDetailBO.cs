using CMT.Common.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract(Name = "ValueDetail")]
    public class ValueDetailBO : BaseObject, IBusinessObject
    {
        [Required]
        [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
        public Guid ObjectId { get; set; }

        [Required]
        [DataMember]
        public ValueDetailType Type { get; set; }

        [MaxLength(200)]
        [Required]
        [DataMember(EmitDefaultValue = false)]
        public string Value { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [MaxLength(40)]
        public string ExternalId { get; set; }

        public Guid ValueId { get; set; }

        public string DisplayName
        {
            get
            {
                return ToString();
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public Guid? CountryId { get; set; }

        public string LocalCode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid? AttributeElementId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid? AttributeValueId { get; set; }
    }
}
