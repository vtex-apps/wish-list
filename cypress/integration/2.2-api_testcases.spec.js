import { loginViaCookies } from '../support/common/support.js'
import { restAndGraphqlAPI } from '../support/outputvalidation.js'
import {
  readwishlistByEmail,
  updateMasterdata,
  deleteWishlistdata,
  readWishListSchema,
} from '../support/api_testcase.js'

describe('REST API Testcase', () => {
  loginViaCookies({ storeFrontCookie: false })

  const { payload, newShopperId } = restAndGraphqlAPI

  readwishlistByEmail(payload.shopperId)
  updateMasterdata(payload.shopperId, newShopperId)
  readWishListSchema()
  readwishlistByEmail(newShopperId)
  deleteWishlistdata(newShopperId)
})
