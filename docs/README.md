📢 Use this project, [contribute](https://github.com/vtex-apps/wish-list) to it or open issues to help evolve it using [Store Discussion](https://github.com/vtex-apps/store-discussion).

# Wishlist

<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-0-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->

The Wishlist app, designed for **B2C** stores, adds a heart icon to shelves and product detail pages, so users can add the desired products to a Wishlist. 

![wishlist-list](https://user-images.githubusercontent.com/52087100/94687168-40fbe500-0302-11eb-8239-135d773324dd.png)
*Example of heart icons on a shelf.*

![wish-list-pdp](https://user-images.githubusercontent.com/52087100/94687148-393c4080-0302-11eb-8ab4-e5bd44e642ec.png)
*Example of a heart icon on a product details page.*

In addition to that, a brand new route called `/wishlist` is generated, creating a page responsible for listing all wishlisted items for your users. 

![wishlist-page](https://user-images.githubusercontent.com/52087100/94687176-448f6c00-0302-11eb-8dcf-11881cb6fa9b.png)
*Example of a wishlist page.*

## Configuration

1. [Install](https://vtex.io/docs/recipes/development/installing-an-app/) the Wishlist app in the desired VTEX account by running `vtex install vtex.wish-list` in your terminal.
2. Open your store's Store Theme app directory in your code editor.
3. Add the Wishlist app to your theme's `manifest.json` file inside **peerDependencies** as shown below:

```diff
 "peerDependencies": {
+  "vtex.wish-list": "1.x"
 }
```

:information_source: *The Wishlist app can export two theme blocks when added as a dependency: `add-to-list-btn` and `list-context.wishlist`. They are responsible, respectively, for adding the heart icon to other theme blocks and for providing product data to build the `/wishlist` page.* 

4. Add the `add-to-list-btn` block into the `store.product` template's children block list. For example:

```diff
{
  "store.product": {
    "children": [
      "product-name",
      "product-reviews",
+      "add-to-list-btn"
    ]
  },
```

5. Declare the `add-to-list-btn` block as a child of the [`product-summary.shelf` blocks](https://vtex.io/docs/components/all/vtex.product-summary/) in your theme. For example:

```diff
  "product-summary.shelf": {
    "children": [
+     "add-to-list-btn",
      "product-summary-name",
      "product-rating-inline",
      "product-summary-price",
      "add-to-cart-button"
    ]
  }
```

:information_source: *The new route called `/wishlist`, responsible for creating the Wishlist custom page that displays wishlisted product items, already contains a default template, meaning it is ready to be rendered and no further actions are required. However, you can **customize the Wishlist page, overwriting the template by creating a brand new one as you wish**. To do so, check the **Advanced configurations** section below.* 

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
3. Add the Wishlist app as a `peerDependency` in the theme's `manifest.json` file:

```diff
 "peerDependencies": {
+  "vtex.wish-list": "1.x"
 }
```

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
