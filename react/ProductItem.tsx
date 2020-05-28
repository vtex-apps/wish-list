/* eslint-disable no-console */
import React, { FC } from 'react'
import { Spinner } from 'vtex.styleguide'
import { useQuery } from 'react-apollo'
import productQuery from './queries/productById.gql'
import styles from './styles.css'
import { useCssHandles } from 'vtex.css-handles'

const ProductItem: FC<ProductItemInterface> = ({ productId, title }) => {
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

  const link = (url: string) => {
    return url.replace(/^(?:https?:\/\/)?(?:[^@\/\n]+@)?(?:www\.)?([^:\/?\n]+)/igm,'')
  }

  const CSS_HANDLES = ['productItemRow','linkThumb', 'columnThumb', 'thumb', 'linkText', 'columnText', 'productTitle', 'productDescription'] as const
  const handles = useCssHandles(CSS_HANDLES)

  return (
    <div className={`product-item flex-row cl ${handles.productItemRow}`}>
      <div className={`flex-column w-10 fl ${handles.columnThumb}`}>
        {!loading && data && (
          <a href={link(data.productsByIdentifier[0].link)} className={`${styles.clearLink} ${handles.linkThumb}`}>
            <img
              src={thumb(data.productsByIdentifier[0].items[0].images[0].imageUrl)}
              className={handles.thumb}
            />
          </a>
        )}
        {loading && <Spinner />}
      </div>
      <div className={`flex-column w-90 fl ${handles.columnText}`}>
        {loading && <h3 className={`ma0 ph6 ${handles.productTitle}`}>{title}</h3>}
        {!loading && data && (
          <a href={link(data.productsByIdentifier[0].link)} className={`${styles.clearLink} ${handles.linkText}`}>
            <h3 className={`ma0 ph6 ${handles.productTitle}`}>{data.productsByIdentifier[0].productName}</h3>
            <p className={`ph6 pb6 ${handles.productDescription}`}>
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
