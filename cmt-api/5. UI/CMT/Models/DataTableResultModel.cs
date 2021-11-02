using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMT
{
    [DataContract]
    public class DataTableResultModel<T>
    {
        [DataMember(Name = "total")]
        public int Total { get; set; }
        [DataMember(Name = "data")]
        public IEnumerable<T> Data { get; set; }
    }
}