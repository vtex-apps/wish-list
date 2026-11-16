# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Security

- Fix broken access control (VSC-1265312) on the GraphQL queries and mutations. Callers are now identified from their VTEX ID token and authorized per request.
  - `viewList`, `viewLists`, `checkList`, `listNames`, `addToList` and `removeFromList`: only the owner of the list (the `shopperId` must match the email or user id of the shopper token) or an admin (token validated in VTEX ID and granted in License Manager) may access them.
  - Personal data (shopper ids and emails) and Master Data response bodies are no longer written to logs.
- The `email` field (the shopper id) of the `wishlist` Master Data schema is no longer public, matching the Master Data PII campaign. `ListItemsWrapper` and `id` stay public; making the whole entity private is tracked in STR-1393. The app never makes a field public again: a schema that is already more private is left as is. The app applies this schema when the account uses the wishlist; accounts with no wishlist traffic keep their previous schema until the app is used.

### Fixed

- Schema verification no longer runs in the repository constructor nor sends a `GET` and a `PUT` to Master Data on every request. The schema is compared by content (field names, `v-indexed`, and a `v-security` that must be at least as private as the app's), the result is cached per account for one hour, a missing schema is created, the schema `PUT` is retried on `429`/`423`/`5xx` and the schema is read back to confirm it, also after a `304` from the `PUT`. A transient failure (`429`/`423`/`5xx`, a network error, a `401`/`403` on the read, or a failed read back after a successful `PUT`) is retried after one minute; a schema the app cannot change (`PUT` rejected, or read back with different content) is left alone for one hour.
- Added the `POWER_USER_DS` policy, required to apply the Master Data schema, and removed the unused `bnb.data.bl.uk` and `nuget.org` outbound policies.
- The wishlist button no longer stays loading when a wishlist request returns an error (for example an expired session).
- A shopper accessing their own list is authorized with a single VTEX ID call; the admin check (VTEX ID + License Manager) only runs when the caller is not the owner of the list.
- `listNames` and the mutations are marked as private in the GraphQL cache, and `viewList`/`viewLists` are no longer cached by the browser, so a list is not served stale after `addToList`/`removeFromList`.
- `viewList` no longer fails with a null reference when the requested list does not exist, and `viewLists`/`listNames` no longer fail for a shopper who never used the wishlist or for an empty list entry.
- The caller's identity (VTEX ID) and the admin check (VTEX ID + License Manager) are resolved once per HTTP request and shared by every GraphQL field in it, so a query with several fields or aliases does not repeat these calls.
- Calls to VTEX ID and License Manager time out after 6 seconds (instead of the 100 s HttpClient default) and the request is denied, so a slow dependency does not hold the request.
- The `shopperId` argument is trimmed once, so the value that is authorized is the same value used to read and write the list.

### Changed

- **Breaking behaviour:** non-admin users can only access their own lists. Call-center/impersonation operators can no longer manage a customer's list unless they are admins. A denied call returns `Unauthorized` when no valid VTEX ID token is present, and `Forbidden` when the token is valid but does not own the list or is not granted in License Manager.
- **Breaking behaviour:** integrations that read, write or filter the `email` field of the `wishlist` Master Data entity without authentication stop working (as on accounts already changed by the PII campaign); they must authenticate with an app key/token or a user token that has Master Data permissions.
- The Master Data schema is account-wide: the first authorized request that reaches Master Data through this version on any workspace, including a non-master one, applies the schema to the whole account (each pod verifies once per hour). A 1.x version without this fix (1.19.3 or older) tries to make `email` public again on its next verification, because it holds the `POWER_USER_DS` policy, which reopens the data exposure if Master Data accepts it. 0.2.3 and older cannot: they lack that policy and their schema check is fire-and-forget. Do not keep a 1.x version without the fix linked or installed on other workspaces after the upgrade. A problem after the upgrade must be handled with a forward fix, not a rollback. Before the rollout, check which versions are installed or linked on each target account, and run pilots per account, not per workspace.
- A list saved with `public: true` is not readable by other shoppers through the API; `public` is kept as data only.

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
