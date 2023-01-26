using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;
using WishList.Models;
using WishList.Services;

namespace WishList.GraphQL.Types
{
    [GraphQLMetadata("WishListWrapperType")]
    public class WishListWrapperType : ObjectGraphType<WishListWrapper>
    {
        public WishListWrapperType(IWishListService wishListService)
        {
            Name = "WishListWrapperType";

            Field(b => b.Id).Description("The wishlist Id");
            Field(b => b.Email).Description("The shopper Id of the wishlist");
            Field(b => b.ListItemsWrapper, type: typeof(ListGraphType<ListItemsType>)).Description("Wrapper of the list items");
        }
    }
}