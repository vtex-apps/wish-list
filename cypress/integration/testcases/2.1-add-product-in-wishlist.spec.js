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

  it('Add product to wish list', updateRetry(2), () => {
    cy.openStoreFront()
    cy.addProductToWishList(wishlistProducts.onion.link)
  })

  it('Login storefront', updateRetry(2), () => {
    cy.getVtexItems().then(vtex => {
      cy.loginStoreFrontAsUser(vtex.robotMail, vtex.robotPassword)
    })
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
      cy.visit('/wishlist')
      cy.get(wishListSelectors.ProductSummaryContainer)
        .should('exist')
        .should('have.attr', 'href')
        .then(href => {
          cy.log(href)
        })
    }
  )

  preserveCookie()
})
