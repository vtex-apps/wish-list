import PropTypes from 'prop-types'
import { injectIntl } from 'react-intl'

const MyAccountWishlistLink = ({ render, intl, label }) => {
  return render([
    {
      name: label ?? intl.formatMessage({ id: 'store/myaccount-menu' }),
      path: '/wishlist',
    },
  ])
}

MyAccountWishlistLink.propTypes = {
  render: PropTypes.func.isRequired,
  intl: PropTypes.any,
  label: PropTypes.string,
}

export default injectIntl(MyAccountWishlistLink)
