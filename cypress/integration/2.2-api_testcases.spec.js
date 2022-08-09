import { loginViaCookies } from '../support/common/support.js'
import { restAndGraphqlAPI } from '../support/outputvalidation.js'
import {
  readwishlistByEmail,
  updateMasterdata,
  deleteWishlistdata,
  readWishListdata,
  readWishListSchema,
  wipe,
} from '../support/api_testcase.js'

describe('Graphql and REST API Testcase', () => {
  loginViaCookies({ storeFrontCookie: false })

  const { payload, newShopperId } = restAndGraphqlAPI

  wipe()

  readwishlistByEmail(payload.shopperId)
  updateMasterdata(payload.shopperId, newShopperId)
  readWishListSchema()
  readWishListdata()
  readwishlistByEmail(newShopperId)
  deleteWishlistdata(newShopperId)
})
