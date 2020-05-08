using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;
using WishList.Models;
using WishList.Services;

namespace WishList.GraphQL.Types
{
    [GraphQLMetadata("ListResponseType")]
    public class ListResponseType : ObjectGraphType<ListResponse>
    {
        public ListResponseType(IWishListService wishListService)
        {
            Name = "ListResponseType";
            Field(b => b.Data, type: typeof(ListGraphType<ListItemType>)).Description("List of Items.");
            Field(b => b.Range, type: typeof(RangeType)).Description("Pagination values.");
        }
    }
}
