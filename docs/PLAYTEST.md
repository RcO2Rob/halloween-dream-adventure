# Prototype validation and playtest log

## Local reticle and arrow alignment correction

The previous fixed-reticle path retained two behaviors from free-cursor aiming: ordinary ground/wall hits were shifted upward by 1.35 units, and nearby enemies/runes or the boss could silently replace the ray hit with a different point. The player also aimed/fired in Update before the follow camera moved in LateUpdate, and new arrows inherited world-forward rotation until their first Update. These cause distinct surface, turning and first-frame visual errors.

The corrected aim is the exact first solid surface under the central ray, with a 120-unit forward sky fallback. Aim and projectile queries consistently ignore trigger-only volumes. Camera evaluation runs first, then the humanoid/procedural pose, then final aim and queued attacks. The bow grip launches the arrow directly, the bow pitches toward the chosen point, and the arrow's visible axis is initialized from its velocity before rendering. Active bow posing follows current aim between repeat shots. The bow-to-target path converges on the reticle surface while actual nearby cover can still block the muzzle.

The full three-chapter regression passed **452 functional integration checks**, including actual enemy hits, matching-candy boss defeat, the enlarged arena, head breakup, celebration music, continuous idle/fireworks and retry cleanup.

The focused isolated signed macOS arm64 player passed 24 checks: ordinary floor/wall points match the reticle, real queued arrows hit small physical targets at 5 and 18 units, first-frame bow/grip/arrow direction agrees, an off-axis rune does not redirect the aim, near-bow cover blocks an otherwise camera-visible target, and same-frame 90-degree turn/zoom and moving jump shots use the updated view and grip. World aim projects within 0.1 pixels of the center in these controlled cases. The tests use the shared input-request queue and real controller/projectiles; they do not emulate physical desktop mouse clicks. Independent human mouse-feel review remains pending.

All three saved maps, the enlarged courtyard and the previous victory audio/assets are preserved. Current Unity runtime scripts are updated; the earlier packaged app/ZIP remain separate snapshots. Evidence is retained in `docs/qa/aim-alignment`; no GitHub push was requested.

## Local enlarged boss arena and victory sequence

The saved castle courtyard is 42 by 38, enlarged from 30 by 28 with its front landing edge fixed at Z=-2. Existing courtyard geometry, walls, tower positions, perimeter props, candy pickups and supply points are adapted in place; tower/prop sizes and the approach island/jump route remain unchanged. The courtyard's side fences are rebuilt at the new edges. The two earlier chapter scenes remain byte-identical. The live editor operation backs up both the saved castle scene and its loaded state, then verifies 663 unrelated components retain their signatures. Its map audit records the backup path. New serialized boss bounds constrain bombs and charge endpoints to the larger usable space.

Victory partitions the actual pumpkin head and crown surfaces into at most 64 moving mesh fragments, disables the intact artwork/collision, and arcs the fragments outward before shrinking and removing them after 3.3 seconds. Debris has no colliders or rigidbodies and does not consume gameplay randomness. A dedicated `dream_celebration` score crossfades in using the existing two-source mixer: C major, 128 BPM, 30 seconds, synthesized bells/plucked arpeggios and light drums. Its original source/hash record is retained; a short loop-seam envelope brings both endpoint samples to zero. The previous three WAV files are unchanged.

The old victory path stopped updating the motor while leaving its animation parameters and procedural attack/jump state intact. The new path cancels running/dashing, clears attack and carrying poses, restores blink-hidden renderers, transitions the humanoid controller to idle/Relax, and lets an airborne hero settle. The animator uses unscaled time during victory. A baked-mesh comparison verifies visible idle deformation, and another check verifies animation advances while gameplay time is frozen.

The final full three-chapter regression passed **437 functional integration checks**, including real color-matched candy hits through boss defeat, the two reinforcement waves, bomb/charge damage and avoidance, the entrance jump route, fireworks, ongoing idle motion and retry cleanup.

