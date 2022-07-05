import {
  graphql,
  validateGetVersionResponse,
  validateGetViewListResponse,
  validateGetViewListsResponse,
  validateGetcheckListResponse,
  validateaddToListResponse,
  validateGetlistNamesResponse,
  validateRemoveFromListResponse,
  version,
  viewList,
  viewLists,
  checkList,
  listNames,
  addToList,
  removeFromList,
} from '../support/graphql_testcase.js'
import { testSetup, updateRetry } from '../support/common/support.js'
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
  testSetup(false)

  const { payload, newShopperId } = restAndGraphqlAPI

  wipe()
  it('Mutation - addToList', updateRetry(3), () => {
    graphql(addToList(payload), response => {
      validateaddToListResponse(response)
      cy.setWishListItem('id', response.body.data.addToList)
    })
  })

  it('Query - Get Version', updateRetry(3), () => {
    graphql(version(), validateGetVersionResponse)
  })

  it('Query - Get ViewList', updateRetry(3), () => {
    graphql(viewList(payload), validateGetViewListResponse)
  })

  it('Query - Get All ViewList', updateRetry(3), () => {
    graphql(viewLists(), validateGetViewListsResponse)
  })

  it('Query - Get checkList', updateRetry(3), () => {
    graphql(checkList(), validateGetcheckListResponse)
  })

  it('Query - Get listNames', updateRetry(3), () => {
    graphql(listNames(), validateGetlistNamesResponse)
  })

  it('Mutation - removeFromList', updateRetry(3), () => {
    cy.getWishListItem().then(wishlistid => {
      graphql(
        removeFromList(wishlistid.id, payload),
        validateRemoveFromListResponse
      )
    })
  })

  readwishlistByEmail(payload.shopperId)
  updateMasterdata(payload.shopperId, newShopperId)
  readWishListSchema()
  readWishListdata()
  readwishlistByEmail(newShopperId)
  deleteWishlistdata(newShopperId)
})
