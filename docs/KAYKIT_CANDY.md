# KayKit candy artwork

The game uses `candyA.fbx` from **KayKit Spooktober Seasonal Pack 1.1 (Legacy)** by **Kay Lousberg**. The author labels this pack **CC0**. The original license is retained verbatim in `Assets/ThirdParty/KayKit/Spooktober/License.txt`.

- [Author's legacy pack](https://kaylousberg.itch.io/kaykit-spooktober)
- [Author's OpenGameArt upload](https://opengameart.org/content/kaykit-spooktober-seasonal-pack-11)
- `Assets/ThirdParty/KayKit/Spooktober/SOURCE.json` records the download URL and SHA-256 hashes.

The import includes the selected candy model and its license. The complete downloaded archive remains in the ignored local `Builds/source-assets` folder.

## Adaptation

The original geometry is centered and fitted to 1.15 units in length before the game's existing ground/hand/projectile size factors are applied. Its original colored and white material regions are saved as two meshes. The colored renderer retains the project's `Sweet` name so existing hue painting works; the white wrapper region remains unchanged during a color swap. Pink, blue and gold use consistent emission to remain readable in night lighting. The model uses material colors.

- Prefab: `Assets/Resources/Props/KayKitCandy.prefab`
- Derived meshes: `Assets/Resources/Meshes/KayKitCandyBody.asset` and `KayKitCandyStripes.asset`
- White wrapper material: `Assets/Resources/Materials/KayKitCandyStripe.mat`
- Colored materials: existing `Candy`, `CandyBlue` and `CandyGold`

`DreamArt.CandyVisual` uses this prefab for new candy, including tutorial refill, boss supplies, reinforcement drops and thrown projectiles. The three saved scenes also contain the imported appearance on their existing pickups and held-candy containers. The visual replacement retains the original container references, positions, scales, pickup/ring components, physics and gameplay settings. Prior saved and open-scene states are backed up under the local path recorded in `docs/qa/kaykit-candy/placement-audit.json`.

## Editing in Unity

To place another **interactive** sweet, duplicate an existing `Collectible dream candy` object in a gameplay scene. Its `CandyPickup` component controls the hue at runtime. The `KayKitCandy` prefab contains appearance only; dragging it alone into a map creates decoration without pickup behavior.

The editor menu **Lost Dream → Art → Build KayKit candy prefab** rebuilds the derived meshes and prefab from the retained source model. **Replace saved candy with KayKit artwork** replaces legacy visual children in saved chapters without regenerating maps; it preserves loaded edits and backs up scenes first. Existing imported candy is skipped so rerunning does not duplicate it.

Current Unity source and scenes are updated. Previously exported applications and ZIPs remain separate snapshots until explicitly rebuilt. Validation records are in `docs/qa/kaykit-candy`.
