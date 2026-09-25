# Shared model inventory across Angry Monkey Cloud

Reviewed on 2026-09-25: all 12 public repositories returned by the Angry Monkey Cloud organization API, plus the local CDM solution. This review covers model ownership and representative contracts; it is not a claim that every implementation file was audited. Local source was used where available; remaining repositories were inspected through GitHub trees, READMEs and relevant model sources.

## Repository decisions

| Repository | Existing concepts examined | Decision |
| --- | --- | --- |
| CDM | Cloud Common/MoneyValue, Address/AddressLocation, Schedule/TimeSlot, FileValue, HttpErrorResponse; pagination | Share portable money/address/coordinate values. Keep persistence attributes, codecs, record identities and field configuration in CDM. |
| CloudGeography | Money, Coordinate, Currency, Country, subdivisions, Language, PhoneNumber | Geography remains owner of reference datasets and parsing. Optional adapters bridge money and coordinates without importing geography into the common model package. |
| CloudCommerce | Commerce prices; CloudPayments amounts; LogisticsAddress, ShippingPackage and shipment statuses; Booking TimeSlot, BlackoutPeriod, reservation intervals | Adopt shared values at boundaries later. Keep recipient/contact details, carrier validation, units, statuses, reservations and provider payloads in their domain packages. |
| CloudComponents | MapCoordinate, grid data requests/results, date-time picker | Coordinate is a consolidation candidate. Grid query contracts remain grid-specific; no generic page/result abstraction was added. |
| CloudLogin | CloudWorkspaceAddress, verified contact identities, provider and workspace contracts | Postal address can be mapped to common values. Verification/security meaning stays in Login. |
| CloudBlazor | Page/SEO/localization configuration, bundles and application infrastructure | No business model to promote. Consume portable theme assets in hosts. |
| CloudWeb | Page, SEO and asset infrastructure | Keep web-specific types in their existing owner. |
| CloudApp | Navigation abstractions and MAUI/browser integrations | Keep navigation and platform behavior in CloudApp. |
| CloudMate | Build/configuration/compiler/package models | Keep development-tool contracts in CloudMate. |
| CloudLocalization | TranslationValue, Language, Translations and TranslationStatus (TypeScript) | Keep translation runtime structures here; do not create a competing language dataset or pretend a .NET package shares runtime types with JavaScript. |
| CloudTranslation | README points to CloudLocalization | Review its successor; no new shared model. |
| CloudCredits | CloudCreditsItem, CloudCreditsSection, CopyrightSection | Display-specific metadata stays in CloudCredits. |
| CloudDocs | Documentation and shared standards | Documentation owner; no runtime models. |

## Implemented contracts

- **Money**: immutable currency + exact decimal amount, nine-decimal maximum, explicit units/nanos bridge, same-currency arithmetic and caller-selected rounding. Missing legacy values map to null, never zero. Currency identifiers are format-validated, not checked against an ISO registry; use CloudGeography for authoritative currency metadata and minor units.
- **PostalAddress**: optional country/subdivision identifiers, locality, postal code, lines and coordinates. Allows incomplete drafts. It does not assert that a partial address is deliverable or valid for a carrier.
- **GeoCoordinate**: immutable, finite, bounded latitude/longitude and great-circle distance in kilometers.
- **DateTimeRange**: validated absolute interval with an exclusive end. It can express booking boundaries, but is not a replacement for CDM's recurring weekly TimeSlot, which uses different semantics.

## Explicitly not promoted

Currency/country/language/time-zone datasets and international phone parsing already belong to CloudGeography. User identity, verified email/phone state, roles and accounts belong to Login. Payment, shipping and order states remain domain-specific.

FileValue carries CDM persistence and upload semantics; it should not become a universal file type by moving it wholesale. A future metadata-only file reference needs consumers and storage requirements agreed first. Generic Result/Page wrappers and status enums were not introduced merely because names look similar. ASP.NET error responses should keep their existing ProblemDetails-compatible transport contract.

## Compatibility

Existing Geography Money and CDM MoneyValue remain authoritative for their existing public/storage APIs until an intentional versioned migration. CloudCommon's immutable money is a target contract, not an automatic wire-compatible replacement. CloudCommon.Geography provides tested adapters without implicit conversions.

Map CDM Country/Subdivision/SubdivisionChild/City to CountryCode/SubdivisionCode/SubdivisionChildCode/Locality. Map Login City/State/Country only after resolving whether their stored values are names or codes. Logistics RecipientName and PhoneNumber stay on a delivery envelope alongside the common postal address. Preserve old JSON payload shapes at public boundaries.

See [migration](migration.md) and [the workshop](../README.md).

Source repositories: [organization](https://github.com/angrymonkeycloud), [CloudGeography](https://github.com/angrymonkeycloud/CloudGeography), [CloudCommerce](https://github.com/angrymonkeycloud/CloudCommerce), [CloudLogin](https://github.com/angrymonkeycloud/CloudLogin), [CloudComponents](https://github.com/angrymonkeycloud/CloudComponents).
