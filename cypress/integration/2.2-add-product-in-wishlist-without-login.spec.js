import {
  testSetup,
  updateRetry,
  preserveCookie,
} from '../support/common/support.js'
import wishlistProducts from '../support/wishlistProducts.js'

describe('Testing Wishlist with anonymous user', () => {
  // Load test setup
  testSetup(false)

  const prefix = '2.2'

  it(`${prefix} - Add product to wish list`, updateRetry(1), () => {
    cy.openStoreFront()
    cy.addProductToWishList(wishlistProducts.coconut.link, true)
  })

  it(
    `${prefix} - Verify we are able to see wishlist section and its product`,
    updateRetry(3),
    () => {
      cy.verifyWishlistProduct(wishlistProducts.coconut.link)
    }
  )

  it(`${prefix} - Verify we are able to see wishlist in /wishlist page`, () => {
    cy.visitWishlistPage()
    cy.verifyProductInWishList(wishlistProducts.coconut.link)
  })

  preserveCookie()
})
