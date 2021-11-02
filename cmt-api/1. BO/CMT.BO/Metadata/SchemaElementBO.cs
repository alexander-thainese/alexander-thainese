using CMT.Common.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract(Name = "SchemaElement")]
    public class SchemaElementBO : BaseObject, IBusinessObject
    {
        [Required]
        public Guid SchemaElementId { get; set; }
        [DataMember(Name = "SchemaElementId")]
        public Guid ObjectId { get; set; }
        [Required]
        [DataMember(EmitDefaultValue = false)]
        public bool IsRequired { get; set; }
        [DataMember]
        public ElementBO Element { get; set; }

        [DataMember]
        public string DefaultValue { get; set; }
        public string DisplayName
        {
            get
            {
                return GetType().Name;
            }
        }
    }
}
