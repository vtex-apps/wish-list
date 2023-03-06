import { updateRetry } from './common/support'
import { MESSAGES } from './utils.js'

export function downloadWishlistFile(prefix) {
  it(
    `In ${prefix} - Open admin dashboard wishlist and download wishlist file`,
    updateRetry(2),
    () => {
      cy.getVtexItems().then(vtex => {
        cy.intercept('POST', `${vtex.baseUrl}/**`, req => {
          if (req.body.operationName === 'ExportList') {
            req.continue()
          }
        }).as('ExportList')
        cy.visit('admin/app/wishlist')
        cy.wait('@ExportList', { timeout: 40000 }).wait('@ExportList', {
          timeout: 40000,
        })
        cy.get('div')
          .contains(MESSAGES.DownloadWishList)
          .should('be.visible')
          .should('be.enabled')
          .click()
      })
    }
  )
}
