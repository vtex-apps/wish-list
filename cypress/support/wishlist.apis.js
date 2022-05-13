import { VTEX_AUTH_HEADER, FAIL_ON_STATUS_CODE } from './common/constants'
import { updateRetry } from './common/support'
import {
  wishlistSchemaAPI,
  wishlistDataAPI,
  wishlistEmailAPI,
  updateWishlistAPI,
  deleteWishlistAPI,
} from '../support/product.apis'

export function readWishListSchema() {
  it(`Read Wishlist Schema`, updateRetry(3), () => {
    cy.addDelayBetweenRetries(2000)
    cy.getVtexItems().then(vtex => {
      cy.getAPI(wishlistSchemaAPI(vtex.baseUrl)).then(response => {
        expect(response.status).to.equal(200)
        expect(response.body).to.have.property('properties')
        expect(response.body).to.have.property('name')
      })
    })
  })
}

export function readWishListdata(dataEnv) {
  it('Read all wishlist data', updateRetry(3), () => {
    cy.addDelayBetweenRetries(2000)
    cy.getVtexItems().then(vtex => {
      cy.getAPI(wishlistDataAPI(vtex.baseUrl)).then(response => {
        expect(response.status).to.have.equal(200)
        expect(response.body).to.have.property('wishLists')
        cy.setWishListItem(dataEnv, response.body.wishLists)
      })
    })
  })
}
export function readwishlistByEmail(data) {
  it('read the wishlist data by using email', updateRetry(3), () => {
    cy.addDelayBetweenRetries(2000)
    cy.getVtexItems().then(vtex => {
      const email = data.email
      cy.getAPI(wishlistEmailAPI(vtex.baseUrl, email)).then(response => {
        expect(response.status).to.have.equal(200)
        expect(response.body).to.have.length(1)
      })
    })
  })
}

export function updateMasterdata(data, env) {
  it('update to the masterdata', updateRetry(3), () => {
    cy.addDelayBetweenRetries(2000)
    cy.getVtexItems().then(vtex => {
      cy.request({
        method: 'PATCH',
        url: updateWishlistAPI(vtex.baseUrl),

        headers: {
          ...VTEX_AUTH_HEADER(vtex.apiKey, vtex.apiToken),
        },
        body: {
          data,
        },

        ...FAIL_ON_STATUS_CODE,
      }).then(response => {
        expect(response.status).to.have.equal(201)
        expect(response.body).to.have.property('DocumentId')
        cy.setWishListItem(env, response.body.DocumentId)
      })
    })
  })
}

export function deleteWishlistdata() {
  it('delete the wishlist data from the masterdata', updateRetry(3), () => {
    cy.addDelayBetweenRetries(2000)
    cy.getVtexItems().then(vtex => {
      cy.getWishListItem().then(wishListId => {
        const documentId = wishListId.wishlistdata
        cy.request({
          method: 'DELETE',
          url: deleteWishlistAPI(vtex.baseUrl, documentId),
          headers: {
            ...VTEX_AUTH_HEADER(vtex.apiKey, vtex.apiToken),
          },
        }).then(response => {
          expect(response.status).to.equal(204)
          expect(response.body).to.have.oneOf([null, ''])
        })
      })
    })
  })
}
