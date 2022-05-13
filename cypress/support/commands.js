import selectors from './common/selectors'
import wishListSelectors from './wish-list-selectors'
const wishlistJson = '.wishlist.json'

Cypress.Commands.add('openStoreFront', (login = false) => {
  cy.intercept('**/rc.vtex.com.br/api/events').as('events')
  cy.visit('/')
  cy.wait('@events')
  if (login === true) {
    cy.get(selectors.ProfileLabel, { timeout: 20000 })
      .should('be.visible')
      .should('have.contain', `Hello,`)
  }
})

Cypress.Commands.add('parseXlsx', inputFile => {
  return cy.task('parseXlsx', { filePath: inputFile })
})

Cypress.Commands.add('addProductToWishList', (productLink, login = false) => {
  cy.get(`a[href="${productLink}"] ${wishListSelectors.WishListIcon}`)
    .should('be.visible')
    .click()

  // eslint-disable-next-line vtex/prefer-early-return
  if (login) {
    cy.get(selectors.ToastMsgInB2B, { timeout: 5000 })
      // .should('be.visible')
      .contains('You need to login')
    cy.get(wishListSelectors.ToastButton)
      .should('be.visible')
      .click()
    cy.getVtexItems().then(vtex => {
      cy.loginStoreFrontAsUser(vtex.robotMail, vtex.robotPassword)
    })
  }
})

Cypress.Commands.add('loginStoreFrontAsUser', (email, password) => {
  cy.get(wishListSelectors.LoginEmail)
    .should('be.visible')
    .clear()
    .type(email)
  cy.get(wishListSelectors.LoginPassword)
    .should('be.visible')
    .clear()
    .type(password)

  cy.get(wishListSelectors.LoginButton).click()
})

Cypress.Commands.add('verifyExcelFile', (fileName, fixtureFile, products) => {
  cy.task('readXlsx', {
    file: fileName,
    sheet: 'Sheet1',
  }).then(rows => {
    cy.writeFile(fixtureFile, rows)
  })
  products.map(product => {
    cy.fixture(fixtureFile).then(wishlistFixture => {
      for (const list in wishlistFixture) {
        cy.intercept('GET', '**/api', { fixture: fixtureFile })

        cy.wrap(wishlistFixture[list])
          .should('be.an', 'object')
          .and('contain', {
            title: product.name,
          })
      }
    })
  })
})

Cypress.Commands.add('verifyWishlistProduct', productLink => {
  cy.get(selectors.ProfileLabel)
    .should('be.visible')
    .should('have.contain', `Hello,`)
  cy.get(selectors.ToastMsgInB2B, { timeout: 50000 })
    // .should('be.visible')
    .contains('Product added')
  cy.get(wishListSelectors.ToastButton)
    .should('be.visible')
    .click()
  cy.getVtexItems().then(vtex => {
    cy.intercept('POST', `${vtex.baseUrl}/**`, req => {
      if (req.body.operationName === 'ViewLists') {
        req.continue()
      }
    }).as('ViewList')
    cy.get(wishListSelectors.AccounPage, { timeout: 50000 }).should(
      'be.visible'
    )
    cy.wait('@ViewList', { timeout: 40000 })
  })
  cy.get(
    `${wishListSelectors.ProductSummaryContainer} > a[href="${productLink}"]`
  ).should('exist')
})

Cypress.Commands.add('addDelayBetweenRetries', delay => {
  if (cy.state('runnable')._currentRetry > 0) cy.wait(delay)
})

// Save wishlists
Cypress.Commands.add('setWishListItem', (wishlistItem, wishlistValue) => {
  cy.readFile(wishlistJson).then(items => {
    items[wishlistItem] = wishlistValue
    cy.writeFile(wishlistJson, items)
  })
})

// Get wishlists
Cypress.Commands.add('getWishListItem', () => {
  cy.readFile(wishlistJson).then(items => {
    return items
  })
})

Cypress.Commands.add('removeWishListItem', () => {
  cy.readFile(wishlistJson).then(items => {
    return items
  })
})
