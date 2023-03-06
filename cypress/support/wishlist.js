import { updateRetry } from './common/support'
import { MESSAGES } from './utils.js'

export function downloadWishlistFile(prefix) {
  it(
    `In ${prefix} - Open admin dashboard wishlist and download wishlist file`,
    updateRetry(2),
    () => {
      cy.visit('admin/app/wishlist')
      cy.getVtexItems().then(vtex => {
        cy.qe(`Adding intercept for ExportList`)
        cy.intercept('POST', `${vtex.baseUrl}/**`, req => {
          if (req.body.operationName === 'ExportList') {
            req.continue()
          }
        }).as('ExportList')
        cy.qe(`Visiting admin/app/wishlist page`)
        cy.visit('admin/app/wishlist')
        cy.wait('@ExportList', { timeout: 40000 }).wait('@ExportList', {
          timeout: 40000,
        })
        cy.qe(
          `Verifying the ${MESSAGES.DownloadWishList} should be visible and enabled`
        )
        cy.get('div')
          .contains(MESSAGES.DownloadWishList)
          .should('be.visible')
          .click()
        cy.wait('@Export', { timeout: 40000 })
      })
    }
  )
}
