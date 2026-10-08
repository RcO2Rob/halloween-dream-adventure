# Prototype validation and playtest log

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
