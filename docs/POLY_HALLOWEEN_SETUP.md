# Poly Halloween artwork and restoration

The selected asset is **Poly Halloween Pack by polyperfect**, Unity Asset Store package **236625**.

- [Official asset page](https://assetstore.unity.com/packages/3d/props/poly-halloween-pack-236625).
- Acquisition: the owner added the package to My Assets and downloaded it through Unity Package Manager.
- License: [Standard Unity Asset Store EULA](https://unity.com/legal/as-terms), not CC0. Original source assets remain local; public Git excludes `Assets/polyperfect` and its root metadata.
- Downloaded package SHA-256: `11a393a120798a3d5819fc7d608ce24e8785e9909ca4f7f9cd27b9c97c334008`.

## Independent art study

Open `Assets/Scenes/Previews/PolyHalloweenPreview.unity` in Unity. Game view shows the selected composition; Scene view supports manual placement and inspection. The study contains a pumpkin-lit path, graveyard, dead trees, entrance wall, cauldron, Timmy and a small dragon at the game's existing scale.

This separate scene has no combat, pickup, lantern-switch or chapter-progression logic and is excluded from the formal build scene list. It remains available for further manual arrangement. Rendered views are in `docs/qa/poly-halloween-preview/overview.png` and `characters-and-props.png`; the latter is the owner-selected v0.12 start-screen image.

## v0.11 chapter artwork

After approval of the preview, the artwork was applied to all three saved maps. The tutorial uses selected edge trees and pumpkins; the gardens add tombstones, coffins and dead trees; the castle adds entrance pillars, a cauldron, perimeter tombstones, wall-top candles and webs. Move or remove decorations in the `Poly Halloween scenery` hierarchy.

Interactive pumpkin lanterns retain their original `Lantern` component, pedestal, interaction ring, light, road references and checkpoint. `LanternSkin` switches imported unlit/lit models with the original switch state. The original geometric renderers are hidden while their objects and collision remain.

Decorations have collision disabled and use Ignore Raycast to preserve movement, shooting, candy placement and camera obstruction rules. Saved-file audits preserve original objects, transforms, existing collision and gameplay values, with Unity's newly serialized default fields recorded separately.

The pre-artwork scenes and metadata are archived locally under `Builds/archive/before-poly-halloween-20261015-rwk_p19a`; validation is in `docs/qa/v0.11.0`. The artwork operation did not call the world generator or change the enabled scene list. Historical QA records retain their original local-only status; the current source synchronization also includes v0.12.

## Restore assets after cloning

1. Acquire the package using your own Unity account and download it from Package Manager's My Assets.
2. Restore the required files at their original import paths, preserving official GUIDs:

   ```sh
   python3 tools/import_poly_halloween.py '/path/to/Poly Halloween Pack.unitypackage' '/path/to/project'
   ```

3. The script restores models, textures, materials, prefabs and original metadata. It omits demo scenes, demo scripts and bundled URP/HDRP archives. This project uses the Built-in render pipeline; do not overwrite its materials with another pipeline package.
4. Restore Timmy following `docs/MIXAMO_SETUP.md` before opening the scenes.

The local asset folder contains `LOST_DREAM_SOURCE.json` with import provenance. The script checks all existing content before writing and stops if a file differs.

The Unity menu **Lost Dream → Preview → Create Poly Halloween art study** creates another study under a new filename. It retains the existing preview and opens additively to preserve other open scenes and their unsaved content.
