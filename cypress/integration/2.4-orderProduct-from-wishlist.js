import {
  testSetup,
  updateRetry,
  preserveCookie,
  promissoryPayment,
  buyProduct,
  saveOrderId,
} from '../support/common/support.js'
import { addToWishList, addToCart } from '../support/utils.js'
import wishlistProducts from '../support/wishlistProducts.js'
import { orderProduct } from '../support/outputvalidation.js'
import selectors from '../support/common/selectors.js'

describe('Testing Order Product from wishlist', () => {
  // Load test setup
  testSetup()

  const { prefix } = orderProduct

  it(`${prefix} - Add product to wish list`, updateRetry(2), () => {
    cy.searchProduct(wishlistProducts.irobot.name)
  })

  it(`${prefix} - Add product to cart & checkout`, updateRetry(2), () => {
    addToWishList(wishlistProducts.irobot.name, wishlistProducts.irobot.link)
    addToCart(wishlistProducts.irobot.link)
  })

  it(`${prefix} - Updating Shipping Information`, updateRetry(3), () => {
    // Update Shipping Section
    cy.get(selectors.CartTimeline).click({ force: true })
    cy.updateShippingInformation(orderProduct)
  })

  it(`${prefix} - Ordering the product`, updateRetry(1), () => {
    promissoryPayment()
    buyProduct()
    saveOrderId()
  })

  preserveCookie()
})
