using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Vtex.Api.Context;
using WishList.Data;
using WishList.Models;

namespace WishList.Services
{
    /// <summary>
    /// C# equivalent of the node @auth directive: validates the VtexIdclientAutCookie
    /// and checks a License Manager resource for the logged in user.
    /// </summary>
    public class ResourceAuthorizationService : IResourceAuthorizationService
    {
        public const string ExportProductCode = "147";
        public const string ExportResourceKey = "storefront.wish-list.export";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IIOServiceContext _context;

        public ResourceAuthorizationService(IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IIOServiceContext context)
        {
            this._httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this._clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<HttpStatusCode> HasExportResource()
        {
            return CheckResource(ExportProductCode, ExportResourceKey);
        }

        public async Task<HttpStatusCode> CheckResource(string productCode, string resourceKey)
        {
            string token = GetUserToken();
            if (string.IsNullOrEmpty(token))
            {
                _context.Vtex.Logger.Warn("CheckResource", null, "Missing VtexIdclientAutCookie");
                return HttpStatusCode.Unauthorized;
            }

            string account = _context.Vtex.Account;
            string appToken = _context.Vtex.AuthToken;
            var client = _clientFactory.CreateClient();

            ValidatedEmailToken user;
            try
            {
                string escapedToken = Uri.EscapeDataString(token);
                string userUrl = $"https://vtexid.vtex.com.br/api/vtexid/pub/authenticated/user?authToken={escapedToken}";
                var userRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://vtexid.vtex.com.br/api/vtexid/pub/authenticated/user?authToken={escapedToken}")
                };
                userRequest.Headers.TryAddWithoutValidation(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, appToken);
                userRequest.Headers.TryAddWithoutValidation(WishListConstants.PROXY_TO_HEADER_NAME, userUrl);

                var userResponse = await client.SendAsync(userRequest);
                if (!userResponse.IsSuccessStatusCode)
                {
                    int code = (int)userResponse.StatusCode;
                    if (code == 401 || code == 403)
                    {
                        _context.Vtex.Logger.Warn("CheckResource", null, $"VTEX ID rejected the token ({code})");
                        return HttpStatusCode.Unauthorized;
                    }

                    // Timeouts, throttling and server errors are dependency failures, not a caller problem.
                    _context.Vtex.Logger.Error("CheckResource", null, $"VTEX ID returned {code}");
                    return HttpStatusCode.BadGateway;
                }

                string userBody = await userResponse.Content.ReadAsStringAsync();
                user = JsonConvert.DeserializeObject<ValidatedEmailToken>(userBody);
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("CheckResource", null, "Error validating session", ex);
                return HttpStatusCode.BadGateway;
            }

            if (user == null || string.IsNullOrEmpty(user.User) || string.IsNullOrEmpty(user.Account)
                || !string.Equals(user.Account, account, StringComparison.OrdinalIgnoreCase))
            {
                _context.Vtex.Logger.Warn("CheckResource", null, "Session is empty or belongs to another account");
                return HttpStatusCode.Unauthorized;
            }

            try
            {
                var grantRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{account}.vtexcommercestable.com.br/api/license-manager/pvt/accounts/{account}/products/{productCode}/logins/{Uri.EscapeDataString(user.User)}/resources/{resourceKey}/granted")
                };
                grantRequest.Headers.TryAddWithoutValidation(WishListConstants.AUTHORIZATION_HEADER_NAME, appToken);

                var grantResponse = await client.SendAsync(grantRequest);
                if (!grantResponse.IsSuccessStatusCode)
                {
                    _context.Vtex.Logger.Error("CheckResource", null, $"License Manager returned {(int)grantResponse.StatusCode}");
                    return HttpStatusCode.BadGateway;
                }

                string grantBody = await grantResponse.Content.ReadAsStringAsync();
                bool granted = JsonConvert.DeserializeObject<bool>(grantBody);
                if (!granted)
                {
                    _context.Vtex.Logger.Warn("CheckResource", null, $"Resource {resourceKey} not granted");
                    return HttpStatusCode.Forbidden;
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("CheckResource", null, "Error checking License Manager resource", ex);
                return HttpStatusCode.BadGateway;
            }

            return HttpStatusCode.OK;
        }

        private string GetUserToken()
        {
            HttpRequest request = this._httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                return null;
            }

            string token = request.Headers[WishListConstants.VtexIdCookie];
            if (string.IsNullOrEmpty(token) && request.Cookies != null)
            {
                request.Cookies.TryGetValue(WishListConstants.VtexIdCookie, out token);
            }

            return token;
        }
    }
}
