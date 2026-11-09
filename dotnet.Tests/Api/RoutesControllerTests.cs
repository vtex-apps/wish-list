using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using service.Controllers;
using Vtex.Api.Context;
using WishList.Data;
using WishList.Exporting;
using WishList.Models;
using Xunit;

namespace WishList.Tests.Api
{
    public class RoutesControllerTests
    {
        private static RoutesController Create(Mock<IWishListRepository> repository, HttpContext httpContext = null)
        {
            return new RoutesController(Mock.Of<IIOServiceContext>(), repository.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext ?? new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task BusyExportReturnsRetryable429()
        {
            var repository = new Mock<IWishListRepository>();
            repository
                .Setup(value => value.GetAllLists(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ExportBusyException());
            var controller = Create(repository);

            var result = await controller.ExportAllLists();

            var response = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status429TooManyRequests, response.StatusCode);
            Assert.Equal("5", controller.Response.Headers["Retry-After"]);
            var body = Newtonsoft.Json.Linq.JObject.FromObject(response.Value);
            Assert.Equal("EXPORT_IN_PROGRESS", (string)body["code"]);
            Assert.True((bool)body["retryable"]);
        }

        [Fact]
        public async Task SuccessReturnsJsonOfWrapper()
        {
            var wrapper = new WishListsWrapper();
            var repository = new Mock<IWishListRepository>();
            repository
                .Setup(value => value.GetAllLists(It.IsAny<CancellationToken>()))
                .ReturnsAsync(wrapper);

            var result = await Create(repository).ExportAllLists();

            var json = Assert.IsType<JsonResult>(result);
            Assert.Same(wrapper, json.Value);
        }

        [Fact]
        public async Task RepositoryReceivesRequestAbortedToken()
        {
            using var cts = new CancellationTokenSource();
            var httpContext = new DefaultHttpContext { RequestAborted = cts.Token };
            var repository = new Mock<IWishListRepository>();
            repository
                .Setup(value => value.GetAllLists(cts.Token))
                .ReturnsAsync(new WishListsWrapper());

            await Create(repository, httpContext).ExportAllLists();

            repository.Verify(value => value.GetAllLists(cts.Token), Times.Once);
        }
    }
}
