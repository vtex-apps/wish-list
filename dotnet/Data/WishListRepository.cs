namespace WishList.Data
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using WishList.Models;
    using WishList.Services;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Web;
    using Vtex.Api.Context;

    /// <summary>
    /// Concrete implementation of <see cref="IWishListRepository"/> for persisting data to/from Masterdata v2.
    /// </summary>
    public class WishListRepository : IWishListRepository
    {
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IIOServiceContext _context;
        private readonly string _applicationName;
        private static readonly ConcurrentDictionary<string, DateTime> _schemaCheckedUntil = new ConcurrentDictionary<string, DateTime>();
        private static readonly TimeSpan SchemaVerifiedTtl = TimeSpan.FromHours(1);
        private static readonly TimeSpan SchemaFailureTtl = TimeSpan.FromMinutes(1);
        // Kept short because the check runs inside a shopper request
        internal static TimeSpan[] SchemaRetryDelays = { TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(400) };
        private static string _tokenResponse;
        public static string tokenResponse 
        {
            get
            {
                return _tokenResponse;
            }
            set
            {
                _tokenResponse = value;
            }
        }

        public WishListRepository(IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IIOServiceContext context)
        {
            this._environmentVariableProvider = environmentVariableProvider ??
                                                throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                               throw new ArgumentNullException(nameof(clientFactory));

            this._context = context ??
                               throw new ArgumentNullException(nameof(context));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";
        }

        public async Task<bool> SaveWishList(IList<ListItem> listItems, string shopperId, string listName, bool? isPublic, string documentId)
        {
            await this.VerifySchema();
            if (listItems == null)
            {
                listItems = new List<ListItem>();
            }

            ListItemsWrapper listItemsWrapper = new ListItemsWrapper
            {
                ListItems = listItems,
                IsPublic = isPublic,
                Name = listName
            };

            WishListWrapper wishListWrapper = new WishListWrapper
            {
                Id = documentId,
                Email = shopperId,
                ListItemsWrapper = new List<ListItemsWrapper> { listItemsWrapper }
            };

            var jsonSerializedListItems = JsonConvert.SerializeObject(wishListWrapper);
            bool isSuccessStatusCode = false;
            try 
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Patch,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/documents"),
                    Content = new StringContent(jsonSerializedListItems, Encoding.UTF8, WishListConstants.APPLICATION_JSON)
                };

                string authToken = _context.Vtex.AuthToken;
                if (authToken != null)
                {
                    request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                    request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }

                var client = _clientFactory.CreateClient();
                var response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();
                isSuccessStatusCode = response.IsSuccessStatusCode;
                if (!isSuccessStatusCode) 
                {
                    _context.Vtex.Logger.Warn("SaveWishList", null, $"Failed to Save", new[] {("listName",  $"'{listName}'"), ("StatusCode",  $"'{response.StatusCode}'")});
                }
            } 
            catch (Exception ex) 
            {
                _context.Vtex.Logger.Error("SaveWishList", null, "Error: ", ex);
            }

            return isSuccessStatusCode;
        }

        public async Task<ResponseListWrapper> GetWishList(string shopperId)
        {
            ResponseListWrapper responseListWrapper = new ResponseListWrapper();
            if (string.IsNullOrEmpty(shopperId)) {
                _context.Vtex.Logger.Warn("GetWishList", null, $"Nonvalid shopperId");
                return responseListWrapper;
            }

            await this.VerifySchema();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/search?_fields=id,email,ListItemsWrapper&_schema={WishListConstants.SCHEMA}&email={HttpUtility.UrlEncode(shopperId)}")
            };

            string authToken = _context.Vtex.AuthToken;
            if (authToken != null)
            {
                request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }
            
            request.Headers.Add("Cache-Control", "no-cache");

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _context.Vtex.Logger.Warn("GetWishList", null, $"Failed to get wishlist", new[] {("StatusCode", $"'{response.StatusCode}'")});
            }

            try
            {
                JArray searchResult = JArray.Parse(responseContent);
                for(int l = 0; l < searchResult.Count; l++)
                {
                    JToken listWrapper = searchResult[l];
                    if(l == 0)
                    {
                        responseListWrapper = JsonConvert.DeserializeObject<ResponseListWrapper>(listWrapper.ToString());
                    }
                    else
                    {
                        ResponseListWrapper listToRemove = JsonConvert.DeserializeObject<ResponseListWrapper>(listWrapper.ToString());
                        bool removed = await this.DeleteWishList(listToRemove.Id);
                    }
                }
            }
            catch(Exception ex)
            {
                responseListWrapper.message = $"Error:{ex.Message}: Rsp = {responseContent} ";
                _context.Vtex.Logger.Error("GetWishList", null, "Error getting wishlist", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                responseListWrapper.message = $"Get:{response.StatusCode}: Rsp = {responseContent}";
            }

            return responseListWrapper;
        }

        public async Task<bool> DeleteWishList(string documentId)
        {
            await this.VerifySchema();
            bool isSuccessStatusCode = false;
            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/documents/{documentId}")
                };

                string authToken = _context.Vtex.AuthToken;
                if (authToken != null)
                {
                    request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                    request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }

                var client = _clientFactory.CreateClient();
                var response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();
                isSuccessStatusCode = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("DeleteWishList", null, "Error: ", ex);
            }

            return isSuccessStatusCode;
        }

        /// <summary>
        /// Makes sure the account schema matches <see cref="WishListConstants.SCHEMA_JSON"/>.
        /// The result is cached per account, so Master Data is only checked once per TTL on each pod.
        /// </summary>
        public async Task VerifySchema()
        {
            string account = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME].ToString();
            if (_schemaCheckedUntil.TryGetValue(account, out DateTime checkedUntil) && checkedUntil > DateTime.UtcNow)
            {
                return;
            }

            bool isVerified = false;
            try
            {
                isVerified = await this.EnsureSchema(account);
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("VerifySchema", null, "Error: ", ex);
            }

            // A failed check is cached for a short time to avoid hammering Master Data on every request.
            _schemaCheckedUntil[account] = DateTime.UtcNow.Add(isVerified ? SchemaVerifiedTtl : SchemaFailureTtl);
        }

        internal static void ResetSchemaCache()
        {
            _schemaCheckedUntil.Clear();
        }

        /// <summary>
        /// Compares only what the app depends on (fields, index and security), ignoring formatting,
        /// key order and properties Master Data may add to the stored schema.
        /// </summary>
        internal static bool IsSchemaUpToDate(string remoteSchema)
        {
            JObject remote;
            try
            {
                remote = JObject.Parse(remoteSchema);
            }
            catch (JsonException)
            {
                return false;
            }

            JObject expected = JObject.Parse(WishListConstants.SCHEMA_JSON);
            JObject expectedProperties = expected["properties"] as JObject;
            JObject remoteProperties = remote["properties"] as JObject;
            bool hasProperties = remoteProperties != null && expectedProperties.Properties().All(p => remoteProperties[p.Name] != null);

            return hasProperties
                && JToken.DeepEquals(expected["v-indexed"], remote["v-indexed"])
                && JToken.DeepEquals(expected["v-security"], remote["v-security"]);
        }

        private async Task<bool> EnsureSchema(string account)
        {
            HttpResponseMessage response = await this.SendSchemaRequest(HttpMethod.Get, account);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                if (IsSchemaUpToDate(responseContent))
                {
                    _context.Vtex.Logger.Debug("VerifySchema", null, "Schema Verified.");
                    return true;
                }
            }
            else if (response.StatusCode != HttpStatusCode.NotFound)
            {
                _context.Vtex.Logger.Warn("VerifySchema", null, $"Failed to Verifying Schema [{response.StatusCode}]");
                return false;
            }

            _context.Vtex.Logger.Info("VerifySchema", null, $"Applying schema [{response.StatusCode}]");
            HttpStatusCode putStatusCode = HttpStatusCode.InternalServerError;
            for (int attempt = 0; attempt <= SchemaRetryDelays.Length; attempt++)
            {
                if (attempt > 0)
                {
                    await Task.Delay(SchemaRetryDelays[attempt - 1]);
                }

                response = await this.SendSchemaRequest(HttpMethod.Put, account, WishListConstants.SCHEMA_JSON);
                putStatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode || putStatusCode == HttpStatusCode.NotModified || !IsRetryableStatus(putStatusCode))
                {
                    break;
                }
            }

            if (putStatusCode == HttpStatusCode.NotModified)
            {
                // Master Data already holds this exact schema
                _context.Vtex.Logger.Debug("VerifySchema", null, "Schema not modified.");
                return true;
            }

            if ((int)putStatusCode < 200 || (int)putStatusCode > 299)
            {
                _context.Vtex.Logger.Warn("VerifySchema", null, $"Failed to Apply Schema [{putStatusCode}]");
                return false;
            }

            response = await this.SendSchemaRequest(HttpMethod.Get, account);
            responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode && IsSchemaUpToDate(responseContent))
            {
                _context.Vtex.Logger.Info("VerifySchema", null, "Schema applied.");
                return true;
            }

            _context.Vtex.Logger.Warn("VerifySchema", null, $"Schema applied but not confirmed [{response.StatusCode}]");
            return false;
        }

        private static bool IsRetryableStatus(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.TooManyRequests
                || statusCode == HttpStatusCode.Locked
                || (int)statusCode >= 500;
        }

        private async Task<HttpResponseMessage> SendSchemaRequest(HttpMethod method, string account, string content = null)
        {
            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri($"http://{account}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/schemas/{WishListConstants.SCHEMA}")
            };

            if (content != null)
            {
                request.Content = new StringContent(content, Encoding.UTF8, WishListConstants.APPLICATION_JSON);
            }

            string authToken = _context.Vtex.AuthToken;
            if (authToken != null)
            {
                request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            return await client.SendAsync(request);
        }

        private async Task<string> FirstScroll()
        {
            string responseContent = string.Empty;
            try
            {

                var client = _clientFactory.CreateClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/scroll?_size=200&_fields=email,ListItemsWrapper")
                };

                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.HEADER_VTEX_CREDENTIAL];

                if (authToken != null)
                {
                    request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                    request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }
                request.Headers.Add("Cache-Control", "no-cache");
                var response = await client.SendAsync(request);
                
                tokenResponse = response.Headers.GetValues("X-VTEX-MD-TOKEN").FirstOrDefault();

                responseContent = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("First Scroll to Get The Lists", null, "Error: ", ex);
            }

            return responseContent;
        }

        private async Task<string> CountList()
        {
            string countList = string.Empty;

            try
            {
                var client = _clientFactory.CreateClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/search?_fields=email")
                };

                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.HEADER_VTEX_CREDENTIAL];
                if (authToken != null)
                {
                    request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                    request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }
                request.Headers.Add("Cache-Control", "no-cache");
                var response = await client.SendAsync(request);
                countList = response.Headers.GetValues("REST-Content-Range").FirstOrDefault();
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("First Scroll to Get The Lists", null, "Error: ", ex);
            }

            return countList;
        }

        private async Task<string> SubScroll()
        {
            string responseContent = string.Empty;
            try
            {
                var client = _clientFactory.CreateClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/scroll?_token={tokenResponse}")
                };

                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.HEADER_VTEX_CREDENTIAL];
                if (authToken != null)
                {
                    request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                    request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }
                request.Headers.Add("Cache-Control", "no-cache");
                var response = await client.SendAsync(request);

                responseContent = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("Sub Scrolls to Get The ListsMerchantDefinedData", null, "Error: ", ex);
            }

            return responseContent;
        }

        public async Task<int> GetListsSize() {

            await this.VerifySchema();
            JArray searchResult = new JArray();
            var res = await CountList();

            string[] subs = res.Split('/');

            return Int32.Parse(subs[1]);
        }

        public async Task<WishListsWrapper> GetAllLists()
        {
            await this.VerifySchema();
            var i = 0;
            var status = true;
            JArray searchResult = new JArray();

            while (status)
            {
                if( i == 0)
                {
                    var res = await FirstScroll();
                    JArray resArray = JArray.Parse(res);
                    searchResult.Merge(resArray);
                }
                else
                {
                    var res = await SubScroll();
                    JArray resArray = JArray.Parse(res);
                    if (resArray.Count < 200) 
                    {
                        status = false;
                    }
                    searchResult.Merge(resArray);
                }
                i++;
            }
            
            WishListsWrapper wishListsWrapper = new WishListsWrapper();
            wishListsWrapper.WishLists = new List<WishListWrapper>();
            WishListWrapper responseListWrapper = new WishListWrapper();

            try
            {
                for (int l = 0; l < searchResult.Count; l++)
                {
                    JToken listWrapper = searchResult[l];
                    if (listWrapper != null)
                    {
                        responseListWrapper = JsonConvert.DeserializeObject<WishListWrapper>(listWrapper.ToString());
                        if (responseListWrapper != null)
                        {
                            wishListsWrapper.WishLists.Add(responseListWrapper);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetAllLists", null, "Error getting lists", ex);
            }

            return wishListsWrapper;
        }

        public async Task<WishListsWrapper> GetAllListsPaged(int pageList)
        {
            await this.VerifySchema();
            var i = 0;
            var status = true;
            JArray searchResult = new JArray();

            while (status)
            {
                if( i == 0)
                {
                    var res = await FirstScroll();
                    JArray resArray = JArray.Parse(res);
                    searchResult.Merge(resArray);
                }
                else
                {
                    var res = await SubScroll();
                    JArray resArray = JArray.Parse(res);
                    if (resArray.Count < 200) 
                    {
                        status = false;
                    }
                    searchResult.Merge(resArray);
                }
                i++;
            }
            
            WishListsWrapper wishListsWrapper = new WishListsWrapper();
            wishListsWrapper.WishLists = new List<WishListWrapper>();
            WishListWrapper responseListWrapper = new WishListWrapper();

            try
            {
                for (int l = (pageList - 1) * 5000; l < pageList * 5000; l++)
                {
                    JToken listWrapper = searchResult[l];
                    if (listWrapper != null)
                    {
                        responseListWrapper = JsonConvert.DeserializeObject<WishListWrapper>(listWrapper.ToString());
                        if (responseListWrapper != null)
                        {
                            wishListsWrapper.WishLists.Add(responseListWrapper);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetAllLists", null, "Error getting lists", ex);
            }

            return wishListsWrapper;
        }
    }
}
