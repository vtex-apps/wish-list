/* eslint-disable no-console */
import { Spinner } from 'vtex.styleguide'
import React, { FC, useState } from 'react'
import { injectIntl, WrappedComponentProps } from 'react-intl'
import { compose, graphql, useApolloClient } from 'react-apollo'
import userProfile from './queries/userProfile.gql'
import viewLists from './queries/viewLists.gql'
import ProductItem from './ProductItem'
import { useCssHandles } from 'vtex.css-handles'
import { useRuntime } from 'vtex.render-runtime'

let initialLoad = false

const Wishlist: FC<WrappedComponentProps & any> = ({
  data: { loading, getSession },
}) => {
  const [state, setState] = useState<any>({
    wishLists: null,
    listsLoading: false,
  })
  const { wishLists, listsLoading } = state
  const client = useApolloClient()
  const { navigate, history } = useRuntime()

  const loadProductsList = async variables => {
    const { data, loading: listsLoading } = await client.query({
      query: viewLists,
      variables,
      fetchPolicy: 'no-cache',
    })

    if (listsLoading) {
      setState({
        ...state,
        listsLoading,
      })
    }

    if (data?.viewLists) {
      console.log('viewLists', data.viewLists)
      setState({
        ...state,
        listsLoading: false,
        wishLists: data.viewLists,
      })
    }
  }

  if(!loading && !getSession?.profile) {
    navigate({
      page: 'store.login',
      query: `returnUrl=${encodeURIComponent(history.location.pathname)}`,
    })
  }

  if (getSession?.profile && !initialLoad) {
    initialLoad = true
    loadProductsList({
      shopperId: getSession.profile.email,
    })
  }

  const CSS_HANDLES = ['wishlistContainer','listTab', 'listName', 'listItemsContainer'] as const
  const handles = useCssHandles(CSS_HANDLES)

  return (
    <div className={`h4 w-80 center pt5 mt5 ${handles.wishlistContainer}`}>
      {loading ||
        (listsLoading && (
          <div>
            <Spinner />
          </div>
        ))}
      {!loading && !listsLoading && wishLists && (
        <div>
          {wishLists.map((element: any, tabIndex: number) => {
            return (
              <div key={`tab_${tabIndex}`} className={`flex-row ${handles.listTab}`}>
                <h2 className={`${handles.listName}`}>{element.name}</h2>
                <div className={`pv5 ${handles.listItemsContainer}`}>
                  {element.data.map((product: any, itemIndex: number) => {
                    return (
                      <ProductItem
                        key={`item_${itemIndex}`}
                        productId={product.productId}
                        title={product.title}
                      />
                    )
                  })}
                </div>
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}

export default injectIntl(
  compose(
    graphql(userProfile, {
      options: {
        ssr: false,
      },
    })
  )(Wishlist)
)
