using CF.Common;
using CMT.Common;
using CMT.DL;
using CMT.DL.Core;
using Microsoft.AspNet.Identity;
using System;
using System.Web;
using System.Web.Http;

namespace CMT.Controllers
{
    public class BaseApiController : ApiController
    {
        public BaseApiController()
        {
            ScopeDataStore.Register(new ThreadLocalScopeDataStore(), false);
            DbConnectionFactory.Register(new DbConnectionFactory(), false);
            //DbConnectionScope.Create(ConfigurationManager.ConnectionStrings["CMTEntitiesConnectionString"].ConnectionString);
            CmtEntities cmtEntities = SimpleInjectorConfig.GetServiceInstance<CmtEntities>();
            cmtEntities.Database.CommandTimeout = BO.ApplicationSettings.CommandTimeout;
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (DbConnectionScope.Current != null)
        //    {
        //        DbConnectionScope.Current.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        public virtual Guid AuthenticatedUserId
        {
            get
            {

                if (HttpContext.Current.IsDebuggingEnabled)
                {
                    return new Guid("177B289E-9BB8-E611-84D3-02C5858EF1CD");
                }

                return new Guid(HttpContext.Current.User.Identity.GetUserId());
            }
        }
    }
}