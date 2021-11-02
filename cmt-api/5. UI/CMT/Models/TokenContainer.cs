using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace CMT.Models
{
    [DataContract]
    public class TokenContainer
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("roles", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string[] Roles { get { return UserRoles?.Split('|'); } }

        [JsonIgnore]
        public string UserRoles { get; set; }

        [JsonProperty("userRoles", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _userRoles { set { UserRoles = value; } }

        [JsonProperty(".issued")]
        public DateTime Issued { get; set; }

        [JsonProperty(".expires")]
        public DateTime Expires { get; set; }

    }
}
