namespace WishList.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

            this.VerifySchema(WishListConstants.SCHEMA_BASE).Wait();
        }

        public async Task<bool> SaveWishList(IList<ListItem> listItems, string shopperId, string listName, bool? isPublic, string documentId, string scopeMode, string organizationId = null, string costCenterId = null)
        {
            await this.VerifySchema(WishListConstants.GetSchemaForScopeMode(scopeMode));
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
                ListItemsWrapper = new List<ListItemsWrapper> { listItemsWrapper },
                OrganizationId = organizationId,
                CostCenterId = costCenterId
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

        public async Task<ResponseListWrapper> GetWishList(string shopperId, string scopeMode, string organizationId = null, string costCenterId = null)
        {
            ResponseListWrapper responseListWrapper = new ResponseListWrapper();
            if (string.IsNullOrEmpty(shopperId)) {
                _context.Vtex.Logger.Warn("GetWishList", null, $"Nonvalid shopperId");
                return responseListWrapper;
            }

            await this.VerifySchema(WishListConstants.GetSchemaForScopeMode(scopeMode));

            string searchUrl = $"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/search?_fields=id,email,ListItemsWrapper,organizationId,costCenterId&_schema={WishListConstants.SCHEMA}&email={HttpUtility.UrlEncode(shopperId)}";
            if (!string.IsNullOrEmpty(organizationId))
            {
                searchUrl += $"&organizationId={HttpUtility.UrlEncode(organizationId)}";
            }
            if (!string.IsNullOrEmpty(costCenterId))
            {
                searchUrl += $"&costCenterId={HttpUtility.UrlEncode(costCenterId)}";
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(searchUrl)
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

        public async Task VerifySchema(string schemaJson)
        {
            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/schemas/{WishListConstants.SCHEMA}")
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
                if (!response.IsSuccessStatusCode)
                {
                    _context.Vtex.Logger.Warn("VerifySchema", null, $"Failed to Verifying Schema [{response.StatusCode}]");
                }
            
                if (response.IsSuccessStatusCode)
                {
                    var existingSchema = JObject.Parse(responseContent);
                    var expectedSchema = JObject.Parse(schemaJson);

                    if (JToken.DeepEquals(existingSchema, expectedSchema))
                    {
                        _context.Vtex.Logger.Debug("VerifySchema", null, "Schema Verified.");
                    }
                    else
                    {
                        _context.Vtex.Logger.Warn("VerifySchema", null, $"Schema does not match.\n{responseContent}");
                        string compactSchema = expectedSchema.ToString(Newtonsoft.Json.Formatting.None);
                        request = new HttpRequestMessage
                        {
                            Method = HttpMethod.Put,
                            RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/schemas/{WishListConstants.SCHEMA}"),
                            Content = new StringContent(compactSchema, Encoding.UTF8, WishListConstants.APPLICATION_JSON)
                        };
                        
                        if (authToken != null)
                        {
                            request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                            request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                            request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                        }

                        response = await client.SendAsync(request);
                        responseContent = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                        {
                            _context.Vtex.Logger.Warn("VerifySchema", null, $"Failed to Apply Schema [{response.StatusCode}] {responseContent}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("VerifySchema", null, "Error: ", ex);
            }
        }

        private string BuildWhereClause(string email, string organizationId, string costCenterId)
        {
            var filters = new List<string>();
            if (!string.IsNullOrEmpty(email))
            {
                filters.Add($"email={HttpUtility.UrlEncode(email)}");
            }
            if (!string.IsNullOrEmpty(organizationId))
            {
                filters.Add($"organizationId={HttpUtility.UrlEncode(organizationId)}");
            }
            if (!string.IsNullOrEmpty(costCenterId))
            {
                filters.Add($"costCenterId={HttpUtility.UrlEncode(costCenterId)}");
            }

            return filters.Count > 0
                ? "&_where=" + string.Join(" AND ", filters)
                : string.Empty;
        }

        private async Task<string> FirstScroll(string email = null, string organizationId = null, string costCenterId = null)
        {
            string responseContent = string.Empty;
            try
            {
                string whereClause = BuildWhereClause(email, organizationId, costCenterId);
                string schemaParam = !string.IsNullOrEmpty(whereClause) ? $"&_schema={WishListConstants.SCHEMA}" : string.Empty;

                var client = _clientFactory.CreateClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/scroll?_size=200&_fields=email,ListItemsWrapper,organizationId,costCenterId{schemaParam}{whereClause}")
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

                if (response.Headers.Contains("X-VTEX-MD-TOKEN"))
                {
                    tokenResponse = response.Headers.GetValues("X-VTEX-MD-TOKEN").FirstOrDefault();
                }

                responseContent = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("First Scroll to Get The Lists", null, "Error: ", ex);
            }

            return responseContent;
        }

        private async Task<string> CountList(string email = null, string organizationId = null, string costCenterId = null)
        {
            string countList = string.Empty;

            try
            {
                string whereClause = BuildWhereClause(email, organizationId, costCenterId);
                string schemaParam = !string.IsNullOrEmpty(whereClause) ? $"&_schema={WishListConstants.SCHEMA}" : string.Empty;

                var client = _clientFactory.CreateClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/search?_fields=email{schemaParam}{whereClause}")
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
                if (response.Headers.Contains("REST-Content-Range"))
                {
                    countList = response.Headers.GetValues("REST-Content-Range").FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("CountList", null, "Error: ", ex);
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

        public async Task<int> GetListsSize(string scopeMode, string email = null, string organizationId = null, string costCenterId = null)
        {
            await this.VerifySchema(WishListConstants.GetSchemaForScopeMode(scopeMode));
            var res = await CountList(email, organizationId, costCenterId);

            if (!string.IsNullOrEmpty(res) && res.Contains("/"))
            {
                string[] subs = res.Split('/');
                if (subs.Length > 1 && Int32.TryParse(subs[1], out int count))
                {
                    return count;
                }
            }

            return 0;
        }

        public async Task<WishListsWrapper> GetAllLists(string scopeMode, string email = null, string organizationId = null, string costCenterId = null)
        {
            await this.VerifySchema(WishListConstants.GetSchemaForScopeMode(scopeMode));
            var i = 0;
            var status = true;
            JArray searchResult = new JArray();

            while (status)
            {
                if( i == 0)
                {
                    var res = await FirstScroll(email, organizationId, costCenterId);
                    if (string.IsNullOrEmpty(res))
                    {
                        status = false;
                        break;
                    }
                    JArray resArray = JArray.Parse(res);
                    searchResult.Merge(resArray);
                    if (resArray.Count < 200) status = false;
                }
                else
                {
                    var res = await SubScroll();
                    if (string.IsNullOrEmpty(res))
                    {
                        status = false;
                        break;
                    }
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

        public async Task<WishListsWrapper> GetAllListsPaged(int pageList, string scopeMode, string email = null, string organizationId = null, string costCenterId = null)
        {
            await this.VerifySchema(WishListConstants.GetSchemaForScopeMode(scopeMode));
            var i = 0;
            var status = true;
            JArray searchResult = new JArray();

            while (status)
            {
                if( i == 0)
                {
                    var res = await FirstScroll(email, organizationId, costCenterId);
                    if (string.IsNullOrEmpty(res))
                    {
                        status = false;
                        break;
                    }
                    JArray resArray = JArray.Parse(res);
                    searchResult.Merge(resArray);
                    if (resArray.Count < 200) status = false;
                }
                else
                {
                    var res = await SubScroll();
                    if (string.IsNullOrEmpty(res))
                    {
                        status = false;
                        break;
                    }
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
                int start = (pageList - 1) * 5000;
                int end = Math.Min(pageList * 5000, searchResult.Count);

                for (int l = start; l < end; l++)
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
                _context.Vtex.Logger.Error("GetAllListsPaged", null, "Error getting lists", ex);
            }

            return wishListsWrapper;
        }
    }
}
