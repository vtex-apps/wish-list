import React, { FC } from 'react'
import { Route } from 'vtex.my-account-commons/Router'
import { ContentWrapper } from 'vtex.my-account-commons'

type MyAccountWishlistPageProps = {
  children: FC
  title?: string
  namespace?: string
}
const MyAccountWishlistPage = ({
  children,
  title,
  namespace = 'wish-list',
}: MyAccountWishlistPageProps) => {
  return (
    <Route
      exact
      path="/wishlist"
      render={() => {
        if (!title || title.trim().length === 0) {
          return children
        }
        return (
          <ContentWrapper title={title} namespace={namespace}>
            {() => children}
          </ContentWrapper>
        )
      }}
    />
  )
}

export default MyAccountWishlistPage