The final focused ending run passed 33 checks on an isolated signed macOS arm64 player using the exact live-edited saved map and final audio. It checks real ground support in newly added areas, the unchanged approach and landing edge, actual moving model fragments, celebration music with one surviving looping source, cleared attack states, idle mesh deformation, unscaled idle/fireworks, supported particles, bounded cleanup and retry restoring the intact boss. This is automated evidence and assistant visual inspection; independent human playtesting remains pending. Evidence is retained in `docs/qa/expanded-boss-ending`. No GitHub push or replacement of the earlier packaged app/ZIP is part of this change.

## Local mouse-follow shoulder camera

Moving the mouse directly rotates the third-person view without holding an orbit button. The shoulder pivot keeps Timmy to the left of a fixed central reticle. The actual player aiming point and HUD share the same screen coordinate, including existing airborne-target assistance. Both bow arrows and candy use that aim point. Upward pitch supports flying targets, pitch limits prevent flipping, and yaw wraps after long rotations. Empty-sky aim follows the viewing ray; boss assistance requires a visible forward target with a clear line of fire.

Playing captures/hides the cursor; pause, menus, defeat, chapter completion and victory release it. Switching away pauses the game. Resume waits for the menu click to be released before accepting mouse attacks and ignores mouse attacks while the application lacks focus. Boss framing retains collision protection and distance adjustment while preserving the player's chosen yaw and pitch indefinitely. Movement remains relative to the view. Scroll zoom, F reset, jumping, interaction and the two attack buttons retain their controls.

The final isolated signed macOS arm64 player passed **422 full integration checks** through three-chapter victory, including jumping, the new candy art, reinforcement waves, color matching, charge damage, castle corner visibility, music, fireworks and scene-reload cleanup.

The 35-check focused run verifies rendered rotation from mouse-axis deltas, framing clearance, pitch/yaw limits, movement, zoom and obstacle recovery, cursor state transitions, focus-loss handling, click-release protection, central-ray target selection, real arrow and candy impacts, and unchanged angles after more than four idle seconds in the castle. The automation injects mouse-axis deltas through the same look-input method and calls gameplay attack methods; it does not emulate physical desktop mouse motion or clicks. Focus-loss delivery is simulated, and cursor readiness is evaluated against actual application focus so background automation does not assert that inactive windows should accept attacks. Human mouse-feel validation remains pending.

The full-run victory assertions originally sampled audio gain after 1.7 seconds and fireworks after fixed delays. Two saved failed runs reached actual boss victory before failing these effect snapshots. The existing fireworks-only path passed 36 checks without changing the effect. The final runner waits for actual fade completion within four seconds and visible opening bursts within four seconds. Frozen-time burst advancement allows 4.5 seconds, covering the existing late launch interval of up to 2.6 seconds plus up to 1.15 seconds of rocket flight. Production music and fireworks code remain unchanged.

The initial render showed the reticle over the hero, prompting the shoulder offset and a separate screen-space clearance check. An initial resume assertion incorrectly demanded active-window input readiness from a background test window; the final assertion checks native cursor capture separately from the focus guard. Iterations and final records are retained in `docs/qa/mouse-look`. All changes are in runtime scripts and documentation; saved maps remain unchanged. The previous packaged app and ZIP remain earlier snapshots.

## Local KayKit candy artwork

The selected `candyA.fbx` from Kay Lousberg's CC0 legacy Spooktober 1.1 pack replaces eleven saved visual containers: eight ground pickups and three held-candy props. Runtime candy generation uses the same prefab for refills, boss supplies, reinforcement drops and thrown projectiles. The original white material region stays separate from the colored body, so pink, blue and gold matching remains clear. The original raw model, verbatim license and source hashes are retained. Derived meshes have a centered pivot and normalized size; the game applies the previous ground, held and projectile size factors.

The isolated signed macOS arm64 player passed 24 focused artwork/color checks and 406 full functional checks through three-chapter victory. Tests verify the imported mesh in saved ground/held props and real thrown objects, separate white and colored material regions, readable emission in all three hues, real E pickups/exchanges, tutorial refill, wrong-color rejection and matching-color boss damage. Final three-color preview, held close-up and blue boss-match capture were visually inspected. The first preview exposed a pre-existing missing emission keyword on pink; the common material builder now initializes all three hues consistently.

