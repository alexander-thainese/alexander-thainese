using CMT.Common.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract(Name = "ValueListLevel")]
    public class ValueListLevelBO : BaseObject, IBusinessObject
    {
        [Required]
        [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
        public Guid ObjectId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [MaxLength(50)]
        public string Name { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public byte Level { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid ElementId { get; set; }

        public string DisplayName
        {
            get { return Name; }
        }
    }
}
