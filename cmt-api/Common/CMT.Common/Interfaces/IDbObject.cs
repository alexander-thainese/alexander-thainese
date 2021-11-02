using System;

namespace CMT.Common.Interfaces
{
    /// <summary>
    /// Interface fo Db Object
    /// </summary>
    public interface IDbObject
    {
        /// <summary>Object identifier</summary>
        Guid ObjectId { get; set; }
    }
}