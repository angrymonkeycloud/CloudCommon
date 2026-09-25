# Adopt the shared theme

The UI libraries now consume CloudCommon tokens directly. This is a breaking CSS-variable rename; regenerate exported themes and update application overrides together with the libraries.

| Before | Current |
| --- | --- |
| --cloud-* theme tokens | --amc-* |
| --cloudgrid-* | --cloudcomponents-grid-* |
| --cloudgridheader-* | --cloudcomponents-grid-header-* |
| --cloudeditor-* | --cloudcomponents-editor-* |
| --cloud-markdown-* | --cloudcomponents-markdown-* |
| --commerce-* | --cloudcommerce-* |
| --amc-datetimepicker-* | --cloudcomponents-datetime-picker-* |
| --amc-gridview-* | --cloudcomponents-grid-view-* |
| --amc-progressbar-* | --cloudcomponents-progress-bar-* |
| --amc-videoplayer-* | --cloudcomponents-video-player-* |
| --amc-accentColor | --amc-color-primary |
| --amc-secondColor | --amc-color-primary-hover |

Library token suffixes use lowercase hyphenated words. CDM's --cdm-* prefix stays. Markup class names stay independent.

## Hosts

Render CloudThemeDocument in a Blazor document head, or load ThemeCss.ExportDocument(theme) in any framework. Set html data-amc-theme to light, dark or system. Use CloudThemeScope for independent embedded interfaces.

- CDM: assign settings.Theme.Definition. AccentColor remains a shorthand when Definition is absent; existing appearance settings continue to control mode.
- CloudLogin: assign configuration.Theme and ThemeMode. PrimaryColor remains a three- or six-digit hex shorthand when Theme is absent.
- CloudCommerce and CloudComponents inherit the surrounding theme.
- CloudBlazor's demo uses the shared theme; its hosting infrastructure has no visual theme dependency.
- Geography, Payments, Logistics and Booking service packages do not need UI dependencies.

Set one definition in each separate browser document. A login redirect, iframe, third-party payment surface or native map canvas cannot inherit the originating page's CSS.

## Overrides

A property resolves a library override, then its shared token, then a grayscale fallback:

    .checkout {
        --cloudcommerce-secondary-button-normal-background: #00695c;
        --cloudcommerce-secondary-button-normal-color: #ffffff;
    }

The same theme continues to govern CDM and Login. Set shared element tokens to change every library, or configure the equivalent ThemeDefinition element state. Provider logos and data-driven map imagery remain owned by their providers.

## Building and releasing

The source workspace requires the sibling repositories described in the root README. Run npm run build:related from CloudCommon; it derives defaults from ThemeResolver and compiles each library's LESS using its asset manifest. Published packages contain generated CSS, so consumers do not need Node or sibling source repositories. The theme package includes the LESS contract for stylesheet authors.

Publish CloudCommon packages before dependent library versions. No packages have been published by this change.

## Shared models

Money, PostalAddress, GeoCoordinate and DateTimeRange remain framework-independent. GeographyAdapters bridges existing Money and Coordinate explicitly. Persisted model contracts, money codecs, geographic identifiers and service API payloads retain their formats; changing their stored shapes needs a separate data migration.

Read the [model inventory](model-inventory.md), [CSS contract](theming.md), and [workshop setup](../README.md).
