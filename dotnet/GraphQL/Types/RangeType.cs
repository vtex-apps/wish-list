using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;
using WishList.Models;
using WishList.Services;

namespace WishList.GraphQL.Types
{
    [GraphQLMetadata("RangeType")]
    public class RangeType : ObjectGraphType<ResultRange>
    {
        public RangeType(IWishListService wishListService)
        {
            Name = "RangeType";
            Field(b => b.From);
            Field(b => b.To);
            Field(b => b.Total);
        }
    }
}
