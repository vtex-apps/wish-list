using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Moq;
using WishList.Exporting;
using WishList.GraphQL;
using WishList.Models;
using WishList.Services;
using Xunit;

namespace WishList.Tests.Api
{
    public class ExportGraphQLTests
    {
        private static Mock<IWishListService> AuthorizedService()
        {
            var service = new Mock<IWishListService>();
            service.Setup(value => value.IsValidAuthUser()).ReturnsAsync(HttpStatusCode.OK);
            return service;
        }

        private static Task<ExecutionResult> Run(Mock<IWishListService> service, string query)
        {
            var schema = new Schema(new FuncDependencyResolver(type =>
                type.GetConstructors().Any(c => c.GetParameters().Length == 1)
                    ? System.Activator.CreateInstance(type, service.Object)
                    : System.Activator.CreateInstance(type)))
            {
                Query = new Query(service.Object)
            };
            return new DocumentExecuter().ExecuteAsync(options =>
            {
                options.Schema = schema;
                options.Query = query;
            });
        }

        [Fact]
        public async Task ExportListSuccessKeepsDataShape()
        {
            var service = AuthorizedService();
            service.Setup(value => value.ExportAllWishLists(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WishListsWrapper
                {
                    WishLists = new List<WishListWrapper> { new WishListWrapper { Id = "abc" } }
                });

            var result = await Run(service, "query { exportList { id } }");

            Assert.Null(result.Errors);
            var data = Assert.IsAssignableFrom<IDictionary<string, object>>(result.Data);
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(data["exportList"]);
            var first = Assert.IsAssignableFrom<IDictionary<string, object>>(list.Single());
            Assert.Equal("abc", first["id"]);
        }

        [Fact]
        public async Task BusyExportReportsExportInProgress()
        {
            var service = AuthorizedService();
            service.Setup(value => value.ExportAllWishLists(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ExportBusyException());

            var result = await Run(service, "query { exportList { id } }");

            Assert.Contains(result.Errors, error => error.Code == "EXPORT_IN_PROGRESS");
        }

        [Fact]
        public async Task BusyPagedExportReportsExportInProgress()
        {
            var service = AuthorizedService();
            service.Setup(value => value.ExportAllWishListsPaged(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ExportBusyException());

            var result = await Run(service, "query { exportListPaged(pageList: 1) { id } }");

            Assert.Contains(result.Errors, error => error.Code == "EXPORT_IN_PROGRESS");
        }

        [Fact]
        public async Task InvalidPageIsRejectedWithoutCallingService()
        {
            var service = AuthorizedService();

            var result = await Run(service, "query { exportListPaged(pageList: 0) { id } }");

            Assert.Contains(result.Errors, error => error.Code == "INVALID_PAGE");
            service.Verify(
                value => value.ExportAllWishListsPaged(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
