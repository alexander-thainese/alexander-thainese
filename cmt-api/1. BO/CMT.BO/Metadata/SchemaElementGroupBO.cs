using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CMT.BO
{
    [DataContract(Name = "SchemaElementGroup")]
    public class SchemaElementGroupBO
    {
        [DataMember(Name = Consts.OBJECT_ID_SERIALIZED_NAME)]
        public Guid ObjectId { get; set; }
    }
}
