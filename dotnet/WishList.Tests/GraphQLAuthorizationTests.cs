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

        /// <summary>
        /// The data method each field resolves through, called once with the authorized shopper id.
        /// </summary>
        private static void VerifyDataAccessedOnce(Mock<IWishListService> service, string field)
        {
            switch (field)
            {
                case "viewList":
                    service.Verify(s => s.GetList(ShopperId, It.IsAny<string>()), Times.Once);
                    break;
                case "viewLists":
                case "checkList":
                case "listNames":
                    service.Verify(s => s.GetLists(ShopperId), Times.Once);
                    break;
                case "addToList":
                    service.Verify(s => s.SaveItem(It.IsAny<ListItem>(), ShopperId, It.IsAny<string>(), It.IsAny<bool?>()), Times.Once);
                    break;
                case "removeFromList":
                    service.Verify(s => s.RemoveItem(It.IsAny<int>(), ShopperId, It.IsAny<string>()), Times.Once);
                    break;
                default:
                    throw new ArgumentException($"No data method mapped for {field}");
            }
        }

        private static object FieldValue(ExecutionResult result, string field)
        {
            var data = Assert.IsType<Dictionary<string, object>>(result.Data);
            return data[field];
        }

        [Fact]
        public void EveryGraphQLField_IsCoveredByAnAccessCheckList()
        {
            // A new field must be added to ShopperFields (and gated), otherwise this fails
            IWishListService service = CreateService(HttpStatusCode.OK).Object;
            IEnumerable<string> schemaFields = new Query(service).Fields.Select(f => f.Name)
                .Concat(new Mutation(service).Fields.Select(f => f.Name))
                .OrderBy(n => n, StringComparer.Ordinal);
            IEnumerable<string> gatedFields = ShopperFields
                .Select(row => (string)row[0])
                .OrderBy(n => n, StringComparer.Ordinal);

            Assert.Equal(gatedFields, schemaFields);
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
            VerifyDataAccessedOnce(service, field);
        }

        [Theory]
        [MemberData(nameof(ShopperFields))]
        public async Task ShopperField_TrimsShopperId_SoTheAuthorizedIdIsTheOneUsedForData(string field, string query)
        {
            var service = CreateService(HttpStatusCode.OK);

            ExecutionResult result = await Execute(service.Object, query.Replace($"\"{ShopperId}\"", $"\"  {ShopperId} \""));

            Assert.True(result.Errors == null || result.Errors.Count == 0, result.Errors?.FirstOrDefault()?.Message);
            service.Verify(s => s.EnsureCanAccessShopper(ShopperId), Times.Once);
            VerifyDataAccessedOnce(service, field);
        }

        [Fact]
        public async Task ListNamesAndViewLists_WhenShopperHasNoList_ReturnEmpty()
        {
            // Master Data returns no document for a shopper who never used the wishlist
            foreach (ResponseListWrapper noList in new[] { new ResponseListWrapper(), null })
            {
                var service = CreateService(HttpStatusCode.OK);
                service.Setup(s => s.GetLists(It.IsAny<string>())).ReturnsAsync(noList);

                ExecutionResult result = await Execute(service.Object, "{ listNames(shopperId: \"shopper-1\") viewLists(shopperId: \"shopper-1\") { name } }");

                Assert.True(result.Errors == null || result.Errors.Count == 0, result.Errors?.FirstOrDefault()?.Message);
                Assert.Empty(Assert.IsAssignableFrom<IEnumerable<object>>(FieldValue(result, "listNames")));
                Assert.Empty(Assert.IsAssignableFrom<IEnumerable<object>>(FieldValue(result, "viewLists")));
            }
        }

        [Fact]
        public async Task ViewLists_SkipsNullListEntries()
        {
            var service = CreateService(HttpStatusCode.OK);
            service.Setup(s => s.GetLists(It.IsAny<string>())).ReturnsAsync(new ResponseListWrapper
            {
                ListItemsWrapper = new List<ListItemsWrapper> { null, new ListItemsWrapper { Name = "Wishlist" } }
            });

            ExecutionResult result = await Execute(service.Object, "{ listNames(shopperId: \"shopper-1\") viewLists(shopperId: \"shopper-1\") { name } }");

            Assert.True(result.Errors == null || result.Errors.Count == 0, result.Errors?.FirstOrDefault()?.Message);
            Assert.Single(Assert.IsAssignableFrom<IEnumerable<object>>(FieldValue(result, "listNames")));
            Assert.Single(Assert.IsAssignableFrom<IEnumerable<object>>(FieldValue(result, "viewLists")));
        }

        [Fact]
        public async Task ViewList_WhenListDoesNotExist_ReturnsEmptyList()
        {
            var service = CreateService(HttpStatusCode.OK);
            service.Setup(s => s.GetList(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new WishListWrapper { ListItemsWrapper = new List<ListItemsWrapper>() });

            ExecutionResult result = await Execute(service.Object, "{ viewList(shopperId: \"shopper-1\", name: \"Wishlist\") { name public } }");

            Assert.True(result.Errors == null || result.Errors.Count == 0, result.Errors?.FirstOrDefault()?.Message);
            Assert.NotNull(FieldValue(result, "viewList"));
        }
    }
}
