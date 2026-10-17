# Game typography

The game's lettering now follows the illustrated, slightly crooked character of its Halloween props. Griffy is used for the game title, chapter headings and modal headings. Averia Sans Libre is used for instructions, counters, buttons, recipes and world-space lettering; its separate Bold face gives buttons and painted signboards emphasis without simulated bold styling.

## Shared hierarchy

| Role | Typeface | Logical size |
| --- | --- | --- |
| Cover title | Griffy Regular | 96, fitted to its actual line height |
| Cover subtitle | Griffy Regular | 38 |
| Chapter heading | Griffy Regular | 32 |
| Pause/defeat heading | Griffy Regular | 43 |
| Instructions and objective | Averia Sans Libre Regular | 21 |
| Counters and control labels | Averia Sans Libre Regular | 18 |
| Main action / ordinary buttons | Averia Sans Libre Bold | 23 / 22 |
| Chapter selection | Averia Sans Libre Regular | 21 |

`DreamTypography` loads the embedded fonts and supplies presentation-only reading case for capitalized UI labels. Keyboard abbreviations, Roman numerals, candy symbols and the underlying gameplay strings retain their meaning. `DreamHUD` removes inherited label padding and fits actual font metrics to each text rectangle. Display names are cached rather than recased every frame. The existing cover artwork and chapter layouts remain authored as before.

At scene startup the theme styles existing `TextMesh` components, including inactive recipe labels and shrines. Each label receives the matching font atlas material; its position, billboard behavior and color remain intact. Sign text uses real Bold and is reduced only if its new measured width would exceed the old width. Runtime-created candy labels and newly constructed signboards use the same theme. No saved map is regenerated or resaved for this update.

## Embedded source and license

The unmodified TTF files come from the official [Google Fonts repository](https://github.com/google/fonts), pinned to revision `47d54ca1ebb14fd864695f1d39dc734f39017c33`:

- [Griffy](https://github.com/google/fonts/tree/47d54ca1ebb14fd864695f1d39dc734f39017c33/ofl/griffy), by Font Diner.
- [Averia Sans Libre](https://github.com/google/fonts/tree/47d54ca1ebb14fd864695f1d39dc734f39017c33/ofl/averiasanslibre), by Dan Sayers.

Original SIL Open Font License files and SHA-256 provenance are retained under `Assets/ThirdParty/Fonts`. The embedded fonts live in `Assets/Resources/Typography`; Unity imports them as dynamic fonts with font data included, so a player does not need to install the fonts on their computer.

## Review

`TypographyBuilder.ValidateAndBuild` checks embedded glyph coverage and builds the four saved scenes. Launch the isolated standalone export with `--smoke-test --typography-smoke` for the focused visual review. It covers title/menu aspect ratios, all four chapter HUDs and world labels, potion recipes, Boss candy prompts, pause, retry and victory. It also measures text bounds on repaint, so successful checks are paired with screenshots rather than relying only on assigned font references. Current evidence and unchanged-map hashes are in `docs/qa/typography`.

This is a visual typography review, not a new full gameplay or human usability assessment. Earlier gameplay results remain historical evidence for their recorded source versions.
