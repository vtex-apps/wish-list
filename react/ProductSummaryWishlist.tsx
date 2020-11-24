import React, { useMemo, useState, useEffect } from 'react'
import { useQuery } from 'react-apollo'
import { ExtensionPoint, useTreePath, useRuntime } from 'vtex.render-runtime'
import { useListContext, ListContextProvider } from 'vtex.list-context'
import { ProductListContext } from 'vtex.product-list-context'
import { Spinner } from 'vtex.styleguide'

import { mapCatalogProductToProductSummary } from './utils/normalize'
import ProductListEventCaller from './components/ProductListEventCaller'
import productsQuery from './queries/productById.gql'
import ViewLists from './queries/viewLists.gql'
import { getSession } from './modules/session'
import storageFactory from './utils/storage'

const localStore = storageFactory(() => localStorage)

let isAuthenticated =
  JSON.parse(String(localStore.getItem('wishlist_isAuthenticated'))) ?? false
let shopperId = localStore.getItem('wishlist_shopperId') ?? null

const useSessionResponse = () => {
  const [session, setSession] = useState()
  const sessionPromise = getSession()

  useEffect(() => {
    if (!sessionPromise) {
      return
    }

    sessionPromise.then(sessionResponse => {
      const { response } = sessionResponse

      setSession(response)
    })
  }, [sessionPromise])

  return session
}

const ProductSummaryList = ({ children }) => {
  const { list } = useListContext() || []
  const { treePath } = useTreePath()
  const { navigate, history } = useRuntime()

  const sessionResponse: any = useSessionResponse()

  const { data: dataLists } = useQuery(ViewLists, {
    ssr: false,
    skip: !isAuthenticated,
    fetchPolicy: 'no-cache',
    variables: {
      shopperId,
    },
  })

  const { data, loading, error } = useQuery(productsQuery, {
    ssr: false,
    skip: !dataLists || !dataLists.viewLists,
    variables: {
      ids: dataLists?.viewLists[0].data.map(item => {
        return item.productId
      }),
    },
  })

  if (sessionResponse) {
    isAuthenticated =
      sessionResponse?.namespaces?.profile?.isAuthenticated?.value === 'true'
    shopperId = sessionResponse?.namespaces?.profile?.email?.value ?? null

    localStore.setItem(
      'wishlist_isAuthenticated',
      JSON.stringify(isAuthenticated)
    )
    localStore.setItem('wishlist_shopperId', String(shopperId))
  }

  const { productsByIdentifier: products } = data || {}

  const newListContextValue = useMemo(() => {
    const getWishlistId = (productId: string) => {
      return dataLists?.viewLists[0].data.find(item => {
        return item.productId === productId
      })?.id
    }
    const componentList = products?.map(product => {
      const normalizedProduct = mapCatalogProductToProductSummary(
        product,
        getWishlistId(product.productId)
      )
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
  }, [products, treePath, list, dataLists])

  if (sessionResponse && !isAuthenticated) {
    navigate({
      page: 'store.login',
      query: `returnUrl=${encodeURIComponent(history?.location?.pathname)}`,
    })
  }

  if (loading) {
    return <Spinner />
  }

  if (!data || error) {
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
    <ProductListProvider listName="wishlist">
      <ProductSummaryList>{children}</ProductSummaryList>
      <ProductListEventCaller />
    </ProductListProvider>
  )
}

export default EnhancedProductList
