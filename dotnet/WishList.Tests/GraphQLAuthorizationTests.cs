using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Moq;
using WishList.GraphQL;
using WishList.Models;
using WishList.Services;
using Xunit;

namespace WishList.Tests
{
    /// <summary>
    /// Executes each GraphQL field against a mocked service and checks that the access check
    /// runs before any data access, and that a denial surfaces as an error with the status code.
    /// </summary>
    public class GraphQLAuthorizationTests
    {
        private const string ShopperId = "shopper-1";

        public static IEnumerable<object[]> ShopperFields => new[]
        {
            new object[] { "viewList", "{ viewList(shopperId: \"shopper-1\", name: \"Wishlist\") { name } }" },
            new object[] { "viewLists", "{ viewLists(shopperId: \"shopper-1\") { name } }" },
            new object[] { "checkList", "{ checkList(shopperId: \"shopper-1\", productId: \"p1\") { inList } }" },
            new object[] { "listNames", "{ listNames(shopperId: \"shopper-1\") }" },
            new object[] { "addToList", "mutation { addToList(shopperId: \"shopper-1\", name: \"Wishlist\", listItem: { productId: \"p1\" }) }" },
            new object[] { "removeFromList", "mutation { removeFromList(id: \"1\", shopperId: \"shopper-1\") }" },
        };

        private static Mock<IWishListService> CreateService(HttpStatusCode shopperAccess)
        {
            var lists = new List<ListItemsWrapper>
            {
                new ListItemsWrapper { Name = "Wishlist", IsPublic = false, ListItems = new List<ListItem>() }
            };

            var service = new Mock<IWishListService>();
            service.Setup(s => s.EnsureCanAccessShopper(It.IsAny<string>())).ReturnsAsync(shopperAccess);
            service.Setup(s => s.GetList(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new WishListWrapper { Id = "doc", ListItemsWrapper = lists });
            service.Setup(s => s.GetLists(It.IsAny<string>()))
                .ReturnsAsync(new ResponseListWrapper { Id = "doc", ListItemsWrapper = lists });
            service.Setup(s => s.LimitList(It.IsAny<IList<ListItem>>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((IList<ListItem> items, int from, int to) => items);
            service.Setup(s => s.SaveItem(It.IsAny<ListItem>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>()))
                .ReturnsAsync(1);
            service.Setup(s => s.RemoveItem(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            return service;
        }

        private static ISchema CreateSchema(IWishListService service)
        {
            // Graph types take IWishListService in their constructors
            var resolver = new FuncDependencyResolver(type =>
            {
                var ctor = type.GetConstructors().FirstOrDefault(c =>
                    c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == typeof(IWishListService));
                return ctor != null ? ctor.Invoke(new object[] { service }) : Activator.CreateInstance(type);
            });

            return new Schema(resolver)
            {
                Query = new Query(service),
                Mutation = new Mutation(service)
            };
        }

        private static async Task<ExecutionResult> Execute(IWishListService service, string query)
        {
            return await new DocumentExecuter().ExecuteAsync(new ExecutionOptions
            {
                Schema = CreateSchema(service),
                Query = query
            });
        }

        private static void VerifyNoDataAccess(Mock<IWishListService> service)
        {
            service.Verify(s => s.GetList(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            service.Verify(s => s.GetLists(It.IsAny<string>()), Times.Never);
            service.Verify(s => s.SaveItem(It.IsAny<ListItem>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>()), Times.Never);
            service.Verify(s => s.RemoveItem(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        private static object FieldValue(ExecutionResult result, string field)
        {
            var data = Assert.IsType<Dictionary<string, object>>(result.Data);
            return data[field];
        }

        [Theory]
        [MemberData(nameof(ShopperFields))]
        public async Task ShopperField_WhenShopperAccessDenied_ReturnsErrorWithoutTouchingData(string field, string query)
        {
            foreach (HttpStatusCode denied in new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden })
            {
                var service = CreateService(denied);

                ExecutionResult result = await Execute(service.Object, query);

                ExecutionError error = Assert.Single(result.Errors);
                Assert.Equal(denied.ToString(), error.Code);
                Assert.Null(FieldValue(result, field));
                service.Verify(s => s.EnsureCanAccessShopper(ShopperId), Times.Once);
                VerifyNoDataAccess(service);
            }
        }

        [Theory]
        [MemberData(nameof(ShopperFields))]
        public async Task ShopperField_WhenShopperAccessGranted_ResolvesData(string field, string query)
        {
            var service = CreateService(HttpStatusCode.OK);

            ExecutionResult result = await Execute(service.Object, query);

            Assert.True(result.Errors == null || result.Errors.Count == 0, result.Errors?.FirstOrDefault()?.Message);
            Assert.NotNull(FieldValue(result, field));
            service.Verify(s => s.EnsureCanAccessShopper(ShopperId), Times.Once);
        }
    }
}
