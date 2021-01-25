import React, { FC } from 'react'
import { Route } from 'vtex.my-account-commons/Router'

const MyAccountWishlistPage: FC = ({ children }) => {
  return <Route exact path="/wishlist" render={() => <>{children}</>} />
}

export default MyAccountWishlistPage
