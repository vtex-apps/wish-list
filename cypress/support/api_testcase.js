import { VTEX_AUTH_HEADER, FAIL_ON_STATUS_CODE } from './common/constants'
import { updateRetry } from './common/support'
import {
  wishlistSchemaAPI,
  wishlistDataAPI,
  wishlistEmailAPI,
  updateWishlistAPI,
  deleteWishlistAPI,
  getAllWishlist,
} from './apis'

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

export function readWishListdata() {
  it('Read all wishlist data', updateRetry(3), () => {
    cy.addDelayBetweenRetries(2000)
    cy.getVtexItems().then(vtex => {
      cy.getAPI(wishlistDataAPI(vtex.baseUrl)).then(response => {
        expect(response.status).to.have.equal(200)
        expect(response.body).to.have.property('wishLists')
      })
    })
  })
}

export function readwishlistByEmail(email = '', validate = true) {
  it(`Read the wishlist data by using ${email}`, updateRetry(3), () => {
    cy.addDelayBetweenRetries(2000)
    cy.getVtexItems().then(vtex => {
      cy.getAPI(wishlistEmailAPI(vtex.baseUrl, email)).then(response => {
        expect(response.status).to.have.equal(200)
        if (validate) {
          expect(response.body).to.have.length(1)
        }

        cy.setWishListItem(email, response.body)
      })
    })
  })
}

export function getAllWishListTestCase(ENV) {
  it(`Read all the wishlist data by using`, updateRetry(3), () => {
    cy.addDelayBetweenRetries(2000)
    cy.getVtexItems().then(vtex => {
      cy.getAPI(getAllWishlist(vtex.baseUrl), {
        'REST-Range': 'resources=0-100',
      }).then(response => {
        expect(response.status).to.have.equal(200)
        cy.setWishListItem(ENV, response.body)
      })
    })
  })
}

export function updateMasterdata(data) {
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
      })
    })
  })
}

export function deleteWishlistdata(env) {
  it(
    `Delete the wishlist data for this email ${env} from the masterdata`,
    updateRetry(3),
    () => {
      cy.addDelayBetweenRetries(2000)
      cy.getVtexItems().then(vtex => {
        cy.getWishListItem().then(wishListId => {
          const arr = wishListId[env]

          for (const { id } of arr) {
            cy.request({
              method: 'DELETE',
              url: deleteWishlistAPI(vtex.baseUrl, id),
              headers: {
                ...VTEX_AUTH_HEADER(vtex.apiKey, vtex.apiToken),
              },
            }).then(response => {
              expect(response.status).to.equal(204)
              expect(response.body).to.have.oneOf([null, ''])
            })
          }
        })
      })
    }
  )
}
