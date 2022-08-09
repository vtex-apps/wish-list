import React, { FC, useState } from 'react'
import { injectIntl, defineMessages } from 'react-intl'
import {
  Layout,
  PageBlock,
  PageHeader,
  ButtonWithIcon,
  IconDownload,
} from 'vtex.styleguide'
import XLSX from 'xlsx'

const WishlistAdmin: FC<any> = ({ intl }) => {
  const [state, setState] = useState<any>({
    loading: false,
  })

  const { loading } = state

  const downloadWishlist = (allWishlists: any) => {
    const header = ['Email', 'Product ID', 'SKU', 'Title']
    const data: any = []

    for (const shopper of allWishlists) {
      const wishlists = shopper.listItemsWrapper
      for (const wishlist of wishlists) {
        for (const wishlistItem of wishlist.listItems) {
          const shopperData = {
            Email: shopper.email,
            'Product ID': wishlistItem.productId,
            SKU: wishlistItem.sku,
            Title: wishlistItem.title,
          }

          data.push(shopperData)
        }
      }
    }

    const ws = XLSX.utils.json_to_sheet(data, { header })
    const wb = XLSX.utils.book_new()
    XLSX.utils.book_append_sheet(wb, ws, 'Sheet1')
    const exportFileName = `wishlists.xls`
    XLSX.writeFile(wb, exportFileName)
  }

  const getAllWishlists = async () => {
    setState({ ...state, loading: true })

    const data: any = await fetch(`/_v/wishlist/export-lists`).then(response =>
      response.json()
    )
    const wishlistArr = data.wishLists

    downloadWishlist(wishlistArr)
    setState({ ...state, loading: false })
  }

  const messages = defineMessages({
    title: {
      id: 'admin/wishlist.menu.label',
      defaultMessage: 'Wishlist',
    },
    exportLabel: {
      id: 'admin/settings.title',
      defaultMessage: 'Wishlist Export',
    },
    download: {
      id: 'admin/settings.download',
      defaultMessage: 'Download Wishlists',
    },
  })

  const download = <IconDownload />

  return (
    <Layout
      pageHeader={<PageHeader title={intl.formatMessage(messages.title)} />}
    >
      <PageBlock variation="full">
        <ButtonWithIcon
          icon={download}
          isLoading={loading}
          onClick={() => {
            getAllWishlists()
          }}
        >
          {intl.formatMessage(messages.download)}
        </ButtonWithIcon>
      </PageBlock>
    </Layout>
  )
}

export default injectIntl(WishlistAdmin)
