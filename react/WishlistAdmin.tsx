/* eslint-disable no-await-in-loop */
/* eslint-disable no-loop-func */
import React, { FC } from 'react'
import { injectIntl, defineMessages } from 'react-intl'
import {
  Layout,
  PageBlock,
  PageHeader,
  ButtonWithIcon,
  IconDownload,
} from 'vtex.styleguide'
import XLSX from 'xlsx'
import { useRuntime } from 'vtex.render-runtime'

const WishlistAdmin: FC<any> = ({ intl }) => {
  const { account } = useRuntime()

  const fetchWishlists = async (from: number, to: number) => {
    const response = await fetch(
      `https://${account}.myvtex.com/_v/wishlist/export-lists?from=${from}&to=${to}`,
      { mode: 'no-cors' }
    )

    return response.json()
  }

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

  let allWishlists: any = []

  const getAllWishlists = async () => {
    let status = true
    let i = 0
    const chunkLength = 100

    while (status) {
      await fetchWishlists(i, i + chunkLength).then(data => {
        const wishlistArr = data.wishLists

        if (!wishlistArr.length) {
          status = false
        }
        allWishlists = [...allWishlists, ...wishlistArr]
      })

      i += 100
    }

    downloadWishlist(allWishlists)
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
