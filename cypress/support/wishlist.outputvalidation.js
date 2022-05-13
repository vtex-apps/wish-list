import { PRODUCTS } from './common/utils.js'

const TESTCASE = {
  testCase1: 'testCase1',
}

const ANONYMOUS_USER = {
  a1: 'a1',
  a2: 'a2',
}

export default {
  anonymousUser: {
    listItem: {
      productId: '880300',
      sku: 'dehydrate',
      title: 'FruitNut',
    },
    shopperId: 'saravananvenkatesan@bitcot.com',
    name: PRODUCTS.coconut,
  },
  testCase1: {
    title: 'Enable Anonymous review with no admin approval',
    product: PRODUCTS.onion,
    productId: 880030,
    configuration: {
      allowAnonymousReviews: true,
      requireApproval: false,
      defaultStarsRating: '3',
    },
    anonymousUser1: {
      // name: `shashi-${TESTCASE.testCase1}-${ANONYMOUS_USER.a1}`,
      line: 'Test',
      location: 'California', // optional
      email: 'sumanrajravi@bitcot.com',
      name: 'sumanraj',
      isPublic: true,
      average: 3,
    },
    anonymousUser2: {
      name: `shashi-${TESTCASE.testCase1}-${ANONYMOUS_USER.a2}`,
      line: 'Test',
      location: null,
      email: 'shashi@bitcot.com',
      rating: 5,
      review: 'Excellent Product',
      average: 4,
    },
    data: {
      name: 'sumanraj',
      email: 'sumanrajravi@bitcot.com',
      IsPublic: true,
    },
  },
}
