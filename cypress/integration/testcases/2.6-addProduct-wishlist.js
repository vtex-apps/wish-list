import {
    testSetup,
    updateRetry,
    preserveCookie,
    promissoryPayment,
    buyProduct
} from '../../support/common/support'
import {
    addToWishList,
    addToCart,
    fillInformation
} from '../../support/common.support'

import wishlistProducts from '../../support/wishlistProducts'

describe('Testing Single Product and total amounts', () => {
    // Load test setup
    testSetup()
    it('Add product to wish list', updateRetry(3), () => {
        cy.searchProduct(wishlistProducts.irobot.name)
        addToWishList(wishlistProducts.irobot.name)
    })

    it('Add product to cart & checkout', updateRetry(2), () => {
        addToCart(wishlistProducts.irobot.link)
    })

    it('filling shipping info', updateRetry(2), () => {
        fillInformation()
    })

    it('ordered the product', updateRetry(2), () => {
        promissoryPayment()
        buyProduct()
    })

    preserveCookie()
})