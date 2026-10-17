# Halloween Dream Adventure

A playable four-level Unity 3D prototype, titled **Lanterns of the Lost Dream**. Play as Mixamo's **Timmy**, restore dream roads with pumpkin lanterns, shoot flying nightmares, and defeat the Pumpkin King with matching-color candy during his vulnerable phase. Flying enemies use animated Dragon, Dragon_Evolved, Ghost, Armabee and Demon models from Quaternius's Ultimate Monsters pack (CC0); stronger enemies use the evolved model.

## Current concept

The player explores a dream being consumed by nightmares. Lighting pumpkin lanterns disperses nightmare fog and reveals paths through the dream.

- **Level 1 — Tutorial:** introduce movement, lantern interactions, and basic combat.
- **Level 2 — Shattered Gardens:** explore seven floating islands and recover three dream memories. Follow the west Grove jumping route, solve the east Observatory's I → II → III bow melody, then shatter the west Crypt's candy seal. Six lanterns, four round-platform crossings and eleven guardians combine traversal, puzzles and combat.
- **Level 3 — Witchlight Marsh:** explore six mushroom islands, brew three color-ordered potions with renewable candy, dodge breathing mist and cross drifting dreamstones. Clear twelve guardians to open the castle road.
- **Level 4 — Castle boss:** dodge a pumpkin-headed boss's bombs, collect scattered candy, and throw it at the boss at the right moment.

Lanterns restore bridge collision permanently, heal one health point, and save a safe checkpoint. Falling costs one of five health points and returns you to the checkpoint. Chapters 1–2 require all lanterns, enemies and seals before the exit opens; the garden additionally requires all three dream memories. Its Terrace and Gate lanterns require memories I and II respectively. Memories save checkpoints and remain recovered after falls; crypt candy replenishes every three seconds until its seal is cleared. Tutorial candy refills at its two designated spots every three seconds until the pink seal is cleared, even when a candy is held or lanterns are unlit. Occupied spots are not duplicated, and pausing stops the timer. The boss blocks attacks until the HUD says SHELL OPEN or STUNNED; one matching-color candy hit closes the window. At half health (3/6), he unlocks a charge with a 1.6-second marked, locked-direction warning. Move sideways or dodge with Shift. After charging, he turns toward the player and exposes his face for 2.4 seconds before returning home. Later volleys end in another charge. Matching starts with pink, introduces blue after the first successful hit, and gold after the second. Required hue stays fixed throughout an attack and its vulnerable window; later cycles alternate unlocked hues. Body tint, candy labels and the HUD show PINK [O], BLUE [<>] or GOLD [*]. Wrong candy is consumed without damaging the boss or closing the window. E at a different-colored pickup exchanges the held candy with the floor candy. Refills provide at least two required-color pickups outside the boss body.

## Project status

The four playable scenes, Timmy humanoid player with idle/run/aim/throw animations, animated Quaternius enemies, procedural boss and terrain, event sounds, menu, pause, retry, chapter selection, and macOS export are implemented. This is a prototype; balance and independent human playtesting remain development work.

v0.10 adds two one-time reinforcement waves to the castle fight. Awakening calls an ember dragon and ghost; half health calls an evolved dragon and diving demon. Each wave has a two-second portal warning, and the Pumpkin King's normal attacks continue concurrently. Use the bow to defeat summoned enemies; they reward the required candy hue. Boss victory clears living summons, pending portals and hostile projectiles. These enemies are generated at runtime over actual ground, preserving all saved map layouts.

Four original synthesized melodic scores loop for exploration, boss combat, the faster half-health stage and victory. A 1.5-second crossfade connects tracks; pause/resume preserves playback position, and M mutes both music and event effects across retries. Boss victory switches to the bright C-major `dream_celebration` loop at 128 BPM; defeat and chapter-completion screens fade music out. Volume is adjustable on the `DreamAudio` component. Scores, provenance and their reproducible generators are included in this repository.

v0.11 applies polyperfect's Poly Halloween artwork to the three saved chapters: restrained tutorial scenery, graveyard details in the gardens, and castle approach and perimeter decoration. Imported lantern models switch between unlit and lit appearances using the existing lantern state. Original map transforms, collisions, jump routes, checkpoints, enemy settings and boss mechanics are preserved. Decorative prefabs have their own collision disabled and stay outside targeting raycasts. An independent [art preview](docs/POLY_HALLOWEEN_SETUP.md) is also included.

v0.12 uses the owner-selected art-study composition as the start-menu backdrop. The complete image fits the window without stretching or cropping its characters. A translucent left-side shade supports the title, start action and three chapter buttons, with a sound toggle at the upper right. Enter still starts the tutorial; the backdrop disappears during gameplay. No saved chapter layout is changed.

