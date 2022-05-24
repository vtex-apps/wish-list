import { PRODUCTS } from './common/utils.js'
import { EMAIL_IDS } from './constants.js'

export default {
  anonymousUser: {
    listItem: {
      productId: '880300',
      sku: 'dehydrate',
      title: 'FruitNut',
    },
    shopperId: EMAIL_IDS[0],
    name: PRODUCTS.coconut,
  },
  testCase1: {
    product: PRODUCTS.onion,
    productId: 880030,
    data: {
      name: 'sumanraj',
      emailId: EMAIL_IDS[0],
      IsPublic: true,
    },
    orderProduct: {
      prefix: 'orderProduct',
      postalCode: '33180',
    },
  },
}
