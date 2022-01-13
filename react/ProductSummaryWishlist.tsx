/* eslint-disable no-console */
import React, { useMemo, useState, useEffect, FC } from 'react'
import { useLazyQuery } from 'react-apollo'
// @ts-expect-error - useTreePath is a private API
import { ExtensionPoint, useRuntime, useTreePath } from 'vtex.render-runtime'
import { useListContext, ListContextProvider } from 'vtex.list-context'
import { ProductListContext } from 'vtex.product-list-context'
import { Spinner } from 'vtex.styleguide'
import { usePixel } from 'vtex.pixel-manager'
import { FormattedMessage } from 'react-intl'

import { mapCatalogProductToProductSummary } from './utils/normalize'
import ProductListEventCaller from './components/ProductListEventCaller'
import productsQuery from './queries/productById.gql'
import ViewLists from './queries/viewLists.gql'
import { getSession } from './modules/session'
import storageFactory from './utils/storage'

const localStore = storageFactory(() => sessionStorage)

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

interface ProductSummaryProps {
  children?: any
  showViewEmptyList?: boolean
}

const ProductSummaryList: FC<ProductSummaryProps> = ({
  children,
  showViewEmptyList = false,
}) => {
  const { list } = useListContext() || []
  const { treePath } = useTreePath()
  const { navigate, history } = useRuntime()
  const { push } = usePixel()

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
      fetchPolicy: 'network-only',
    }
  )

  console.log('loadProducts error =>', error)

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
  let productList = [] as any
  productList = dataLists?.viewLists[0]?.data.map((item: any) => {
    const [id] = item.productId.split('-')
    return {
      productId: id,
      sku: item.sku,
    }
  })

  if (!called && dataLists && productList) {
    const ids = productList.map((item: any) => item.productId)
    localStore.setItem('wishlist_wishlisted', JSON.stringify(productList))
    loadProducts({
      variables: {
        ids,
      },
    })
  }

  const { productsByIdentifier: products } = data || {}

  const newListContextValue = useMemo(() => {
    const getWishlistId = (productId: string) => {
      if (productId) {
        const [id] = productId.split('-')
        return dataLists?.viewLists[0]?.data.find((item: any) => {
          const [itemId] = item.productId.split('-')
          return itemId === id
        })?.id
      }
    }
    let newProductList = []
    if (productList) {
      newProductList = productList
        .map((_product: any) => {
          if (products) {
            const product = products.find(
              (item: any) =>
                item.productId === _product.productId &&
                item.items &&
                item.items.find((sku: any) => sku.itemId === _product.sku)
            )
            const _sku = product?.items.find(
              (sku: any) => sku.itemId === _product.sku
            )
            return { ...product, sku: _sku }
          }
          return undefined
        })
        .filter((item: any) => item !== undefined)
      // console.log('>>', newProductList)
    }

    const componentList = newProductList
      ?.filter((item: any) => item.sku && item.productId)
      ?.map((product: any, index: number) => {
        const sku = dataLists?.viewLists[0]?.data[index]?.sku
        const { items } = product
        const position = index + 1

        const normalizedProduct = mapCatalogProductToProductSummary(
          product,
          getWishlistId(product.productId)
        )

        if (sku && items && items.length) {
          for (const item of items) {
            if (item.itemId === sku) {
              normalizedProduct.sku.image = item.images[0]
            }
          }
        }

        const handleOnClick = () => {
          push({
            event: 'productClick',
            list: 'wishlist',
            product: normalizedProduct,
            position,
          })
        }

        return (
          <ExtensionPoint
            id="product-summary"
            key={product.id}
            treePath={treePath}
            product={normalizedProduct}
            actionOnClick={handleOnClick}
          />
        )
      })
    return list.concat(componentList)
  }, [products, treePath, list, dataLists, productList, push])

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
    if (error?.message?.includes('products') && showViewEmptyList) {
      return <ExtensionPoint id="wishlist-empty-list" />
    }
    return null
  }

  if (listCalled && !listLoading && !dataLists?.viewLists[0]?.data?.length) {
    if (showViewEmptyList) {
      return <ExtensionPoint id="wishlist-empty-list" />
    }
    return <FormattedMessage id="store/myaccount-empty-list" />
  }

  return (
    <ListContextProvider list={newListContextValue}>
      {children}
    </ListContextProvider>
  )
}

const EnhancedProductList: FC<ProductSummaryProps> = props => {
  const { children, showViewEmptyList } = props
  const { ProductListProvider } = ProductListContext
  return (
    <ProductListProvider listName="wishlist">
      <ProductSummaryList showViewEmptyList={showViewEmptyList}>
        {children}
      </ProductSummaryList>
      <ProductListEventCaller />
    </ProductListProvider>
  )
}

export default EnhancedProductList