Local map edits after v0.12 add 142 Poly Halloween iron fence panels along the left and right edges of the nine main islands. Each saved chapter has an editable `Iron side fences` hierarchy with island and side groups. Thin solid barriers prevent ordinary sideways walking off the edge; bridge approaches remain clear, and jump-platform gaps are unchanged. Existing castle walls remain. Placement and saved-map validation are recorded in `docs/qa/side-fences`. These are Unity scene edits; the previously packaged v0.12 application and ZIP retain their original maps.

The local victory update adds a scene-owned fireworks show after the Pumpkin King is defeated. Rockets rise behind the castle and burst into gold, pink, teal and violet sparks with trails. The camera eases into a wide castle-and-sky view; victory text, restart and chapter selection sit below the fireworks. Particle counts are bounded, the effect uses real time, and retry or chapter selection removes it. This update changes runtime code and adds an original glow shader; it does not rewrite saved maps or replace the previously packaged v0.12 app.

The current local castle update enlarges the saved courtyard from 30 by 28 to 42 by 38 while retaining its original front landing edge and the complete entrance jump route. Walls, towers, graveyard props, candy positions and supply points follow the enlarged layout; only courtyard side fences are rebuilt. Bomb and charge bounds now match the usable arena. A final matching hit breaks the actual head/crown mesh surfaces into collision-free fragments that arc away and dissolve. Fireworks launch behind the wider castle while a new major-key victory loop plays. Timmy clears running and upper-body attack states, restores visibility and continues imported idle motion using unscaled animation time. The saved scene and current Unity scripts are updated; the earlier packaged app is a separate snapshot.

The local jump-pose fix replaces raised fixed-height hands with softly bent arms positioned from Timmy's evaluated shoulders and arm lengths. Knuckle-derived wrist axes keep free hands aligned with the forearms. Bow aiming blends over the jump arms, throws retain priority, and candy carrying keeps ownership of the right arm. Takeoff, falling and landing poses vary continuously. Jump height, movement physics and saved maps are unchanged; the previously packaged application is not replaced by this source edit.

The local candy-art update replaces saved ground/held sweets and runtime replenishment/projectiles with the wrapped candy from KayKit's CC0 legacy Spooktober pack. Original white wrapper regions remain while the body matches pink, blue or gold. The three hues now have consistent night-readable glow. Pickup positions, references, collision, supply timers and matching-color damage rules remain. Source, license and hashes are retained; see [candy artwork and map editing](docs/KAYKIT_CANDY.md). This updates Unity source/assets/scenes; previous application and ZIP snapshots are not replaced.

## Play

On an Apple Silicon Mac, open the local `Builds/macOS/Lanterns of the Lost Dream.app`. The v0.12 archive is `Builds/Lanterns-of-the-Lost-Dream-v0.12.0-macOS-arm64.zip`; previous archives are retained locally. Builds are excluded from Git and are not downloadable from this repository. That older package contains its original three chapters. The current four-chapter journey is available in the Unity source; the older packaged app is a separate snapshot.

This v0.12 source snapshot includes the chapter artwork integration, start screen, saved maps, project-authored code, rendered previews and validation records. Mixamo FBX files, extracted textures and Poly Halloween source assets stay local under their redistribution terms. A fresh clone requires [restoring Timmy assets](docs/MIXAMO_SETUP.md) and [Poly Halloween assets](docs/POLY_HALLOWEEN_SETUP.md) before opening Unity or playing; the existing local project already has these files.

| Input | Action |
| --- | --- |
| WASD / arrows | Move relative to camera |
| Move mouse | Rotate the third-person view; aim through the central reticle |
| Scroll wheel | Zoom camera |
| F | Reset camera |
| Left-click (hold) | Fire bow |
| E | Light lantern / pick up or exchange one candy |
| Right-click / Q | Throw carried candy |
| Space | Jump once from the ground |
| Shift | Dash with brief damage immunity |
| Esc | Pause and release mouse / resume and capture mouse |
| R | Restart current chapter |
| Enter | Begin / continue / retry |
| M | Toggle sound |

The current local camera keeps the lower perspective follow view inspired by the user's It Takes Two reference, with smooth movement lookahead, zoom and camera collision against solid scenery. Moving the mouse directly rotates the view without holding a button, with bounded vertical pitch for airborne targets. Movement remains relative to the view. The reticle stays at the screen center. Bow/candy aim uses the exact first solid surface on that ray, and sky aim follows the viewing direction. Camera and humanoid pose evaluation finish before queued shots launch from the bow grip, with bow pitch and arrow orientation aligned on the first rendered frame. Boss encounters pull the camera back while preserving the player's orbit pivot and chosen angles. Menus, pause, defeat and victory release the mouse; automatic chapter fades restore mouse look on arrival; switching away pauses play. Start/resume captures it again after the menu click is released, preventing accidental shots. F resets the camera. This update changes current Unity scripts; the earlier packaged app is a separate snapshot.

