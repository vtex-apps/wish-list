import { testSetup } from '../../support/common/support.js'
import { deleteWishlistdata, readwishlistByEmail, readWishListdata, readWishListSchema, updateMasterdata } from '../../support/wishlist.api.js'

import { testCase1 } from '../../support/wishlist.outputvalidation'

const {data} = testCase1
const wishlistEnv = 'wishlistEnv'
const wishlistDataEnv = 'wishlistDataEnv'






describe('Rest-api-testcases', () => {
    testSetup()

    readWishListSchema()
    readWishListdata(wishlistDataEnv)
    readwishlistByEmail(data)
    updateMasterdata(data,wishlistEnv)
    deleteWishlistdata()



})
