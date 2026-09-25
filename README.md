# CloudCommon
[![Website](https://img.shields.io/badge/Website-angrymonkeycloud.com-0B5FFF?style=flat-square)](https://angrymonkeycloud.com) [![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square)](https://dotnet.microsoft.com)

Shared models and a portable visual contract for Angry Monkey Cloud and external applications. Configure one theme to resolve six complete shade scales, semantic colors, typography, borders, radii, shadows, motion and element states. Export plain CSS or use a Blazor scope.

## Packages

| Project / package ID | Purpose | Release |
| --- | --- | --- |
| AngryMonkey.CloudCommon.Models | Money, PostalAddress, GeoCoordinate, DateTimeRange; no package dependencies | Local 0.1.0 |
| AngryMonkey.CloudCommon.Theming | Typed theme configuration, resolution and portable CSS export; no package dependencies | Local 0.1.0 |
| AngryMonkey.CloudCommon.Theming.Blazor | Optional Blazor theme scopes | Local 0.1.0 |
| AngryMonkey.CloudCommon.Geography | Explicit adapters for existing CloudGeography Money and Coordinate | Local 0.1.0 |

Packages have not been published. Reference the projects locally or pack them into a local NuGet feed. The workshop references local CDM, Login, Commerce and CloudComponents projects. The core models and theme resolver have no dependencies on them.

## Run the developer workshop

Requires .NET 10 and Node.js, with CloudComponents, CloudLogin, CloudCommerce and CloudBlazor alongside CloudCommon, and CDM alongside their parent angrymonkeycloud folder. From this folder:

    npm ci
    npm run build:related
    dotnet build CloudCommon.slnx
    dotnet run --project CloudCommon.Demo

For Visual Studio, open CloudCommon.slnx and press F5. CloudCommon.Demo is the default startup project; the shared CloudCommon Demo launch profile opens the browser. If Visual Studio remembers an earlier startup choice, select CloudCommon Demo in the debug toolbar once.

Open [the local workshop](http://127.0.0.1:5188). It follows the existing View / Code / Instructions workshop pattern, with live presets, primary color, typography, radii, light/dark mode, visual CSS, animation, shade inspection, independent scopes and model examples. Code reflects the current valid theme. Downloaded CSS works without .NET or Blazor.

The workshop embeds the actual CloudComponents DataGrid, CloudLogin MainInput, CloudCommerce CouponInput and CDM OptionButtons. Type a coupon, select a status, and toggle a Commerce-only override. Examples run locally without authentication or payment calls.

## Use a theme

    ThemeDefinition theme = CloudThemes.Color("#6750a4", design =>
    {
        design.Visuals.ControlRadius = ".75rem";
        design.Colors.PrimaryShades[700] = "#423066";
        design.Element(ThemeElements.Button).Hover.Css = "box-shadow: 0 0 8px #6750a4";
    });

    <CloudThemeScope Theme="theme" Mode="ThemeModes.Light">
        <button data-cloud-element="button">Continue</button>
    </CloudThemeScope>

Add layout and padding in the consuming application's LESS. The shared stylesheet owns visual properties only. Import AngryMonkey.CloudCommon.Theming and AngryMonkey.CloudCommon.Theming.Blazor.

For another framework, save ThemeCss.Export(theme) as a stylesheet, load it, and wrap participating markup in class="cloud-theme". Use data-cloud-element attributes, documented cloud-* classes, or semantic CSS variables. Read [the CSS contract](docs/theming.md).

## Existing project adoption

Related UI libraries consume the shared contract directly: --amc-* for the theme, --cloudcomponents-*, --cloudlogin-*, --cloudcommerce-* and --cdm-* for library overrides. See [the migration guide](docs/migration.md) for renamed tokens and host configuration.

No existing persisted models were renamed or rewritten. The new contracts are an opt-in foundation, with explicit geography adapters and a documented consolidation path. Read [the repository/model review](docs/model-inventory.md).

## Validation

    dotnet test CloudCommon.Tests
    npm run test:browser
    npm run test:libraries
    dotnet pack CloudCommon.slnx -o artifacts/packages

Start the demo before browser checks. Browser tests use installed Microsoft Edge and cover standalone CSS, partial overrides, gradients, mode changes, real grid rendering, isolated scopes, models and mobile layout. Screenshots and results are written under artifacts/.

Authored styles live in src/css/*.less; compiled CSS is checked in for reproducible .NET-only builds. Run npm run build:related after changing LESS. See [documentation](docs/README.md).

CloudCommon is part of Angry Monkey Cloud. Follow the [shared AI instructions](https://github.com/angrymonkeycloud/CloudDocs/blob/main/docs/ai/instructions.md). Visit [Angry Monkey Cloud](https://angrymonkeycloud.com) and the [GitHub organization](https://github.com/angrymonkeycloud).

Runtime component styles import their project-local theme.less and use local variables/mixins. See [the local LESS theme pattern](docs/theming.md#project-local-less-themes).
