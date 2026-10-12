# Lanterns of the Lost Dream

Game Design Document

Roucheng Ou | Project 2 | Draft 2 | October 8 2026

## Part One High Concept Document

### Game Overview

Lanterns of the Lost Dream is a short, single-player 3D action adventure made in Unity. Timmy, a dreamwalker, lights pumpkin lanterns to restore floating roads, fights flying nightmares with a bow, and uses candy against the Pumpkin King. My design builds a complete journey around three chapters: learning the actions, applying them during exploration and combat, and combining them in a boss encounter. The intended first-play duration is five to ten minutes.

### Concept Genre and Background

The game combines third-person action, platform traversal, and environmental interaction. An orbiting, adjustable camera follows Timmy and adapts its framing in the boss arena. Nightmares have broken the dream's connections; restoring light reconnects the islands and leads to the castle. Purple scenery, warm pumpkin light, ghosts, and wrapped sweets create a playful, slightly eerie Halloween setting. The story gives the journey a purpose without requiring additional reading.

### Expected Player

The intended player enjoys short action adventures and knows basic keyboard and mouse controls. No previous game or story knowledge is required. Landing rims, interaction prompts, a small aiming assist, and checkpoints support players learning 3D movement.

### Platform and Scope

The prototype uses Unity 6000.6.2f1 and three saved 3D scenes. The tested build runs on Apple Silicon macOS; required Windows and Linux releases remain pending. Input uses keyboard and mouse. There is no persistent save or controller support.

## Part Two Game Script

### What the Game Teaches

I want players to learn spatial judgment and timing by seeing what their actions change. Each mechanic begins with a simple use, then returns in a more demanding situation.

| Mechanic | Introduction | Later application |
| --- | --- | --- |
| Lantern interaction | Restore the first crossing | Find routes through the garden and castle entrance |
| Jumping | Cross wide platforms with landing rims | Reach smaller, staggered, raised landings |
| Bow aiming | Hit a stationary rune and a moving dragon | Aim while avoiding different enemy attacks |
| Candy pickup and throw | Break a pink seal | Choose a matching color and time a boss hit |
| Warning recognition | React to enemy windup | Avoid bomb areas and the charge corridor |
| Dash | Read the Shift instruction and try a dodge | Use protection while managing the cooldown |

A dedicated, low-pressure dash exercise and a clearer demonstration of the boss's color and shell conditions are proposed improvements for the next playtest.

### Controls and Core Mechanics

| Input | Action |
| --- | --- |
| WASD or arrow keys | Move relative to the camera |
| Mouse | Aim |
| Left mouse button | Fire bow; hold for repeated shots |
| E | Interact with the nearest available lantern or candy |
| Right mouse button or Q | Throw held candy |
| Space | Jump from the ground |
| Shift | Dash from the ground |
| Middle drag or Alt plus left drag | Orbit camera |
| Scroll wheel and F | Zoom and reset camera |
| Esc | Pause or resume |
| R and Enter | Restart; begin, continue, or retry |
| M | Toggle sound |

### Main Rules

Each chapter starts with five health points. Ordinary hits cost one point and grant 1.05 seconds of protection. Falling below the world costs one point even during immunity, then returns Timmy to the latest checkpoint. A nonfatal fall preserves completed objectives; zero health ends the attempt. Retry or restart reloads the chapter and resets local progress. Lighting a lantern restores its crossing for the attempt, heals one point up to the maximum, and updates the checkpoint.

Jump height is 2.2 world units. There is no double jump or jump immunity, but steering and attacking remain available in the air. A dash lasts 0.22 seconds, grants protection, and has a 1.05-second cooldown. Jumping while grounded can end an active dash. An ordinary jump cannot clear the charging boss's height.

The bow has unlimited arrows, a 0.38-second cooldown, and a small aiming assist; projectile collision determines hits. Candy follows a curved trajectory, is consumed on throwing, and has a 0.6-second throw cooldown. Timmy carries one sweet. E at a different color exchanges it and leaves the old sweet on the floor; a same-color pickup is unavailable.

