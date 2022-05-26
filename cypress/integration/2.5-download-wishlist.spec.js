import {
  testSetup,
  updateRetry,
  preserveCookie,
} from '../support/common/support'
import { downloadWishlistFile } from '../support/wishlist'
import wishlistProducts from '../support/wishlistProducts'

const fileName = 'cypress/downloads/wishlists.xls'
const fixtureFileName = 'wishlistData.json'
const fixtureFilePath = 'cypress/fixtures/wishlistData.json'

const products = [
  wishlistProducts.onion,
  wishlistProducts.orange,
  wishlistProducts.watermelon,
  wishlistProducts.coconut,
  wishlistProducts.irobot,
]

describe('Download wishlist csv and verify data', () => {
  // Load test setup
  testSetup(false)

  const prefix = 2.5

  downloadWishlistFile(prefix)

  it(`In ${prefix} - Verify wishlist data`, updateRetry(2), () => {
    cy.verifyExcelFile(fileName, fixtureFileName, products)
  })

  it(`In ${prefix} - Deleting files`, () => {
    cy.task('deleteFile', { fileName })
    cy.task('deleteFile', { fileName: fixtureFilePath })
  })

  preserveCookie()
})
