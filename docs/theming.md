# Portable theming and CSS contract

CloudCommon.Theming is independent of Blazor and third-party packages. CloudCommon.Theming.Blazor adds an optional scoped component. All authored styles are LESS; ThemeCss exports generated CSS for any web stack.

## Resolution

The same definition resolves independently for Light and Dark. Defaults are fully grayscale, including semantic statuses. A primary color produces complete primary, secondary, neutral and status palettes; explicit secondary/status colors and shade overrides take precedence.

Every palette exposes 50, 100, 200, 300, 400, 500, 600, 700, 800, 900 and 950. The generator mixes the seed toward white/black in sRGB; shade 500 preserves the seed. This is deterministic tint/shade generation, not a perceptually uniform color-space algorithm. Explicit primary/secondary/neutral shade dictionaries can replace generated shades.

Semantic values cover backgrounds, surfaces, text, muted text, borders, selected/focus states, links and status/action colors. Typography covers family, small/body/heading size, normal/strong weight and line height. Visuals cover radii, borders, shadow and motion.

Foregrounds for generated filled actions choose black or white by contrast ratio. Link colors are adjusted against the surface to at least 4.5:1. Arbitrary CSS, shade and mode overrides remain the caller's responsibility; contrast adjustment is not a guarantee for every custom combination.

Order: defaults → generated palettes → design values → mode token overrides → typed element values → element CSS. Each property merges independently. A normal-state override carries into states unless a state has a derived or explicit value for that property.

## Shared elements and states

Elements: button, secondary-button, danger-button, input, link, card, dialog, badge, table and navigation. Apply data-cloud-element="button" or class="cloud-button" to opt in. A select/textarea can use the input target. Checkboxes/radios can consume semantic variables in their owning component; this release does not restyle their native structure.

States: normal, hover, active, focus-visible, disabled, selected/pressed and invalid. Element CSS does not impose positioning, layout, padding, dimensions or breakpoint behavior.

The element/state custom properties use --amc-{element}-{state}-{property}. Semantic tokens use names such as --amc-color-primary, --amc-color-surface, --amc-color-text, --amc-radius-control and --amc-font-family. Palette tokens use --amc-primary-500.

## External applications

    string stylesheet = ThemeCss.Export(theme, ThemeModes.Light, scope: "customer-theme");

Load that stylesheet and put class="customer-theme cloud-theme" on the host. The default scope is already cloud-theme.

Your own LESS may consume semantic variables without using the element classes. Override tokens on the defining theme scope, or override element styles directly with ordinary CSS. Replacing a semantic color does not regenerate the palette or its contrast foreground; update related values deliberately or regenerate through CloudThemes.Color.

CSS variable aliases are computed at their defining scope. For an independently themed subtree, use another exported scope/provider instead of assuming a descendant primary-color override recomputes inherited aliases. Defaults use low-specificity :where selectors, so ordinary consuming CSS can override them.

## Visual CSS and animations

VisualCss.Properties lists the supported declaration subset. It includes background (including gradients), color, borders, typography, shadow, opacity, outline, transition and filter. Longhand background-color and shorthands font/border are deliberately not accepted; use background, font-family/font-size and border-color/border-width/border-style. This is a constrained declaration API, not a general-purpose CSS parser.

Selectors, structural properties, rules, URLs, escapes, comments and !important are rejected. Theme configuration is application/developer configuration, not an untrusted end-user CSS sandbox. Validate any externally supplied values and retain the application's CSP.

Define a named ThemeAnimation with From/To visual declarations and a duration, then reference it using VisualStyle.Animation. Export namespaces keyframe names by scope. Reduced-motion preference disables animations and transitions.

## Blazor scopes

CloudThemeScope accepts Theme, Mode, Class, Nonce and ChildContent. It resolves during server rendering, avoiding an initial unthemed render; each scope receives a unique class. Child scopes own complete independent themes. Render popup/portal content inside the correct scope or give its external container a matching theme scope.

The component emits its own style block; Nonce supports hosts that require a CSP nonce. Do not serialize per-user themes into process-wide mutable singletons. Browser/tenant configuration belongs to the host.

See [migration](migration.md) for existing variable aliases and remaining work.

## CSS namespaces

Shared values use --amc-*, including --amc-primary-500, --amc-color-surface and --amc-button-hover-background. Component variables use --cloudcomponents-*, --cloudlogin-*, --cloudcommerce-* or --cdm-*. Class names and data-cloud-element attributes are independent of variable names.

Define override chains in the project-local theme.less and consume LESS aliases at each property. See the local theme pattern below.

ThemeCss.ExportDocument(theme) produces document-wide CSS. Set data-amc-theme on html to light, dark or system. CloudThemeDocument emits this CSS in Blazor; CloudThemeScope is for independent islands. Export(theme) keeps the .cloud-theme wrapper contract for scoped use. Previously exported stylesheets must be regenerated with --amc-* variables.

The shared library.less mixins apply normal, hover, active, selected, invalid, disabled and focus visuals to existing selectors. Focus-visible keeps its outline when states overlap. Layout and sizing remain in the consuming LESS. Defaults and fallbacks are generated from the same resolver with npm run build:related.

## Project-local LESS themes

Each runtime UI project owns `src/css/theme.less` (CloudLogin uses `Src/css/theme.less`). Keep CloudTheme variable mappings and wrappers for shared control mixins there. Every consuming stylesheet begins with a reference import of its local theme and uses project names:

```less
// theme.less
@import (reference) "path/to/CloudCommon.Theming/src/css/library.less";
@commerce-action-background: var(--cloudcommerce-button-normal-background, var(--amc-button-normal-background, #525252));
.commerce-control(@element) { .amc-control(cloudcommerce; @element); }

// component.less
@import (reference) "path/to/theme.less";
.checkout-button {
    .commerce-control(button);
    background: @commerce-action-background;
}
```

LESS aliases expand at each declaration, preserving nested theme scopes and library overrides. Do not place these aliases on `:root`, where CSS variables would resolve before a nested scope can supply its values. The emitted public variable names remain unchanged.

## Preserving component appearance

CloudTheme supplies shared palette and typography values. Existing CDM, CloudLogin, CloudCommerce and CloudComponents controls keep their own layout, sizes, radii and state recipes. Do not apply the full shared control mixin to every button or input: tabs, icon buttons, selection controls, ghost actions and provider buttons have different visual roles. Map the properties a component owns through its local theme.less. Full shared element recipes remain opt-in for controls intentionally adopting that appearance.

CDM and CloudLogin retain their original default brand colors and expose ResolveTheme() on their theme configuration. A supplied complete ThemeDefinition still takes precedence. CDM bounds the shell to 100dvh; forms, full-height grids and sidebar navigation own their scroll containers.

Run npm run test:appearance in CloudCommon to verify original component geometry, button variants and scrolling. The browser baseline records the pre-migration CDM and CloudLogin styles; it does not depend on the current Git HEAD.
