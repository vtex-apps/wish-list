namespace WishList.Data
{
    public class WishListConstants
    {
        public const string AppToken = "X-VTEX-API-AppToken";
        public const string AppKey = "X-VTEX-API-AppKey";
        public const string IsProduction = "X-Vtex-Workspace-Is-Production";
        public const string VtexIdCookie = "VtexIdclientAutCookie";

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

        public const string SCHEMA_JSON = "{\"name\":\"wishlist\",\"properties\":{\"email\":{\"type\":\"string\",\"title\":\"Shopper ID\",\"description\":\"\"},\"ListItemsWrapper\":{\"type\":\"array\",\"title\":\"The ListItemsWrapper schema\",\"description\":\"An explanation about the purpose of this instance.\"}},\"v-indexed\":[\"email\"],\"v-default-fields\":[\"email\",\"ListItemsWrapper\"],\"v-cache\":false,\"v-security\":{\"allowGetAll\":false,\"publicRead\":[\"email\",\"ListItemsWrapper\",\"id\"],\"publicWrite\":[\"email\",\"ListItemsWrapper\"],\"publicFilter\":[\"email\",\"ListItemsWrapper\"]},\"v-immediate-indexing\":true}";
    }
}
