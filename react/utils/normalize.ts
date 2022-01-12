export const DEFAULT_WIDTH = 'auto'
export const DEFAULT_HEIGHT = 'auto'
export const MAX_WIDTH = 3000
export const MAX_HEIGHT = 4000

/**
 * Having the url below as base for the LEGACY file manager,
 * https://storecomponents.vteximg.com.br/arquivos/ids/155472/Frame-3.jpg?v=636793763985400000
 * the following regex will match https://storecomponents.vteximg.com.br/arquivos/ids/155472
 *
 * Also matches urls with defined sizes like:
 * https://storecomponents.vteximg.com.br/arquivos/ids/155473-160-auto
 * @type {RegExp}
 *
 * On the new vtex.file-manager isn't necessary replace the URL, just add the param on the querystring, like:
 * "?width=WIDTH&height=HEIGHT&aspect=true"
 *
 */
const baseUrlRegex = new RegExp(/.+ids\/(\d+)/)

const httpRegex = new RegExp(/http:\/\//)

function toHttps(url: string) {
  return url.replace(httpRegex, 'https://')
}

function cleanImageUrl(imageUrl: string) {
  const result = baseUrlRegex.exec(imageUrl)
  if (result && result.length > 0) return result[0]
  return imageUrl
}

function replaceLegacyFileManagerUrl(
  imageUrl: string,
  width: string,
  height: string
) {
  const legacyUrlPattern = '/arquivos/ids/'
  const isLegacyUrl = imageUrl.includes(legacyUrlPattern)
  if (!isLegacyUrl) return imageUrl
  return `${cleanImageUrl(imageUrl)}-${width}-${height}`
}

export function changeImageUrlSize(
  imageUrl: string,
  width?: number,
  height?: number
) {
  if (!imageUrl) return
  width && (width = Math.min(width, MAX_WIDTH))
  height && (height = Math.min(height, MAX_HEIGHT))

  const normalizedImageUrl = replaceLegacyFileManagerUrl(
    imageUrl,
    width ? width.toString() : DEFAULT_WIDTH,
    height ? height.toString() : DEFAULT_HEIGHT
  )
  const queryStringSeparator = normalizedImageUrl.includes('?') ? '&' : '?'

  return `${normalizedImageUrl}${queryStringSeparator}width=${width ??
    DEFAULT_WIDTH}&height=${height ?? DEFAULT_HEIGHT}&aspect=true`
}

function findAvailableProduct(item: any) {
  return item.sellers.find(
    ({ commertialOffer = {} }: { commertialOffer: any }) =>
      commertialOffer.AvailableQuantity > 0
  )
}

const defaultImage = { imageUrl: '', imageLabel: '' }
const defaultReference = { Value: '' }
const defaultSeller = { commertialOffer: { Price: 0, ListPrice: 0 } }

const resizeImage = (url: string, imageSize: number) =>
  changeImageUrlSize(toHttps(url), imageSize)

export function mapCatalogProductToProductSummary(
  product: any,
  wishlistId: string
) {
  if (!product) return null
  const normalizedProduct = {
    ...product,
    wishlistPage: true,
    wishlistId,
  }
  const items = normalizedProduct.items || []

  items.forEach((eachSku: any, skuIndex: number) => {
    eachSku.sellers.forEach((eachSeller: any, sellerIndex: number) => {
      const skuSpotPrice = eachSeller.commertialOffer.spotPrice
      const skuPrice = eachSeller.commertialOffer.Price
      if (skuSpotPrice && skuSpotPrice === skuPrice) {
        delete items[skuIndex].sellers[sellerIndex].commertialOffer.spotPrice
      }
    })
  })

  const sku = product.sku || items.find(findAvailableProduct) || items[0]
  if (sku) {
    const [seller = defaultSeller] = sku.sellers ?? []
    const [referenceId = defaultReference] = sku.referenceId ?? []
    const catalogImages = sku.images ?? []
    const normalizedImages = catalogImages.map((image: any) => ({
      ...image,
      imageUrl: resizeImage(image.imageUrl, 500),
    }))
    const [image = defaultImage] = normalizedImages
    normalizedProduct.sku = {
      ...sku,
      seller,
      referenceId,
      image,
      images: normalizedImages,
    }
  }
  return normalizedProduct
}
