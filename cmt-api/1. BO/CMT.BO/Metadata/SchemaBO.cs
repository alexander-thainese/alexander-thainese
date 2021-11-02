using CMT.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract(Name = "Schema")]
    public class SchemaBO : BaseObject, IBusinessObject
    {
        [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
        public Guid ObjectId { get; set; }

        [DataMember()]
        public Guid ChannelId { get; set; }


        [DataMember]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string CountryCode { get; set; }

        public string DisplayName
        {
            get
            {
                return ToString();
            }
        }

        [DataMember]
        public List<SchemaElementBO> SchemaElements { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public string Description { get; set; }

        public string ActivatedBy { get; set; }

        public DateTime? ActivationDate { get; set; }

        public string DefinedBy { get; set; }
        public DateTime? DefinitionDate { get; set; }

        public string DeactivatedBy { get; set; }
        public DateTime? DeactivationDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid? SourceSchemaId { get; set; }

        [DataMember]
        public List<string> Tags { get; set; }
    }
}
