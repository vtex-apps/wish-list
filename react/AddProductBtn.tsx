/* eslint-disable no-console */
import React, { FC, useState, useContext } from 'react'
import { ProductContext } from 'vtex.product-context'
import { Button } from 'vtex.styleguide'
import { compose, graphql } from 'react-apollo'
import { injectIntl } from 'react-intl'
import userProfile from './queries/userProfile.gql'
import styles from './styles.css'

let isAuthenticated = false
const productCheck = {}

const AddBtn: FC<any> = ({ data: { getSession } }: any) => {
  const [state, setState] = useState<any>({
    isLoading: false,
    isWishlisted: false,
  })

  const { isLoading, isWishlisted } = state

  if (!!getSession?.profile && !isAuthenticated) {
    isAuthenticated = true
  }

  const { product } = useContext(ProductContext) as any

  if(isAuthenticated && product && !productCheck[product.productId]) {
    console.log(`Check if user has the product ${product.productId} wishlisted`)
    productCheck[product.productId] = product
  }

  console.log('Hello Add to Wishlist Button!', state)
  const handleAddProductClick = (e) => {
    e.preventDefault()
    e.stopPropagation()

    console.log('Clicked')
    setState({
      ...state,
      isLoading: true,
    })
    setTimeout(() => {
      setState({
        ...state,
        isLoading: false,
        isWishlisted: !isWishlisted
      })
    }, 3000)
  }
  return (
    <div>
      <Button
        variation="tertiary"
        onClick={handleAddProductClick}
        isLoading={isLoading}
        >
          <span className={`${isWishlisted?styles.fill:styles.outline} ${styles.iconSize}`}></span>
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
