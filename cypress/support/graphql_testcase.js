export function version() {
  return {
    query: 'query' + '{version}',
    queryVariables: {},
  }
}
export function viewList({ shopperId, name }) {
  return {
    query:
      'query' +
      '($shopperId: String!,$name: String!)' +
      '{  viewList(shopperId:$shopperId,name:$name) {public,name,range {total,from,to},data {id,productId,sku,title}}}',
    queryVariables: {
      shopperId,
      name,
    },
  }
}

export function viewLists() {
  return {
    query:
      'query' +
      '($shopperId: String!)' +
      '{  viewLists(shopperId:$shopperId) {public,name,range {total,from,to},data {id,productId,sku,title}}}',
    queryVariables: {
      shopperId: 'saravananvenkatesan@bitcot.com',
    },
  }
}

export function checkList() {
  return {
    query:
      'query' +
      '($shopperId: String!,$productId:String!)' +
      '{  checkList(shopperId:$shopperId,productId:$productId) {inList,listNames,listIds,message}}',
    queryVariables: {
      shopperId: 'saravananvenkatesan@bitcot.com',
      productId: '880300',
    },
  }
}

export function listNames() {
  return {
    query:
      'query' + '($shopperId: String!)' + '{  listNames(shopperId:$shopperId)}',
    queryVariables: {
      shopperId: 'saravananvenkatesan@bitcot.com',
    },
  }
}

export function addToList(productId) {
  const query =
    'mutation' +
    '($shopperId: String!, $listItem: ListItemInputType!, $name: String!)' +
    '{addToList(shopperId: $shopperId, listItem: $listItem, name: $name)}'

  return {
    query,
    queryVariables: productId,
  }
}

export function removeFromList(productId, { shopperId, name }) {
  const query =
    'mutation' +
    '($shopperId: String!,$id: ID!,$name: String)' +
    '{removeFromList(shopperId: $shopperId,id: $id,name: $name)}'

  return {
    query,
    queryVariables: {
      id: productId,
      shopperId,
      name,
    },
  }
}

export function validateGetVersionResponse(response) {
  expect(response.body.data).to.not.equal(null)
}

export function validateGetViewListResponse(response) {
  expect(response.body.data.viewList.data)
    .to.be.an('array')
    .and.to.have.lengthOf.above(0)
}

export function validateGetViewListsResponse(response) {
  expect(response.body.data.viewLists)
    .to.be.an('array')
    .and.to.have.lengthOf.above(0)
}

export function validateGetcheckListResponse(response) {
  expect(response.body.data).to.not.equal(null)
}

export function validateaddToListResponse(response) {
  expect(response.body.data).to.not.equal(null)
}

export function validateGetlistNamesResponse(response) {
  expect(response.body.data.listNames)
    .to.be.an('array')
    .and.to.have.lengthOf.above(0)
}

export function validateRemoveFromListResponse(response) {
  expect(response.body.data.removeFromList).to.equal(true)
}
