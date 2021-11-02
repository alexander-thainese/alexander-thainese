using System;

namespace CMT.Common
{
    public static class AuditDataHelper
    {
        public static event EventHandler<AuditDataEventArgs> CollectAuditData;

        public static AuditDataEventArgs GetAuditData()
        {
            if (CollectAuditData == null)
            {
                throw new Exception("Unable to collect audit data.");
            }

            AuditDataEventArgs eventArgs = new AuditDataEventArgs(string.Empty);
            CollectAuditData(null, eventArgs);

            return eventArgs;
        }
    }
}
