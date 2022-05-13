import {
  testSetup,
  updateRetry,
  preserveCookie,
} from '../../support/common/support'
import wishListSelectors from '../../support/wish-list-selectors'
import wishlistProducts from '../../support/wishlistProducts'

describe('Testing Single Product and total amounts', () => {
  // Load test setup
  testSetup(false)

  it('Add product to wish list', updateRetry(3), () => {
    cy.openStoreFront()
    cy.addProductToWishList(wishlistProducts.onion.link, true)
  })

  it(
    'Verify we are able to see wishlist section and its product',
    updateRetry(3),
    () => {
      cy.verifyWishlistProduct(wishlistProducts.onion.link)
    }
  )

  it(
    'Verify we are able to see wishlist in /wishlist page',
    updateRetry(3),
    () => {
      cy.verifyProductInWishList()
    }
  )

  preserveCookie()
})
