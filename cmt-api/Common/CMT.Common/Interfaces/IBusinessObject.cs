using System;

namespace CMT.Common.Interfaces
{
    public interface IBusinessObject
    {
        string DisplayName { get; }
        Guid ObjectId { get; set; }


    }
}
