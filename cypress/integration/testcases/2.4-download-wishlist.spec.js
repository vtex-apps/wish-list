import selectors from '../../support/common/selectors'
import {
  testSetup,
  updateRetry,
  preserveCookie,
} from '../../support/common/support'
import { downloadWishlistFile } from '../../support/wishlist'
import wishlistProducts from '../../support/wishlistProducts'

const fileName = 'cypress/downloads/wishlists.xls'
const fixtureFile = 'cypress/fixtures/wishlistData.json'

describe('Download wishlist csv and verify data', () => {
  // Load test setup
  testSetup(false)

  it('Add product to wish list', updateRetry(3), () => {
    cy.openStoreFront()
    cy.addProductToWishList(wishlistProducts.cauliflower.link, true)
  })

  it(
    'Add another product to wishlist and verify we are able to see wishlist section and its product',
    updateRetry(3),
    () => {
      cy.get(selectors.ProfileLabel)
        .should('be.visible')
        .should('have.contain', `Hello,`)
      cy.addProductToWishList(wishlistProducts.coconut.link)
      cy.get(selectors.ToastMsgInB2B, { timeout: 50000 })
        // .should('be.visible')
        .contains('Product added')
    }
  )

  downloadWishlistFile()

  it('Verify wishlist data', updateRetry(2), () => {
    cy.verifyExcelFile(fileName, fixtureFile)
  })

  it('Deleting files', () => {
    cy.task('deleteFile', { fileName })
    cy.task('deleteFile', { fileName: fixtureFile })
  })

  preserveCookie()
})