An exit opens only after all required objectives are complete. Stage One requires two lanterns, one dragon, a bow rune, and a candy seal. Stage Two requires three lanterns and five enemy encounters. A closed gate explains that objectives remain.

The Pumpkin King has six health points and takes no arrow damage. Candy removes one point only when its color matches and the shell is open. A successful hit ends the window. A wrong-color throw is consumed without damage or closing the window; a protected shell also blocks damage. The HUD displays shell state separately from hue. Pink starts the fight, the first hit unlocks blue, and the second unlocks gold; later cycles rotate unlocked hues. The hue stays fixed through each attack sequence and vulnerability window. Held and airborne sweets retain their colors. Labels and symbols supplement color: PINK [O], BLUE [<>], GOLD [*].

Boss supply checks occur every four seconds and at relevant phase or color changes, targeting four floor sweets, including two of the required hue, at safe positions outside the boss footprint. Tutorial candy spots refill every three seconds until the seal breaks, even while candy is held. Pause freezes gameplay timers.

### Game Modes

The single-player adventure includes chapter selection for practice using the same rules. Its interface states are:

| State | Transition or available action |
| --- | --- |
| Menu | Begin the journey or select a chapter |
| Playing | Gameplay controls and updates are active |
| Paused | Updates stop; resume preserves the current situation |
| Defeated | Retry the chapter |
| Level Complete | Continue from Stage One or Two to the next chapter |
| Victory | Boss defeat ends the journey; begin again |

### Level

#### Stage One The First Light

The opening island provides room to move and a stationary bow rune. The first lantern reveals two wide platforms with a Space sign and landing rims. A moving dragon and second lantern introduce combat along the route. The final island supplies replenishing pink candy for a matching seal. Each task gives immediate evidence of what an action does before the player proceeds.

#### Stage Two Shattered Gardens

A bending chain of islands leads through three crossings, ending with smaller, staggered platforms and a raised middle landing. Encounters progress through an ember dragon, ghost, elder dragon, fan-shot bee, and diving demon. The dragons share an attack family; the other enemies require responses to a slow orb, spreading shots, or a physical dive. The player must keep aiming while adapting movement to each pattern.

#### Stage Three The Pumpkin Keep

A lantern restores two narrow, raised platforms at the castle entrance. Entering the courtyard wakes the boss. Early bomb volleys teach players to leave marked danger areas and use the resulting opening. At half health, a charge adds a locked corridor and a shorter counterattack opportunity. Later cycles combine bombs and charges, testing the actions introduced earlier. Boss defeat clears remaining hazards and ends the journey.

### Design Schemas from Rules of Play

I use the four schemas emphasized in the course's Design Schemas slide. The definitions below paraphrase Rules of Play, chapters 14, 15, 18, and 20, and connect each schema to this game's design.

#### Emergent Systems

Emergence occurs when interacting rules produce patterns beyond any individual rule. Here, carrying one candy, exchanging colors, positioning before a locked charge, and attacking during recovery allow different tactics. A player can prepare a sweet and steer the warning lane away from a pickup area; another can dodge first and collect candy afterward. These routes are not prescribed as one solution. My aim is bounded tactical variety within a structured adventure; playtesting must establish which tactics players discover.

#### Systems of Uncertainty

Uncertainty concerns unresolved events and outcomes, including chance, decisions, and execution. Random bomb offsets near the player and randomized safe candy spawn selection vary immediate situations. Aiming and landing remain uncertain even when an attack pattern is known. Visible warnings and guaranteed matching supply keep that variation manageable. Hue progression is controlled, so color unlocking is not a random event.

#### Cybernetic Systems

A cybernetic system senses a state, compares it with a target, and acts to change it. Negative feedback reduces a deviation; positive feedback amplifies it. The candy supply provides negative feedback on shortages: its sensor counts floor sweets and hues, its comparator checks the target of four sweets including two matching ones, and its activator fills shortages or adjusts floor colors. Pickups reduce supply; later checks restore it. Held and airborne sweets are excluded from recoloring. This regulates resources rather than adapting difficulty to player skill. A sound response or a fixed half-health phase change alone is not such a control loop.

