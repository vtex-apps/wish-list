import React, { useMemo, useState, useEffect, FC } from 'react'
import { useLazyQuery } from 'react-apollo'
import { FormattedMessage } from 'react-intl'
// @ts-expect-error - useTreePath is a private API
import { ExtensionPoint, useRuntime, useTreePath } from 'vtex.render-runtime'
import { useListContext, ListContextProvider } from 'vtex.list-context'
import { ProductListContext } from 'vtex.product-list-context'
import { Spinner } from 'vtex.styleguide'
import { useCssHandles } from 'vtex.css-handles'

import { mapCatalogProductToProductSummary } from './utils/normalize'
import ProductListEventCaller from './components/ProductListEventCaller'
import productsQuery from './queries/productById.gql'
import ViewLists from './queries/viewLists.gql'
import { getSession } from './modules/session'
import storageFactory from './utils/storage'

const localStore = storageFactory(() => localStorage)
const CSS_HANDLES = ['emptyMessage'] as const

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

const ProductSummaryList: FC = ({ children }) => {
  const { list } = useListContext() || []
  const { treePath } = useTreePath()
  const { navigate, history } = useRuntime()
  const handles = useCssHandles(CSS_HANDLES)

  const sessionResponse: any = useSessionResponse()

  const [
    loadLists,
    { data: dataLists, loading: listLoading, called: listCalled },
  ] = useLazyQuery(ViewLists, {
    ssr: false,
    fetchPolicy: 'network-only',
  })

  const [loadProducts, { data, loading, error, called }] = useLazyQuery(
    productsQuery,
    {
      ssr: false,
      variables: {
        ids: dataLists?.viewLists[0]?.data.map((item: any) => {
          return item.productId
        }),
      },
    }
  )

  if (sessionResponse) {
    isAuthenticated =
      sessionResponse?.namespaces?.profile?.isAuthenticated?.value === 'true'
    shopperId = sessionResponse?.namespaces?.profile?.email?.value ?? null

    localStore.setItem(
      'wishlist_isAuthenticated',
      JSON.stringify(isAuthenticated)
    )
    localStore.setItem('wishlist_shopperId', String(shopperId))
    if (!listCalled) {
      loadLists({
        variables: {
          shopperId,
        },
      })
    }
  }

  if (!called && dataLists) {
    loadProducts()
  }

  const { productsByIdentifier: products } = data || {}

  const newListContextValue = useMemo(() => {
    const getWishlistId = (productId: string) => {
      return dataLists?.viewLists[0]?.data.find((item: any) => {
        return item.productId === productId
      })?.id
    }
    const componentList = products?.map((product: any) => {
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

  if (!dataLists || !data || error) {
    return null
  }

  if (listCalled && !listLoading && !dataLists?.viewLists[0]?.data?.length) {
    return (
      <div className={`ml5 ${handles.emptyMessage}`}>
        <FormattedMessage id="store/myaccount-empty-list" />
      </div>
    )
  }

  return (
    <ListContextProvider list={newListContextValue}>
      {children}
    </ListContextProvider>
  )
}

const EnhancedProductList: FC = ({ children }) => {
  const { ProductListProvider } = ProductListContext
  return (
    <ProductListProvider listName="wishlist">
      <ProductSummaryList>{children}</ProductSummaryList>
      <ProductListEventCaller />
    </ProductListProvider>
  )
}

export default EnhancedProductList
