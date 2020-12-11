import React from 'react'
import { Route } from 'vtex.my-account-commons/Router'

function MyAccountWishlistPage({ children }) {
  return (
    <Route exact path="/wishlist" render={() => <>{children}</>}/>
  )
}

export default MyAccountWishlistPage