using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vtex.Api.Context;
using WishList.Data;
using WishList.Models;
using System.Net;

namespace WishList.Services
{
    public class WishListService : IWishListService
    {
        private readonly IWishListRepository _wishListRepository;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIOServiceContext _context;

        private const int MaximumReturnedRecords = 999;

        public WishListService(IWishListRepository wishListRepository, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IIOServiceContext context)
        {
            this._wishListRepository = wishListRepository ??
                                            throw new ArgumentNullException(nameof(wishListRepository));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                                  throw new ArgumentNullException(nameof(clientFactory));

            this._context = context ??
                               throw new ArgumentNullException(nameof(context));
        }

        public async Task<WishListWrapper> GetList(string shopperId, string listName)
        {
            ListItemsWrapper listItemsWrapper = new ListItemsWrapper();
            WishListWrapper wishListWrapper = await _wishListRepository.GetWishList(shopperId);
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
                _context.Vtex.Logger.Debug("GetList", null, "Retrying...");
                wishListWrapper = await _wishListRepository.GetWishList(shopperId);
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

        public async Task<ResponseListWrapper> GetLists(string shopperId)
        {
            return await _wishListRepository.GetWishList(shopperId);
        }

        public async Task<bool> SaveList(IList<ListItem> listItems, string shopperId, string listName, bool? isPublic, string documentId)
        {
            IList<ListItem> listItemsToSave = null;

            WishListWrapper wishListWrapper = await this.GetList(shopperId, listName);
            ListItemsWrapper listItemsWrapper = wishListWrapper.ListItemsWrapper.FirstOrDefault();
            if (listItemsWrapper != null && listItemsWrapper.ListItems != null)
            {
                _context.Vtex.Logger.Debug("SaveList", null, $"Saving {listItems.Count} new items {listItemsWrapper.ListItems.Count} existing items.");
                listItemsToSave = listItemsWrapper.ListItems;
                foreach (ListItem listItem in listItems)
                {
                    listItemsToSave.Add(listItem);
                }
            }
            else
            {
                _context.Vtex.Logger.Debug("SaveList", null, $"Saving {listItems.Count} new items.");
                listItemsToSave = listItems;
            }

            return await _wishListRepository.SaveWishList(listItemsToSave, shopperId, listName, isPublic, documentId);
        }

        public async Task<int?> SaveItem(ListItem listItem, string shopperId, string listName, bool? isPublic)
        {
            IList<ListItem> listItemsToSave = null;

            WishListWrapper wishListWrapper = await this.GetList(shopperId, listName);
            ListItemsWrapper listItemsWrapper = wishListWrapper.ListItemsWrapper.FirstOrDefault();
            if (listItemsWrapper != null && listItemsWrapper.ListItems != null)
            {
                _context.Vtex.Logger.Debug("SaveItem", null, $"Saving {listItemsWrapper.ListItems.Count} existing items.");
                listItemsToSave = listItemsWrapper.ListItems;
                foreach (ListItem item in listItemsToSave)
                {
                    if (listItem.ProductId ==  item.ProductId)
                    {
                        listItem.Id = item.Id;
                    }
                }
                if(listItem.Id == null)
                {
                    int maxId = 0;
                    if (listItemsToSave.Count > 0)
                    {
                        maxId = listItemsToSave.Max(t => t.Id ?? 0);
                    }

                    listItem.Id = ++maxId;
                    _context.Vtex.Logger.Debug("SaveItem", null, $"Setting Id: {listItem.Id}");
                }
                else
                {
                    // If an Id has been specified, remove existing item
                    ListItem itemToRemove = listItemsToSave.Where(r => r.Id == listItem.Id).FirstOrDefault();
                    if (itemToRemove != null && listItemsToSave.Remove(itemToRemove))
                    {
                        _context.Vtex.Logger.Debug("SaveItem", null, $"Removing {listItem.Id}");
                        listItemsToSave.Remove(itemToRemove);
                    }
                }

                listItemsToSave.Add(listItem);
            }
            else
            {
                listItem.Id = listItem.Id ?? 0;
                listItemsToSave = new List<ListItem> { listItem };
                _context.Vtex.Logger.Debug("SaveItem", null, $"First Item: {listItem.Id}");
            }

            if(await _wishListRepository.SaveWishList(listItemsToSave, shopperId, listName, isPublic, wishListWrapper.Id))
            {
                _context.Vtex.Logger.Debug("SaveItem", null, $"Saved: {listItem.Id}");
            }
            else
            {
                _context.Vtex.Logger.Warn("SaveItem", null, $"Failed to save: {listItem.Id}");
            }

            return listItem.Id;
        }

        public async Task<bool> RemoveItem(int itemId, string shopperId, string listName)
        {
            bool wasRemoved = false;
            IList<ListItem> listItemsToSave = null;
            WishListWrapper wishListWrapper = await this.GetList(shopperId, listName);
            ListItemsWrapper listItemsWrapper = wishListWrapper.ListItemsWrapper.FirstOrDefault();
            if (listItemsWrapper != null && listItemsWrapper.ListItems != null)
            {
                listItemsToSave = listItemsWrapper.ListItems;
                ListItem itemToRemove = listItemsToSave.FirstOrDefault(r => r.Id == itemId);
                if (itemToRemove != null && listItemsToSave.Remove(itemToRemove))
                {
                    wasRemoved = await _wishListRepository.SaveWishList(listItemsToSave, shopperId, listName, listItemsWrapper.IsPublic, wishListWrapper.Id);
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

        public async Task<ValidatedUser> ValidateUserToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            ValidatedUser validatedUser = null;
            ValidateToken validateToken = new ValidateToken
            {
                Token = token
            };

            var jsonSerializedToken = JsonConvert.SerializeObject(validateToken);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://{this.GetAccount()}.vtexcommercestable.com.br/api/vtexid/credential/validate"),
                Content = new StringContent(jsonSerializedToken, Encoding.UTF8, WishListConstants.APPLICATION_JSON)
            };

            string authToken = this.GetCredentialHeader();

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
                _context.Vtex.Logger.Error("ValidateUserToken", null, $"Error validating user token", ex);
            }

            return validatedUser;
        }

        private string GetAccount()
        {
            return this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME].ToString();
        }

