import { VTEX_AUTH_HEADER } from './common/constants'
import { updateRetry } from './common/support'
import {
  wishlistSchemaAPI,
  wishlistEmailAPI,
  updateWishlistAPI,
  deleteWishlistAPI,
  getAllWishlist,
} from './apis'

const PREFIX = 'REST API'

export function readWishListSchema() {
  it(`${PREFIX} - Read Wishlist Schema`, updateRetry(3), () => {
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

export function readwishlistByEmail(shopperId, validate = true) {
  it(
    `${PREFIX} - Read the wishlist data by using ${shopperId}`,
    updateRetry(3),
    () => {
      cy.addDelayBetweenRetries(2000)
      cy.getVtexItems().then(vtex => {
        cy.getAPI(wishlistEmailAPI(vtex.baseUrl, shopperId)).then(response => {
          expect(response.status).to.have.equal(200)
          if (validate) {
            expect(response.body).to.have.length(1)
          }

          cy.setWishListItem(shopperId, response.body)
        })
      })
    }
  )
}

function getAllWishListTestCase(ENV) {
  it(`${PREFIX} - Read all the wishlist data`, updateRetry(3), () => {
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

export function updateMasterdata(shopperId, newShopperId) {
  it(`${PREFIX} - Update shopperId from ${shopperId} to ${newShopperId} to the masterdata`, () => {
    cy.addDelayBetweenRetries(2000)
    cy.getWishListItem().then(wishListId => {
      const [data] = wishListId[shopperId]
      data.email = newShopperId

      cy.getVtexItems().then(vtex => {
        cy.addLogsForRestAPI({
          method: 'PATCH',
          url: updateWishlistAPI(vtex.baseUrl),
          headers: {
            ...VTEX_AUTH_HEADER(vtex.apiKey, vtex.apiToken),
          },
          body: data,
        }).then(response => {
          expect(response.status).to.have.equal(200)
          expect(response.body).to.have.property('DocumentId')
        })
      })
    })
  })
}

export function deleteWishlistdata(env) {
  it(
    `${PREFIX} - Delete the wishlist data from the masterdata`,
    updateRetry(3),
    () => {
      cy.addDelayBetweenRetries(2000)
      cy.getVtexItems().then(vtex => {
        cy.getWishListItem().then(wishListId => {
          const arr = wishListId[env]

          for (const { id } of arr) {
            cy.addLogsForRestAPI({
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

export function wipe() {
  const ALL_RECORDS = 'all'

  getAllWishListTestCase(ALL_RECORDS)
  deleteWishlistdata(ALL_RECORDS)
}
