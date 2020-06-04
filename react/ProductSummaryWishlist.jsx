import React, { useMemo, useEffect } from 'react'
import { ExtensionPoint, useTreePath } from 'vtex.render-runtime'
import { useListContext, ListContextProvider } from 'vtex.list-context'
import { ProductListContext } from 'vtex.product-list-context'
import { mapCatalogProductToProductSummary } from './utils/normalize'
import ProductListEventCaller from './components/ProductListEventCaller'
import { useQuery, useLazyQuery } from 'react-apollo'
import productsQuery from './queries/productById.gql'
import userProfile from './queries/userProfile.gql'
import ViewLists from './queries/viewLists.gql'

const ProductSummaryList = ({ children }) => {
  console.log('ProductSummaryList')

  const { loading: loadingProfile, data: profileData } = useQuery(userProfile, {
    ssr: false,
  })
  const [qWishlist, { data: dataLists, loading: loadingLists }] = useLazyQuery(
    ViewLists
  )
  const [qProducts, { data, loading, error }] = useLazyQuery(productsQuery)

  console.log('loadingProfile', loadingProfile, profileData)

  if (
    loadingProfile ||
    !profileData ||
    !profileData.getSession ||
    !profileData.getSession.profile
  ) {
    return null
  }

  const email = profileData.getSession.profile.email
  console.log('qWishlist', {
    variables: {
      shopperId: email,
    },
  })

  qWishlist({
    variables: {
      shopperId: email,
    },
  })

  console.log('loadingLists', loadingLists, dataLists)

  if (loadingLists || !dataLists) {
    return null
  }
  console.log('viewLists', dataLists.viewLists)
  const ids = dataLists.viewLists[0].data.map(item => {
    return item.productId
  })

  qProducts({
    variables: {
      ids,
    },
  })

  if (loading || error) {
    return null
  }
  // console.log('viewLists', dataLists.viewLists)
  // const ids = dataLists.viewLists[0].data.map((item) => {
  //   return item.productId
  // })
  // const ids = ["1","6"]

  // const { data, loading, error } = useQuery(productsQuery, {
  //   variables: {
  //     ids,
  //   },
  //     ssr: false
  // })
  console.log('Query result =>', data)
  const { list } = useListContext() || []
  const { treePath } = useTreePath()

  const { productsByIdentifier: products } = data || {}

  const newListContextValue = useMemo(() => {
    const componentList =
      products &&
      products.map(product => {
        const normalizedProduct = mapCatalogProductToProductSummary(product)
        console.log('Product =>', product)
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

  if (loading || error) {
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
  console.log('EnhancedProductList')
  return (
    <ProductListProvider>
      <ProductSummaryList>{children}</ProductSummaryList>
      <ProductListEventCaller />
    </ProductListProvider>
  )
}

export default EnhancedProductList
