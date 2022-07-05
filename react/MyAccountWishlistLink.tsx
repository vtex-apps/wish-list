import PropTypes from 'prop-types'
import { useIntl } from 'react-intl'

const MyAccountWishlistLink = ({
  render,
  label,
}: {
  render: ([{ name, path }]: Array<{ name: string; path: string }>) => any
  label: string
}) => {
  const intl = useIntl()
  return render([
    {
      name: label ?? intl.formatMessage({ id: 'store/myaccount-menu' }),
      path: '/wishlist',
    },
  ])
}

MyAccountWishlistLink.propTypes = {
  render: PropTypes.func.isRequired,
  label: PropTypes.string,
}

export default MyAccountWishlistLink
