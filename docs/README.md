📢 Use this project, [contribute](https://github.com/vtex-apps/wish-list) to it or open issues to help evolve it using [Store Discussion](https://github.com/vtex-apps/store-discussion).
# Wish List

An app to store and retrieve a wish list of products

Schema
{
    "name": "wishlist",
    "properties": {
        "email": {
            "type": "string",
            "title": "Shopper ID",
            "description": ""
        },
        "ListItemsWrapper": {
            "type": "array",
            "title": "The ListItemsWrapper schema",
            "description": "An explanation about the purpose of this instance."
        }
    },
    "v-indexed": [
        "email"
    ],
    "v-default-fields": [
        "email",
        "ListItemsWrapper"
    ],
    "v-cache": false,
    "v-security": {
    "allowGetAll": false,
    "publicRead": [ "email", "ListItemsWrapper" ],
    "publicWrite": [ "email", "ListItemsWrapper" ],
    "publicFilter": [ "email", "ListItemsWrapper" ]
  }
}