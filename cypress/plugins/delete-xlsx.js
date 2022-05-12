const fs = require('fs')

const deleteFile = ({ fileName }) => {
  fs.unlinkSync(fileName)
  return true
}

module.exports = {
  deleteFile,
}
