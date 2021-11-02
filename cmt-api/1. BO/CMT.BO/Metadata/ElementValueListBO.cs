using CMT.Common.Interfaces;
using System;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract(Name = "ElementValueList")]
    public class ElementValueListBO : BaseObject, IBusinessObject
    {
        [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
        public Guid ObjectId { get; set; }

        public string DisplayName
        {
            get
            {
                return GetType().Name;
            }
        }
    }
}