The live-editor migration backs up saved files and loaded scene edits, replacing only candy visual descendants while preserving their containers and all other evaluated transforms, component settings and references. The exact saved maps and generated assets match the validation build. Records are in `docs/qa/kaykit-candy`; source and editing guidance are in `docs/KAYKIT_CANDY.md`. This is automated evidence and assistant visual inspection, not independent human playtesting. Current Unity assets and scenes are updated; earlier exported app/ZIP snapshots were not replaced, and no GitHub push was requested.

## Local jump arm and wrist correction

The old jump used fixed character-space hand heights near Timmy's shoulders, producing a raised-hand silhouette on his short torso. It also inherited wrist orientation from the underlying animation. The correction derives hand targets from evaluated shoulders and arm lengths, lowers the hands beside the body with a soft elbow bend, and varies the pose across takeoff, descent and landing. Wrist orientation uses the rig's knuckle axes so fingers follow the forearm without assuming mirrored Mixamo axes. Bow aiming blends over the jump arms, throwing retains priority, and the candy-carry solver owns the right arm.

The final isolated signed macOS arm64 player passed 76 focused jump/character checks and 390 full integration checks through three-chapter victory. Independent geometry checks keep both free hands below the shoulders and prevent a sharply bent wrist during rising and falling. The focused run captures front, side and behind views, carrying candy, shooting and throwing in the air, and a return to ordinary locomotion. These final captures and the full-run rising screenshot were visually inspected. An initial fixed-height revision failed the shoulder-clearance check; a later proportional pose was visually too far forward, so the final pose lowers the hands beside the body. Those iterations are retained separately.

Evidence is in `docs/qa/jump-hands`. Jump physics, ceiling collision, platform routes, attacks, three saved maps and the victory-fireworks flow remain verified. This is a project-authored procedural adaptation, with no new downloaded animation. It is automated validation and assistant visual inspection; independent human playtesting remains pending. Current Unity scripts are updated; the previously delivered application and ZIP remain unchanged, and no GitHub push was requested.

## Local victory fireworks

Defeating the Pumpkin King starts a scene-owned particle show behind the castle. Rising rockets burst into gold, pink, teal and violet sparks with fading trails. The camera eases out to show the castle and sky. Victory text, restart and chapter selection occupy the lower screen; battle HUD panels are omitted during victory. The existing victory sound and battle-music fade remain. The effect uses unscaled time and a fixed 1,800-particle maximum, with a slower cadence after twelve seconds. Repeated victory notifications do not duplicate the show; scene reload removes its particles and material.

The isolated signed macOS arm64 player passed 382 full integration checks through actual matching-candy boss defeat, including the previously added side-fence maps. A focused 19-check run also verified that fireworks are absent during normal castle play, render with the included shader, continue beyond the default particle-system duration, progress with frozen gameplay time, and disappear on retry and return to the menu. Final focused and actual-victory screenshots were visually inspected after increasing the initially faint sparks and trails. This is automated evidence and assistant visual inspection, not independent human playtesting.

Records are in `docs/qa/victory-fireworks`. All three saved scene files remain byte-identical to the fenced-map snapshot at the start of this change. Runtime scripts and the original glow shader match the tested build. The Unity project is updated; the previously delivered v0.12 app and ZIP were not replaced, and no GitHub push was requested.

## Local v0.12: owner-selected start-screen composition

The start screen uses the selected `characters-and-props.png` render as a static backdrop. The source PNG is copied byte-for-byte with a provenance record. The entire image fits the window without cropping or stretching. Titles, a primary start button, three chapter buttons, control hints and a sound toggle are overlaid. Enter still begins the tutorial; starting or selecting a chapter removes the backdrop. Existing menu music continues.