        private string GetCredentialHeader()
        {
            string credential = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.HEADER_VTEX_CREDENTIAL];
            return string.IsNullOrEmpty(credential) ? null : credential;
        }

        private async Task<bool> IsUserLoginGrantedInLicenseManagerAsync(string account, string userId, string userToken)
        {
            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            var grantedRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://{account}.vtexcommercestable.com.br/api/pvt/accounts/{account}/logins/{Uri.EscapeDataString(userId)}/granted")
            };

            // Proxy-Authorization carries the app credential, which the IO outbound proxy authorizes the call against.
            // VtexIdclientAutCookie carries the user's own token. The app credential is not sent as Authorization,
            // otherwise the router resolves the app as the current user.
            grantedRequest.Headers.Add(WishListConstants.USE_HTTPS_HEADER_NAME, "true");
            grantedRequest.Headers.Add(WishListConstants.VtexIdCookie, userToken);
            string credentialHeader = this.GetCredentialHeader();
            if (credentialHeader != null)
            {
                grantedRequest.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, credentialHeader);
            }

            try
            {
                var client = _clientFactory.CreateClient();
                var grantedResponse = await client.SendAsync(grantedRequest);
                string body = (await grantedResponse.Content.ReadAsStringAsync()).Trim();

                if (!grantedResponse.IsSuccessStatusCode)
                {
                    _context.Vtex.Logger.Warn("IsUserLoginGrantedInLicenseManagerAsync", null, $"License Manager grant check failed [{(int)grantedResponse.StatusCode}]");
                    return false;
                }

                if (string.Equals(body, "true", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (string.Equals(body, "false", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                try
                {
                    return JsonConvert.DeserializeObject<bool>(body);
                }
                catch (JsonException)
                {
                    _context.Vtex.Logger.Warn("IsUserLoginGrantedInLicenseManagerAsync", null, "Unexpected License Manager granted response body.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("IsUserLoginGrantedInLicenseManagerAsync", null, "Error checking License Manager grant", ex);
                return false;
            }
        }

        private async Task<bool> IsAdminAsync()
        {
            // Admin candidates: the admin cookie and the VtexIdclientAutCookie header (REST integrations).
            // Each one is only accepted after VTEX ID validation and the License Manager grant check.
            string headerToken = this._httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VtexIdCookie];
            string[] candidateTokens = { _context.Vtex.AdminUserAuthToken, headerToken };

            foreach (string token in candidateTokens.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct())
            {
                if (await IsAdminTokenAsync(token))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> IsAdminTokenAsync(string token)
        {
            ValidatedUser admin = await ValidateUserToken(token);
            if (admin == null
                || !"Success".Equals(admin.AuthStatus, StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(admin.Id))
            {
                return false;
            }

            return await IsUserLoginGrantedInLicenseManagerAsync(this.GetAccount(), admin.Id, token);
        }

        public async Task<RequesterContext> GetRequesterAsync()
        {
            RequesterContext requester = new RequesterContext();

            try
            {
                requester.IsAdmin = await IsAdminAsync();

                string storeToken = _context.Vtex.StoreUserAuthToken;
                if (!string.IsNullOrWhiteSpace(storeToken))
                {
                    ValidatedUser shopper = await ValidateUserToken(storeToken);
                    if (shopper != null && "Success".Equals(shopper.AuthStatus, StringComparison.OrdinalIgnoreCase))
                    {
                        requester.UserId = string.IsNullOrWhiteSpace(shopper.Id) ? null : shopper.Id.Trim();
                        requester.Email = string.IsNullOrWhiteSpace(shopper.User) ? null : shopper.User.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetRequesterAsync", null, "Error resolving requester", ex);
                return new RequesterContext();
            }

            requester.IsAuthenticated = requester.IsAdmin || requester.UserId != null || requester.Email != null;

            return requester;
        }

        public async Task<HttpStatusCode> EnsureAdmin()
        {
            RequesterContext requester = await GetRequesterAsync();
            if (requester.IsAdmin)
            {
                return HttpStatusCode.OK;
            }

            if (!requester.IsAuthenticated)
            {
                return HttpStatusCode.Unauthorized;
            }

            _context.Vtex.Logger.Warn("EnsureAdmin", null, "Access denied: admin permission required.");

            return HttpStatusCode.Forbidden;
        }

        public async Task<HttpStatusCode> EnsureCanAccessShopper(string shopperId)
        {
            RequesterContext requester = await GetRequesterAsync();
            if (!requester.IsAuthenticated)
            {
                return HttpStatusCode.Unauthorized;
            }

            if (requester.IsAdmin)
            {
                return HttpStatusCode.OK;
            }

            string requested = shopperId == null ? null : shopperId.Trim();
            if (!string.IsNullOrEmpty(requested)
                && ((requester.Email != null && requested.Equals(requester.Email, StringComparison.OrdinalIgnoreCase))
                    || (requester.UserId != null && requested.Equals(requester.UserId, StringComparison.OrdinalIgnoreCase))))
            {
                return HttpStatusCode.OK;
            }

            _context.Vtex.Logger.Warn("EnsureCanAccessShopper", null, "Access denied: token does not match requested shopperId.");

            return HttpStatusCode.Forbidden;
        }

        public async Task<int> GetListSizeBase()
        {

            int wishListAllSize = await _wishListRepository.GetListsSize();
            return wishListAllSize;
        }

        public async Task<WishListsWrapper> ExportAllWishLists()
        {
            WishListsWrapper wishListsWrapper = await _wishListRepository.GetAllLists();
            return wishListsWrapper;
        }

        public async Task<WishListsWrapper> ExportAllWishListsPaged(int pageList)
        {
            WishListsWrapper wishListsWrapper = await _wishListRepository.GetAllListsPaged(pageList);
            return wishListsWrapper;
        }
    }
}
