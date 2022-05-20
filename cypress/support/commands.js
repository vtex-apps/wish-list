import selectors from './common/selectors'
import wishListSelectors from './wish-list-selectors'

const wishlistJson = '.wishlist.json'

Cypress.Commands.add('openStoreFront', (login = false) => {
  cy.intercept('**/rc.vtex.com.br/api/events').as('events')
  cy.visit('/')
  cy.wait('@events')
  // Page loads heart icon only on scroll
  // So, scroll first then look for selectors
  cy.scrollTo(0, 800)
  // cy.wait(1000)
  cy.scrollTo(0, -200)
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
  if (!login) {
    cy.get(selectors.ToastMsgInB2B, { timeout: 50000 }).contains(
      'Product added to the list'
    )
    cy.get(wishListSelectors.CloseToast).click()
  }
  cy.get(`a[href="${productLink}"] ${wishListSelectors.WishListIcon}`)
    .should('be.visible')
    .click()

  // eslint-disable-next-line vtex/prefer-early-return
  if (login) {
    cy.get(selectors.ToastMsgInB2B, { timeout: 50000 })
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

Cypress.Commands.add('removeProductFromWishlist', productLink => {
  cy.get(`a[href="${productLink}"] ${wishListSelectors.WishListIcon}`)
    .should('be.visible')
    .click()
})

Cypress.Commands.add('addWishListItem', (searchKey, link) => {
  cy.get(selectors.Search)
    .should('be.visible')
    .clear()
    .type(searchKey)
    .type('{enter}')
  // Page should load successfully now searchResult & Filter should be visible
  cy.get(selectors.searchResult).should('have.text', searchKey.toLowerCase())
  cy.get(selectors.FilterHeading).should('be.visible')

  cy.get(`a[href="${link}"]`)
    .should('be.visible')
    .click({ multiple: true })

  cy.get(wishListSelectors.WishListIcon)
    .should('be.visible')
    .click({ multiple: true })
})

Cypress.Commands.add('addProductFromProductSpecification', productLink => {
  cy.get(`a[href="${productLink}"]`)
    .should('be.visible')
    .click()
  cy.get(wishListSelectors.WishListIcon)
    .should('be.visible')
    .click()
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
    cy.writeFile(`cypress/fixtures/${fixtureFile}`, rows)
  })
  // eslint-disable-next-line array-callback-return
  products.map(product => {
    cy.log(product)
    cy.fixture(fixtureFile).then(fixtureData => {
      cy.intercept('GET', '**/api', { fixture: fixtureFile })
      const filterProducts = fixtureData.filter(
        list => list.Title === product.name
      )
      expect(filterProducts.length).to.be.equal(1)
    })
  })
})

Cypress.Commands.add('verifyWishlistProduct', productLink => {
  cy.get(selectors.ProfileLabel)
    .should('be.visible')
    .should('have.contain', `Hello,`)

  cy.get(selectors.ToastMsgInB2B, { timeout: 130000 })
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

Cypress.Commands.add('verifyProductInWishList', productLink => {
  cy.visit('/wishlist')
  cy.get(
    `${wishListSelectors.ProductSummaryContainer} > a[href="${productLink}"]`,
    { timeout: 40000 }
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
