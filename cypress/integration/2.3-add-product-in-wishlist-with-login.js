import selectors from '../support/common/selectors.js'
import {
  preserveCookie,
  testSetup,
  updateRetry,
} from '../support/common/support.js'
import wishListSelectors from '../support/selectors.js'
import wishlistProducts from '../support/wishlistProducts.js'

function verifyProducts(cauliflower = true) {
  cy.get(wishlistProducts.onion.link).should('be.visible')
  cy.get(wishlistProducts.orange.link).should('be.visible')
  cy.get(wishlistProducts.watermelon.link).should('be.visible')
  if (cauliflower) {
    cy.get(wishlistProducts.cauliflower.link).should('be.visible')
  } else {
    cy.get(wishlistProducts.cauliflower.link).should('not.exist')
  }
}

const prefix = 2.3

describe(`${prefix} - Testing wishlist with logged in user`, () => {
  testSetup()

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
    `${prefix} - adding a cauliflower to wishlists from homepage`,
    updateRetry(1),
    () => {
      // adding cauliflower to wishlists
      cy.addProductToWishList(wishlistProducts.cauliflower.link)
    }
  )

  it(
    `${prefix} -adding orange from product specification page`,
    updateRetry(2),
    () => {
      // adding orange from product specification page
      cy.addWishListItem(
        wishlistProducts.orange.name,
        wishlistProducts.orange.link
      )
    }
  )

  it(
    `${prefix} - adding watermelon from product secification page`,
    updateRetry(2),
    () => {
      // adding watermelon from the product specification page
      cy.addWishListItem(
        wishlistProducts.watermelon.name,
        wishlistProducts.watermelon.link
      )
    }
  )

  it(`${prefix} - Verify we are able to see wishlist in /wishlist page`, () => {
    cy.visitWishlistPage()
    verifyProducts()
  })

  it(`${prefix} - Verify we are able to see wishlist section and its product`, () => {
    cy.gotoMyAccountWishListPage()
    verifyProducts()
  })

  it(
    `${prefix} - Remove the product onion from the wishlist page`,
    updateRetry(1),
    () => {
      // Removing product onion from wishlist
      cy.removeProductFromWishlist(wishlistProducts.onion.link)
    }
  )

  it(
    `${prefix} - Remove the product cauliflower from the wishlist page`,
    updateRetry(1),
    () => {
      // Removing product cauliflower from wishlist
      cy.removeProductFromWishlist(wishlistProducts.cauliflower.link)
    }
  )

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

  it(
    `${prefix} - Verify now we are able to see three products in account wishlist page`,
    updateRetry(2),
    () => {
      cy.gotoMyAccountWishListPage()
      verifyProducts(false)
    }
  )

  it(
    `${prefix} - Verify we are able to see wishlist in /wishlist page`,
    updateRetry(1),
    () => {
      cy.visitWishlistPage()
      verifyProducts(false)
    }
  )

  preserveCookie()
})