The signed macOS arm64 application passed 373 automated integration checks through three-chapter victory after a normal macOS launch. A focused eight-check run also captured the transition into gameplay. Menu screenshots at 1440×900, 1920×1080 and 1280×960 were visually inspected: controls remain visible and the selected scene composition is intact. A source-sky color probe matches exactly at all three sizes after replacing an unsuitable IMGUI texture path with a camera-space canvas. This probe checks color transfer at the sampled sky, not full-image pixel equality.

All three saved chapter scenes are byte-identical to the v0.11 files; quality settings and the enabled-scene list are unchanged. The copied app matches the tested bundle in file hashes, modes and symlink targets. Reports, screenshots, filtered logs, packaging checks and the preservation audit are in `docs/qa/v0.12.0`. The previous v0.11 app and ZIP are retained locally. No GitHub push was performed. This is automated evidence and assistant visual inspection; independent human playtesting remains pending.

## Local v0.11: Poly Halloween artwork

The owner approved the independent art-study preview before this iteration. The saved tutorial, gardens and castle now use imported decorative props and unlit/lit pumpkin models. Original object records, transforms, active states, collider parameters, camera/light settings and gameplay values are retained; some previously implicit C# defaults are now serialized explicitly by Unity. The [saved-file audit](qa/v0.11.0/serialized-layout-audit.json) records these additions, while the editor-side audit compares gameplay and physics components before and after dressing.

The macOS arm64 build passes 366 functional checks through three-chapter victory. The 27 added checks cover scenery presence, disabled decorative collision, exclusion from targeting raycasts, and each of the six lanterns' initial and post-interaction imported appearance. Existing music, jumping, real-projectile combat, color matching, two reinforcement waves, retry and cleanup checks remain. The final bundle is ad-hoc signed for local play, passes deep signature verification, and passed the same 366 checks after launch through macOS Application Services. Future macOS exports apply and verify this signature in `PrototypeBuilder`. Reports and screenshots are retained under `docs/qa/v0.11.0`. This is automated integration evidence and assistant visual inspection; independent human playtesting remains pending.

## Evidence levels

Scene validation checks saved references, missing scripts, materials, hidden-road colliders, and checkpoint floors. Runtime integration uses the exported player with real E interactions, projectiles, bridge movement, falling recovery, gate triggers, chapter transitions, bomb damage, dash immunity, and boss victory. Controlled teleports and accelerated time make this reproducible. This does not establish first-time player comprehension or enjoyment.

The opt-in runner is `Assets/Scripts/PrototypeSmoke.cs`. The **v0.9 macOS arm64** development build passed **283 runtime checks** on 2026-10-07. The rendered player completes all three chapters with real interactions, jumps, projectiles and boss victory, using controlled teleports and a mixture of normal and accelerated time. No independent human playtest has been performed.

Candy matching is introduced as pink, then blue, then gold before the half-health charge. Checks cover actual E pickups/exchanges, conservation of candy on exchange, held/projectile hue, a real wrong-color projectile rejected without damage or shortening the open window, mismatch feedback, pause, two reachable required-color sweets at each hue, fixed hue throughout a charge/recovery cycle, and six real matching projectile hits through victory. New Ghost, Armabee and Demon checks verify visible atlas-textured skinned models, actual animated mesh deformation, attack warning and pause, one slow blue orb, three separated gold fan shots, physical locked-direction diving, a safe sideways dodge, and exactly one contact hit. All five original garden encounter locations, health values and gate references are retained.

The existing half-health charge checks remain: visible warning, locked direction, shell protection, actual movement, exactly one contact damage point, fixed stone dais, safe endpoint, exposed face toward the player, return home, bomb/charge sequencing, and warning cleanup at victory. Contact/avoidance testing runs at normal speed so accelerated screenshot waits cannot pass the warning before the test player is positioned. The [charge warning](qa/03-charge-warning.png), [recovery](qa/03-charge-recovery.png), [blue matching](qa/03-color-blue.png), [gold matching](qa/03-color-gold.png), [mismatch feedback](qa/03-color-mismatch.png), [ghost](qa/02-enemy-ghost.png), [bee](qa/02-enemy-armabee.png) and [demon](qa/02-enemy-demon.png) were visually inspected in the tested build or the same gameplay build's focused retest. The recovery capture keeps Timmy and the colored weak point visible; the ordinary post-hit blink remains part of gameplay. Required hues and shell state have separate text cues; world candy labels are small and appear only nearby with a clear camera sightline in the boss arena. Castle roof avoidance now protects the player's sightline as well as the camera's focus ray.

