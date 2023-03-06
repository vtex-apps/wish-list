import selectors from './common/selectors.js'
import wishListSelectors from './selectors.js'
import { scroll, MESSAGES } from './utils.js'

const wishlistJson = '.wishlist.json'

Cypress.Commands.add('openStoreFront', (login = false) => {
  const checkItemOperation = 'CheckItem'

  cy.url().then(url => {
    if (url.includes('blank')) {
      cy.getVtexItems().then(vtex => {
        cy.intercept('**/rc.vtex.com.br/api/events').as('events')
        cy.visit('/')
        if (login === true) {
          cy.get(selectors.ProfileLabel, { timeout: 20000 })
            .should('be.visible')
            .should('have.contain', `Hello,`)
        }
        cy.wait('@events')
        interceptByOperationName(vtex, checkItemOperation)
        scroll()
        cy.wait(`@${checkItemOperation}`, { timeout: 20000 }).then(req => {
          expect(req.response.statusCode).to.equal(200)
        })
      })
    } else {
      cy.log('Already in storefront page')
    }
  })
})

Cypress.Commands.add('parseXlsx', inputFile => {
  return cy.task('parseXlsx', { filePath: inputFile })
})

function interceptByOperationName(vtex, operationName) {
  cy.intercept('POST', `${vtex.baseUrl}/**`, req => {
    if (req.body.operationName === operationName) {
      req.continue()
    }
  }).as(operationName)
}

function clickWishListIcon(productLink = '', login = '') {
  const addWishListOperation = 'AddToList'

  cy.getVtexItems().then(vtex => {
    cy.qe(`Verifying the wishlistcontainer should be visible`)
    cy.get(wishListSelectors.WishListContainer).should('be.visible')
    if (productLink) {
      cy.get(productLink).should('be.visible')
    }
    const wishListOutLineSelector = `${productLink} ${wishListSelectors.WishListOutLine}`
    const wishListIconSelector = `${productLink} ${wishListSelectors.WishListIcon}`
    const wishListFillSelector = `${productLink} ${wishListSelectors.WishListFill}`

    cy.get('body').then($body => {
      if ($body.find(wishListOutLineSelector).length) {
        interceptByOperationName(vtex, addWishListOperation)
        cy.qe(
          `Verifying wishlistIcon sholud be visible and enabled then clicking on it`
        )
        cy.get(wishListIconSelector, { timeout: 15000 })
          .should('be.visible')
          .should('be.enabled')
          .click({ force: true })
        if (!login) {
          cy.qe(
            `Verifying ToastMsgInB2B should contain ${MESSAGES.AddedToWishList} before timeout`
          )
          cy.get(selectors.ToastMsgInB2B, { timeout: 10000 })
            .contains(MESSAGES.AddedToWishList)
            .should('be.visible')
          cy.qe(
            `Verify wishlistFillSelector should be visible before the timeout`
          )
          cy.get(wishListFillSelector, { timeout: 10000 }).should('be.visible')
          cy.wait(`@${addWishListOperation}`, { timeout: 20000 }).then(req => {
            expect(req.response.statusCode).to.equal(200)
          })
        } else {
          cy.qe(
            `Verifying ToastMsgInB2B should contain ${MESSAGES.AddedToWishList} before timeout`
          )
          cy.get(selectors.ToastMsgInB2B, { timeout: 10000 })
            .contains(MESSAGES.NotLoggedInUser)
            .should('be.visible')
        }
      } else {
        cy.get(wishListFillSelector, { timeout: 10000 }).should('be.visible')
        cy.log('Product already added to wishlist')
      }
    })
  })
}

Cypress.Commands.add('addProductToWishList', (productLink, login = false) => {
  clickWishListIcon(productLink, login)
  // eslint-disable-next-line vtex/prefer-early-return
  if (login) {
    cy.qe(
      `Verifying ToastMsgInB2B should contain ${MESSAGES.AddedToWishList} before timeout`
    )
    cy.get(selectors.ToastMsgInB2B, { timeout: 15000 }).contains(
      MESSAGES.NotLoggedInUser
    )
    cy.qe(`Verifying the toast button should be visible and clicking on it`)
    cy.get(wishListSelectors.ToastButton)
      .should('be.visible')
      .click()
    cy.getVtexItems().then(vtex => {
      cy.loginStoreFrontAsUser(vtex.robotMail, vtex.robotPassword)
    })
    cy.qe(
      `Verifying profile lable should be visible and it should contain Hello`
    )
    cy.get(selectors.ProfileLabel)
      .should('be.visible')
      .should('have.contain', `Hello,`)
  }
})

Cypress.Commands.add('removeProductFromWishlist', productLink => {
  cy.qe(`Verifying wishlist product summary container should be visible`)
  cy.qe(
    `Clicking on wishlist icon to remove the product from the product summary container`
  )
  cy.get(wishListSelectors.ProductSummaryContainer).should('be.visible')
  cy.get(`${productLink} ${wishListSelectors.WishListIcon}`)
    .should('be.visible')
    .click()
})

