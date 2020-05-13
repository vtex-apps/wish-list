📢 Use this project, [contribute](https://github.com/vtex-apps/wish-list) to it or open issues to help evolve it using [Store Discussion](https://github.com/vtex-apps/store-discussion).
# Wish List

An app to store and retrieve a wish list of products

Schema
{
    "properties": {
        "ListItemsWrapper": {
            "$id": "#/properties/ListItemsWrapper",
            "type": "array",
            "title": "The ListItemsWrapper schema",
            "description": "An explanation about the purpose of this instance.",
            "default": [],
            "examples": [
                [
                    {
                        "ListItems": [
                            {
                                "Id": 2,
                                "ProductId": "testprodid",
                                "Sku": "testsku",
                                "Title": "prodtitle"
                            },
                            {
                                "Id": 4,
                                "ProductId": "testprodid",
                                "Sku": null,
                                "Title": null
                            },
                            {
                                "Id": 5,
                                "ProductId": "testprodid",
                                "Sku": null,
                                "Title": null
                            },
                            {
                                "Id": 6,
                                "ProductId": "testprodid",
                                "Sku": null,
                                "Title": null
                            }
                        ],
                        "IsPublic": false,
                        "Name": "default"
                    }
                ]
            ]
        }
    }
}