import React, { FC, useCallback, useEffect, useMemo, useState } from 'react'
import { injectIntl, defineMessages } from 'react-intl'
import { useLazyQuery, useQuery } from 'react-apollo'
import {
  Layout,
  PageBlock,
  PageHeader,
  EXPERIMENTAL_Table as Table,
  EXPERIMENTAL_useTableMeasures as useTableMeasures,
  Input,
  ButtonWithIcon,
} from 'vtex.styleguide'
import XLSX from 'xlsx'

import exportListPaged from './queries/exportListPaged.gql'
import listSize from './queries/listSize.gql'
import scopeModeQuery from './queries/scopeMode.gql'

interface FlatRow {
  id: string
  email: string
  organizationId: string
  costCenterId: string
  productId: string
  sku: string
  title: string
}

const ROWS_OPTIONS = [15, 25, 50, 100]

const flattenWishlists = (wishlists: any[]): FlatRow[] => {
  const rows: FlatRow[] = []

  if (!wishlists) return rows

  for (const shopper of wishlists) {
    let hasItems = false

    if (shopper.listItemsWrapper) {
      for (const wrapper of shopper.listItemsWrapper) {
        if (!wrapper.listItems) continue
        for (const item of wrapper.listItems) {
          hasItems = true
          rows.push({
            id: `${shopper.email}-${item.productId}-${item.sku}`,
            email: shopper.email || '',
            organizationId: shopper.organizationId || '',
            costCenterId: shopper.costCenterId || '',
            productId: item.productId || '',
            sku: item.sku || '',
            title: item.title || '',
          })
        }
      }
    }

    if (!hasItems) {
      rows.push({
        id: `${shopper.email}-empty`,
        email: shopper.email || '',
        organizationId: shopper.organizationId || '',
        costCenterId: shopper.costCenterId || '',
        productId: '',
        sku: '',
        title: '',
      })
    }
  }

  return rows
}

const messages = defineMessages({
  title: {
    id: 'admin/wishlist.menu.label',
    defaultMessage: 'Wishlist',
  },
  email: {
    id: 'admin/settings.email',
    defaultMessage: 'Email',
  },
  organizationId: {
    id: 'admin/settings.organizationId',
    defaultMessage: 'Organization ID',
  },
  costCenterId: {
    id: 'admin/settings.costCenterId',
    defaultMessage: 'Cost Center ID',
  },
  applyFilters: {
    id: 'admin/settings.applyFilters',
    defaultMessage: 'Apply Filters',
  },
  productId: {
    id: 'admin/settings.productId',
    defaultMessage: 'Product ID',
  },
  productTitle: {
    id: 'admin/settings.productTitle',
    defaultMessage: 'Title',
  },
  exportLabel: {
    id: 'admin/settings.download',
    defaultMessage: 'Download',
  },
  totalWishlists: {
    id: 'admin/settings.totalWishlists',
    defaultMessage: 'Total wishlists',
  },
  totalItems: {
    id: 'admin/settings.totalItems',
    defaultMessage: 'Total items',
  },
  emptyState: {
    id: 'admin/settings.emptyState',
    defaultMessage: 'No wishlists found.',
  },
  density: {
    id: 'admin/settings.density',
    defaultMessage: 'Line density',
  },
  densityLow: {
    id: 'admin/settings.densityLow',
    defaultMessage: 'Low',
  },
  densityMedium: {
    id: 'admin/settings.densityMedium',
    defaultMessage: 'Medium',
  },
  densityHigh: {
    id: 'admin/settings.densityHigh',
    defaultMessage: 'High',
  },
  showRows: {
    id: 'admin/settings.showRows',
    defaultMessage: 'Show rows',
  },
  of: {
    id: 'admin/settings.of',
    defaultMessage: 'of',
  },
})