Cypress.Commands.add('addWishListItem', (searchKey, link) => {
  cy.qe(
    `Verifying the search bar should be visible and searching the product - ${searchKey}`
  )
  cy.get(selectors.Search)
    .should('be.visible')
    .clear()
    .type(searchKey)
    .type('{enter}')

  // Page should load successfully now searchResult & Filter should be visible
  cy.qe(
    `Verifying the search result should have text in the lowercase ${searchKey.toLowerCase()}`
  )
  cy.get(selectors.searchResult).should('have.text', searchKey.toLowerCase())
  cy.qe(`Verifying filter heading should be visible`)
  cy.get(selectors.FilterHeading).should('be.visible')

  cy.get(link)
    .should('be.visible')
    .click()
  cy.qe('Verifying the thumpnailswiper should be visible')
  cy.get(wishListSelectors.ThumbnailSwiper).should('be.visible')

  clickWishListIcon()
})

Cypress.Commands.add('addProductFromProductSpecification', productLink => {
  cy.qe(`Verifying ${productLink} should be visible then clicking on it`)
  cy.get(productLink)
    .should('be.visible')
    .click()
  cy.qe(`Verifying the wishlist icon should be visible and clicking on it`)
  cy.get(wishListSelectors.WishListIcon)
    .should('be.visible')
    .click()
})

Cypress.Commands.add('loginStoreFrontAsUser', (email, password) => {
  cy.qe(`Login as a loginStoreFrontAsUser`)
  cy.qe(`Entering the email`)
  cy.get(wishListSelectors.LoginEmail)
    .should('be.visible')
    .clear()
    .type(email, { log: false })
  cy.qe(`Entering the password`)
  cy.get(wishListSelectors.LoginPassword)
    .should('be.visible')
    .clear()
    .type(password, { log: false })
  cy.qe(`Clicking on login button`)
  cy.get(wishListSelectors.LoginButton).click()
})

Cypress.Commands.add('verifyExcelFile', (fileName, fixtureFile, products) => {
  cy.qe(`Verifying the data of the XLS file by using readXLSX`)
  cy.task('readXlsx', {
    file: fileName,
    sheet: 'Sheet1',
  }).then(rows => {
    cy.qe(`Writing the data in cypress/fixtures/${fixtureFile} `)
    cy.writeFile(`cypress/fixtures/${fixtureFile}`, rows)
  })
  for (const product of products) {
    cy.fixture(fixtureFile).then(fixtureData => {
      cy.intercept('GET', '**/api', { fixture: fixtureFile })
      const filterProducts = fixtureData.filter(
        list => list.Title === product.title
      )
      expect(product).to.be.equal(product)
      expect(filterProducts.length).to.be.equal(1)
    })
  }
})

Cypress.Commands.add('verifyWishlistProduct', productLink => {
  cy.qe(
    `Verifying Profile lable should be visible and it should contain 'Hello'`
  )
  cy.get(selectors.ProfileLabel)
    .should('be.visible')
    .should('have.contain', `Hello,`)

  scroll()
  cy.qe(
    `Verifying ToastMsgInB2B should contain ${MESSAGES.AddedToWishList} before timeout`
  )
  cy.get(selectors.ToastMsgInB2B, { timeout: 60000 }).contains(
    MESSAGES.AddedToWishList
  )
  cy.qe(`Verifying the Toast button should be visible and clicking on it`)
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
  cy.qe(`Verifying the products in wishlist container`)
  cy.get(
    `${wishListSelectors.ProductSummaryContainer} > ${productLink}`
  ).should('exist')
})

Cypress.Commands.add('verifyProductInWishList', productLink => {
  cy.qe(`Verifying the products in wishlist container`)
  cy.get(`${wishListSelectors.ProductSummaryContainer} > ${productLink}`, {
    timeout: 40000,
  }).should('exist')
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

Cypress.Commands.add('visitWishlistPage', () => {
  cy.qe(`Verifying wishlistmenu should be visble and clicking on it`)
  cy.get(wishListSelectors.WishListMenu)
    .should('be.visible')
    .click()
  cy.qe(`Verifying ProductSummaryContainer should be visible`)
  cy.get(wishListSelectors.ProductSummaryContainer).should('be.visible')
})

Cypress.Commands.add('gotoMyAccountWishListPage', () => {
  cy.qe(`Verifying the profile label should be visible`)
  cy.get(selectors.ProfileLabel).should('be.visible')
  cy.qe(`Clicking on SignIn  button`)
  cy.get(selectors.SignInBtn).click()
  cy.qe('Clicking on myAccount')
  cy.get(selectors.MyAccount, { timeout: 5000 }).click()
  cy.qe(
    `Verifying the wishlistAccountPage should be visible and clicking on it`
  )
  cy.get(wishListSelectors.WishListAccountPage)
    .should('be.visible')
    .click()
})
