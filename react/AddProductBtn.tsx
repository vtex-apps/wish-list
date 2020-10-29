/* eslint-disable no-console */
import React, { FC, useState, useContext, useEffect } from 'react'
import { ProductContext } from 'vtex.product-context'
import { Button, ToastContext } from 'vtex.styleguide'
import { compose, graphql, useApolloClient, useMutation } from 'react-apollo'
import { injectIntl, WrappedComponentProps, defineMessages } from 'react-intl'
import { useRuntime } from 'vtex.render-runtime'
import { useCssHandles } from 'vtex.css-handles'

import userProfile from './queries/userProfile.gql'
import checkItem from './queries/checkItem.gql'
import addToList from './queries/addToList.gql'
import removeFromList from './queries/removeFromList.gql'
import styles from './styles.css'

const CSS_HANDLES = ['wishlistIconContainer', 'wishlistIcon'] as const

let isAuthenticated = false

const productCheck = {}
const defaultValues = {
  LIST_NAME: 'Wishlist',
}

const messages = defineMessages({
  addButton: {
    defaultMessage: '',
    id: 'store/wishlist.addButton',
  },
  seeLists: {
    defaultMessage: '',
    id: 'store/wishlist-see-lists',
  },
  productAddedToList: {
    defaultMessage: '',
    id: 'store/wishlist-product-added-to-list',
  },
  addProductFail: {
    defaultMessage: '',
    id: 'store/wishlist-add-product-fail',
  },
  listNameDefault: {
    defaultMessage: '',
    id: 'store/wishlist-default-list-name',
  },
  login: {
    defaultMessage: '',
    id: 'store/wishlist-login',
  },
  notLogged: {
    defaultMessage: '',
    id: 'store/wishlist-not-logged',
  },
})

const AddBtn: FC<any & WrappedComponentProps> = ({
  data: { loading: sessionLoading, getSession },
  intl,
}: any) => {
  const [state, setState] = useState<any>({
    isLoading: true,
    isWishlisted: false,
    wishListId: null,
    isChecked: false,
  })

  const { navigate, history } = useRuntime()
  const client = useApolloClient()
  const handles = useCssHandles(CSS_HANDLES)
  const { showToast } = useContext(ToastContext)
  const { product } = useContext(ProductContext) as any

  if (!product) return null

  const toastMessage = (messsageKey: string) => {
    let action: any

    if (messsageKey === 'notLogged') {
      action = {
        label: intl.formatMessage(messages.login),
        onClick: () =>
          navigate({
            page: 'store.login',
            query: `returnUrl=${encodeURIComponent(history.location.pathname)}`,
          }),
      }
    }
    if (messsageKey === 'productAddedToList') {
      action = {
        label: intl.formatMessage(messages.seeLists),
        onClick: () =>
          navigate({
            page: 'store.wishlist',
          }),
      }
    }

    showToast({
      message: intl.formatMessage(messages[messsageKey]),
      action,
    })
  }

  const { isLoading, isWishlisted, wishListId } = state

  if (getSession?.profile && !isAuthenticated) {
    isAuthenticated = getSession.profile.email
  }

  const getIdFromList = (list: string, item: any) => {
    const pos = item.listNames.findIndex((listName: string) => {
      return list === listName
    })
    return item.listIds[pos]
  }

  const handleCheck = async variables => {
    if (!state.isChecked) {
      const { data } = await client.query({
        query: checkItem,
        variables,
        fetchPolicy: 'no-cache',
      })
      if (data?.checkList?.inList) {
        setState({
          ...state,
          isWishlisted: data.checkList.inList,
          isLoading: false,
          isChecked: true,
          wishListId: getIdFromList(defaultValues.LIST_NAME, data.checkList),
        })
      } else {
        setState({
          ...state,
          isLoading: false,
          isChecked: true,
        })
      }
    }
  }

  useEffect(() => {
    if (
      !sessionLoading &&
      isAuthenticated &&
      product &&
      !productCheck[product.productId]
    ) {
      productCheck[product.productId] = product
      if (product) {
        handleCheck({
          shopperId: String(isAuthenticated),
          productId: String(product.productId),
        })
      }
    } else if (!isAuthenticated && state.isLoading) {
      setState({
        ...state,
        isLoading: false,
      })
    } else if (isAuthenticated && state.isLoading) {
      productCheck[product.productId] = product
      if (product) {
        handleCheck({
          shopperId: String(isAuthenticated),
          productId: String(product.productId),
        })
      }
    }
  })

  const [addProduct] = useMutation(addToList, {
    onCompleted: (res: any) => {
      setState({
        ...state,
        isLoading: false,
        isWishlisted: !!res.addToList,
        wishListId: res.addToList,
      })
      if (res.addToList) {
        toastMessage('productAddedToList')
      } else {
        toastMessage('addProductFail')
      }
    },
  })

  const [removeProduct] = useMutation(removeFromList, {
    onCompleted: (res: any) => {
      setState({
        ...state,
        isLoading: false,
        isWishlisted: !res.removeFromList,
        wishListId: res.removeFromList ? null : wishListId,
      })
    },
  })

  const handleAddProductClick = e => {
    e.preventDefault()
    e.stopPropagation()
    if (isAuthenticated) {
      setState({
        ...state,
        isLoading: true,
      })
      if (!isWishlisted) {
        addProduct({
          variables: {
            listItem: {
              productId: product.productId,
              title: product.productName,
            },
            shopperId: getSession.profile.email,
            name: defaultValues.LIST_NAME,
          },
        })
      } else {
        removeProduct({
          variables: {
            id: wishListId,
            shopperId: getSession.profile.email,
            name: defaultValues.LIST_NAME,
          },
        })
      }
    } else {
      toastMessage('notLogged')
    }
  }
  return (
    <div className={handles.wishlistIconContainer}>
      <Button
        variation="tertiary"
        onClick={handleAddProductClick}
        isLoading={sessionLoading || isLoading}
      >
        <span
          className={`${handles.wishlistIcon} ${
            isWishlisted ? styles.fill : styles.outline
          } ${styles.iconSize}`}
        />
      </Button>
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
  )(AddBtn)
)
