using Newtonsoft.Json;

namespace WishList.Models
{
    public class AppSettings
    {
        [JsonProperty("scopeMode")]
        public string ScopeMode { get; set; } = "none";
    }
}
