using System;
using System.Collections.Generic;
using System.Text;

namespace WishList.Models
{
    public class ListItemsWrapper
    {
        public string Id { get; set; }
        public IList<ListItem> ListItems { get; set; }
        public bool IsPublic { get; set; }
        public string Name { get; set; }
    }
}
