using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace WishList.GraphQL.Types
{
    [GraphQLMetadata("ListItemInputType")]
    public class ListItemInputType : InputObjectGraphType<ListItemInput>
    {
        public ListItemInputType()
        {
            Name = "ListItemInputType";

            Field(x => x.Id, nullable: true);
            Field(x => x.ProductId, nullable: false);
            Field(x => x.Sku, nullable: true);
            Field(x => x.Title, nullable: true);
        }
    }
}
