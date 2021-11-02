using CF.Common;
using CMT.Web;

namespace CMT.Configuration
{
    public class CmtConfiguration
    {
        public virtual void Initialize()
        {
            ScopeDataStore.Register(new WebScopeDataStore(), true);

        }
    }
}
