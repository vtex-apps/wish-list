import { PRODUCTS } from './common/utils.js'
import { EMAIL_IDS } from './constants.js'

export default {
  restAndGraphqlAPI: {
    newShopperId: EMAIL_IDS[1],
    payload: {
      listItem: {
        productId: '880300',
        sku: '880030',
        title: PRODUCTS.coconut,
      },
      shopperId: EMAIL_IDS[0],
      name: 'Wishlist',
    },
  },
  orderProduct: {
    prefix: '2.4',
    postalCode: '33180',
  },
}
