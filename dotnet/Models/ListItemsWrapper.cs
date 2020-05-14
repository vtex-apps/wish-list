using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WishList.Models
{
    public class WishListWrapper
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        public List<ListItemsWrapper> ListItemsWrapper { get; set; }
    }

    public class ListItemsWrapper
    {
        public IList<ListItem> ListItems { get; set; }
        public bool? IsPublic { get; set; }
        public string? Name { get; set; }
    }
}
