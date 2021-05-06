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

    /// <summary>
    /// Concrete implementation of <see cref="IWishListRepository"/> for persisting data to/from Masterdata v2.
    /// </summary>
    public class WishListRepository : IWishListRepository
    {
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _applicationName;


        public WishListRepository(IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory)
        {
            this._environmentVariableProvider = environmentVariableProvider ??
                                                throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                               throw new ArgumentNullException(nameof(clientFactory));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";

            this.VerifySchema();
        }

        public async Task<bool> SaveWishList(IList<ListItem> listItems, string shopperId, string listName, bool? isPublic, string documentId)
        {
            // PATCH https://{{accountName}}.vtexcommercestable.com.br/api/dataentities/{{data_entity_name}}/documents

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
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/documents"),
                Content = new StringContent(jsonSerializedListItems, Encoding.UTF8, WishListConstants.APPLICATION_JSON)
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Save:{response.StatusCode} Id:{documentId}");

            return response.IsSuccessStatusCode;
        }

        public async Task<ResponseListWrapper> GetWishList(string shopperId)
        {
            // GET https://{{accountName}}.vtexcommercestable.com.br/api/dataentities/{{data_entity_name}}/documents/{{id}}
            // GET https://{{accountName}}.vtexcommercestable.com.br/api/dataentities/{{data_entity_name}}/search

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/search?_fields=id,email,ListItemsWrapper&_schema={WishListConstants.SCHEMA}&email={HttpUtility.UrlEncode(shopperId)}")
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Get:{response.StatusCode} ");
            ResponseListWrapper responseListWrapper = new ResponseListWrapper();
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
                Console.WriteLine($"Error:{ex.Message}: Rsp = {responseContent} ");
            }

            if (!response.IsSuccessStatusCode)
            {
                responseListWrapper.message = $"Get:{response.StatusCode}: Rsp = {responseContent}";
            }

            return responseListWrapper;
        }

        public async Task<bool> DeleteWishList(string documentId)
        {
            // DEL https://{{accountName}}.vtexcommercestable.com.br/api/dataentities/{{data_entity_name}}/documents/{{id}}

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/documents/{documentId}")
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Delete:{response.StatusCode} Id:{documentId}");

            return response.IsSuccessStatusCode;
        }

        public async Task VerifySchema()
        {
            //Console.WriteLine("Verifing Schema");
            // https://{{accountName}}.vtexcommercestable.com.br/api/dataentities/{{data_entity_name}}/schemas/{{schema_name}}
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/schemas/{WishListConstants.SCHEMA}")
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            if(response.IsSuccessStatusCode && !responseContent.Equals(WishListConstants.SCHEMA_JSON))
            {
                Console.WriteLine("--------------- Applying Schema ---------------");
                request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/schemas/{WishListConstants.SCHEMA}"),
                    Content = new StringContent(WishListConstants.SCHEMA_JSON, Encoding.UTF8, WishListConstants.APPLICATION_JSON)
                };

                if (authToken != null)
                {
                    request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                    request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }

                response = await client.SendAsync(request);
            }

            Console.WriteLine($"Schema Response: {response.StatusCode}");
        }

        public async Task<WishListsWrapper> GetAllLists()
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/search?_fields=email,ListItemsWrapper")
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            WishListsWrapper wishListsWrapper = new WishListsWrapper();
            wishListsWrapper.WishLists = new List<WishListWrapper>();
            WishListWrapper responseListWrapper = new WishListWrapper();
            try
            {
                JArray searchResult = JArray.Parse(responseContent);
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
                Console.WriteLine($"Error:{ex.Message}: Rsp = {responseContent} ");
            }

            return wishListsWrapper;
        }
    }
}