Jumping uses a 2.2-unit height with gravity, ceiling collision, and takeoff, rising, falling and landing skeletal poses. Airborne movement and bow/candy actions remain available. Jumping has no damage immunity or double jump; Shift retains the existing dash. The v0.7 maps replace one selected connection in each chapter with lantern-activated round jumping platforms: two wide tutorial platforms, three smaller staggered garden platforms with elevation changes, and two narrow raised castle-entry platforms. The other bridges, island layouts, combat, checkpoints and tutorial candy supply are retained. Platform tops have flat mesh colliders and glowing rims; the gaps have no supporting floor.

The v0.5 aiming pose extends the left bow hand forward along the shot direction, with the right hand drawing beside the face. A two-bone arm solve and chest/head alignment adapt the original Mixamo clip; the string follows the drawing hand, and arrows launch from the forward bow grip.

The v0.9 garden keeps five encounters at their original locations and replaces three enemy models with distinct species. Wisp Ghost fires one slow blue orb at 5 units/s; Fanshot Bee fires three gold projectiles 18 degrees apart; Diving Demon marks a locked lane for 1.3 seconds, dives physically at 10 units/s, then returns home. The original ember dragon and evolved dragon remain, and the tutorial retains its basic dragon. All enemy types count toward the existing gate requirement.

## Open and build

For a fresh clone, first follow [Mixamo setup](docs/MIXAMO_SETUP.md). Open this folder with **Unity 6000.6.2f1**, open `Assets/Scenes/01_FirstLight.unity`, and press Play. This uses the built-in render pipeline and legacy input; no purchased assets are needed. All four saved scenes are enabled in Build Settings.

The **Lost Dream** editor menu validates scenes and builds macOS. Regeneration is disabled when saved maps exist, protecting manual edits; use the saved-map build command. Windows/Linux exports require their respective Unity platform modules; only macOS is exported here.

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

Unity project folders `Assets`, `Packages`, and `ProjectSettings` are included, except for raw Mixamo and Poly Halloween assets. Generated caches, local builds, raw Mixamo FBX/PNG files, and the `Assets/polyperfect` source folder are excluded by `.gitignore`; their existing local copies are preserved. Historical QA records describe the build and sync status at the time of each run. The historical v0.12 records are in `docs/qa/v0.12.0`, including 373 automated integration checks, three menu aspect ratios, a menu-to-game capture, source-color probe and byte-identical saved-map audit. The earlier v0.11 art-validation records are retained. Independent human playtesting remains pending.

## Expanded second chapter

The saved `Assets/Scenes/02_ShatteredGardens.unity` contains the expanded adventure. Its new content is grouped under **Garden adventure - three lost memories**; original islands and scenery stay in place. Move the new islands, routes, shrines, rune targets and guardians in Unity as ordinary scene objects. Lanterns and shrines expose their prerequisite references in the Inspector. Update checkpoint/candy positions when moving their floors. The original scene generator is an earlier base layout: use the saved scene for editing. See [garden layout and playtest notes](docs/GARDEN_ADVENTURE.md).

The second-chapter exit uses the installed Poly Halloween iron gate. After all garden objectives are complete, its two leaves swing inward, then the floor lights mark a walk-through exit. Pause freezes opening, and retry restores the closed gate and unlit trail. See [garden editing notes](docs/GARDEN_ADVENTURE.md).

Both chapter-one and chapter-two exits now use opening iron gates with continuous fence wings joining the island side rails and a rear side railings with a clear central exit opening. The tutorial's eighteen tree positions use the same Scary Dead and Dead Poplar models as the garden. Original tree collisions and gameplay positions are retained; former round-canopy visuals are hidden.

## Four-chapter expansion

Unlocked gates now fade to black, load the next scene and fade back in automatically. Chapter three is **Witchlight Marsh**, a separate six-island adventure with three color recipes, renewable supplies, marked mist hazards, twelve guardians and moving jump platforms. The preserved Boss map is now `Assets/Scenes/04_PumpkinKeep.unity`; the new map is `Assets/Scenes/03_WitchlightMarsh.unity`. All four appear in the title menu and Build Settings. See [marsh layout, editing and pacing notes](docs/WITCHLIGHT_MARSH.md). First-time human play duration remains to be measured.

The current four-chapter journey passed **719 rendered standalone checks**, including all three automatic chapter fades, marsh recipes/hazards/traversal and the complete Boss ending. Validation scope, preserved-map hashes and screenshots are in [four-chapter QA](docs/qa/four-chapters/README.md); this does not measure human play time.
