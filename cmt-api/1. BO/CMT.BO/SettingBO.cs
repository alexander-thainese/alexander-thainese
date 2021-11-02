using CMT.Common.Interfaces;
using System;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract(Name = "Setting")]
    public class SettingBO : BaseObject, IBusinessObject
    {
        public string DisplayName
        {
            get
            {
                return ToString();
            }
        }
        [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
        public Guid ObjectId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public Guid? CountryId { get; set; }
    }
}
