using CF.Common;
using CMT.Common;
using CMT.DL.Core;
using System.Web;

namespace CMT.Web
{
    public static class Configuration
    {
        public static void Initialize()
        {
            ScopeDataStore.Register(new AspNetThreadLocalHybridScopeDataStore(), true);
            DbConnectionFactory.Register(new DbConnectionFactory(), false);
            //EntityConnectionScope.GlobalConnectionString = ConfigurationManager.ConnectionStrings["CMTEntitiesConnectionString"].ConnectionString;
            CMT.Common.AuditDataHelper.CollectAuditData += AuditDataHelper_CollectAuditData;
        }

        private static void AuditDataHelper_CollectAuditData(object sender, AuditDataEventArgs e)
        {
            if (HttpContext.Current != null && HttpContext.Current.User != null)
            {
                e.User = HttpContext.Current.User.Identity.Name;
            }
            else
            {
                e.User = "BackgroundWorker";
            }
        }
    }
}
