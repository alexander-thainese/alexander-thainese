﻿using CMT.Common.Interfaces;
using System;
using System.Runtime.Serialization;

namespace CMT.BO
{
    [DataContract(Name = "ElementType")]
    public class ElementTypeBO : BaseObject, IBusinessObject
    {
        [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
        public Guid ObjectId { get; set; }
        [DataMember]
        public string Name { get; set; }

        public string DisplayName
        {
            get
            {
                return Name;
            }
        }
    }
}