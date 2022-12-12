import {
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
import { loginViaCookies, updateRetry } from '../support/common/support.js'
import { restAndGraphqlAPI } from '../support/outputvalidation.js'
import { wipe } from '../support/api_testcase.js'
import { syncCheckoutUICustom } from '../support/common/testcase.js'
import { wishList } from '../support/app_list'
import { graphql } from '../support/common/graphql_utils.js'

describe('Graphql Testcase', () => {
  loginViaCookies({ storeFrontCookie: false })

  const { payload } = restAndGraphqlAPI

  wipe()

  it('Mutation - addToList', updateRetry(3), () => {
    graphql(wishList, addToList(payload), response => {
      validateaddToListResponse(response)
      cy.setWishListItem('id', response.body.data.addToList)
    })
  })

  it('Query - Get Version', updateRetry(3), () => {
    graphql(wishList, version(), validateGetVersionResponse)
  })

  it('Query - Get checkList', updateRetry(3), () => {
    graphql(wishList, checkList(), validateGetcheckListResponse)
  })

  syncCheckoutUICustom()

  it('Query - Get ViewList', updateRetry(3), () => {
    graphql(wishList, viewList(payload), validateGetViewListResponse)
  })

  it('Query - Get All ViewList', updateRetry(3), () => {
    graphql(wishList, viewLists(), validateGetViewListsResponse)
  })

  it('Query - Get listNames', updateRetry(3), () => {
    graphql(wishList, listNames(), validateGetlistNamesResponse)
  })

  it('Mutation - removeFromList', updateRetry(3), () => {
    cy.getWishListItem().then(wishlistid => {
      graphql(
        wishList,
        removeFromList(wishlistid.id, payload),
        validateRemoveFromListResponse
      )
    })
  })
})
