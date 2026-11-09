using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WishList.Data;
using WishList.Models;

namespace WishList.Exporting
{
    public sealed class MasterDataScrollReader : IMasterDataScrollReader
    {
        private const int BatchSize = 200;
        private const string TokenHeader = "X-VTEX-MD-TOKEN";

        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MasterDataScrollReader(
            IHttpClientFactory clientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<ScrollStart> StartAsync(CancellationToken cancellationToken)
        {
            using (var response = await SendAsync(BuildInitialUri(), cancellationToken))
            {
                /* Read before any validation so rejected responses still count their bytes in telemetry. */
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new WishListExportException(
                        ExportResultCode.MasterDataError,
                        $"Master Data initial scroll failed with {(int)response.StatusCode}.",
                        receivedBytes: Encoding.UTF8.GetByteCount(body));
                }

                if (!response.Headers.TryGetValues(TokenHeader, out var values))
                {
                    throw new WishListExportException(
                        ExportResultCode.InvalidResponse,
                        "Master Data initial scroll did not return X-VTEX-MD-TOKEN.",
                        receivedPages: 1,
                        receivedBytes: Encoding.UTF8.GetByteCount(body));
                }

                var token = values.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new WishListExportException(
                        ExportResultCode.InvalidResponse,
                        "Master Data initial scroll returned an empty X-VTEX-MD-TOKEN.",
                        receivedPages: 1,
                        receivedBytes: Encoding.UTF8.GetByteCount(body));
                }

                var page = ParsePage(body);
                return new ScrollStart(new ScrollSession(token), page);
            }
        }

        public async Task<ScrollPage> ReadNextAsync(
            ScrollSession session,
            CancellationToken cancellationToken)
        {
            using (var response = await SendAsync(BuildNextUri(session.Token), cancellationToken))
            {
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new WishListExportException(
                        ExportResultCode.MasterDataError,
                        $"Master Data scroll failed with {(int)response.StatusCode}.",
                        receivedBytes: Encoding.UTF8.GetByteCount(body));
                }

                return ParsePage(body);
            }
        }

        private string BaseUri()
        {
            var account = _httpContextAccessor.HttpContext.Request.Headers[WishListConstants.VTEX_ACCOUNT_HEADER_NAME];
            return $"http://{account}.vtexcommercestable.com.br/api/dataentities/{WishListConstants.DATA_ENTITY}/scroll";
        }

        private Uri BuildInitialUri()
        {
            return new Uri($"{BaseUri()}?_size={BatchSize}&_fields=email,ListItemsWrapper");
        }

        private Uri BuildNextUri(string token)
        {
            return new Uri($"{BaseUri()}?_token={Uri.EscapeDataString(token)}");
        }

        private async Task<HttpResponseMessage> SendAsync(Uri uri, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = uri })
            {
                string authToken = _httpContextAccessor.HttpContext.Request.Headers[WishListConstants.HEADER_VTEX_CREDENTIAL];
                if (authToken != null)
                {
                    request.Headers.Add(WishListConstants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(WishListConstants.VtexIdCookie, authToken);
                    request.Headers.Add(WishListConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }

                request.Headers.Add("Cache-Control", "no-cache");
                var client = _clientFactory.CreateClient();
                return await client.SendAsync(request, cancellationToken);
            }
        }

        private static ScrollPage ParsePage(string body)
        {
            var rawBytes = Encoding.UTF8.GetByteCount(body);

            List<WishListWrapper> items;
            try
            {
                items = JsonConvert.DeserializeObject<List<WishListWrapper>>(body);
            }
            catch (JsonException exception)
            {
                throw new WishListExportException(
                    ExportResultCode.InvalidResponse,
                    "Master Data returned an unparseable scroll page.",
                    exception,
                    receivedPages: 1,
                    receivedBytes: rawBytes);
            }

            if (items == null)
            {
                throw new WishListExportException(
                    ExportResultCode.InvalidResponse,
                    "Master Data returned an empty scroll body.",
                    receivedPages: 1,
                    receivedBytes: rawBytes);
            }

            var isLast = items.Count < BatchSize;
            items.RemoveAll(item => item == null);
            return new ScrollPage(items, rawBytes, isLast, PageSignature.Compute(body));
        }
    }
}
