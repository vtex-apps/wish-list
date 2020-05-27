/* eslint-disable no-console */
import { Spinner } from 'vtex.styleguide'
import React, { FC, useState } from 'react'
import { injectIntl, WrappedComponentProps } from 'react-intl'
import { compose, graphql, useApolloClient } from 'react-apollo'
import userProfile from './queries/userProfile.gql'
import viewLists from './queries/viewLists.gql'
import ProductItem from './ProductItem'

let initialLoad = false

const Wishlist: FC<WrappedComponentProps & any> = ({
  data: { called, loading, getSession },
}) => {
  const [state, setState] = useState<any>({
    wishLists: null,
    listsLoading: false,
  })
  const { wishLists, listsLoading } = state
  const client = useApolloClient()
  console.log('Called', called)
  console.log('loading', loading)
  console.log('getSession', getSession)
  // console.log('SESSION =>', getSession)

  const loadProductsList = async variables => {
    console.log('loadProductsList', variables)
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
      console.log('wishLists => ', data.viewLists)
      setState({
        ...state,
        listsLoading: false,
        wishLists: data.viewLists,
      })
    }
  }

  if (getSession?.profile && !initialLoad) {
    initialLoad = true
    loadProductsList({
      shopperId: getSession.profile.email,
    })
  }

  return (
    <div className="h4 w-80 center pt5 mt5">
      {loading ||
        (listsLoading && (
          <div>
            <Spinner />
          </div>
        ))}
      {!loading && !listsLoading && wishLists && (
        <div>
          {wishLists.map((element: any, tabIndex: number) => {
            console.log('Tab index', tabIndex)
            return (
              <div key={`tab_${tabIndex}`} className="flex-row">
                <h2>{element.name}</h2>
                <div className="pv5">
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
