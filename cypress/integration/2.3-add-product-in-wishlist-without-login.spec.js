import {
  loginViaCookies,
  updateRetry,
  preserveCookie,
} from '../support/common/support.js'
import wishlistProducts from '../support/wishlistProducts.js'

const prefix = '2.2'

describe(`${prefix} - Testing Wishlist with anonymous user`, () => {
  loginViaCookies({ storeFrontCookie: false })

  it(`${prefix} - Add coconut to wish list`, updateRetry(1), () => {
    cy.openStoreFront()
    cy.addProductToWishList(wishlistProducts.coconut.link, true)
  })

  it(
    `${prefix} - Verify we are able to see coconut in wishlist section`,
    updateRetry(3),
    () => {
      cy.verifyWishlistProduct(wishlistProducts.coconut.link)
    }
  )

  it(`${prefix} - Verify we are able to see coconut in /wishlist page`, () => {
    cy.visitWishlistPage()
    cy.verifyProductInWishList(wishlistProducts.coconut.link)
  })

  preserveCookie()
})