Regression checks for jumping platforms, tutorial candy supply, movement, character animation, camera, flying enemies and chapter progression also pass. Each chapter contains one lantern-activated round-platform crossing. The runner teleports only to the route entry and uses the actual jump controller and movement steering to land on each platform and then the destination island: 3 tutorial hops, 4 garden hops and 3 castle hops. It verifies flat mesh colliders, a missing continuous bridge floor, real fallable gaps, no fall damage on successful crossings, and tutorial checkpoint recovery after a missed landing. A separate walking attempt follows the tutorial platform centers without jumping and falls, confirming that jumping is necessary. The test uses controlled teleports and accelerated time and does not establish first-time player understanding, keyboard feel or enjoyment.

The rendered [tutorial route](qa/01-round-platform-route.png), [garden route](qa/02-round-platform-route.png), [castle route](qa/03-round-platform-route.png), and chapter 2/3 airborne captures were visually inspected. Cyan rims outline the landing areas, gold dots mark their centers, and signs introduce Space. Tutorial platforms are 2.2 units wide with a gentle alternating offset; garden platforms are 1.9 units wide with a raised middle landing; castle platforms are 1.6 units wide and rise from 0.4 to 0.8 units. The tutorial airborne screenshot falls during the existing post-fall invulnerability blink, so the overview is used for that route's visual evidence.

An initial tutorial layout could be walked across. The walking regression exposed this; narrowing and staggering the platforms increased the real gap while retaining a comfortable jump distance. The failed pre-tuning report and screenshot are retained under `qa/iterations/platform-gap-before-tuning`.

The [tutorial refill screenshot](qa/01-tutorial-candy-refill.png) was visually inspected: both replenished candies appear at their original spots, and the two-line HUD instruction states that candy refills every three seconds. The timer now replenishes each empty tutorial spot independently rather than waiting for all lanterns, no held candy and zero global pickups.

All three saved scenes passed validation. The build used an isolated project copy, preserving the open Unity editor. The tutorial and castle saved scenes remain byte-identical to v0.8; only three garden model subtrees/animation references, enemy kind fields, warning materials and new default candy-hue fields changed. The [serialized object audit](qa/scene-preservation-audit.json) verifies preservation of unrelated records, including enemy positions/health/counts, islands, lanterns, bridges, jumping platforms and gates. Previous maps are backed up in `docs/map-backups/v0.8.0`. Stone-base and castle-roof collision repairs happen at runtime and do not rewrite the castle map. Source files and the entire copied app bundle match the tested build. The [check list](qa/integration-results.json), [build record](qa/build-validation.json), runtime log and screenshots are saved in `docs/qa`.

The earlier v0.7 platform migration replaced only the first tutorial connection, final garden connection and castle entrance. Pre-migration maps are backed up under `docs/map-backups/v0.6.1`; its [serialized object audit](qa/v0.7.0/scene-preservation-audit.json) confirms preservation of unrelated scene records. Earlier v0.1 (72 checks), v0.2 (82 checks), v0.3 (97 checks), v0.4 (107 checks), v0.5 (118 checks), v0.6.0 (141 checks), v0.6.1 (165 checks), v0.7.0 (208 checks) and v0.8.0 (228 checks) results are retained. No independent human playtest has been performed. Windows/Linux and Intel Mac exports have not been tested.

The [rising jump](qa/00-jump-rising.png), [falling jump](qa/00-jump-falling.png) and [landing compression](qa/00-jump-landing.png) were visually inspected in the rendered build. These are project-authored procedural Humanoid poses layered over the existing Mixamo rig. Space now triggers a single grounded jump, with gravity and ceiling collision; Shift retains the grounded dodge. Jump height defaults to 2.2 units. Upper-body bow and throw actions take priority while the legs keep the airborne pose.