#### Systems of Conflict

Conflict is a rule-governed struggle to reach a goal against opposition. Timmy faces enemies, gaps, and restricted damage opportunities rather than another player. Health and chapter objectives make progress and failure measurable. The carrying limit and dash cooldown constrain choices within that struggle. Warnings, shell indicators, and an explanation after a wrong-color hit are intended to make the opposition understandable and learnable.

### Finite State Machines

The player uses concurrent movement, action, and carrying regions because these behaviors overlap. These design FSMs map to controller checks, timers, and flags; they are not one player enum. Enemy and boss phases use explicit enums. All gameplay updates require Playing; pause retains state, while a chapter reload resets it.

#### Player Locomotion

| Current state | Trigger or guard | Next state and result |
| --- | --- | --- |
| Grounded Idle | Movement input | Grounded Moving |
| Grounded Moving | No movement input | Grounded Idle |
| Any grounded state including Dashing | Space | Rising; start jump and end dash |
| Grounded Idle or Moving | Shift and cooldown ready | Dashing; apply dash movement and protection |
| Dashing | Duration ends | Idle or Moving if supported; otherwise Falling |
| Rising | Upward speed ends or ceiling is hit | Falling |
| Grounded or Dashing | Support is lost | Falling |
| Falling | Land on usable surface | Grounded Idle or Moving |
| Any live state | Fall below world | Fall Recovery; apply fall penalty and checkpoint return |
| Fall Recovery | Health above zero | Grounded at checkpoint |
| Any live state | Health reaches zero | Defeated; controls stop |

Post-hit protection is a separate finite region: Vulnerable becomes Protected after damage, then returns when its timer ends. Dash independently supplies protection. Damage does not impose a stationary hurt state.

#### Player Actions and Candy

| Region and state | Trigger or guard | Transition and effect |
| --- | --- | --- |
| Bow Ready | Fire input | Bow Cooldown; release arrow and start aiming pose |
| Bow Cooldown | Timer ends | Bow Ready |
| Empty Hands | E at available candy | Holding Candy of its hue |
| Holding Candy | E at different hue | Holding New Hue; old sweet drops |
| Holding Candy | E at same hue | No transition |
| Holding Candy | Throw input and cooldown ready | Empty Hands; release current sweet |
| Throw Ready | Successful throw | Throw Cooldown; return to Ready when timer ends |
| Interaction Ready | E at unlit lantern | Apply lantern effects without a sustained interaction state |

Held hue has three possible values. Bow actions remain available while carrying candy.

#### Flying Enemy Behavior

| Transition | Condition and behavior |
| --- | --- |
| Patrol to Telegraph | Player in range and patrol timer ends; capture target and warn |
| Telegraph to Attack | Warning ends; fire projectiles or begin dive |
| Attack to Recover | Release phase ends or dive reaches endpoint or time limit |
| Recover to Patrol | Recovery ends; demon must also return home |
| Any live phase to Defeated | Health reaches zero; remove warnings and stop attacks |

Dragon and ghost warnings last 0.9 seconds, bee warnings 1.15, and demon warnings 1.3. The ghost fires one orb at five units per second; the bee fires three shots at 7.5 with 18-degree spacing. The demon dives along a locked lane at ten units per second, can hit once per attack, and returns without another contact attack. These variations reuse the same phase sequence.

#### Pumpkin King Behavior

| Current state | Transition condition | Next state |
| --- | --- | --- |
| Dormant | Entrance lantern lit and player enters courtyard | Telegraph |
| Telegraph | Warning ends | Bombardment |
| Bombardment | Volley ends with health above three | Exposed for four seconds |
| Exposed | Window ends or a hit leaves health above three | Telegraph |
| Exposed | Matching hit reduces health to three | Charge Telegraph immediately |
| Bombardment | Volley ends at three health or below | Charge Telegraph |
| Charge Telegraph | 1.6-second warning ends | Charging |
| Charging | Endpoint reached | Recovering for 2.4 seconds |
| Recovering | Window ends or a nonfatal matching hit lands | Returning |
| Returning | Home reached | Telegraph |
| Exposed or Recovering | Matching hit reduces health to zero | Defeated; clear hazards and trigger Victory |

