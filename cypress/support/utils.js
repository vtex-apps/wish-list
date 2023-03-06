import selectors from './common/selectors.js'
import messages from '../../messages/en.json'

export function scroll() {
  // Page loads heart icon only on scroll
  // So, scroll first then look for selectors
  cy.scrollTo(0, 2000)
  // eslint-disable-next-line cypress/no-unnecessary-waiting
  cy.wait(1000)
  cy.scrollTo(0, -1000)
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
  cy.gotoMyAccountWishListPage()
  cy.get(`${productLink} span[class*=cart]`)
    .should('be.visible')
    .click()
  cy.get(selectors.ProceedtoCheckout)
    .should('be.visible')
    .click()
}

export const MESSAGES = {
  NotLoggedInUser: messages['store/wishlist-not-logged'],
  AddedToWishList: messages['store/wishlist-product-added-to-list'],
  DownloadWishList: messages['admin/settings.download'],
}