The corrected bow pose was visually inspected from [behind](qa/00-timmy-bow.png) and [in front](qa/00-bow-forward-front.png). The left hand reaches in front along the shot direction, the right hand draws at the cheek, and the bow curve's grip sits in the left palm.

The [normal follow view](qa/00-perspective-follow.png), [dragon encounter](qa/01-quaternius-dragon.png), and [boss arena](qa/03-perspective-arena.png) were visually inspected from the rendered player. Timmy's carrying/aiming closeups temporarily zoom the camera for inspection. Normal play uses a 58-degree perspective view, default 25-degree pitch and 10.5-unit distance, with adjustable orbit/zoom and movement lookahead. Boss framing pulls back and faces the pumpkin, with a temporary manual override. The bow aim is a masked animation rather than a complete arrow draw/release cycle, and thrown candy launches immediately on input. These are prototype animation limits. Solid colliders stop the camera; decorative canopy meshes do not have collision and can still occlude at some manual angles.

## Implementation iterations

| Observation | Change | Reason |
| --- | --- | --- |
| Missing roads could be only cosmetic if collision stayed active | Disable floor and colliders until lantern activation | Lighting meaningfully changes traversal |
| Falls could force an entire chapter restart | Safe lantern checkpoints; one-point fall damage | Keep learning recoverable |
| Candy could be unfamiliar when entering the boss fight | Tutorial candy pickup and pink breakable seal | Teach inputs before adding bombs |
| Unconstrained random candy could be unreachable | Randomize among safe courtyard points; replenish four | Maintain reachable supply |
| Elevated camera makes airborne targets difficult to click | Small line-of-sight-aware aim assist | Improve early combat usability |
| Pause would destroy in-flight arrows | Freeze projectiles while paused | Preserve combat on resume |
| Shorter aspect ratios could overlap title/chapter controls | Separate title sizes; scale HUD by both dimensions | Keep controls visible |
| Tutorial candy supply should support unlimited retries after missed throws | Every three seconds, refill each empty final-island candy spot until the seal clears; held candy, unlit lanterns and unrelated pickups do not block it | Prevent exhausted teaching supplies from blocking chapter progression |
| Imported FBX bind-pose bounds were 100 times the evaluated animation units; enemies looked invisible despite passing combat checks | Normalize in authored rig units with a fit transform outside the animation root; add normal/evolved visual-size and collider-centering checks | Verify visible artwork as well as functional collision |
| User requested Mixamo Timmy instead of the procedural player | Import the original model/textures and four Humanoid clips; blend locomotion, mask upper-body actions, follow animated hands for bow/candy and disable root motion | Preserve existing movement and combat while replacing the visible character |
| User requested a camera closer to It Takes Two | Replace overhead orthographic projection with a lower perspective follow view; add movement lookahead, orbit/zoom, obstruction checks and adaptive boss framing | Show Timmy more clearly and give traversal visible depth while preserving aim and throw inputs |
| Source bow clip extended the left hand sideways relative to the gameplay heading | Square the chest/head, solve both arms toward forward/face targets, align the bow grip and drawing string, and launch arrows from the grip | Match the requested forward bow pose across turns, movement and upward aim |
| User requested Space for jumping and a visible jump animation | Add grounded vertical motion, ceiling collision and takeoff/airborne/landing Humanoid poses; move dodge to Shift | Give traversal an animated jump while retaining combat and map layouts |
| User requested jumping connections in all three chapters | Replace one bridge section per chapter with lantern-activated round platforms, progressively smaller and raised | Teach landing accuracy, then reuse it in exploration and at the castle entrance |
| The first circular layout could still be walked across | Narrow and stagger the tutorial platforms and verify a no-jump traversal attempt falls | Ensure the new connection actually teaches jumping |

