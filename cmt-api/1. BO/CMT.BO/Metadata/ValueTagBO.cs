using CMT.Common.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract(Name = "ValueTag")]
    public class ValueTagBO : BaseObject, IBusinessObject
    {
        [Required]
        [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
        public Guid ObjectId { get; set; }

        [Required]
        [DataMember(Name = "TypeId")]
        public ValueTagType Type { get; set; }

        public Guid ValueId { get; set; }

        public string DisplayName
        {
            get
            {
                return ToString();
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "ElementId")]
        public Guid? TagElementId { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "ValueId")]
        public Guid? TagValueId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SchemaTag { get; set; }


        [DataMember(EmitDefaultValue = false)]
        public string Element { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Value { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string CountryCode { get; set; }

        public Guid? CountryId { get; set; }

        [DataMember(Name = "Type")]
        public string TypeName
        {
            get { return Enum.GetName(typeof(ValueTagType), Type); }
        }


    }
}
