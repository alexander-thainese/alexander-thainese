using System;

namespace CMT.Common.Interfaces
{
    /// <summary>
    /// Interface for auditable object
    /// </summary>
    public interface IAuditableObject
    {/// <summary>
     /// Name of the user that created record
     /// </summary>
        string CreateUser { get; set; }

        /// <summary>
        /// Date and time of record creation
        /// </summary>
        DateTime CreateDate { get; set; }

        /// <summary>
        /// Name of the user that modified record
        /// </summary>
        string ChangeUser { get; set; }

        /// <summary>
        /// Date and time of record modification
        /// </summary>
        DateTime? ChangeDate { get; set; }
    }
}
