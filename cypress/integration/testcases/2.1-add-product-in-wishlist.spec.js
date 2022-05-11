import selectors from '../../support/common/selectors'
import {
  testSetup,
  updateRetry,
  preserveCookie,
} from '../../support/common/support'
import wishListSelectors from '../../support/wish-list-selectors'

describe('Testing Single Product and total amounts', () => {
  // Load test setup
  testSetup(false)

  it('Open storefront', updateRetry(3), () => {
    cy.openStoreFront()
  })

  it('Add product to wish list', updateRetry(4), () => {
    cy.get(wishListSelectors.WishListIcon)
      .should('be.visible')
      .eq(0)
      .click()
    cy.get(selectors.ToastMsgInB2B, { timeout: 5000 })
      // .should('be.visible')
      .contains('You need to login')
    cy.get(wishListSelectors.ToastButton)
      .should('be.visible')
      .click()
  })

  it('Login storefront', updateRetry(2), () => {
    cy.get(wishListSelectors.LoginEmail)
      .should('be.visible')
      .clear()
      .type('robot.partnerhere@gmail.com')
    cy.get(wishListSelectors.LoginPassword)
      .should('be.visible')
      .clear()
      .type('!Q2w#E4r%T6y&U')

    cy.get(wishListSelectors.LoginButton).click()
  })

  it(
    'Verify we are able to see wishlist section and its product',
    updateRetry(5),
    () => {
      cy.get(selectors.ProfileLabel)
        .should('be.visible')
        .should('have.contain', `Hello,`)
      cy.get(selectors.ToastMsgInB2B, { timeout: 5000 })
        // .should('be.visible')
        .contains('Product added')
      cy.get(wishListSelectors.ToastButton)
        .should('be.visible')
        .click()
      cy.get(wishListSelectors.ProductSummaryContainer)
        .should('exist')
        .should('have.attr', 'href')
        .then(href => {
          cy.log(href)
        })
    }
  )

  it(
    'Verify we are able to see wishlist in /wishlist page',
    updateRetry(5),
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
