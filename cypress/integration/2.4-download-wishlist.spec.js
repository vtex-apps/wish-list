import {
  testSetup,
  updateRetry,
  preserveCookie,
} from '../support/common/support.js'
import { downloadWishlistFile } from '../support/wishlist.js'
import wishlistProducts from '../support/wishlistProducts.js'

const fileName = 'cypress/downloads/wishlists.xls'
const fixtureFile = 'wishlistData.json'

describe('Download wishlist csv and verify data', () => {
  // Load test setup
  testSetup(false)

  const products = [wishlistProducts.cauliflower, wishlistProducts.coconut]

  downloadWishlistFile()

  it('Verify wishlist data', updateRetry(2), () => {
    cy.verifyExcelFile(fileName, fixtureFile, products)
  })

  it('Deleting files', () => {
    cy.task('deleteFile', { fileName })
    cy.task('deleteFile', { fileName: fixtureFile })
  })

  preserveCookie()
})
