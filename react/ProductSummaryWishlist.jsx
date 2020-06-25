import React, { useMemo } from 'react'
import { ExtensionPoint, useTreePath } from 'vtex.render-runtime'
import { useListContext, ListContextProvider } from 'vtex.list-context'
import { ProductListContext } from 'vtex.product-list-context'
import { mapCatalogProductToProductSummary } from './utils/normalize'
import ProductListEventCaller from './components/ProductListEventCaller'
import { useQuery } from 'react-apollo'
import productsQuery from './queries/productById.gql'
import userProfile from './queries/userProfile.gql'
import ViewLists from './queries/viewLists.gql'
import { useRuntime } from 'vtex.render-runtime'

const ProductSummaryList = ({ children }) => {
  const { list } = useListContext() || []
  const { treePath } = useTreePath()
  const { navigate, history } = useRuntime()

  const { data: profileData, loading: profileLoading } = useQuery(userProfile, {
    ssr: false,
  })

  const { data: dataLists } = useQuery(ViewLists, {
    ssr: false,
    skip: !profileData || !profileData.getSession,
    fetchPolicy: 'no-cache',
    variables: {
      shopperId: profileData && profileData.getSession && profileData.getSession.profile && profileData.getSession.profile.email,
    },
  })

  const { data, loading, error } = useQuery(productsQuery, {
    ssr: false,
    skip: !dataLists || !dataLists.viewLists,
    variables: {
      ids:
        dataLists &&
        dataLists.viewLists[0].data.map(item => {
          return item.productId
        }),
    },
  })
  
  const { productsByIdentifier: products } = data || {}

  const newListContextValue = useMemo(() => {
    const componentList =
      products &&
      products.map(product => {
        const normalizedProduct = mapCatalogProductToProductSummary(product)
        return (
          <ExtensionPoint
            id="product-summary"
            key={product.id}
            treePath={treePath}
            product={normalizedProduct}
          />
        )
      })
    return list.concat(componentList)
  }, [products, treePath, list])

  if( !profileLoading && (!profileData || !profileData.getSession.profile) ) {
    navigate({
      page: 'store.login',
      query: `returnUrl=${encodeURIComponent(history.location.pathname)}`,
    })
  }

  if (!data || loading || error) {
    return null
  }

  return (
    <ListContextProvider list={newListContextValue}>
      {children}
    </ListContextProvider>
  )
}

const EnhancedProductList = ({ children }) => {
  const { ProductListProvider } = ProductListContext
  return (
    <ProductListProvider>
      <ProductSummaryList>{children}</ProductSummaryList>
      <ProductListEventCaller />
    </ProductListProvider>
  )
}

export default EnhancedProductList
