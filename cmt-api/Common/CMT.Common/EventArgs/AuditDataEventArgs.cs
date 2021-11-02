using System;

namespace CMT.Common
{
    public class AuditDataEventArgs : EventArgs
    {
        public string User { get; set; }

        public AuditDataEventArgs(string user)
        {
            User = user;
        }
    }
}
