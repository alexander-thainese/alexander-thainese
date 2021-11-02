using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract(Name = "ExtElement")]
    public class ExtElementBO : ElementBO
    {
        [DataMember(Name = "LovLabels", EmitDefaultValue = false)]
        public List<string> LOVLevels { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid? GroupId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public override Guid TypeId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid? SourceElementId { get; set; }
    }
}
