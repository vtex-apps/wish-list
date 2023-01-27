import { updateRetry } from './common/support'
import wishListSelectors from './selectors.js'

export function downloadWishlistFile(prefix) {
  it(
    `In ${prefix} - Open admin dashboard wishlist and download wishlist file`,
    updateRetry(2),
    () => {
      cy.visit('admin/app/wishlist')
      cy.contains('Wishlist').should('be.visible')
      cy.getVtexItems().then(vtex => {
        cy.intercept('POST', `${vtex.baseUrl}/**`, req => {
          if (req.body.operationName === 'ExportList') {
            req.continue()
          }
        }).as('ExportList')

        cy.get(wishListSelectors.WishlistDownloadButton)
          .should('be.visible')
          .click()
        cy.wait('@ExportList', { timeout: 40000 })
      })
    }
  )
}
