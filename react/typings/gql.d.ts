declare module '*.graphql' {
  import { DocumentNode } from 'graphql'

  const value: DocumentNode
  export default value
}

declare module '*.gql' {
  const gql: any
  export default gql
}
