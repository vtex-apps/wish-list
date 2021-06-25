import React, { FC } from 'react';
import { useCssHandles } from 'vtex.css-handles';
import { FormattedMessage } from 'react-intl'

const CSS_HANDLES = ['emptyMessage'] as const;

const WishlistEmpty: FC = ({ children }) => {
  const isChildren = children?.toString() === '';
  const handles = useCssHandles(CSS_HANDLES);

  return (
    <div className={`ml5 ${handles.emptyMessage}`}>
      {isChildren ? 
        <FormattedMessage id="store/myaccount-empty-list" />
      : children 
      }
    </div>
  )
}

export default WishlistEmpty;
