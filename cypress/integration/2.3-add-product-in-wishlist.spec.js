import {
  testSetup,
  updateRetry,
  preserveCookie,
} from '../support/common/support.js'
import wishlistProducts from '../support/wishlistProducts.js'

describe('Testing Wishlist with single Product', () => {
  // Load test setup
  testSetup(false)

  it('Add product to wish list', updateRetry(3), () => {
    cy.clearLocalStorage()
    cy.openStoreFront()
    cy.addProductToWishList(wishlistProducts.onion.link, true)
  })

  it(
    'Verify we are able to see wishlist section and its product',
    updateRetry(3),
    () => {
      cy.verifyWishlistProduct(wishlistProducts.onion.link, false)
      cy.visit('/wishlist')
    }
  )

  it('Verify we are able to see wishlist in /wishlist page', () => {
    cy.verifyProductInWishList(wishlistProducts.onion.link)
  })

  preserveCookie()
})
