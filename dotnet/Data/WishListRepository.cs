namespace WishList.Data
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using WishList.Exporting;
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
        private readonly IWishListExportCoordinator _exportCoordinator;

        public WishListRepository(IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IIOServiceContext context, IWishListExportCoordinator exportCoordinator)
        {
            this._environmentVariableProvider = environmentVariableProvider ??
                                                throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                               throw new ArgumentNullException(nameof(clientFactory));

            this._context = context ??
                               throw new ArgumentNullException(nameof(context));

            this._exportCoordinator = exportCoordinator ??
                               throw new ArgumentNullException(nameof(exportCoordinator));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";

        }

        /* Accounts whose schema was already checked; avoids a GET/PUT to Master Data on every request. */
        private static readonly ConcurrentDictionary<string, DateTime> _verifiedSchemas = new ConcurrentDictionary<string, DateTime>();
        private static readonly TimeSpan SchemaVerificationTtl = TimeSpan.FromHours(1);
        private static readonly TimeSpan SchemaFailureTtl = TimeSpan.FromMinutes(2);

        /* Stores the expiry instant; failures use a short TTL so Master Data isn't hammered while it's degraded. */
        private static void MarkVerified(string account, TimeSpan ttl)
        {
            if (account != null)
            {
                _verifiedSchemas[account] = DateTime.UtcNow.Add(ttl);
            }
        }

        /* Master Data forces v-immediate-indexing to false for non-vtex.com callers, so it can't be compared. */
        private static bool SchemaMatches(string actualJson)
        {
            try
            {
                JObject actual = JObject.Parse(actualJson);
                JObject expected = JObject.Parse(WishListConstants.SCHEMA_JSON);
                actual.Remove("v-immediate-indexing");
                expected.Remove("v-immediate-indexing");
                return JToken.DeepEquals(actual, expected);
            }
            catch (JsonException)
            {
                return false;
            }
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
                    _context.Vtex.Logger.Warn("SaveWishList", null, $"Failed to Save", new[] {("shopperId",  $"'{shopperId}'"), ("listName",  $"'{listName}'"), ("StatusCode",  $"'{response.StatusCode}'"), ("responseContent",  $"'{responseContent}'")});
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
                _context.Vtex.Logger.Warn("GetWishList", null, $"Failed to get wishlist", new[] {("shopperId", $"'{shopperId}'"), ("StatusCode", $"'{response.StatusCode}'"), ("responseContent", $"'{responseContent}'")});
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
                _context.Vtex.Logger.Error("GetWishList", null, $"Error getting wishlist for {shopperId}", ex);
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

        public async Task VerifySchema(CancellationToken cancellationToken = default)
        {
            string account = null;
            try
            {
                account = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME];
                if (account != null && _verifiedSchemas.TryGetValue(account, out DateTime expiresAt) && DateTime.UtcNow < expiresAt)
                {
                    return;
                }

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{account}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/schemas/{WishListConstants.SCHEMA}")
                };

                string authToken = _context.Vtex.AuthToken;
                if (authToken != null)
                {
                    request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                    request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }

                var client = _clientFactory.CreateClient();
                var response = await client.SendAsync(request, cancellationToken);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _context.Vtex.Logger.Warn("VerifySchema", null, $"Failed to Verifying Schema [{response.StatusCode}] {responseContent.Equals(WishListConstants.SCHEMA_JSON)}");
                    MarkVerified(account, SchemaFailureTtl);
                }
            
                if (response.IsSuccessStatusCode)
                {
                    if (SchemaMatches(responseContent))
                    {
                        _context.Vtex.Logger.Debug("VerifySchema", null, "Schema Verified.");
                        MarkVerified(account, SchemaVerificationTtl);
                    }
                    else
                    {
                        _context.Vtex.Logger.Warn("VerifySchema", null, $"Schema does not match.\n{responseContent}");
                        request = new HttpRequestMessage
                        {
                            Method = HttpMethod.Put,
                            RequestUri = new Uri($"http://{account}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/schemas/{WishListConstants.SCHEMA}"),
                            Content = new StringContent(WishListConstants.SCHEMA_JSON, Encoding.UTF8, WishListConstants.APPLICATION_JSON)
                        };
                        
                        if (authToken != null)
                        {
                            request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                            request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                            request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                        }

                        response = await client.SendAsync(request, cancellationToken);
                        responseContent = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                        {
                            _context.Vtex.Logger.Warn("VerifySchema", null, $"Failed to Apply Schema [{response.StatusCode}] {responseContent}");
                        }

                        /* Cached even on failure so a schema the app can't fix doesn't retry on every request. */
                        MarkVerified(account, SchemaVerificationTtl);
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("VerifySchema", null, "Error: ", ex);
                MarkVerified(account, SchemaFailureTtl);
            }
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

        public async Task<int> GetListsSize() {

            await this.VerifySchema();
            JArray searchResult = new JArray();
            var res = await CountList();

            string[] subs = res.Split('/');

            return Int32.Parse(subs[1]);
        }

        public async Task<WishListsWrapper> GetAllLists(CancellationToken cancellationToken = default)
        {
            // Schema verification runs inside the export gate so a rejected export never touches Master Data.
            return await _exportCoordinator.ExportAllAsync(this.VerifySchema, cancellationToken);
        }

        public async Task<WishListsWrapper> GetAllListsPaged(int pageList, CancellationToken cancellationToken = default)
        {
            return await _exportCoordinator.ExportPageAsync(pageList, this.VerifySchema, cancellationToken);
        }
    }
}
