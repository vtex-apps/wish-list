import selectors from '../support/common/selectors.js'
import {
  preserveCookie,
  testSetup,
  updateRetry,
} from '../support/common/support.js'
import wishListSelectors from '../support/selectors.js'
import wishlistProducts from '../support/wishlistProducts.js'

function verifyProducts() {
  cy.get(wishlistProducts.onion.link).should('be.visible')
  cy.get(wishlistProducts.orange.link).should('be.visible')
  cy.get(wishlistProducts.watermelon.link).should('be.visible')
}

describe('Testing wishlist with logged in user', () => {
  testSetup()

  const prefix = 2.3

  it(
    `${prefix} - adding onion to wishlist from homepage`,
    updateRetry(1),
    () => {
      cy.openStoreFront(true)
      // adding onion to wishlists
      cy.addProductToWishList(wishlistProducts.onion.link)
    }
  )

  it(
    `${prefix} -adding orange from product specification page`,
    updateRetry(1),
    () => {
      // adding orange from product specification page
      cy.addWishListItem(
        wishlistProducts.orange.name,
        wishlistProducts.orange.link
      )
    }
  )

  it(
    `${prefix} - adding a cauliflower to wishlists from homepage`,
    updateRetry(1),
    () => {
      cy.openStoreFront(true)
      // adding cauliflower to wishlists
      cy.addProductToWishList(wishlistProducts.cauliflower.link)
    }
  )

  it(
    `${prefix} - adding watermelon from product secification page`,
    updateRetry(3),
    () => {
      // adding watermelon from the product specification page
      cy.addWishListItem(
        wishlistProducts.watermelon.name,
        wishlistProducts.watermelon.link
      )
    }
  )

  it(`${prefix} - Verify we are able to see wishlist section and its product`, () => {
    cy.gotoMyAccountWishListPage()
    verifyProducts()
    cy.get(wishlistProducts.cauliflower.link).should('be.visible')
  })

  it(`${prefix} - Verify we are able to see wishlist in /wishlist page`, () => {
    cy.visitWishlistPage()
    verifyProducts()
    cy.get(wishlistProducts.cauliflower.link).should('be.visible')
  })

  it(`${prefix} - Remove the product onion from the wishlist page`, () => {
    // Removing product onion from wishlist
    cy.removeProductFromWishlist(wishlistProducts.onion.link)
  })

  it(`${prefix} - Remove the product cauliflower from the wishlistpage`, () => {
    // Removing product cauliflower from wishlist
    cy.removeProductFromWishlist(wishlistProducts.cauliflower.link)
  })

  it(`${prefix} - Re add the product onion to wishlist`, updateRetry(1), () => {
    const searchKey = wishlistProducts.onion.name
    cy.get(selectors.Search)
      .should('be.visible')
      .type(searchKey)
      .type('{enter}')
    // Page should load successfully now searchResult & Filter should be visible
    cy.get(selectors.searchResult).should('have.text', searchKey.toLowerCase())
    cy.get(selectors.FilterHeading).should('be.visible')

    cy.get(wishlistProducts.onion.link)
      .should('be.visible')
      .click()

    cy.get(wishListSelectors.WishListIcon)
      .should('be.visible')
      .click()
  })

  it(`${prefix} - Verify we are able to see wishlist in /wishlist page`, () => {
    cy.visitWishlistPage()
    verifyProducts()
  })

  it(`${prefix} - Verify now we are able to see three products in account wishlist page`, () => {
    cy.gotoMyAccountWishListPage()
    verifyProducts()
  })

  preserveCookie()
})
