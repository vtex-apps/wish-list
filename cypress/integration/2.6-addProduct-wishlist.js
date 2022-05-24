import {
  testSetup,
  updateRetry,
  preserveCookie,
  promissoryPayment,
  buyProduct,
} from '../support/common/support.js'
import { addToWishList, addToCart } from '../support/utils.js'
import wishlistProducts from '../support/wishlistProducts.js'
import { orderProduct } from '../support/outputvalidation.js'
import selectors from '../support/common/selectors.js'

describe('Testing Single Product and total amounts', () => {
  // Load test setup
  testSetup()

  const { prefix } = orderProduct

  it(`In ${prefix} - Add product to wish list`, updateRetry(3), () => {
    cy.searchProduct(wishlistProducts.irobot.name)
  })

  it(`In ${prefix} - Add product to cart & checkout`, updateRetry(2), () => {
    addToWishList(wishlistProducts.irobot.name, wishlistProducts.irobot.link)
    addToCart(wishlistProducts.irobot.link)
  })

  it(`In ${prefix} - Updating Shipping Information`, updateRetry(3), () => {
    // Update Shipping Section
    cy.get(selectors.CartTimeline).click({ force: true })
    cy.updateShippingInformation(orderProduct)
  })

  it(`In ${prefix} - ordered the product`, updateRetry(2), () => {
    promissoryPayment()
    buyProduct()
  })

  preserveCookie()
})
