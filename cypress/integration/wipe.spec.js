import { loginViaCookies } from '../support/common/support.js'
import { wipe } from '../support/api_testcase.js'

describe('Wipe the wishlist which we created in master data', () => {
  loginViaCookies()
  wipe()
})
