import React, { FC, useEffect, useState } from 'react'
import { injectIntl, defineMessages } from 'react-intl'
import { useQuery } from 'react-apollo'
import {
  Layout,
  PageBlock,
  PageHeader,
  ButtonWithIcon,
  IconDownload,
  Dropdown
} from 'vtex.styleguide'
import XLSX from 'xlsx'

import exportList from './queries/exportList.gql'
import exportListPaged from './queries/exportListPaged.gql'
import listSize from './queries/listSize.gql'

const WishlistAdmin: FC<any> = ({ intl }) => {
  const [state, setState] = useState<any>({
    loading: false,
  })
  const [isLongList, setIsLongList] = useState<boolean>(false)
  const [selected1, setSelected1] = useState<any>(null)
  const [options, setOptions] = useState<any>([])

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
    const exportFileName = `wishlists_page_${selected1}.xls`
    XLSX.writeFile(wb, exportFileName)
  }

  const { data, loading: queryLoading } = useQuery(exportList, {
    fetchPolicy: 'no-cache',
    variables: { pageList: 1 },
  })

  const { data: dataSize, loading: queryLoadingSize } = useQuery(listSize, {
    fetchPolicy: 'no-cache'
  })   

  const { data: dataPaged, loading: queryLoadingPaged, refetch } = useQuery(exportListPaged, {
    fetchPolicy: 'no-cache',
    variables: { pageList: selected1 },
  })

  useEffect(() => {
     
    if(queryLoadingSize) return

    let pages: number = dataSize.listSize/5000 + 1

    for(let i=1; i <= pages; i++) {
    
      setOptions((current: any) => [...current, 
        {
          value: `${i}`,
          label: `${i}`
        }
      ])
    }

    if(dataSize?.listSize > 5000) {
      setIsLongList(true)
    }

  },[queryLoadingSize, dataSize])

  const GetAllWishlistsPaged = async () => {

    setState({ ...state, loading: true })

    if (!queryLoadingPaged) {
      const parsedDataPaged = dataPaged?.exportListPaged
      downloadWishlist(parsedDataPaged)
    }
    setState({ ...state, loading: false })
  }

  const GetAllWishlists = async () => {
    setState({ ...state, loading: true })
    console.log(loading)

    if (!queryLoading) {
      const parsedData = data?.exportList
      downloadWishlist(parsedData)
    }
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
    page: {
      id: 'admin/settings.page',
      defaultMessage: 'Page',
    },
  })

  const download = <IconDownload />

  return (
    <Layout
      pageHeader={<PageHeader title={intl.formatMessage(messages.title)} />}
    >
      {isLongList ? 
        <PageBlock variation="full">
          <div className="w-100 mb4">
            <div className="w-30">
              <Dropdown
                size="small"
                label={intl.formatMessage(messages.page)}
                options={options}
                value={selected1}
                onChange={(event: any) => {
                  console.log(event.target.value)
                  setSelected1(event.target.value)
                  setTimeout(()=>{refetch()},500)
                  
                }}
              />
            </div>
          </div>
          <ButtonWithIcon
            icon={download}
            isLoading={queryLoadingPaged}
            onClick={() => {
              GetAllWishlistsPaged()
            }}
          >
            {intl.formatMessage(messages.download)}
          </ButtonWithIcon>
        </PageBlock>
      :
        <PageBlock variation="full">
          <ButtonWithIcon
            icon={download}
            isLoading={loading}
            onClick={() => {
              GetAllWishlists()
            }}
          >
            {intl.formatMessage(messages.download)}
          </ButtonWithIcon>
        </PageBlock>
      }
    </Layout>
  )
}

export default injectIntl(WishlistAdmin)