Only Exposed and Recovering open the shell. Required hue is a separate finite state; wrong-color hits cause no transition. Above half health, volleys contain three bombs; later volleys use four with shorter fuses and finish exploding before the charge warning. Charging follows the direction locked at warning start at 15 units per second and can cause at most one hit. Recovery turns the face toward the player; Returning closes the shell.

### Animation and Audio

Timmy uses Mixamo idle, running, masked bow-aiming, and throwing clips, with procedural jump and landing poses. Gameplay controls movement; torso and arm adjustments align the bow with the target. Flying creatures use Quaternius flight, attack, and death clips; the boss uses procedural motion. The bow lacks a complete draw-and-release cycle, and candy launches on input rather than an animation event.

Synthesized ambience and event sounds accompany lanterns, weapons, pickups, damage, attacks, movement, and results. Visual warnings remain readable when sound is muted. Quaternius Ultimate Monsters assets are CC0; Mixamo assets use Adobe's project-use terms. Raw Mixamo FBX files and extracted textures are excluded from the public source repository. Project-authored scenery, equipment, boss, palette, and audio used Codex assistance; asset records remain in ASSET_CREDITS.md.

### Development and Iteration

Local Git records the concept in 9efa147 on October 5 and the v0.9 snapshot in 8ff4752 on October 7. Intermediate v0.1 to v0.8 builds and QA records are retained locally, not as separate commits. Development observations and requests produced these revisions:

| Observation or request | Revision and purpose |
| --- | --- |
| Missed throws could exhaust tutorial candy | Refill empty spots so practice can continue |
| Platforms could be crossed by walking | Narrow and stagger them to require jumping |
| Bow arm pointed sideways | Align torso, arms, and launch point with aim |
| Recovery showed the boss's back | Turn the exposed face toward the player |
| Pedestal blocked candy above its visible surface | Use a flat mesh collider to match geometry |
| Castle roofs hid pickups and the player | Correct camera sightlines to keep choices visible |
| User requested colors and more species | Introduce hues gradually and vary garden attack patterns |

The v0.9 log reports 283 passing automated runtime checks using controlled positioning and some accelerated time. These verify scripted behavior under those conditions. No independent human playtest is recorded, so tutorial comprehension, fairness, enjoyment, and actual completion time remain unverified.

The next test should begin at the title screen using only in-game instructions. Record first lantern activation, missed jumps, aiming difficulties, dash discovery, understanding of boss damage conditions, deaths, completion time, and comments. Link each observed problem to a focused revision and retest, then record the corresponding GitHub iteration as required by the assignment.

### References

Salen Tekinbas, Katie, and Eric Zimmerman. Rules of Play Game Design Fundamentals. MIT Press, 2003. Chapters 14, 15, 18, and 20. https://mitpress.mit.edu/9780262240451/rules-of-play/

Reading copy: https://openlab.citytech.cuny.edu/aujimtec3175fall2021/files/2017/08/1-rules-of-play-game-design-fundamentals.pdf

Walter, Charles. Design Schemas. Course slide supplied October 8 2026. Emphasizes the four schemas above and skips Information Theory Systems and Game Theory Systems.

Walter, Charles. Project 2 CSCI 5499B 5999B. Course assignment, 2026, pages 1 to 3.

Project evidence: docs/GAME_DESIGN.md, docs/PLAYTEST.md, ASSET_CREDITS.md, gameplay scripts, and commits 9efa147 and 8ff4752, reviewed October 8 2026.

AI use: Codex assisted with implementation, project assets, synthesized audio, validation, and drafting. Mixamo and Quaternius supplied the credited third-party models and clips.
