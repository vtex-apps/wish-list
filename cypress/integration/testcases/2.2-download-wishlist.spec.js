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
    cy.get(`a[href="/cauliflower-fresh/p"] ${wishListSelectors.WishListIcon}`)
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
    }
  )

  it('Open admin dashboard wishlist', updateRetry(3), () => {
    cy.visit('admin/app/wishlist')
  })

  it('Download wishlist file', updateRetry(4), () => {
    cy.get('.layout__container button')
      .should('be.visible')
      .click()
  })

  it('Verify wishlist data', updateRetry(4), () => {
    cy.task('readXlsx', {
      file: '../../../../downloads/wishlists.xls',
      sheet: 'Sheet1',
    }).then(rows => {
      cy.log(rows.length)
      cy.writeFile('cypress/fixtures/xlsxData.json', { rows })
    })
  })

  preserveCookie()
})
