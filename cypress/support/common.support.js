import selectors from './common/selectors'
import {
    generateAddtoCartCardSelector,
} from './common/utils.js'

export function addToWishList(searchKey) {
    cy.get(selectors.searchResult).should('have.text', searchKey.toLowerCase())
    cy.get(selectors.ProductAnchorElement)
        .should('have.attr', 'href')
        .then(href => {
            cy.get(selectors.ProfileLabel)
                .should('be.visible')
                .should('have.contain', `Hello,`)
            cy.get(generateAddtoCartCardSelector(href))
                .first()
                .click()
        })
    cy.get('.vtex-wish-list-1-x-wishlistIconContainer > button')
        .should('be.visible')
        .click()
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
    // cy.get('.vtex-slider-layout-0-x-slide--lastVisible > .vtex-slider-layout-0-x-slideChildrenContainer > .vtex-product-summary-2-x-container > .vtex-product-summary-2-x-clearLink > .vtex-product-summary-2-x-element > .bg-action-primary > .vtex-button__label')
    //   .should('be.visible')
    //   .click()
    cy.get(`a[href=${productLink}] button`).last()
        .should('be.visible')
        .click()
    cy.get('#proceed-to-checkout > .vtex-button__label')
        .should('be.visible')
        .click()
    cy.get('#cart-to-orderform')
        .should('be.visible')
        .click()
}

export function fillInformation() {
    cy.get('#client-first-name')
        .type('saravanan')
    cy.get('#client-last-name')
        .type('reddy')
    cy.get('#client-phone')
        .type('(778) 822 3311', {
            delay: 50,
        })
    cy.get('#go-to-shipping')
        .should('be.visible')
        .click()
    cy.get('#ship-country')
        .should('be.visible')
        .select('United States of America')
    cy.get('#ship-addressQuery')
        .clear()
        .type('19501 19501 Biscayne Blvd Aventura 33180', { delay: 80 })
        .wait(500)
        .type('{downarrow}{enter}')
    cy.get('#ship-complement')
        .clear()
        .type('Kingston Plaza')
    cy.get('#btn-go-to-payment')
        .should('be.visible')
        .click()
}