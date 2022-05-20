import { testSetup } from '../support/common/support.js'
import {
  deleteWishlistdata,
  readwishlistByEmail,
  readWishListdata,
  readWishListSchema,
  updateMasterdata,
} from '../support/api_testcase.js'
import { testCase1 } from '../support/outputvalidation.js'

const { data } = testCase1

describe('Rest-api-testcases', () => {
  testSetup()

  readWishListSchema()
  readWishListdata()
  readwishlistByEmail(data.emailId)
  updateMasterdata(data)
  deleteWishlistdata(data.emailId)
})
