import PropTypes from 'prop-types'
import { injectIntl } from 'react-intl'

const MyAccountWishlistLink = ({ render, intl }) => {
  return render([
    {
      name: intl.formatMessage({ id: 'store/myaccount-menu' }),
      path: '/wishlist',
    },
  ])
}

MyAccountWishlistLink.propTypes = {
  render: PropTypes.func.isRequired,
  intl: PropTypes.any,
}

export default injectIntl(MyAccountWishlistLink)
