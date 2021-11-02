using System;

namespace CMT.BO.Admin
{
    public class ThirdPartySystem
    {
        public Guid Id { get; set; }
        public Guid RelatedObjectId { get; set; }
        public string Name { get; set; }
        public bool IsAllowed { get; set; }
    }
}
