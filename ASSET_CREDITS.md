# Asset provenance

The player is **Timmy** from [Mixamo](https://www.mixamo.com/), with Mixamo humanoid animation clips. Flying enemies use the **Ultimate Monsters** pack by [Quaternius](https://quaternius.com/packs/ultimatemonsters.html), licensed **CC0 1.0**. Environments and the pumpkin boss are procedural.

| Asset | Source | Implementation |
| --- | --- | --- |
| Dragon, Dragon_Evolved, Ghost, Armabee and Demon | Quaternius, Ultimate Monsters; CC0 1.0 | Original FBX rigs, animation clips, and color atlas under `Assets/ThirdParty/Quaternius/UltimateMonsters`; normalized prefabs under `Assets/Resources/Monsters` |
| Timmy player | Adobe Mixamo character; royalty-free project use under Mixamo terms | Original FBX and extracted textures in `Assets/ThirdParty/Mixamo/Timmy`; humanoid prefab, material and controller in `Assets/Resources/Characters` |
| Pumpkin boss, castle, islands, round jumping platforms, vegetation, bow, candy | Original procedural geometry authored for this project with Codex assistance | `DreamArt.cs`, `DreamWorld.cs`, `PrototypeBuilder.cs`; Unity primitives and generated meshes |
| Materials | Original project palette | `PrototypeBuilder.MakeMaterials`; `Assets/Resources/Materials` |
| Enemy flight, attack windup, death | Quaternius authored animation clips; CC0 1.0 | Animation playback driven by the enemy state machine |
| Timmy idle, running, bow aiming, throwing | Adobe Mixamo authored humanoid animation | `Idle`, `Running`, `Standing Aim Idle 01`, `Throw`; locomotion blend, upper-body mask and prop following are project adaptations |
| Jump takeoff/airborne/landing poses, forward bow pose, candy carrying pose, floating, boss squash | Original procedural animation adaptations | Chest/head alignment and two-bone arm solve over Mixamo bow aiming, hand target adjustment in `TimmyAnimation.cs`; environment and boss scripts |
| Event audio and dream ambience | Original deterministic synthesized PCM audio generated for this project with Codex assistance; no commercial samples or recordings | `Assets/Resources/Audio`; 22,050 Hz mono WAV |
| Interface font | Unity bundled `LegacyRuntime.ttf` | Unity built-in font resource |
| Engine and primitive meshes | Unity | Governed by Unity's terms; engine binaries are excluded from this source repository |

The game develops the user's three-level dream/lantern/dragon/pumpkin-boss concept. Tutorial seals, dash immunity, enemy timings, checkpoint healing, and visual treatment are prototype design choices.

## Quaternius acquisition and adaptation

Acquired 2026-10-05. Individual downloads from the creator's Google Drive folder returned quota-exceeded responses. Selected files were extracted from a [publicly redistributed copy of the pack](https://ferretforge.fr/lucastucious/Chatboat-Assets/src/branch/kenney-packs/Ultimate%20Monsters-20250326T085816Z-001.zip). The archive's SHA-256 was checked against its Git LFS pointer. Original paths, per-file hashes, official source, and mirror are recorded in `Assets/ThirdParty/Quaternius/UltimateMonsters/SOURCE.json`.

The supplied `License.txt` is preserved exactly. Its heading says “Ultimate Platformer Pack,” while its license section states CC0 1.0; the creator's Ultimate Monsters page also labels this pack CC0. The heading was not silently corrected.

Source files are unmodified. Import settings, a Standard atlas material, scale/centering, prefabs, and animation selection are project adaptations. The artwork is credited to Quaternius, not claimed as project-generated. Five selected models and their shared atlas are included. Ghost, Armabee and Demon were added on 2026-10-07 from the same locally cached archive after verifying its recorded SHA-256; their original paths and file hashes are appended to SOURCE.json. Their species names, projectile patterns and dive behavior are project gameplay adaptations. The official Ultimate Monsters page was rechecked and labels the pack CC0.

## Mixamo acquisition and adaptation

Downloaded from the official Mixamo interface on 2026-10-05 using the user's signed-in session. Timmy was exported as FBX for Unity in T-pose with skin; the four animations were exported without skin at 30 FPS with no keyframe reduction. Running uses the In Place option. Original FBX bytes are preserved, with SHA-256 hashes and download settings in `Assets/ThirdParty/Mixamo/Timmy/SOURCE.json`.

[Adobe's Mixamo FAQ](https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html) permits royalty-free use of characters and animations in personal, commercial and nonprofit projects including video games. These assets are **not CC0**. The model and animations are credited to Mixamo and are not claimed as project-authored artwork. [Adobe's distribution FAQ](https://community.adobe.com/questions-696/mixamo-faq-licensing-royalties-ownership-eula-and-tos-589400?lang=en) excludes distribution of raw character and animation files to non-team members. The public repository therefore omits Mixamo FBX files and extracted PNG textures while retaining import metadata, project integration code and source records. Local assets are preserved. See [restoration instructions](docs/MIXAMO_SETUP.md) for a fresh clone.

Unity adaptations include extracted textures, a Standard material, a 2.1-unit character fit, Humanoid avatar reuse, baked root motion, idle/run blending, an upper-body mask for aiming/throwing, and props following the animated hands. Since v0.6, procedural Humanoid bone poses provide takeoff, rising/falling leg tuck and landing compression, with bow/throw actions taking priority on the upper body. These jump poses are project-authored adaptations over the Mixamo rig, not an additional downloaded Mixamo clip. Since v0.5, a procedural chest/head alignment and two-bone arm solve put the left hand in front along the shot direction and the right hand beside the face; the bow string follows the right hand and arrows originate at the bow grip. Player movement, collision, damage and projectiles remain controlled by the prototype's gameplay scripts. Candy launches immediately on input while the throw pose follows; this prototype does not simulate a full draw-and-release archery cycle.
