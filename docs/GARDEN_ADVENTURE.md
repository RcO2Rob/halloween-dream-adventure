# Shattered Gardens: three lost memories

The owner's reported completion time was under one minute. This iteration adds decisions, puzzle steps and outward/return traversal to the saved second scene. A first-exploration target of approximately 4–6 minutes is provisional; no human completion time has been measured for this revision.

| Stage | Route and challenge | Unlock |
| --- | --- | --- |
| Arrival → orchard | Light Orchard and cross the original diagonal road; clear the ember dragon and ghost | Reach the west branch lantern |
| I: Whisper Grove | Light Grove; jump across three circular platforms; defeat its ghost and fan-shot bee; E at the blue shrine | Memory I, shrine checkpoint, Terrace and Observatory lanterns |
| II: Moon Observatory | Cross Terrace; clear its evolved dragon and bee; light the east branch; jump to Observatory; clear its evolved dragon and bee; shoot numbered runes I → II → III; E at the shrine | Memory II, shrine checkpoint, Gate and Crypt lanterns |
| III: Candy Crypt | Light Gate and cross its original round platforms; defeat the garden demon; light the west branch; cross to Crypt; defeat its demon and ghost; throw candy at the pink seal; E at the shrine | Memory III, shrine checkpoint, return to the original gate |

The gate requires all three recovered memories, all six lanterns, all eleven enemies, and all four rune targets. Taking a wrong-order shot resets only the unfinished moon melody; its targets stay shootable, and arrows are unlimited. Completed melodies stay solved. Shrine and rune labels face the mouse-controlled camera, so the sequence remains readable from either side. Two Crypt candy points replenish every three seconds while their route is lit and the seal remains unsolved, including after missed throws. Pause freezes timers. Falls preserve progress, cost one health and return to the most recent lamp or memory checkpoint. Retry reloads the complete chapter.

## Scene editing

Open `Assets/Scenes/02_ShatteredGardens.unity`. New objects are grouped under `Garden adventure - three lost memories`; existing islands, props, player, camera, music and gate are retained. New islands are Whisper Grove (-44, 0, 25), Moon Observatory (31, 0, 45), and Candy Crypt (-33, 0, 74). Each route has `jumpEntry`, `jumpPlatforms` and `jumpExit` anchors. Keep jump spacing reachable and entry/exit anchors above island floors.

`GardenAdventure` stores three shrine references, the ordered rune array, the candy seal, branch lights and refill points. Each `DreamMemory` stores its guardian list and stage index. `Lantern.requiredMemories` controls its lock. If moving a shrine, its recovery checkpoint follows its transform; if moving a lamp or the Crypt floor, update the lamp's `safeCheckpoint` or controller's world-space `candyPoints`. The builder is idempotent and edits only the saved second scene, with backups of both disk and loaded scene state. It never regenerates existing islands. The old full scene generator is a historical base layout; it does not reconstruct manual edits or this expansion.

The original four islands and their scenery are preserved. Only the Orchard and Terrace fence groups are refreshed to leave side openings at the new routes. New islands retain the installed Poly Halloween trees, pumpkins, gravestones, coffins and iron side fences. No additional downloaded assets are required.

## Validation

Use standalone `--smoke-test --garden-smoke` for the targeted chapter-two walkthrough. It tests locked progression, real E interactions, actual bow/candy collisions, forward and return controller jumps, wrong-sequence recovery, partial progress across pause, memory progress across falls, replenishment after throwing away both candies, stopping replenishment after solving the seal, and a fresh chapter retry. The full smoke route also crosses the physical restored gate into chapter three and completes the boss ending.

Evidence is retained in `docs/qa/expanded-garden/`. Automated traversal establishes functional reachability, not human navigation clarity or completion time. Player testing remains needed for those judgments.

Expansion validation before the left-side relocation: 589 full-game integration checks and 33 focused ending checks passed on that saved scene revision and runtime code. See [validation evidence](qa/expanded-garden/README.md).

Layout follow-up: the final Crypt now sits beside the gate garden on its left (west), rather than behind the portal. The three existing round platforms turn west at z=76. Its shrine, seal, two enemies, two supplies and props move together; the Crypt lamp and checkpoint are on the main island's left side. The gate stays in its original location. See `docs/qa/crypt-left/` for the relocation audit and validation.

Left-side layout validation: 213 targeted second-chapter checks passed on the exact saved scene, including both-way jumping, replenishment, seal collision, physical portal completion and retry reset.

## Iron exit gate

The second chapter now uses Poly Halloween's `Gate_Graveyard` prefab at the existing exit anchor (2, 0, 81). Until all three memories, six lamps, eleven enemies and four runes are complete, both leaves remain closed, a passage collider blocks entry, and the ground trail stays dark. Completion starts a 1.8-second inward swing. Once both leaves are fully open, the blocker disables and teal stepping lights, gold directional arrows and a local glow light turn on. The player must approach from the front and walk beyond the open doorway to finish the chapter. Opening alone, or already standing behind the gate when it unlocks, does not complete the chapter. Pause freezes the animation; retry restores the closed gate and dark trail.

In Unity, the existing `Dream gate` object contains `GardenIronGate`, the imported model with its native door hinges, the closed-passage collider and `Unlocked glowing exit trail`. The old portal artwork is retained inactive under that same object. Its original position and the rest of the map stay fixed. `GardenIronGateBuilder` makes this targeted edit with disk/loaded-scene backups.

Iron gate validation: 225 targeted garden integration checks passed on the exact saved scene and current code, including closed collision, partial-progress locking, animated opening and pause, floor illumination, behind-door auto-exit prevention and actual walk-through completion. See [iron gate evidence](qa/garden-iron-gate/README.md).

## Connected exit enclosures and tutorial art

Both chapter-one and chapter-two gates now connect to the existing side rails through iron fence wings. Rear side railings preserve a clear opening aligned with the doorway. Continuous 2.8-metre side-wing colliders close gaps beside the doorway; the central opening keeps its existing objective-based animated gate behavior and illuminated trail. The second chapter's left-side Crypt route stays outside the exit enclosure and remains accessible.

The tutorial exit at (0, 0, 68) now uses the same gate and trail as the garden exit at (2, 0, 81). All eighteen tutorial tree anchors use the garden's `Tree_Scary_Dead` and `Tree_Poplar_Dead_A` prefab models. Old round-canopy renderers and earlier sparse imported-tree visuals are hidden; original trunk collision stays fixed. The new decorative meshes add no collision and remain outside targeting raycasts. The third scene is unchanged.

The targeted migration menu is `Lost Dream/Map/Enclose both exits and match tutorial tree art`. Its scene backups and preservation audit are under `docs/qa/exit-enclosures/`. In Unity the added railings are grouped under each `Dream gate/Exit perimeter enclosure`; tree artwork is under `Tutorial trees - Poly Halloween`.

Exit enclosure validation: the full three-chapter flow passed 637 checks before the final rear-sightline/mesh-axis visual corrections. The final exact-source revision passed 265 targeted checks across both enclosures, tutorial tree art and garden gate completion. See [current enclosure evidence](qa/exit-enclosures/README.md). When moving tree locations manually, move the original `Dream orchard tree` collision anchor with its matching visual anchor.