const WishlistAdmin: FC<any> = ({ intl }) => {
  const [emailFilter, setEmailFilter] = useState('')
  const [organizationFilter, setOrganizationFilter] = useState('')
  const [costCenterFilter, setCostCenterFilter] = useState('')

  const [allRows, setAllRows] = useState<FlatRow[]>([])
  const [tableLoading, setTableLoading] = useState(false)
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(ROWS_OPTIONS[0])

  const { data: scopeModeData } = useQuery(scopeModeQuery, {
    fetchPolicy: 'network-only',
  })

  const scopeMode = scopeModeData?.scopeMode ?? 'none'
  const showOrganization =
    scopeMode === 'organization' ||
    scopeMode === 'organization-and-cost-center'
  const showCostCenter = scopeMode === 'organization-and-cost-center'

  const filterVariables = useMemo(
    () => ({
      email: emailFilter || undefined,
      organizationId:
        showOrganization && organizationFilter
          ? organizationFilter
          : undefined,
      costCenterId:
        showCostCenter && costCenterFilter ? costCenterFilter : undefined,
    }),
    [
      emailFilter,
      organizationFilter,
      costCenterFilter,
      showOrganization,
      showCostCenter,
    ]
  )

  const [fetchListSize, { data: dataSize }] = useLazyQuery(listSize, {
    fetchPolicy: 'no-cache',
  })

  const [
    fetchExportListPaged,
    { data: dataPaged, loading: pagedQueryLoading },
  ] = useLazyQuery(exportListPaged, { fetchPolicy: 'no-cache' })

  const totalWishlists = dataSize?.listSize ?? 0

  const loadTableData = useCallback(() => {
    setTableLoading(true)
    setAllRows([])
    setCurrentPage(1)
    fetchListSize({ variables: filterVariables })
  }, [filterVariables, fetchListSize])

  useEffect(() => {
    loadTableData()
  }, [])

  const [totalPages, setTotalPages] = useState(0)
  const [currentLoadPage, setCurrentLoadPage] = useState(1)

  useEffect(() => {
    if (!dataSize) return

    const total = dataSize.listSize ?? 0
    const pages = Math.ceil(total / 5000)

    setTotalPages(pages)

    if (pages > 0) {
      setCurrentLoadPage(1)
      fetchExportListPaged({
        variables: { pageList: 1, ...filterVariables },
      })
    } else {
      setTableLoading(false)
      setAllRows([])
    }
  }, [dataSize])

  useEffect(() => {
    if (pagedQueryLoading || !dataPaged?.exportListPaged) return

    const newRows = flattenWishlists(dataPaged.exportListPaged)

    setAllRows((prev) => [...prev, ...newRows])

    const nextPage = currentLoadPage + 1

    if (nextPage <= totalPages) {
      setCurrentLoadPage(nextPage)
      fetchExportListPaged({
        variables: { pageList: nextPage, ...filterVariables },
      })
    } else {
      setTableLoading(false)
    }
  }, [pagedQueryLoading, dataPaged])

  const paginatedItems = useMemo(() => {
    const from = (currentPage - 1) * pageSize
    const to = from + pageSize

    return allRows.slice(from, to)
  }, [allRows, currentPage, pageSize])

  const measures = useTableMeasures({
    size: paginatedItems.length || pageSize,
  })

  const columns = useMemo(() => {
    const cols: any[] = [
      {
        id: 'email',
        title: intl.formatMessage(messages.email),
      },
    ]

    if (showOrganization) {
      cols.push({
        id: 'organizationId',
        title: intl.formatMessage(messages.organizationId),
      })
    }

    if (showCostCenter) {
      cols.push({
        id: 'costCenterId',
        title: intl.formatMessage(messages.costCenterId),
      })
    }

    cols.push(
      {
        id: 'productId',
        title: intl.formatMessage(messages.productId),
        cellRenderer: ({ data }: { data: FlatRow }) => (
          <span>{data.productId || '—'}</span>
        ),
      },
      {
        id: 'sku',
        title: 'SKU',
        cellRenderer: ({ data }: { data: FlatRow }) => (
          <span>{data.sku || '—'}</span>
        ),
      },
      {
        id: 'title',
        title: intl.formatMessage(messages.productTitle),
        cellRenderer: ({ data }: { data: FlatRow }) => (
          <span>{data.title || '—'}</span>
        ),
      }
    )

    return cols
  }, [intl, showOrganization, showCostCenter])

  const downloadWishlist = useCallback(
    (rows: FlatRow[]) => {
      const header = ['Email']

      if (showOrganization) header.push('Organization ID')
      if (showCostCenter) header.push('Cost Center ID')
      header.push('Product ID', 'SKU', 'Title')

      const data = rows.map((row) => {
        const entry: Record<string, string> = {
          Email: row.email,
          'Product ID': row.productId,
          SKU: row.sku,
          Title: row.title,
        }

        if (showOrganization) {
          entry['Organization ID'] = row.organizationId
        }

        if (showCostCenter) {
          entry['Cost Center ID'] = row.costCenterId
        }

        return entry
      })

      const ws = XLSX.utils.json_to_sheet(data, { header })
      const wb = XLSX.utils.book_new()

      XLSX.utils.book_append_sheet(wb, ws, 'Wishlists')
      XLSX.writeFile(wb, 'wishlists.xls')
    },
    [showOrganization, showCostCenter]
  )

  const handleExport = useCallback(() => {
    if (allRows.length > 0) {
      downloadWishlist(allRows)
    }
  }, [allRows, downloadWishlist])

  const totalItems = allRows.length
  const currentTo = Math.min(currentPage * pageSize, totalItems)
  const currentFrom = Math.min(currentTo, (currentPage - 1) * pageSize + 1)

  const handleNextClick = () => setCurrentPage((p) => p + 1)
  const handlePrevClick = () => setCurrentPage((p) => Math.max(1, p - 1))

  const handleRowsChange = (_: any, value: string) => {
    setPageSize(parseInt(value, 10))
    setCurrentPage(1)
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      loadTableData()
    }
  }

  const density = {
    label: intl.formatMessage(messages.density),
    compactLabel: intl.formatMessage(messages.densityHigh),
    regularLabel: intl.formatMessage(messages.densityMedium),
    comfortableLabel: intl.formatMessage(messages.densityLow),
  }

  return (
    <Layout
      pageHeader={<PageHeader title={intl.formatMessage(messages.title)} />}
    >
      <PageBlock variation="full">
        <div className="flex items-center flex-wrap">
          <div className="mr3 mb3" style={{ minWidth: 200 }}>
            <Input
              placeholder={intl.formatMessage(messages.email)}
              value={emailFilter}
              onChange={(e: any) => setEmailFilter(e.target.value)}
              onKeyDown={handleKeyDown}
              size="small"
            />
          </div>
          {showOrganization && (
            <div className="mr3 mb3" style={{ minWidth: 200 }}>
              <Input
                placeholder={intl.formatMessage(messages.organizationId)}
                value={organizationFilter}
                onChange={(e: any) => setOrganizationFilter(e.target.value)}
                onKeyDown={handleKeyDown}
                size="small"
              />
            </div>
          )}
          {showCostCenter && (
            <div className="mr3 mb3" style={{ minWidth: 200 }}>
              <Input
                placeholder={intl.formatMessage(messages.costCenterId)}
                value={costCenterFilter}
                onChange={(e: any) => setCostCenterFilter(e.target.value)}
                onKeyDown={handleKeyDown}
                size="small"
              />
            </div>
          )}
          <div className="mb3">
            <ButtonWithIcon
              variation="secondary"
              size="small"
              onClick={() => loadTableData()}
              isLoading={tableLoading}
            >
              {intl.formatMessage(messages.applyFilters)}
            </ButtonWithIcon>
          </div>
        </div>
      </PageBlock>

      <div className="bg-base pa5 br3">
        <Table
          measures={measures}
          items={paginatedItems}
          columns={columns}
          loading={tableLoading ? { renderAs: () => null } : false}
          empty={!tableLoading && totalItems === 0}
          emptyState={{
            label: intl.formatMessage(messages.emptyState),
          }}
          composableSections
        >
          <Table.Toolbar>
            <Table.Toolbar.ButtonGroup>
              <Table.Toolbar.ButtonGroup.Density
                density={measures}
                {...density}
              />
              <Table.Toolbar.ButtonGroup.Download
                label={intl.formatMessage(messages.exportLabel)}
                onClick={handleExport}
                disabled={totalItems === 0}
              />
            </Table.Toolbar.ButtonGroup>
          </Table.Toolbar>

          <Table.Totalizer
            items={[
              {
                label: intl.formatMessage(messages.totalWishlists),
                value: `${totalWishlists}`,
              },
              {
                label: intl.formatMessage(messages.totalItems),
                value: `${totalItems}`,
              },
            ]}
          />

          <Table.Sections>
            <Table.Sections.Head />
            <Table.Sections.Body />
          </Table.Sections>

          <Table.Pagination
            onNextClick={handleNextClick}
            onPrevClick={handlePrevClick}
            currentItemFrom={currentFrom}
            currentItemTo={currentTo}
            totalItems={totalItems}
            textOf={intl.formatMessage(messages.of)}
            textShowRows={intl.formatMessage(messages.showRows)}
            rowsOptions={ROWS_OPTIONS}
            selectedOption={pageSize}
            onRowsChange={handleRowsChange}
          />
        </Table>
      </div>
    </Layout>
  )
}

export default injectIntl(WishlistAdmin)
