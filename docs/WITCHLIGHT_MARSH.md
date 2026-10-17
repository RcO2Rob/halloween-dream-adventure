# Four-chapter journey and Witchlight Marsh

The saved journey now follows this order:

| Chapter | Editable scene | Main activity |
| --- | --- | --- |
| 1 | `Assets/Scenes/01_FirstLight.unity` | Learn movement, jumping, bow, lanterns and renewable candy |
| 2 | `Assets/Scenes/02_ShatteredGardens.unity` | Seven islands, three memories, moon melody and candy crypt |
| 3 | `Assets/Scenes/03_WitchlightMarsh.unity` | Six mushroom islands, three potion recipes, mist vents and drifting platforms |
| 4 | `Assets/Scenes/04_PumpkinKeep.unity` | Original expanded castle, two reinforcement waves, color-matching Boss and celebration |

Walking through an unlocked iron gate starts a 0.45-second fade to black. The next saved scene loads asynchronously behind the persistent black overlay, followed by a short black hold and a 0.55-second fade in. Movement and combat are frozen during the transition. Mouse look resumes automatically; no completion modal, click or Enter confirmation is required. Pause/retry/title selection and the final Boss victory screen retain their existing purposes.

## Third-chapter route

1. **Mushroom landing** — light the first lantern and jump across the wide glowcap stones.
2. **Glowcap grove** — brew Moonwake: **PINK → BLUE**. Clear the three guardians, then light the west crossing.
3. **Witch's workshop** — brew Starlight: **BLUE → GOLD**. Dodge the breathing mist, clear its guardians and reveal the drifting crossing north.
4. **Breathing marsh** — brew Dawnbloom: **GOLD → PINK → BLUE**. The longer recipe runs alongside marked mist vents and a third guardian group.
5. **Abandoned watchtower** — defeat the three remaining guardians, then restore the last crossing.
6. **Road to the keep** — follow the illuminated iron-gate trail into chapter four.

The potion liquid, floating recipe and next-ingredient text identify the required color. Pick up or swap candy with E, then aim at the pot and throw with Q/right-click. Arrows and wrong colors cannot advance a recipe; wrong throws preserve accepted ingredients. Each unfinished pot replenishes pink, blue and gold supply spots every three seconds, even when the player holds candy. Every supply slot also restores its designated color after a swap leaves a different color there, so occupied wrong-color candy cannot prevent replenishment. Finished pots stop replenishing their supplies. All three completed potions, twelve defeated guardians and five lit lanterns are required to unlock the exit.

Mist vents show a gold floor warning for 1.5 seconds before a two-second damaging pulse. Move outside the marked circle or jump over its low mist. Brewing the local potion permanently dispels its vents. The last three crossings each have one teal platform that drifts sideways; a grounded character rides with it. Pause freezes hazards, platforms, enemies and refill timers. Falls preserve recipe progress and lanterns while returning the player to the last safe checkpoint; retry starts a fresh chapter.

## Editing in Unity

Open the saved scenes above directly. The castle scene was renamed with its original GUID retained; its layout, enemy setup, camera and Boss properties were preserved. Chapters one and two were not regenerated. `FourChapterBuilder.Upgrade` backs up both the disk and loaded castle scene before changing only its chapter index and asset path. It creates chapter three only when that new scene does not already exist. Re-running this migration never replaces an edited marsh scene.

Chapter three has ordinary scene objects beneath **Lost Dream - Chapter 3 - Witchlight Marsh**. Each island, crossing, cauldron, lantern, guardian and mist vent can be moved in the Inspector. `WitchlightMarsh` exposes the three potions, five lights and tower guardians. Each `WitchCauldron` exposes its recipe, supply positions and guardians; `Lantern.marshStage` controls its prerequisite. `MarshMist` references the local potion and exposes its radius and cycle offset. `DriftingPlatform` exposes speed, phase and amplitude.

When moving an island, also move its checkpoint and candy supply **world-space positions**, and adjust crossing endpoints, route anchors, side-fence openings and associated guardians. Keep potion receivers clear of decorative meshes. Imported scenery has collision disabled and uses the ignored targeting layer. Existing-map regeneration is disabled; build and validate the saved scenes instead.

## Pacing and validation boundary

The design budget is approximately 1.5–2 minutes for the tutorial, 3–4 for the gardens, 3–4 for the marsh and 2–3 for the Boss (9.5–13 minutes total). These are planning estimates for first-time exploration, **not measured human play times or a minimum guarantee**. The added chapter supplies traversal, recipe decisions, hazards and combat rather than a mandatory waiting timer. Time at least three first-time human playthroughs and record active play, falls, retries and recipe confusion before tuning toward a ten-minute submission requirement.

The opt-in `--marsh-smoke` standalone mode tests real controller jumps across all five crossings, platform riding, recipe projectile collisions, wrong-color recovery, refill, mist warning/damage/pause, guardian combat, the physical gate and automatic black transition into chapter four. The full smoke mode also plays the existing tutorial, garden and Boss. Tests use controlled teleports and occasional acceleration; their duration does not measure human play time. Evidence for the current implementation is stored in `docs/qa/four-chapters`.
