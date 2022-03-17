# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.11.5] - 2022-03-17

### Fixed

- Set timeout to 60s to avoid proxy time out error when downloading a large amount of data

### Fixed

- Add immediate indexing to the schema to solve masterdata delay update

## [1.11.4] - 2022-03-15

### Fixed

- Fix VerifySchema invalid header error when VerifySchema called without data entry, using app's authtoken instead of the user's authtoken.

## [1.11.3] - 2022-03-14

### Fixed

- Fix favorited check and stop duplicate item added to same list

## [1.11.2] - 2022-03-02

### Fixed

- Fix products added to wishlist before logging in are not displayed

## [1.11.1] - 2022-03-01
### Added

- Added URL information in README file for the user

## [1.11.0] - 2022-02-25

### Fixed

- Fix the api for downloading all the wishlist records

## [1.10.0] - 2022-01-28

### Added

- Added a property title on the my-account-page.wishlist-page / MyAccountWishlistLink in order to show the back button (ContentWrapper). Also, it needs to remove "flex-layout.row#top" to avoid duplicating the header.
- Quality engineering actions

### Fixed

- Fix on image list on the wish-list page
- Fix on heart button and session storage array

## [1.9.5] - 2022-01-13

- Hotfix when the SKU is unvailable

## [1.9.4] - 2022-01-13

- Bug fixed on adding wishlist with the same product ID, filtering by SKU values

## [1.9.3] - 2021-12-21

### Added

- Add pixel in ProductSummaryWishlist to trigger productClick analytics event

## [1.9.2] - 2021-09-29

### Added

- I18n bg

### Fixed

- I18n ro

## [1.9.1] - 2021-09-28

### Fixed

- I18n en, es, fr, it, ja, ko, nl, pt and ro

## [1.9.0] - 2021-08-31

### Feature

- Add pixel in AddProductBtn and removeProductBtn

## [1.8.4] - 2021-08-31

### Fix

- Add view empty when user has only one product and this product has no inventory

## [1.8.3] - 2021-08-31

## [1.8.2] - 2021-08-18

## [1.8.1] - 2021-08-18

### Changed

- Use context.Vtex.AdminUserAuthToken when creating schema

## [1.8.0] - 2021-08-18

### Added

- Added logging

### Changed

- Wait for VerifySchema to complete

## [1.7.12] - 2021-07-20

### Fixed

- Added policy to fix inability to apply db schema
- Removed Console writes

## [1.7.11] - 2021-06-30

### Feature

- Custom view in case you do not add any produtc to the wish list

## [1.7.10] - 2021-06-10

### Fixed

- Wishlist displays cortect sku variation

## [1.7.9] - 2021-06-08

### Added

- prop `toastURL` to component AddBtn with default value '/account/#wishlist'

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
