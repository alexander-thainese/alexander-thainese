using CF.Common;
using System.Web;

namespace CMT.Web
{
    public class AspNetThreadLocalHybridScopeDataStore : ScopeDataStore
    {
        protected readonly WebScopeDataStore WebScopeDataStore = new WebScopeDataStore();
        protected readonly ThreadLocalScopeDataStore ThreadLocalScopeDataStore = new ThreadLocalScopeDataStore();

        public override object this[string key]
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return WebScopeDataStore[key];
                }
                else
                {
                    return ThreadLocalScopeDataStore[key];
                }
            }
            set
            {
                if (HttpContext.Current != null)
                {
                    WebScopeDataStore[key] = value;
                }
                else
                {
                    ThreadLocalScopeDataStore[key] = value;
                }
            }
        }

        public override bool TryGetObject<TValue>(string key, out TValue value)
        {
            if (HttpContext.Current != null)
            {
                return WebScopeDataStore.TryGetObject(key, out value);
            }
            else
            {
                return ThreadLocalScopeDataStore.TryGetObject(key, out value);
            }
        }

        public override void RemoveObject(string key)
        {
            if (HttpContext.Current != null)
            {
                WebScopeDataStore.RemoveObject(key);
            }
            else
            {
                ThreadLocalScopeDataStore.RemoveObject(key);
            }
        }

        public override bool Contains(string key)
        {
            if (HttpContext.Current != null)
            {
                return WebScopeDataStore.Contains(key);
            }
            else
            {
                return ThreadLocalScopeDataStore.Contains(key);
            }
        }
    }
}
