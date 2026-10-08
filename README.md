# Halloween Dream Adventure

A playable three-level Unity 3D prototype, titled **Lanterns of the Lost Dream**. Play as Mixamo's **Timmy**, restore dream roads with pumpkin lanterns, shoot flying nightmares, and defeat the Pumpkin King with matching-color candy during his vulnerable phase. Flying enemies use animated Dragon, Dragon_Evolved, Ghost, Armabee and Demon models from Quaternius's Ultimate Monsters pack (CC0); stronger enemies use the evolved model.

## Current concept

The player explores a dream being consumed by nightmares. Lighting pumpkin lanterns disperses nightmare fog and reveals paths through the dream.

- **Level 1 — Tutorial:** introduce movement, lantern interactions, and basic combat.
- **Level 2 — Adventure:** combine exploration, lantern interactions, and bow-and-arrow encounters with dragons, ghosts, fan-shot bees and diving demons.
- **Level 3 — Castle boss:** dodge a pumpkin-headed boss's bombs, collect scattered candy, and throw it at the boss at the right moment.

Lanterns restore bridge collision permanently, heal one health point, and save a safe checkpoint. Falling costs one of five health points and returns you to the checkpoint. Chapters 1–2 require all lanterns, dragons, and practice seals before the exit opens. Tutorial candy refills at its two designated spots every three seconds until the pink seal is cleared, even when a candy is held or lanterns are unlit. Occupied spots are not duplicated, and pausing stops the timer. The boss blocks attacks until the HUD says SHELL OPEN or STUNNED; one matching-color candy hit closes the window. At half health (3/6), he unlocks a charge with a 1.6-second marked, locked-direction warning. Move sideways or dodge with Shift. After charging, he turns toward the player and exposes his face for 2.4 seconds before returning home. Later volleys end in another charge. Matching starts with pink, introduces blue after the first successful hit, and gold after the second. Required hue stays fixed throughout an attack and its vulnerable window; later cycles alternate unlocked hues. Body tint, candy labels and the HUD show PINK [O], BLUE [<>] or GOLD [*]. Wrong candy is consumed without damaging the boss or closing the window. E at a different-colored pickup exchanges the held candy with the floor candy. Refills provide at least two required-color pickups outside the boss body.

## Project status

The three playable scenes, Timmy humanoid player with idle/run/aim/throw animations, animated Quaternius enemies, procedural boss and environments, event sounds, menu, pause, retry, chapter selection, and macOS export are implemented. This is a prototype; balance and independent human playtesting remain development work.

## Play

On an Apple Silicon Mac, open the local `Builds/macOS/Lanterns of the Lost Dream.app`. The local archive is `Builds/Lanterns-of-the-Lost-Dream-v0.9.0-macOS-arm64.zip`; the previous v0.1–v0.8.0 archives are retained locally. Builds are excluded from Git and are not downloadable from this repository. Play all three chapters or choose a chapter on the title screen.

The v0.9 source snapshot is prepared for [the public GitHub repository](https://github.com/RcO2Rob/halloween-dream-adventure). It includes gameplay, all three saved maps, CC0 monster assets, and validation records. Mixamo FBX files and extracted textures stay local under their redistribution terms. A fresh clone requires [restoring Timmy assets](docs/MIXAMO_SETUP.md) before opening Unity or playing; the existing local project already has these files.

| Input | Action |
| --- | --- |
| WASD / arrows | Move relative to camera |
| Mouse | Aim, with light assistance for airborne targets |
| Middle-button drag / Alt + left-button drag | Orbit the third-person camera |
| Scroll wheel | Zoom camera |
| F | Reset camera |
| Left-click (hold) | Fire bow |
| E | Light lantern / pick up or exchange one candy |
| Right-click / Q | Throw carried candy |
| Space | Jump once from the ground |
| Shift | Dash with brief damage immunity |
| Esc | Pause / resume |
| R | Restart current chapter |
| Enter | Begin / continue / retry |
| M | Toggle sound |

The v0.4 camera is a lower perspective follow view inspired by the user's It Takes Two reference, with a larger character in frame, smooth movement lookahead, orbit/zoom, and camera collision against solid scenery. The boss camera follows the player and frames the pumpkin weak point, pulling back for the encounter. Manual orbit temporarily overrides boss auto-facing. Mouse aiming and right-click candy throwing retain their existing behavior; Alt-drag suppresses bow shots.

Jumping uses a 2.2-unit height with gravity, ceiling collision, and takeoff, rising, falling and landing skeletal poses. Airborne movement and bow/candy actions remain available. Jumping has no damage immunity or double jump; Shift retains the existing dash. The v0.7 maps replace one selected connection in each chapter with lantern-activated round jumping platforms: two wide tutorial platforms, three smaller staggered garden platforms with elevation changes, and two narrow raised castle-entry platforms. The other bridges, island layouts, combat, checkpoints and tutorial candy supply are retained. Platform tops have flat mesh colliders and glowing rims; the gaps have no supporting floor.

The v0.5 aiming pose extends the left bow hand forward along the shot direction, with the right hand drawing beside the face. A two-bone arm solve and chest/head alignment adapt the original Mixamo clip; the string follows the drawing hand, and arrows launch from the forward bow grip.

The v0.9 garden keeps five encounters at their original locations and replaces three enemy models with distinct species. Wisp Ghost fires one slow blue orb at 5 units/s; Fanshot Bee fires three gold projectiles 18 degrees apart; Diving Demon marks a locked lane for 1.3 seconds, dives physically at 10 units/s, then returns home. The original ember dragon and evolved dragon remain, and the tutorial retains its basic dragon. All enemy types count toward the existing gate requirement.

## Open and build

For a fresh clone, first follow [Mixamo setup](docs/MIXAMO_SETUP.md). Open this folder with **Unity 6000.6.2f1**, open `Assets/Scenes/01_FirstLight.unity`, and press Play. This uses the built-in render pipeline and legacy input; no purchased assets are needed. All three saved scenes are enabled in Build Settings.

The **Lost Dream** editor menu validates scenes and builds macOS. Its regeneration command replaces the saved scenes, so preserve manual edits first. Windows/Linux exports require their respective Unity platform modules; only macOS is exported here.

```sh
UNITY="/Applications/Unity/Hub/Editor/6000.6.2f1/Unity.app/Contents/MacOS/Unity"
"$UNITY" -batchmode -nographics -projectPath "$PWD" -executeMethod PrototypeBuilder.ValidateScenes -quit -logFile /tmp/lost-dream-validation.log
"$UNITY" -batchmode -nographics -projectPath "$PWD" -executeMethod PrototypeBuilder.BuildMac -quit -logFile /tmp/lost-dream-build.log
```

For opt-in runtime integration checks, launch the built executable with `--smoke-test --test-output /tmp/lost-dream-smoke`. It uses actual colliders, projectiles, interactions, and scene transitions, with controlled teleports and accelerated time. This is not a human playtest. Add `-batchmode -nographics` for headless execution. Normal launches never enable the test runner.

## Development notes

- [High concept and game script](docs/GAME_DESIGN.md)
- [Playtest plan and iteration log](docs/PLAYTEST.md)
- [Asset provenance](ASSET_CREDITS.md)

Unity project folders `Assets`, `Packages`, and `ProjectSettings` are included, except for raw Mixamo assets. Generated caches, local builds, and raw Mixamo FBX/PNG files are excluded by `.gitignore`; their existing local copies are preserved. Historical QA records describe the build and sync status at the time of each run. The v0.9 integration report records 283 passing automated checks; independent human playtesting remains pending.
