using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vtex.Api.Context;
using WishList.Data;
using WishList.Models;

namespace WishList.Services
{
    public class WishListService : IWishListService
    {
        private readonly IWishListRepository _wishListRepository;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIOServiceContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;

        private const int MaximumReturnedRecords = 999;
        private const string SCOPE_MODE_CACHE_KEY = "wishlist_scope_mode";
        private const string APP_SETTINGS = "vtex.wish-list";
        private static readonly TimeSpan ScopeModeCacheDuration = TimeSpan.FromMinutes(60);

        public WishListService(IWishListRepository wishListRepository, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IIOServiceContext context, IMemoryCache memoryCache, IVtexEnvironmentVariableProvider environmentVariableProvider)
        {
            this._wishListRepository = wishListRepository ??
                                            throw new ArgumentNullException(nameof(wishListRepository));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                                  throw new ArgumentNullException(nameof(clientFactory));

            this._context = context ??
                               throw new ArgumentNullException(nameof(context));

            this._memoryCache = memoryCache ??
                               throw new ArgumentNullException(nameof(memoryCache));

            this._environmentVariableProvider = environmentVariableProvider ??
                               throw new ArgumentNullException(nameof(environmentVariableProvider));
        }

        public async Task<WishListWrapper> GetList(string shopperId, string listName, string organizationId = null, string costCenterId = null)
        {
            string scopeMode = await GetScopeMode();
            ListItemsWrapper listItemsWrapper = new ListItemsWrapper();
            WishListWrapper wishListWrapper = await _wishListRepository.GetWishList(shopperId, scopeMode, organizationId, costCenterId);
            if (wishListWrapper != null && wishListWrapper.ListItemsWrapper != null)
            {
                listItemsWrapper = wishListWrapper.ListItemsWrapper.Where(n => n.Name.Equals(listName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (listItemsWrapper == null)
                {
                    listItemsWrapper = new ListItemsWrapper();
                }
            }
            else
            {
                _context.Vtex.Logger.Debug("GetList", null, $"Retrying... '{shopperId}' '{listName}'");
                wishListWrapper = await _wishListRepository.GetWishList(shopperId, scopeMode, organizationId, costCenterId);
                if (wishListWrapper != null && wishListWrapper.ListItemsWrapper != null)
                {
                    listItemsWrapper = wishListWrapper.ListItemsWrapper.Where(n => n.Name.Equals(listName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                    if (listItemsWrapper == null)
                    {
                        listItemsWrapper = new ListItemsWrapper();
                    }
                }
            }

            wishListWrapper.ListItemsWrapper = new List<ListItemsWrapper> { listItemsWrapper };

            return wishListWrapper;
        }

        public async Task<ResponseListWrapper> GetLists(string shopperId, string organizationId = null, string costCenterId = null)
        {
            string scopeMode = await GetScopeMode();
            return await _wishListRepository.GetWishList(shopperId, scopeMode, organizationId, costCenterId);
        }

        public async Task<bool> SaveList(IList<ListItem> listItems, string shopperId, string listName, bool? isPublic, string documentId, string organizationId = null, string costCenterId = null)
        {
            string scopeMode = await GetScopeMode();
            IList<ListItem> listItemsToSave = null;

            WishListWrapper wishListWrapper = await this.GetList(shopperId, listName, organizationId, costCenterId);
            ListItemsWrapper listItemsWrapper = wishListWrapper.ListItemsWrapper.FirstOrDefault();
            if (listItemsWrapper != null && listItemsWrapper.ListItems != null)
            {
                _context.Vtex.Logger.Debug("SaveList", null, $"Saving '{shopperId}' '{listName}' {listItems.Count} new items {listItemsWrapper.ListItems.Count} existing items.");
                listItemsToSave = listItemsWrapper.ListItems;
                foreach (ListItem listItem in listItems)
                {
                    listItemsToSave.Add(listItem);
                }
            }
            else
            {
                _context.Vtex.Logger.Debug("SaveList", null, $"Saving '{shopperId}' '{listName}' {listItems.Count} new items.");
                listItemsToSave = listItems;
            }

            return await _wishListRepository.SaveWishList(listItemsToSave, shopperId, listName, isPublic, documentId, scopeMode, organizationId, costCenterId);
        }

        public async Task<int?> SaveItem(ListItem listItem, string shopperId, string listName, bool? isPublic, string organizationId = null, string costCenterId = null)
        {
            string scopeMode = await GetScopeMode();
            IList<ListItem> listItemsToSave = null;

            WishListWrapper wishListWrapper = await this.GetList(shopperId, listName, organizationId, costCenterId);
            ListItemsWrapper listItemsWrapper = wishListWrapper.ListItemsWrapper.FirstOrDefault();
            if (listItemsWrapper != null && listItemsWrapper.ListItems != null)
            {
                _context.Vtex.Logger.Debug("SaveItem", null, $"Saving '{shopperId}' '{listName}' {listItemsWrapper.ListItems.Count} existing items.");
                listItemsToSave = listItemsWrapper.ListItems;
                foreach (ListItem item in listItemsToSave)
                {
                    if (listItem.ProductId == item.ProductId)
                    {
                        listItem.Id = item.Id;
                    }
                }
                if (listItem.Id == null)
                {
                    int maxId = 0;
                    if (listItemsToSave.Count > 0)
                    {
                        maxId = listItemsToSave.Max(t => t.Id ?? 0);
                    }

                    listItem.Id = ++maxId;
                    _context.Vtex.Logger.Debug("SaveItem", null, $"Saving '{shopperId}' '{listName}' Setting Id: {listItem.Id}");
                }
                else
                {
                    ListItem itemToRemove = listItemsToSave.Where(r => r.Id == listItem.Id).FirstOrDefault();
                    if (itemToRemove != null && listItemsToSave.Remove(itemToRemove))
                    {
                        _context.Vtex.Logger.Debug("SaveItem", null, $"Saving '{shopperId}' '{listName}' Removing {listItem.Id}");
                        listItemsToSave.Remove(itemToRemove);
                    }
                }

                listItemsToSave.Add(listItem);
            }
            else
            {
                listItem.Id = listItem.Id ?? 0;
                listItemsToSave = new List<ListItem> { listItem };
                _context.Vtex.Logger.Debug("SaveItem", null, $"Saving '{shopperId}' '{listName}' First Item: {listItem.Id}");
            }

            if (await _wishListRepository.SaveWishList(listItemsToSave, shopperId, listName, isPublic, wishListWrapper.Id, scopeMode, organizationId, costCenterId))
            {
                _context.Vtex.Logger.Debug("SaveItem", null, $"Saving '{shopperId}' '{listName}' Saved: {listItem.Id}");
            }
            else
            {
                _context.Vtex.Logger.Warn("SaveItem", null, $"Saving '{shopperId}' '{listName}' Failed to save: {listItem.Id}");
            }

            return listItem.Id;
        }

        public async Task<bool> RemoveItem(int itemId, string shopperId, string listName, string organizationId = null, string costCenterId = null)
        {
            string scopeMode = await GetScopeMode();
            bool wasRemoved = false;
            IList<ListItem> listItemsToSave = null;
            WishListWrapper wishListWrapper = await this.GetList(shopperId, listName, organizationId, costCenterId);
            ListItemsWrapper listItemsWrapper = wishListWrapper.ListItemsWrapper.FirstOrDefault();
            if (listItemsWrapper != null && listItemsWrapper.ListItems != null)
            {
                listItemsToSave = listItemsWrapper.ListItems;
                ListItem itemToRemove = listItemsToSave.FirstOrDefault(r => r.Id == itemId);
                if (itemToRemove != null && listItemsToSave.Remove(itemToRemove))
                {
                    wasRemoved = await _wishListRepository.SaveWishList(listItemsToSave, shopperId, listName, listItemsWrapper.IsPublic, wishListWrapper.Id, scopeMode, organizationId, costCenterId);
                }
            }

            return wasRemoved;
        }

        public async Task<IList<ListItem>> LimitList(IList<ListItem> listItems, int from, int to)
        {
            int take = MaximumReturnedRecords;
            if (to > 0)
            {
                take = Math.Min((to - from) + 1, MaximumReturnedRecords);
            }

            listItems = listItems.Skip(from - 1).Take(take).ToList();

            return listItems;
        }

        public async Task<string> GetScopeMode()
        {
            if (_memoryCache.TryGetValue(SCOPE_MODE_CACHE_KEY, out string cachedMode))
            {
                return cachedMode;
            }

            string scopeMode = "none";

            try
            {
                string account = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME];
                string workspace = this._httpContextAccessor.HttpContext.Request.Headers["X-Vtex-Workspace"];

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://apps.{this._environmentVariableProvider.Region}.vtex.io/{account}/{workspace}/apps/{APP_SETTINGS}/settings")
                };

                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.HEADER_VTEX_CREDENTIAL];
                if (authToken != null)
                {
                    request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                }

                var client = _clientFactory.CreateClient();
                var response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var settings = JsonConvert.DeserializeObject<AppSettings>(responseContent);
                    scopeMode = settings?.ScopeMode ?? "none";
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetScopeMode", null, "Error fetching app settings", ex);
            }

            _memoryCache.Set(SCOPE_MODE_CACHE_KEY, scopeMode, ScopeModeCacheDuration);
            return scopeMode;
        }

        public async Task<SessionContext> GetSessionContext()
        {
            SessionContext sessionContext = new SessionContext();

            string sessionToken = this._httpContextAccessor.HttpContext.Request.Cookies["vtex_session"];
            if (string.IsNullOrEmpty(sessionToken))
            {
                sessionToken = this._httpContextAccessor.HttpContext.Request.Headers["X-Vtex-Session-Token"];
            }

            if (string.IsNullOrEmpty(sessionToken))
            {
                return sessionContext;
            }

            try
            {
                string account = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME];

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{account}.vtexcommercestable.com.br/api/sessions?items=profile.isAuthenticated,profile.email,profile.id,storefront-permissions.organization,storefront-permissions.costcenter")
                };

                string authToken = _context.Vtex.AuthToken;
                if (authToken != null)
                {
                    request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }

                request.Headers.Add("Cookie", $"vtex_session={sessionToken}");

                var client = _clientFactory.CreateClient();
                var response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var vtexSession = JsonConvert.DeserializeObject<VtexSession>(responseContent);

                    string isAuthValue = vtexSession?.Namespaces?.Profile?.IsAuthenticated?.Value;
                    sessionContext.IsAuthenticated = "true".Equals(isAuthValue, StringComparison.OrdinalIgnoreCase);
                    sessionContext.Email = vtexSession?.Namespaces?.Profile?.Email?.Value;

                    string scopeMode = await GetScopeMode();

                    if (scopeMode.Equals("organization", StringComparison.OrdinalIgnoreCase) ||
                        scopeMode.Equals("organization-and-cost-center", StringComparison.OrdinalIgnoreCase))
                    {
                        sessionContext.OrganizationId = vtexSession?.Namespaces?.StorefrontPermissions?.Organization?.Value;
                    }

                    if (scopeMode.Equals("organization-and-cost-center", StringComparison.OrdinalIgnoreCase))
                    {
                        sessionContext.CostCenterId = vtexSession?.Namespaces?.StorefrontPermissions?.CostCenter?.Value;
                    }
                }
                else
                {
                    _context.Vtex.Logger.Warn("GetSessionContext", null,
                        $"Failed to get session [{response.StatusCode}] {responseContent}");
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetSessionContext", null, "Error fetching session context", ex);
            }

