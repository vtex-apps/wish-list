import selectors from '../support/common/selectors.js'
import {
  preserveCookie,
  testSetup,
  updateRetry,
} from '../support/common/support.js'
import wishListSelectors from '../support/selectors.js'
import wishlistProducts from '../support/wishlistProducts.js'

describe('Adding product in wishlist', () => {
  testSetup()

  it('adding onion to wishlist from homepage', updateRetry(3), () => {
    cy.openStoreFront(true)
    // adding onion to wishlists
    cy.addProductToWishList(wishlistProducts.onion.link)
  })

  it('adding a cauliflower to wishlists from homepage', updateRetry(3), () => {
    // adding cauliflower to wishlists
    cy.addProductToWishList(wishlistProducts.cauliflower.link)
  })

  it('adding orange from product specification page', updateRetry(3), () => {
    // adding orange from product specification page

    cy.addWishListItem(
      wishlistProducts.orange.name,
      wishlistProducts.orange.link
    )
  })

  it('adding watermelon from product secification page', updateRetry(3), () => {
    // adding watermelon from the product specification page
    cy.addWishListItem(
      wishlistProducts.watermelon.name,
      wishlistProducts.watermelon.link
    )
  })

  it('Verify we are able to see wishlist section and its product', () => {
    cy.get(selectors.ProfileLabel)
      .should('be.visible')
      .should('have.contain', `Hello,`)
    cy.get('div.relative > .vtex-button')
      .should('be.visible')
      .click()
    cy.get('.vtex-login-2-x-button')
      .should('be.visible')
      .click()
    cy.get(':nth-child(6) > .vtex-account_menu-link')
      .should('be.visible')
      .click()
    cy.get(wishlistProducts.onion.link).should('be.visible')
    cy.get(wishlistProducts.orange.link).should('be.visible')
    cy.get(wishlistProducts.cauliflower.link).should('be.visible')
    cy.get(wishlistProducts.watermelon.link).should('be.visible')
  })

  it(
    'Verify we are able to see wishlist in /wishlist page',
    updateRetry(3),
    () => {
      cy.visit('/wishlist')
      cy.get(wishListSelectors.ProductSummaryContainer).should('be.visible')
      cy.get(wishlistProducts.onion.link).should('be.visible')
      cy.get(wishlistProducts.orange.link).should('be.visible')
      cy.get(wishlistProducts.cauliflower.link).should('be.visible')
      cy.get(wishlistProducts.watermelon.link).should('be.visible')
    }
  )

  it('remove the product onion from the wishlist page', updateRetry(3), () => {
    // Removing product onion from wishlist
    cy.removeProductFromWishlist(wishlistProducts.onion.link)
  })

  it(
    'remove the product cauliflower from the wishlistpage',
    updateRetry(3),
    () => {
      // Removing product cauliflower from wishlist

      cy.removeProductFromWishlist(wishlistProducts.cauliflower.link)
    }
  )

  it('Re add the product onion to wishlist', updateRetry(3), () => {
    const searchKey = wishlistProducts.onion.name
    cy.get(selectors.Search)
      .should('be.visible')
      .clear()
      .type(searchKey)
      .type('{enter}')
    // Page should load successfully now searchResult & Filter should be visible
    cy.get(selectors.searchResult).should('have.text', searchKey.toLowerCase())
    cy.get(selectors.FilterHeading).should('be.visible')

    cy.get(wishlistProducts.onion.link)
      .should('be.visible')
      .click({ multiple: true })

    cy.get(wishListSelectors.WishListIcon)
      .should('be.visible')
      .click({ multiple: true })
  })

  it(
    'verify now we are able to see three products in account wishlist page',
    updateRetry(3),
    () => {
      cy.get(selectors.ProfileLabel)
        .should('be.visible')
        .should('have.contain', `Hello,`)
      cy.get('div.relative > .vtex-button')
        .should('be.visible')
        .click()
      cy.get('.vtex-login-2-x-button', { timeout: 10000 })
        .should('be.visible')
        .click()
      cy.get(':nth-child(6) > .vtex-account_menu-link')
        .should('be.visible')
        .click()
      cy.get(wishlistProducts.onion.link).should('be.visible')
      cy.get(wishlistProducts.orange.link).should('be.visible')
      cy.get(wishlistProducts.watermelon.link).should('be.visible')
    }
  )

  preserveCookie()
})
