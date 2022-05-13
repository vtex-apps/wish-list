export default {
  wishlistSchemaAPI: baseUrl => {
    return `${baseUrl}/api/dataentities/wishlist/schemas/wishlist`
  },
  wishlistDataAPI: baseUrl => {
    return `${baseUrl}/_v/wishlist/export-lists`
  },
  wishlistEmailAPI: (baseUrl, email) => {
    return `${baseUrl}/api/dataentities/wishlist/search?_fields=_all&_schema=wishlist&email=${email}`
  },
  updateWishlistAPI: baseUrl => {
    return `${baseUrl}/api/dataentities/wishlist/documents`
  },
  deleteWishlistAPI: (baseUrl, documentId) => {
    return `${baseUrl}/api/dataentities/wishlist/documents/${documentId}`
  },
}
