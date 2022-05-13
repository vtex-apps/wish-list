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
} from '../../support/wishlist.graphql'
import { testSetup } from '../../support/common/support'
import { anonymousUser } from '../../support/wishlist.outputvalidation'

describe('Graphql queries', () => {
  testSetup(false)
  it('addToList', () => {
    graphql(addToList(anonymousUser), response => {
      validateaddToListResponse
      expect(response.body.data.addToList).to.contain('1')
      cy.setWishListItem('id', response.body.data.addToList)
    })
  })

  it('Get Version', () => {
    graphql(version(), validateGetVersionResponse)
  })

  it('Get ViewList', () => {
    graphql(viewList(), validateGetViewListResponse)
  })

  it('Get All ViewList', () => {
    graphql(viewLists(), validateGetViewListsResponse)
  })

  it('Get checkList', () => {
    graphql(checkList(), validateGetcheckListResponse)
  })

  it('Get listNames', () => {
    graphql(listNames(), validateGetlistNamesResponse)
  })

  it('removeFromList', () => {
    cy.removeWishListItem().then(wishlistid => {
      graphql(removeFromList(wishlistid.id), validateRemoveFromListResponse)
    })
  })
})
