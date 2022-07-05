import React, { FC } from 'react'
import { useCssHandles } from 'vtex.css-handles'

const CSS_HANDLES = ['emptyMessage'] as const

const WishlistEmpty: FC = ({ children }) => {
  const handles = useCssHandles(CSS_HANDLES)

  return <div className={`ml5 ${handles.emptyMessage}`}>{children}</div>
}

export default WishlistEmpty
