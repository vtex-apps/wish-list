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
import { anonymousUser } from '../support/outputvalidation.js'

describe('Graphql queries', () => {
  testSetup(false)

  it('addToList', updateRetry(3), () => {
    graphql(addToList(anonymousUser), response => {
      validateaddToListResponse(response)
      cy.setWishListItem('id', response.body.data.addToList)
    })
  })

  it('Get Version', updateRetry(3), () => {
    graphql(version(), validateGetVersionResponse)
  })

  it('Get ViewList', updateRetry(3), () => {
    graphql(viewList(), validateGetViewListResponse)
  })

  it('Get All ViewList', updateRetry(3), () => {
    graphql(viewLists(), validateGetViewListsResponse)
  })

  it('Get checkList', updateRetry(3), () => {
    graphql(checkList(), validateGetcheckListResponse)
  })

  it('Get listNames', updateRetry(3), () => {
    graphql(listNames(), validateGetlistNamesResponse)
  })

  it('removeFromList', updateRetry(3), () => {
    cy.getWishListItem().then(wishlistid => {
      graphql(removeFromList(wishlistid.id), validateRemoveFromListResponse)
    })
  })
})