            return sessionContext;
        }

        public async Task<int> GetListSizeBase(string email = null, string organizationId = null, string costCenterId = null)
        {
            string scopeMode = await GetScopeMode();
            int wishListAllSize = await _wishListRepository.GetListsSize(scopeMode, email, organizationId, costCenterId);
            return wishListAllSize;
        }

        public async Task<WishListsWrapper> ExportAllWishLists(string email = null, string organizationId = null, string costCenterId = null)
        {
            string scopeMode = await GetScopeMode();
            WishListsWrapper wishListsWrapper = await _wishListRepository.GetAllLists(scopeMode, email, organizationId, costCenterId);
            return wishListsWrapper;
        }

        public async Task<WishListsWrapper> ExportAllWishListsPaged(int pageList, string email = null, string organizationId = null, string costCenterId = null)
        {
            string scopeMode = await GetScopeMode();
            WishListsWrapper wishListsWrapper = await _wishListRepository.GetAllListsPaged(pageList, scopeMode, email, organizationId, costCenterId);
            return wishListsWrapper;
        }

        public async Task<HttpStatusCode> IsValidAdminAuthUser()
        {
            string VtexIdclientAutCookieKey = this._httpContextAccessor.HttpContext.Request.Headers["VtexIdclientAutCookie"];

            if (string.IsNullOrEmpty(_context.Vtex.AdminUserAuthToken) && string.IsNullOrEmpty(VtexIdclientAutCookieKey))
            {
                return HttpStatusCode.Unauthorized;
            }

            ValidatedUser validatedAdminUser = null;
            ValidatedUser validatedKeyApp = null;

            try
            {
                validatedAdminUser = await ValidateUserToken(_context.Vtex.AdminUserAuthToken);
                validatedKeyApp = await ValidateUserToken(VtexIdclientAutCookieKey);
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("IsValidAdminAuthUser", null, "Error validating admin user", ex);
                return HttpStatusCode.BadRequest;
            }

            bool hasAdminPermission = validatedAdminUser != null && validatedAdminUser.AuthStatus.Equals("Success");
            bool hasPermissionToken = validatedKeyApp != null && validatedKeyApp.AuthStatus.Equals("Success");

            if (!hasAdminPermission && !hasPermissionToken)
            {
                _context.Vtex.Logger.Warn("IsValidAdminAuthUser", null, "User Does Not Have Admin Permission");
                return HttpStatusCode.Forbidden;
            }

            return HttpStatusCode.OK;
        }

        private async Task<ValidatedUser> ValidateUserToken(string token)
        {
            ValidatedUser validatedUser = null;

            if (string.IsNullOrEmpty(token))
            {
                return validatedUser;
            }

            ValidateToken validateToken = new ValidateToken
            {
                Token = token
            };

            var jsonSerializedToken = JsonConvert.SerializeObject(validateToken);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/vtexid/credential/validate"),
                Content = new StringContent(jsonSerializedToken, Encoding.UTF8, WishListConstants.APPLICATION_JSON)
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();

            try
            {
                var response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    validatedUser = JsonConvert.DeserializeObject<ValidatedUser>(responseContent);
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("ValidateUserToken", null, "Error validating user token", ex);
            }

            return validatedUser;
        }
    }
}
