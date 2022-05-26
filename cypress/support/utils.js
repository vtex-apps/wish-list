import selectors from './common/selectors.js'

export function scroll() {
  // Page loads heart icon only on scroll
  // So, scroll first then look for selectors
  cy.scrollTo(0, 2000)
  // eslint-disable-next-line cypress/no-unnecessary-waiting
  cy.wait(1000)
  cy.scrollTo(0, -500)
}

export function addToWishList(searchKey, productLink) {
  cy.get(selectors.searchResult).should('have.text', searchKey.toLowerCase())
  cy.get(selectors.ProfileLabel)
    .should('be.visible')
    .should('have.contain', `Hello,`)
  cy.get(productLink).should('be.visible')
  cy.addProductToWishList(productLink)
}

export function addToCart(productLink) {
  cy.get('div.relative > .vtex-button')
    .should('be.visible')
    .click()
  cy.get('.vtex-login-2-x-button')
    .should('be.visible')
    .click()
  cy.get(':nth-child(6) > .vtex-account_menu-link')
    .should('be.visible')
    .click()
  cy.get(`${productLink} span[class*=cart]`)
    .should('be.visible')
    .click()
  cy.get('#proceed-to-checkout > .vtex-button__label')
    .should('be.visible')
    .click()
  cy.get('#cart-to-orderform')
    .should('be.visible')
    .click()
}
