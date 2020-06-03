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


const ProductSummaryList = async ({ children }) => {

  console.log('ProductSummaryList');

  const { loading: loadingProfile, data: profileData } = await useQuery(userProfile)

  console.log('getSession', loadingProfile, profileData)
  const email = profileData.getSession.profile.email
  console.log('email', email)

  const { loading: loadingLists, data: dataLists } = await useQuery(ViewLists, {
    variables: {
      shopperId: email,
    },
  })

  console.log('loadingLists', loadingLists)
  console.log('viewLists', dataLists.viewLists)
  const ids = dataLists.viewLists[0].data.map((item) => {
    return item.productId
  })

  const { data, loading, error } = await useQuery(productsQuery, {
    variables: {
      ids,
    },
  })

  const { list } = useListContext() || []
  const { treePath } = useTreePath()

  const { products } = data || {}

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

  if (loading || error) {
    return null
  }

  return (
    <ListContextProvider list={newListContextValue}>
      {children}
    </ListContextProvider>
  )
}

const EnhancedProductList = ({ children, ...props }) => {
  const { ProductListProvider } = ProductListContext
console.log('EnhancedProductList =>', children)
  return (
    <ProductListProvider>
      <ProductSummaryList {...props}>{children}</ProductSummaryList>
      <ProductListEventCaller />
    </ProductListProvider>
  )
}

export default EnhancedProductList
