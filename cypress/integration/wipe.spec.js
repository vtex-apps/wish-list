import { testSetup } from '../support/common/support.js'
import {
  deleteWishlistdata,
  getAllWishListTestCase,
} from '../support/api_testcase.js'

describe('Wipe the wishlist which we created in master data', () => {
  testSetup()
  const ALL_RECORDS = 'all'

  getAllWishListTestCase(ALL_RECORDS)
  deleteWishlistdata(ALL_RECORDS)
})
