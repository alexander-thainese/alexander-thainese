using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CMT.Helpers
{
    public static class JsonSerializerHelper
    {
        public static Dictionary<string, object> GetObjectFieldValues(string jsonObjectString)
        {
            return GetObjectFieldValues(jsonObjectString, null);
        }

        public static Dictionary<string, object> GetObjectFieldValues(string jsonObjectString, JsonSerializerSettings jsonSerializerSettings)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(jsonObjectString))
            {
                JObject jObject = jsonSerializerSettings != null ? (JObject)JsonConvert.DeserializeObject(jsonObjectString, jsonSerializerSettings) : (JObject)JsonConvert.DeserializeObject(jsonObjectString);

                foreach (KeyValuePair<string, JToken> item in jObject)
                {
                    if (item.Value is JValue)
                    {
                        result.Add(item.Key, ((JValue)item.Value).Value);
                    }
                }
            }

            return result;
        }
    }
}