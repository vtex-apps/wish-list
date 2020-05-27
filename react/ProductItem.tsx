/* eslint-disable no-console */
import React, { FC } from 'react'
import { Spinner } from 'vtex.styleguide'
import { useQuery } from 'react-apollo'
import productQuery from './queries/productById.gql'
import styles from './styles.css'

const ProductItem: FC<ProductItemInterface> = ({ productId, title }) => {
  console.log('productId', productId)
  const { loading, error, data } = useQuery(productQuery, {
    variables: {
      ids: [productId],
    },
  })
  if (error) {
    console.error(error)
  }

  const thumb = (url: string) => {
    let arrUrl = url.split('/')
        arrUrl.pop()
    const newUrl = arrUrl.join('/')+'-auto-150'
    return newUrl
  }

  return (
    <div className="product-item flex-row cl">
      <div className="flex-column w-10 fl">
        {!loading && data && (
          <a href={data.productsByIdentifier[0].link} className={styles.clearLink}>
            <img
              src={thumb(data.productsByIdentifier[0].items[0].images[0].imageUrl)}
            />
          </a>
        )}
        {loading && <Spinner />}
      </div>
      <div className="flex-column w-90 fl">
        {loading && <h3 className="ma0 ph6">{title}</h3>}
        {!loading && data && (
          <a href={data.productsByIdentifier[0].link} className={styles.clearLink}>
            <h3 className="ma0 ph6">{data.productsByIdentifier[0].productName}</h3>
            <p className="ph6 pb6">
              {data.productsByIdentifier[0].description}
            </p>
          </a>
        )}
      </div>
    </div>
  )
}

interface ProductItemInterface {
  productId: string
  title: string
}

export default ProductItem
