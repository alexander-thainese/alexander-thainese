using CMT.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMT.Models
{
    public class ValueTagModel
    {
        public string SchemaTag { get; set; }
        public Guid ValueId { get; set; }
        public Guid? TagElementId { get; set; }
        public Guid? TagValueId { get; set; }
        public Guid? CountryId { get; set; }
        public ValueTagType Type { get; set; }
    }
}