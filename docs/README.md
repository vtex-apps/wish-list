📢 Use this project, [contribute](https://github.com/vtex-apps/quick-order) to it or open issues to help evolve it using [Store Discussion](https://github.com/vtex-apps/store-discussion).

# Wishlist

<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-0-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->

The Wishlist app is designed for **B2C** stores. It adds a heart icon to shelves and product detail pages, so users can add the desired products to a Wishlist. In addition to that, a brand new route called `/wishlist` is generated, creating a page responsible for listing all wishlisted items for your users. 

![Shelf](./image/shelf.png)

*Heart icon on a shelf*

![Product Page](./image/pdp.png)
*Heart icon on a product details page*

![Wishlist](./image/wishlist-context.png)
*Wishlist page*

## Configuration

1. [Install](https://vtex.io/docs/recipes/development/installing-an-app/) the Wishlist app by running `vtex install vtex.wish-list@0.x`.
2. Open your store's Store Theme app directory in your code editor.
3. Add the Wishlist app as a `peerDependency` in the `manifest.json` file:

```diff
 "peerDependencies": {
+  "vtex.wish-list": "0.x"
 }
```

:information_source: *Once installed, the app will generate a new route called `/wishlist` for your store, creating the Wishlist custom page that displays wishlisted product items. The new page already contains a default template, meaning that the Wishlist page is ready to be rendered and no further actions are required. However, you can **customize the Wishlist page overwriting the template by creating a brand new one as you wish**. To do so, check the **Advanced configurations** section below. In addition to that, the app also exports two theme blocks: `add-to-list-btn` and `list-context.wishlist`. They are responsible, respectively, for adding the heart icon to other theme blocks and for providing product data to build the `/wishlist` page.* 

3. Add the `add-to-list-btn` block into the `store.product` template (product page). 
4. Declare the `add-to-list-btn` block as a child of the [`product-summary.shelf` block(s)](https://vtex.io/docs/components/all/vtex.product-summary/). 

## Advanced configurations

According to the Wishlist app composition, the `/wishlist` page can be highly customizable using other blocks. Currently, its default implementation is as follows:

```json
{
  "store.wishlist": {
    "blocks": [
      "flex-layout.row#top",
      "list-context.wishlist"
    ]
  },
  "flex-layout.row#top": {
    "children": [
      "flex-layout.col#title"
    ]
  },
  "flex-layout.col#title": {
    "children": [
      "rich-text#title"
    ],
    "props": {
      "blockClass": "titleWishlist",
      "preventVerticalStretch": true
    }
  },
  "rich-text#title":{
    "props": {
      "text": "### Wishlist"
    }
  },
  "list-context.wishlist": {
    "blocks": ["product-summary.shelf#wishlist"],
    "children": ["slider-layout#wishlist"]
  },
  "product-summary.shelf#wishlist": {
    "children": [
      "add-to-list-btn",
      "product-summary-image",
      "product-summary-name",
      "product-summary-space",
      "product-summary-price",
      "add-to-cart-button"
    ]
  },
  "slider-layout#wishlist": {
    "props": {
      "itemsPerPage": {
        "desktop": 5,
        "tablet": 3,
        "phone": 1
      },
      "showNavigationArrows": "desktopOnly",
      "showPaginationDots":"always",
      "infinite": false,
      "fullWidth": true,
      "blockClass": "shelf"
    }
  }
}
```


By default implementation we mean that by installing the Wishlist app in your store you're actually using the `json` above behind the scenes to build the new page template (`/wishlist`).

Therefore, in order to customize the `/wishlist` page configuration, you should:

1. Create a `wishlist.jsonc` file under `store/blocks`. 
2. Copy the code above, paste it in the new file and change it as you wish. 

## Customization

In order to apply CSS customizations to this and other blocks, follow the instructions given in the recipe on [Using CSS Handles for store customization](https://vtex.io/docs/recipes/style/using-css-handles-for-store-customization).

| CSS Handles |
| ---------------------------- |
| `columnText` |
| `columnThumb` |
| `linkText` |
| `linkThumb` |
| `listItemsContainer` |
| `listName` |
| `listTab` |
| `productDescription` |
| `productItemRow` |
| `productTitle` |
| `thumb` |
| `wishlistContainer` |
| `wishlistIcon` |
| `wishlistIconContainer` |
  
<!-- DOCS-IGNORE:start -->

## Contributors ✨

Thanks goes to these wonderful people:

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<!-- markdownlint-enable -->
<!-- prettier-ignore-end -->
<!-- ALL-CONTRIBUTORS-LIST:END -->

This project follows the [all-contributors](https://github.com/all-contributors/all-contributors) specification. Contributions of any kind are welcome!

<!-- DOCS-IGNORE:end -->
