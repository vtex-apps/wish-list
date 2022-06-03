const readXlsx = require('./read-xlsx')
const deleteXlsx = require('./delete-xlsx')

module.exports = on => {
  on('task', {
    readXlsx: readXlsx.read,
    deleteFile: deleteXlsx.deleteFile,
  })
}