| User requested a forward charge at half boss health | Add a locked ground lane, warning, swept contact damage and exposed recovery followed by return | Give the second half a movement challenge and a predictable counterattack window |
| Initial recovery view showed the back of the head and the player was in the normal post-hit blink | Turn the exposed face toward the player; capture after the blink | Make the weak point and player visible for visual QA |

| User requested matching-color candy and more enemy species | Add gradually unlocked pink/blue/gold sweets with swapping, readable names/symbols and matching supplies; replace three garden models with Ghost, Armabee and Demon | Combine a learned selection rule with half-health movement pressure, and vary the existing five encounters |
| A real wrong-color projectile hit the stone base at y=2.50 instead of reaching the boss | Replace the thin cylinder's oversized capsule with its actual flat mesh collider at runtime | Remove invisible shielding while keeping the stone and saved map fixed |
| Blue/gold pickup views near castle corners were covered by roof geometry | Give roofs mesh collision and check both focus and player sightlines for arena camera obstruction | Keep Timmy and carried candy visible when collecting at corners |
| Accelerated screenshots could let a half-health charge begin before the contact test placed the player in its lane | Run the charge contact/avoidance subroutine at normal speed and assert setup before impact | Test actual swept contact without confusing a missed test setup with gameplay behavior |

These are implementation and design-review observations, not invented participant feedback.

## v0.10 owner feedback and implementation

The owner described the overall prototype as good and requested two additions: two reinforcement summons during the castle boss fight, with the boss attacking concurrently, and background music to address the monotonous audio. This is owner feedback, not an independent first-time participant study.

The implementation adds one dragon and one ghost at awakening, then one evolved dragon and one diving demon at half health. Two-second portal warnings precede spawning. Imported enemy models, their existing attack patterns, real bow collisions and the boss's normal bomb/charge state machine remain active. Summoned enemies drop the current matching candy on real ground outside the boss footprint and at least 2.5 units from other sweets. Three original melodic scores crossfade between exploration, battle and half-health battle. Pause freezes music position and summon warnings; retry resets waves; defeat/victory removes reinforcements, pending portals and hostile projectiles. All saved scene files remain unchanged.

The QA iterations retained under `qa/iterations/v0.10.0-*` include failures and repairs. Reward placement was strengthened for a moving boss and repeated wave locations. Cursor selection was adjusted so a nearby monster takes priority over boss aim assistance. The traversal regression now freezes enemy attacks during its fall-only checks; species combat is tested separately with actual attacks. The boss regression waits for enough time in a real vulnerable window to pick up candy, capture a screenshot and launch a projectile, and removes its deliberately injected test sweet after the color-swap checks. These test changes do not extend the game's vulnerability windows or disable enemies in normal play.

The final v0.10 macOS arm64 build passed 339 functional integration checks through all three chapters and boss victory. All 247 packaged app entries matched the tested build, and ZIP contents matched the same manifest. The final report is `qa/integration-results.json`, with build/package evidence in `qa/build-validation.json` and a separate saved-map hash audit. Background volume zero/restore, mute persistence, pause sample position, stage transitions and victory fade-out were checked; no independent listener assessment was performed.

For the next owner playtest, check whether the two simultaneous threats feel manageable, whether the bow remains easy to aim while the boss moves, and whether music stays below warning/shot sounds. The compositions and their synthesis source are included; automated import/playback checks do not establish a listener's assessment of musical quality.

## First independent playtest

Ask a person unfamiliar with the project to play from the title screen with only the game's own instructions. Observe before explaining. Record:

1. Time to first lantern activation; whether the restored road is noticed.
2. Whether the first dragon can be aimed at and defeated; misses versus hits.
3. Whether E pickup and right-click candy throwing are discovered.
4. Whether chapter 2's route and exit conditions are understood.
5. Whether enemy attack hints, bomb circles and the charge lane communicate danger; whether SHELL OPEN and STUNNED communicate a counterattack opportunity independently of the required candy color.
6. Falls, deaths, completion time, camera/occlusion and interaction problems.
7. Whether candy colors, wrong-color feedback and E swapping are understood.
8. Player answers: What is the lantern for? When can the boss take damage? What was frustrating?

