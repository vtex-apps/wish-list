# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Security

- Fix broken access control (VSC-1265312) on the GraphQL queries and mutations. Callers are now identified from their VTEX ID token and authorized per request.
  - `viewList`, `viewLists`, `checkList`, `listNames`, `addToList` and `removeFromList`: only the owner of the list (the `shopperId` must match the email or user id of the shopper token) or an admin (token validated in VTEX ID and granted in License Manager) may access them.
  - Personal data (shopper ids and emails) and Master Data response bodies are no longer written to logs.
- The `wishlist` Master Data schema no longer has public fields (`publicRead`, `publicWrite` and `publicFilter` are empty). The app applies this schema when the account uses the wishlist; accounts with no wishlist traffic keep their previous schema until the app is used.

### Fixed

- Schema verification no longer runs in the repository constructor nor sends a `GET` and a `PUT` to Master Data on every request. The schema is compared by content (fields, index and security), the result is cached per account for one hour, a missing schema is created, `429`/`423`/`5xx` responses are retried and the applied schema is confirmed.
- `viewList` no longer fails with a null reference when the requested list does not exist.

### Changed

- **Breaking behaviour:** non-admin users can only access their own lists. Call-center/impersonation operators can no longer manage a customer's list unless they are admins. Requests without a valid token receive `Unauthorized` and requests for another shopper receive `Forbidden`.
- **Breaking behaviour:** integrations that read or write the `wishlist` Master Data entity directly must authenticate with an app key/token or user token that has Master Data permissions; unauthenticated requests receive `403`.

## [0.2.3] - 2020-07-14

### Fixed

- Performance issue

## [0.2.2] - 2020-07-14

### Removed

- Old file from the listing page that now is a context

### Fixed

- CSS Handles definition moved out of the main component

## [0.2.1] - 2020-06-25

### Fixed

- `/wishlist` page not forcing login

## [0.2.0] - 2020-06-05

### Changed

- Now the `/wishlist` route uses the `blocks` structure

### Updated

- Documentation with new blocks info

## [0.1.4] - 2020-06-05

## [0.1.3] - 2020-06-01

### Fixed

- Session Loading

## [0.1.2] - 2020-05-29

### Updated

- Verify database schema
- Remove duplicate documents

## [0.1.1] - 2020-05-28

### Updated

- Spanish translation with different keys

### Fixed

- Icon loading for non-authenticated users

## [0.1.0] - 2020-05-28

### Added

- Initial stable release
- Added Changelog
- Added Docs
