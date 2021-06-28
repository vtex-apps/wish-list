# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Feature

- Custom view in case you do not add any produtc to the wish list

## [1.7.10] - 2021-06-10

### Fixed
- Wishlist displays cortect sku variation

## [1.7.9] - 2021-06-08

## [1.7.8] - 2021-06-08

### Fixed
- Occasionally when trying to remove a product from the wish list it doesn't work.

## [1.7.7] - 2021-06-03

### Fixed
- User logged and then logout, the button is not shown as activated.

## [1.7.6] - 2021-05-28

### Fixed

- Changed search to scroll when getting all records.

## [1.7.5] - 2021-05-27

### Fixed
- Spotprice not showing up
- Error when the component loads before the content exists at the PDP
## [1.7.4] - 2021-05-14

### Fixed
- Cannot remove from Wishlist immediately after adding it
## [1.7.3] - 2021-05-14

### Fixed
- Cannot remove from Wishlist
## [1.7.2] - 2021-05-13

### Fixed
- At PDP, heart stays filled when changing to another product from the shelf

### Added
- No SSR
## [1.7.1] - 2021-05-12

### Fixed

- Disable cache when getting lists

## [1.7.0] - 2021-05-06

## [1.6.2] - 2021-05-06

### Added

- Add ability to download all wishlists from the admin page as an xls file

## [1.6.1] - 2021-04-28

### Fixed

- After checking as wishlisted, you cannot uncheck

## [1.6.0] - 2021-04-23

### Added

- Add product after login

### Fixed

- Products recently added from the shelf don't show up as wishlisted at the PDP

## [1.5.2] - 2021-04-06

### Fixed

- Normalize Title field

## [1.5.1] - 2021-01-25

### Fixed

- Update tooling, lint code
- Adjust GraphQL caching

## [1.5.0] - 2021-01-22

### Fixed

- First product addition with error message
- First `/account/#/wishlist` page load with empty list

### Added

- Message when the Wishlist is empty
- CSS handle `emptyMessage`

## [1.4.2] - 2021-01-22

### Fixed

- Null object check

## [1.4.1] - 2021-01-21

### Fixed

- UrlEncode shopperId

## [1.4.0] - 2021-01-14

### Updated

- Docs
- Plugins dependency to the My Account page

### Added

- Change menu label under My Account page

### Removed

- `plugins.json` from the app

### Fixed

- Blocks configuration not being overwritten by the theme's block

## [1.3.3] - 2020-12-22

### Fixed

- Docs.

## [1.3.2] - 2020-12-22

### Added

- Romanian translation

## [1.3.1] - 2020-12-22

### Added

- Italian translation

## [1.3.0] - 2020-12-21

### Updated

- Message after adding to the wishlist now has a link to the `/account/#wishlist` page

### Fixed

- Missing seller's additional information from GraphQL search

## [1.2.1] - 2020-12-11

### Fixed

- My Account page rendering.

## [1.2.0] - 2020-12-07

### Added

- Added Wishlist menu under My Account

## [1.1.1] - 2020-11-25

### Fixed

- SSR loading old data at the listing page
- Error removing recent added item on the listing page
- Duplicated lists on the same email
- Retry checking existent list

### Added

- Default title on the listing page route

## [1.1.0] - 2020-11-11

### Fixed

- Performance improvements

## [1.0.10] - 2020-11-06

### Fixed

- New terms of use

## [1.0.9] - 2020-11-06

### Updated

- Doc update

## [1.0.8] - 2020-11-06

### Fixed

- `/wishlist` products link not working

## [1.0.7] - 2020-10-14

### Fixed

- Doc review and update

## [1.0.6] - 2020-09-25

### Added

- App Store Assets - new format.

## [1.0.5] - 2020-09-21

- Doc update `peerDependencies`

## [1.0.4] - 2020-09-21

### Added

- `crowdin.yml` config file

## [1.0.3] - 2020-09-16

### Fixed

- App documentation update (`readme.md` file)

## [1.0.2] - 2020-09-04

### Updated

- Doc update

## [1.0.1] - 2020-08-19

### Changed

- Update app store descriptions
- App store icon is now transparent

### Fixed

- Add billingOptions type and availableCountries

## [1.0.0] - 2020-07-21

### Added

- BillingOptions

### Updated

- APP's icon update
- Document

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
