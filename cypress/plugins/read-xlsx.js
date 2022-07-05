const fs = require('fs')

const XLSX = require('xlsx')

const read = ({ file, sheet }) => {
  const buf = fs.readFileSync(file)
  const workbook = XLSX.read(buf, { type: 'buffer' })
  return XLSX.utils.sheet_to_json(workbook.Sheets[sheet])
}

module.exports = {
  read,
}
