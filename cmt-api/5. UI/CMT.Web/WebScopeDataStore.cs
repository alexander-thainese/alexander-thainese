using CF.Common;
using System;
using System.Web;

namespace CMT.Web
{
    public class WebScopeDataStore : ScopeDataStore
    {
        public override object this[string key]
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    return null;
                }

                return HttpContext.Current.Items[key];
            }
            set
            {
                if (HttpContext.Current == null)
                {
                    throw new InvalidOperationException("No HttpContext available.");
                }

                HttpContext.Current.Items[key] = value;
            }
        }

        public override bool TryGetObject<TValue>(string key, out TValue value)
        {
            if (HttpContext.Current == null)
            {
                value = default(TValue);
                return false;
            }

            object valueObject = HttpContext.Current.Items[key];

            if (valueObject != null)
            {
                value = (TValue)valueObject;
            }
            else
            {
                value = default(TValue);
            }

            return valueObject != null;
        }

        public override void RemoveObject(string key)
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException("No HttpContext available.");
            }

            HttpContext.Current.Items.Remove(key);
        }

        public override bool Contains(string key)
        {
            if (HttpContext.Current == null)
            {
                return false;
            }

            return HttpContext.Current.Items.Contains(key);
        }
    }
}
