import { updateRetry } from './common/support'
import wishListSelectors from './selectors.js'

export function downloadWishlistFile() {
  it(
    'Open admin dashboard wishlist and download wishlist file',
    updateRetry(2),
    () => {
      cy.visit('admin/app/wishlist')
      cy.getVtexItems().then(vtex => {
        cy.intercept('GET', `${vtex.baseUrl}/_v/wishlist/export-lists`).as(
          'Export'
        )
        cy.get(wishListSelectors.WishlistDownloadButton)
          .should('be.visible')
          .click()
        cy.wait('@Export', { timeout: 40000 })
      })
    }
  )
}