Record observations, make focused changes, and repeat the troublesome section. Preserve before/after evidence. Do not substitute automated checks for human playtests.

| Date / participant | Observation | Change | Retest |
| --- | --- | --- | --- |
| Pending | No independent human test yet | — | — |

## Expanded garden: owner feedback

The owner reported that chapter two could be completed in under one minute and asked for more complexity. The saved garden now has three extra islands, three recoverable dream memories, four circular-platform routes in total, an ordered three-rune archery puzzle, a replenishing candy seal puzzle, six lanterns and eleven enemies. Existing enemy health remains two or three hits. New memory checkpoints preserve progress after falls; stage-specific HUD text identifies prerequisites and remaining guardians. The original four islands and decorative objects are preserved; two side-fence groups have openings for the new routes.

For the next owner playtest, time a first unassisted chapter-two run, note whether west/east/north signs are noticed, whether the I → II → III puzzle reset is understood, how often returning jumps cause falls, and whether guardians on both the main island and its branch can be found. A roughly four-to-six-minute first exploration is a design target, not a measured human result. Adjust travel length, hints or encounter placement based on that playtest rather than adding forced waiting or extra health.

## Final island placement follow-up

The owner requested the last floating island on the left rather than behind the portal. The existing Candy Crypt and its contents now sit west of the gate garden at (-33, 0, 74). Its round-platform connection turns west, the branch lamp/checkpoint move to the main island's left edge, both relevant side-fence openings are rebuilt, candy refill positions and enemy patrol homes follow the translation, and guidance now points west/east for the outward/return trip. The portal itself and the other six islands are retained.

## Iron exit gate: owner feedback

The owner requested replacing chapter two's portal with an iron gate, opening it only when every objective is complete, and lighting the floor as a traversal hint. The existing installed Poly Halloween gate now opens both leaves inward over 1.8 seconds. A blocker prevents walking through while closed or opening. After opening, an emissive trail and forward arrows illuminate the floor. Finishing requires a real front-to-back traversal; unlocking while standing elsewhere does not change scenes. For owner playtesting, check whether the opened silhouette and bright trail make the exit clear without explaining it aloud.

## Exit fence continuity and tutorial trees: owner feedback

The owner's screenshot showed side gaps beside the isolated garden gate. They requested enclosing that gate, using the same opening gate in chapter one, and matching the tutorial trees to chapter two. Both exit ends now have fence wings joining the island side rails, continuous side-wing collision, and rear side railings with a clear central opening. The tutorial gate keeps the original objective requirements and opens with the same floor-light cue. All eighteen tutorial tree positions now use the garden's two dead-tree models; original tree collision is preserved. Check the complete entrance silhouette and whether the new trees remain clear of the tutorial route during the next owner playtest.


## Four-chapter flow and marsh acceptance

- Walk through each unlocked exit without clicking or pressing Enter: the screen must fade fully black, load the next chapter and fade back into gameplay with mouse look restored. No completion modal should appear. Confirm this for tutorial → gardens → marsh → keep.
- In the marsh, waste every supply color, pause, then resume. All three unfinished-pot supply spots should refill within three active seconds. Wrong colors and arrows must preserve recipe progress without advancing it.
- Brew PINK → BLUE, BLUE → GOLD and GOLD → PINK → BLUE through real throws. Check that local guardians still gate their lantern, and a completed recipe dispels its local mist. Falls preserve ingredients and checkpoints; retry resets the entire chapter.
- Jump all five crossings. On the last three, stand still on the teal drifting stone and verify it carries the character. Pause should stop both platform motion and mist phases.
- Record at least three first-time human playthroughs. Target a full journey around ten minutes through exploration and varied actions. Planning estimates and accelerated automated-test duration are not human playtime measurements.
- Recheck chapter-four Boss waves, half-health charge, candy matching, breakup, fireworks, cheerful victory music and animated idle after moving the scene.

Current source and saved maps supersede the older three-chapter package. See `docs/WITCHLIGHT_MARSH.md` and `docs/qa/four-chapters` for implementation details and validation evidence.
