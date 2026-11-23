namespace WishList.Data
{
    public class WishListConstants
    {
        public const string AppToken = "X-VTEX-API-AppToken";
        public const string AppKey = "X-VTEX-API-AppKey";
        public const string IsProduction = "X-Vtex-Workspace-Is-Production";
        public const string VtexIdCookie = "VtexIdclientAutCookie";
        // Named HttpClient for VTEX ID and License Manager. The authorization gate runs on every request, so a slow
        // dependency must fail (and deny) well before the 60 s route timeout instead of the 100 s HttpClient default.
        public const string AUTH_HTTP_CLIENT = "vtex-auth";
        public static readonly System.TimeSpan AuthHttpClientTimeout = System.TimeSpan.FromSeconds(6);

        public const string FORWARDED_HEADER = "X-Forwarded-For";
        public const string FORWARDED_HOST = "X-Forwarded-Host";
        public const string APPLICATION_JSON = "application/json";
        public const string HEADER_VTEX_CREDENTIAL = "X-Vtex-Credential";
        public const string AUTHORIZATION_HEADER_NAME = "Authorization";
        public const string ACCOUNT_ID_HEADER_NAME = "account-id";
        public const string PROXY_AUTHORIZATION_HEADER_NAME = "Proxy-Authorization";
        public const string USE_HTTPS_HEADER_NAME = "X-Vtex-Use-Https";
        public const string PROXY_TO_HEADER_NAME = "X-Vtex-Proxy-To";
        public const string VTEX_ACCOUNT_HEADER_NAME = "X-Vtex-Account";
        public const string DATA_ENTITY = "wishlist";
        public const string SCHEMA = "wishlist";

        // No public field: the app accesses the entity only with its own token. Keep it identical on 0.x and 1.x.
        public const string SCHEMA_JSON = "{\"name\":\"wishlist\",\"properties\":{\"email\":{\"type\":\"string\",\"title\":\"Shopper ID\",\"description\":\"\"},\"ListItemsWrapper\":{\"type\":\"array\",\"title\":\"The ListItemsWrapper schema\",\"description\":\"An explanation about the purpose of this instance.\"}},\"v-indexed\":[\"email\"],\"v-default-fields\":[\"email\",\"ListItemsWrapper\"],\"v-cache\":false,\"v-security\":{\"allowGetAll\":false,\"publicRead\":[],\"publicWrite\":[],\"publicFilter\":[]},\"v-immediate-indexing\":true}";
    }
}
