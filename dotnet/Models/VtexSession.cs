using Newtonsoft.Json;
using System.Collections.Generic;

namespace WishList.Models
{
    public class VtexSession
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("namespaces")]
        public SessionNamespaces Namespaces { get; set; }
    }

    public class SessionNamespaces
    {
        [JsonProperty("storefront-permissions")]
        public StorefrontPermissions StorefrontPermissions { get; set; }

        [JsonProperty("profile")]
        public SessionProfile Profile { get; set; }
    }

    public class StorefrontPermissions
    {
        [JsonProperty("organization")]
        public SessionValue Organization { get; set; }

        [JsonProperty("costcenter")]
        public SessionValue CostCenter { get; set; }
    }

    public class SessionProfile
    {
        [JsonProperty("isAuthenticated")]
        public SessionValue IsAuthenticated { get; set; }

        [JsonProperty("email")]
        public SessionValue Email { get; set; }

        [JsonProperty("id")]
        public SessionValue Id { get; set; }
    }

    public class SessionValue
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class SessionContext
    {
        public bool IsAuthenticated { get; set; }
        public string Email { get; set; }
        public string OrganizationId { get; set; }
        public string CostCenterId { get; set; }
    }
}
