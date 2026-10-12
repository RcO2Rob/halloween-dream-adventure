# Lanterns of the Lost Dream

Game Design Document

Roucheng Ou | Project 2 | Draft 1 | October 8 2026

## Part One High Concept Document

### Game Overview

Lanterns of the Lost Dream is a short, single-player 3D action adventure made in Unity. The player controls Timmy, a dreamwalker crossing floating islands in a Halloween dream. Nightmare creatures have interrupted the roads, and a Pumpkin King waits inside a castle. The player lights pumpkin lanterns to restore crossings, uses a bow against flying enemies, and collects candy to defeat the boss when his shell opens.

My main design idea is to make the same familiar actions useful in increasingly demanding situations. A lantern changes where the player can travel. Jumping becomes a test of landing accuracy. Candy starts as a simple pickup and throw, then becomes a choice about color and timing. The three chapters form one complete journey: a teaching level, an exploration and combat level, and a castle boss encounter. The intended first-play duration is five to ten minutes; this is a design target that still needs to be measured with unfamiliar players.

### Concept Genre and Background

The genre combines third-person action, platform traversal, and light environmental interaction. A lower perspective camera follows Timmy and gives the islands visible depth. The player can orbit and zoom the camera, while the boss encounter adjusts framing to keep the player and the pumpkin face visible.

The dream is losing its connections as nightmares spread. Pumpkin lanterns restore these connections, so bringing light back also means making progress through the world. The final goal is to defeat the Pumpkin King and finish the dream journey. Floating islands, purple scenery, warm pumpkin light, a castle, ghosts, and wrapped sweets support the Halloween setting. The tone is playful and slightly eerie rather than graphic horror. This background explains the journey; the player does not need to read additional story material to understand the controls or goals.

### Expected Player

The expected player enjoys short action adventures and has basic familiarity with keyboard and mouse controls. They do not need to know the story, memorize an inventory system, or have played the previous project. The teaching chapter should let them learn through actions and visible results. Large landing rims, nearby interaction prompts, a small aiming assist, and checkpoints are intended to support players with limited experience in 3D movement. Understanding and comfort remain playtest questions.

### Platform and Scope

The prototype uses Unity 6000.6.2f1 with three saved 3D scenes. The current tested build is for Apple Silicon macOS. The assignment also requires Windows and Linux x86-family releases, which remain part of the delivery work. Keyboard and mouse are the current input devices. The prototype has no persistent save or controller support.

## Part Two Game Script

### What the Game Teaches

The main learning goal is reading a situation and choosing an appropriate action. A glowing lantern is useful because it restores a real crossing. A flying enemy requires aiming and movement. A danger marker asks the player to reposition before an attack. In the boss fight, a matching candy and an open shell are separate conditions: having the right color does not make the boss vulnerable.

The first chapter introduces actions with relatively little pressure. Later chapters change the context in which those actions are used. The garden asks the player to distinguish projectile patterns and a physical dive. The castle combines candy selection with movement, attack warnings, and short counterattack opportunities. This progression aims to teach spatial judgment and timing through gameplay, without presenting them as measured educational outcomes.

| Mechanic | First learning opportunity | Later application |
| --- | --- | --- |
| Lantern interaction | Light a nearby lantern and see a crossing appear | Identify the next crossing in the garden and restore the castle approach |
| Jumping | Cross two wide circular platforms with visible landing rims | Land on smaller, staggered and raised platforms in the garden and castle |
| Bow aiming | Hit a stationary practice rune, then a moving dragon | Aim while reacting to slow orbs, a fan of shots, and a locked dive |
| Candy pickup and throw | Pick up replenishing pink candy and break a pink seal | Carry one sweet, exchange its color, and hit the matching boss weak point |
| Warning recognition | Notice enemy windup and avoid its attack | Read bomb circles and a locked charge corridor, then counterattack during recovery |
| Dash | Shift provides a grounded dodge and brief protection | Use it against projectiles, bombs, or a charge; its cooldown limits repeated use |

The current tutorial includes written controls and practice for several abilities. A dedicated low-pressure dash exercise and a clearer demonstration of closed-shell versus open-shell damage would strengthen the teaching sequence. These are proposed revisions to evaluate in the first independent playtest.

### Controls and Core Mechanics

| Input | Action | Purpose |
| --- | --- | --- |
| WASD or arrow keys | Move relative to the camera | Explore and reposition around hazards |
| Mouse | Aim | Select a bow or candy target |
| Left mouse button | Fire bow; holding repeats shots | Defeat flying enemies and the bow practice rune |
| E | Interact with the nearest available object | Light lanterns; pick up or exchange candy |
| Right mouse button or Q | Throw held candy | Break the tutorial seal or attack an exposed boss |
| Space | Jump once from the ground | Cross gaps and reach platform landings |
| Shift | Dash from the ground | Avoid danger with brief damage immunity |
| Middle drag or Alt plus left drag | Orbit camera | Adjust the view around scenery |
| Scroll wheel and F | Zoom and reset camera | Adjust framing and restore the default view |
| Esc | Pause or resume | Stop the action and return when ready |
| R and Enter | Restart; begin, continue, or retry | Move through chapter and result screens |
| M | Toggle sound | Control audio output |

The lantern is a spatial switch. Before activation, its crossing has no usable floor. After E activates it, the road or round platforms become usable and remain restored for the current chapter attempt. Lighting also restores one health point, up to the maximum, and saves a safe checkpoint. This makes the action visible, useful, and connected to progress.

The bow has unlimited arrows and a 0.38-second firing cooldown. A small aiming assist helps with airborne enemies, while actual projectile collision determines a hit. Candy follows a curved throwing trajectory and is consumed on throwing. The player carries only one candy at a time. Picking up a different color exchanges it with the held sweet, leaving the old one on the floor. These rules make color selection a physical action in the arena rather than a separate inventory menu.

### Main Rules

The player starts each chapter with five health points. Enemy attacks normally remove one point. Damage gives 1.05 seconds of protection, preventing several ordinary hits from taking all health at once. Falling below the world costs one health point and returns the player to the latest lantern checkpoint. Falling can still cause damage during immunity. If health reaches zero, the defeat screen appears; retry reloads the current chapter and resets its local progress. A nonfatal fall preserves already completed chapter objectives.

Space produces one grounded jump, with a default height of 2.2 world units. There is no double jump. Steering and attacks remain available in the air, but jumping itself gives no damage immunity. Shift starts a grounded dash lasting 0.22 seconds, with a 1.05-second cooldown. Its protection makes it useful against attacks; an ordinary jump cannot clear the height of the charging boss.

The first two chapters require every lantern to be lit, every required flying enemy to be defeated, and any practice runes to be cleared before the exit opens. In the current tutorial, this means two lanterns, one dragon, a bow rune, and a candy seal. The garden has three lanterns and five enemy encounters. A closed exit explains that objectives remain rather than letting the player leave early.

The Pumpkin King has six health points. Arrows cannot damage him. Candy deals one point only if its color matches the required hue and the shell is open. A successful hit ends that vulnerability window. Wrong-color candy is consumed, deals no damage, and leaves the window open. Protected-phase candy also deals no damage. The HUD shows shell state separately from required color.

Pink is the only required and supplied boss color at the start. The first successful hit introduces blue; the second introduces gold. Later cycles rotate among unlocked hues. The required color stays fixed throughout an attack sequence and its open-shell window. Held and thrown candy retain their own colors when the boss changes hue. Names and symbols supplement color: PINK [O], BLUE [<>], and GOLD [*].

Boss candy replenishes on a four-second timer and at relevant color or phase changes. Safe spawn positions stay outside the boss footprint, and the supply system provides at least two sweets of the required hue. Other floor slots can contain unlocked colors. In the tutorial, each empty candy spot replenishes every three seconds until the candy seal is broken. Holding candy does not stop replenishment. Pause freezes gameplay timers.

### Game Modes

The game has one single-player adventure mode. Chapter selection offers direct entry into a chapter for practice or testing; it uses the same chapter rules. The following interface states define what the player can do during the adventure.

| State | Player experience and transition |
| --- | --- |
| Menu | Read controls, begin the journey, or select a chapter |
| Playing | Move, interact, jump, dash, aim, and attack |
| Paused | Gameplay movement, enemy behavior, projectiles, and timers stop; resume returns to the current situation |
| Defeated | Health is zero; retry reloads the current chapter |
| Level Complete | An unlocked gate ends chapter 1 or 2; continue loads the next chapter |
| Victory | Boss defeat ends the journey; the player can begin again |

Restart reloads a chapter rather than rewinding to a lantern checkpoint. Checkpoints support recovery from a nonfatal fall within an attempt. Progress is not saved after leaving the game.

### Level

#### Stage One The First Light

The player starts on a floating island with enough room to try moving and aiming. A gold rune provides a stationary bow target. A nearby pumpkin lantern restores a route with two wide round platforms. A Space sign and clear landing rims introduce jumping. Another section introduces a moving dragon and a second lantern. The final island supplies candy and a pink seal, requiring the player to pick up a sweet with E and throw it. Replenishment supports repeated attempts after missed throws. Completing the two lanterns, dragon, and both practice targets opens the exit.

The purpose of this chapter is to give each important action an understandable result before combining it with pressure. Lighting reveals a path, a shot clears a target, and candy breaks a different kind of seal. The tutorial should take approximately two to three minutes in the intended pacing, subject to playtesting.

#### Stage Two Shattered Gardens

The garden follows a bending chain of islands. Three lanterns create successive crossings, and the last crossing uses three smaller, staggered platforms with a raised middle landing. Five encounters introduce an ember dragon, a ghost, an elder dragon, a fan-shot bee, and a diving demon. The two dragon models share the same basic attack family, while the other species change how the player reads danger.

The ghost releases one slow blue orb at five units per second. The bee fires three gold projectiles at 7.5 units per second, separated by 18 degrees. The demon marks a locked lane for 1.3 seconds, dives at ten units per second, then returns home. Sideways movement can avoid the dive because it does not track the player's new position. The exit requires all lanterns and five encounters to be completed. The pacing target is two to three minutes.

#### Stage Three The Pumpkin Keep

A lantern restores two narrow, raised platforms at the castle entrance. Reaching the courtyard wakes the Pumpkin King. In the first half of the encounter, warnings lead into falling pumpkin bombs. Orange circles show the danger areas. After the volley, the shell opens for four seconds, allowing one matching-color hit.

The hit that reduces health to three immediately unlocks the first charge. A 1.6-second warning marks a corridor aimed at the player's initial position. The boss charges along that locked direction at 15 units per second, can inflict at most one damage point per charge, then stops and faces the player. Recovery exposes the face for 2.4 seconds. A nonfatal matching hit or the end of recovery closes the shell and sends the boss home. Later cycles use four bombs with shorter fuses, followed by another charge; bomb explosions finish before the charge warning. Defeat clears remaining boss hazards and displays victory. The pacing target is two to four minutes.

### Design Schemas from Rules of Play

A design schema is a way of examining how a game's rules produce play. I use the four schemas emphasized in the course's Design Schemas slide: Emergent Systems, Systems of Uncertainty, Cybernetic Systems, and Systems of Conflict. Each explains a different aspect of the same prototype. The following definitions are paraphrased from Rules of Play, chapters 14, 15, 18, and 20; the examples and design judgments concern this game.

#### Emergent Systems

An emergent system produces patterns through the interaction of its parts that are not individually specified by a single rule. In a game, a small set of rules can support tactics and situations beyond a fixed sequence of instructions. More features or more enemy models alone do not establish emergence.

My design creates room for limited tactical emergence by combining camera-relative movement, a one-candy carrying limit, candy exchange, fixed-direction warnings, and timed vulnerability. For example, the player can prepare a matching candy before the charge, move so the locked lane points away from a useful pickup area, step sideways, and approach the exposed face after the charge. Another player may keep more distance and collect the matching sweet later. The game specifies the charge direction and candy rules, but does not require one particular route through that sequence.

These possibilities come from rules interacting, rather than a separate instruction for every tactic. The prototype remains a small, structured adventure with scripted chapter goals and boss phases. I would describe its emergence as bounded tactical variety, and use playtesting to see which strategies players actually discover.

#### Systems of Uncertainty

This schema examines what is unresolved during play. Uncertainty can concern individual chance events or the eventual outcome of the game. It also comes from the player's execution and decisions, so it does not require every mechanic to be random.

The boss chooses bomb positions with random offsets near the player's current location, and candy replenishment begins at a randomized safe spawn point. These rules vary the immediate movement problem between cycles. At the larger level, success depends on the player's aiming, landing, movement, and timing. A known boss attack pattern still leaves the result uncertain until the player responds.

I limit randomness so that decisions remain useful. Bomb circles reveal danger before impact, the charge direction locks after its warning, and replenishment restores matching sweets at safe floor positions. Candy color progression is controlled rather than randomly selected: pink introduces the rule, followed by blue and gold. These limits preserve variation while giving the player a readable opportunity to act. They do not guarantee a successful throw or dodge.

#### Cybernetic Systems

A cybernetic system monitors a state, compares it with a target condition, and acts to change that state. Negative feedback counteracts a deviation; positive feedback amplifies it. Here, feedback means a regulating loop, rather than simply a sound or visual response to an action.

The boss candy supply is a concrete regulating loop. Its environment is the set of floor candies. At refill times and relevant phase boundaries, the sensor reads the current pickups and their colors. The comparator checks them against a target supply of four pickups, including two of the required hue. The activator creates missing safe pickups and reassigns floor hues where necessary. Player pickups and throws change the supply, and the next check corrects the resulting shortage. Held and in-flight sweets retain their colors.

This is negative feedback on resource shortage: consuming candy reduces available supply, and replenishment counteracts that reduction. In the tutorial, checking each designated spot and replacing a missing sweet is a simpler version of the same idea. The purpose is to preserve another opportunity to try after a missed throw. The system regulates supply; it does not adjust enemy speed or damage based on player skill. Likewise, unlocking the boss charge at half health is a phase rule, not by itself evidence of a positive feedback loop.

#### Systems of Conflict

This schema examines the rule-governed struggle to reach a goal against opposition. Conflict can involve another player or the game system, and need not be violent. In this single-player game, the opposition is provided by enemies, traversal hazards, and attack conditions.

The overall goal is to restore the route and defeat the Pumpkin King. Flying enemies obstruct progress, gaps require successful landings, and the boss blocks damage outside two exposed states. Five player health points and six boss health points make failure and progress measurable. The one-candy limit and dash cooldown create decisions within that struggle: the player must choose a useful sweet and a moment to dodge rather than storing every color or relying on continuous protection.

I aim to make this conflict understandable. Warnings show attacks before impact, shell state is separate from required hue, and a wrong-color throw produces an explanation without closing the remaining opportunity. Later chapters intensify the conflict by combining previously introduced actions. The design goal is a challenge that players can learn to overcome; whether its timing feels fair still needs independent playtesting.

### Finite State Machines

The following machines specify allowed behavior and transition conditions. The player uses concurrent action regions because movement, attacking, and carrying candy can overlap. These are design-level FSMs mapped to the existing controller's grounded checks, timers, and flags; the implementation does not store every player state in one enum. Enemy and boss phases are explicit enums. The game-state machine above allows action updates only during Playing. Pause retains the current gameplay state for resume, while retry resets it by reloading the chapter.

#### Player Locomotion

| Current state | Trigger or guard | Next state and result |
| --- | --- | --- |
| Grounded Idle | Movement input | Grounded Moving |
| Grounded Moving | No movement input | Grounded Idle |
| Any grounded state including Dashing | Space while grounded | Rising; start one jump and end any active dash |
| Either grounded state | Shift while cooldown is ready | Dashing; use dash movement and damage immunity |
| Dashing | 0.22 seconds elapsed | Grounded Idle or Moving; if unsupported, Falling |
| Rising | Upward speed ends or ceiling is hit | Falling |
| Grounded or Dashing | Support is lost | Falling |
| Falling | Land on a usable surface | Grounded Idle or Moving; play landing response |
| Any live locomotion state | Fall below the world | Fall Recovery; lose one health and return to checkpoint |
| Fall Recovery | Health remains above zero | Grounded state at checkpoint |
| Any live state | Health reaches zero | Defeated; ordinary controls stop |

Bow firing and candy throwing can occur during movement or airborne states. Dash activation requires being grounded. Jumping has no special protection against attacks. Post-hit immunity is an additional finite flag region: Vulnerable becomes Protected after damage, then returns after 1.05 seconds; dash can independently supply protection. Receiving damage does not lock the player into a stationary hurt state.

#### Player Actions and Candy

| Region and state | Trigger or guard | Transition and effect |
| --- | --- | --- |
| Bow Ready | Fire input while Playing | Bow Cooldown; release an arrow and start the aiming pose |
| Bow Cooldown | 0.38 seconds elapsed | Bow Ready |
| Empty Hands | E at available candy | Holding Candy of that hue |
| Holding Candy | E at a different hue | Holding New Hue; previous sweet stays on the floor |
| Holding Candy | E near the same hue | Remain Holding; same-color pickup is unavailable |
| Holding Candy | Throw input and throw cooldown ready | Empty Hands; release the sweet with its current hue |
| Throw Ready | Successful throw | Throw Cooldown; return to Ready after 0.6 seconds |
| Interaction Ready | E at an unlit lantern | Remain Ready; restore crossing, heal, and update checkpoint |

The held hue is one of three finite values. Holding candy does not remove the bow action. Lantern interaction is an immediate transition effect, so it does not need an invented long interaction animation state.

#### Flying Enemy Behavior

Patrol leads to Telegraph, then Attack, then Recover, and back to Patrol. Any live phase can transition to Defeated when enemy health reaches zero. A defeated enemy stops attacking and plays its death response.

| Transition | Condition and behavior |
| --- | --- |
| Patrol to Telegraph | Player is within encounter range and the patrol timer ends; capture the attack target and show a warning |
| Telegraph to Attack | Warning timer ends; fire the species projectile pattern or begin the demon's physical dive |
| Attack to Recover | Projectile release phase ends, or dive reaches its endpoint or time limit; remove dive warning |
| Recover to Patrol | Recovery ends; demon must also reach home before resuming patrol |
| Any live phase to Defeated | Arrow damage reduces health to zero; remove warnings and stop attacks |

Dragons and ghosts warn for 0.9 seconds, the bee for 1.15 seconds, and the demon for 1.3 seconds. Their shared machine keeps the attack rhythm consistent while attack types create different movement problems. A demon dive can cause at most one contact hit per attack; its return movement does not repeat the dive's contact attack.

#### Pumpkin King Behavior

| Current state | Transition condition | Next state |
| --- | --- | --- |
| Dormant | Entrance lantern is lit and player enters the courtyard | Telegraph |
| Telegraph | Warning ends | Bombardment |
| Bombardment | Volley ends and health is above three | Exposed for four seconds |
| Exposed | Window ends or a hit leaves health above three | Telegraph for the next cycle |
| Exposed | Matching hit reduces health to three | Charge Telegraph immediately |
| Bombardment | Volley ends at three health or below | Charge Telegraph |
| Charge Telegraph | 1.6-second locked-lane warning ends | Charging |
| Charging | Endpoint reached | Recovering with open face for 2.4 seconds |
| Recovering | Window ends or a nonfatal matching hit lands | Returning with shell closed |
| Returning | Home reached | Telegraph |
| Exposed or Recovering | Matching candy reduces health to zero | Defeated; clear hazards and trigger Victory |

Exposed and Recovering are the two damage-accepting states. A wrong-color hit causes no state transition and does not shorten the window. Required hue is a separate finite state so changing color cannot accidentally open the shell. During Charging, the destination remains fixed rather than tracking the player's subsequent movement.

### Animation and Audio

Timmy uses a Mixamo Humanoid model with idle and running animation, a masked bow-aiming clip, and a throwing clip. Project adaptations align the chest and arms with the shot direction and place the bow grip in the forward hand. Procedural poses provide jump takeoff, rising, falling, and landing. Movement remains controlled by the gameplay controller rather than animation root motion.

The flying creatures use Quaternius models with imported flight, attack, and death clips. The boss uses procedural motion for breathing, attack phases, and recovery. The current bow pose does not simulate a complete draw-and-release cycle, and candy launches on input rather than waiting for a hand-release animation event. These are prototype limits to consider when evaluating visual clarity.

Event sounds support lantern activation, bow firing, candy pickup and throwing, damage, enemy attacks, movement, and results. The project also includes synthesized dream ambience. This audio was generated for the project rather than taken from commercial recordings. Visual warnings remain necessary because the player can mute sound.

Quaternius Ultimate Monsters supplies the dragon, evolved dragon, ghost, bee, and demon artwork under CC0. Timmy and the four downloaded humanoid clips come from Adobe Mixamo under its project-use terms and are not CC0. The public source repository excludes raw Mixamo FBX files and extracted textures; compiled game assets remain in the local build. Source records, hashes, and credits are kept with the project. The islands, castle, pumpkin boss, platforms, bow, candy, palette, and synthesized audio are project-authored with Codex assistance.

### Development and Iteration

The local Git history currently records the initial concept in commit 9efa147 on October 5 and the v0.9 prototype snapshot in commit 8ff4752 on October 7. Intermediate v0.1 to v0.8 build and QA records are retained locally, but they are not separate commits in this history. The change descriptions below come from the development log and distinguish observed problems from requested feature additions.

| Observation or request | Change | Design reason |
| --- | --- | --- |
| Missed tutorial throws could exhaust candy and block progress | Refill each empty tutorial candy spot every three seconds | Support repeated practice without restarting |
| First circular platform layout could be crossed by walking | Narrow and stagger the platforms; verify walking now falls | Require the jump that the section is intended to teach |
| Original bow pose pointed the left arm sideways | Align torso and arms with the aiming direction and launch from the grip | Make the character's pose agree with the shot |
| Boss recovery showed the back of the head | Turn the exposed face toward the player after charging | Make the counterattack target visible |
| The stone pedestal blocked a real candy projectile above its visible surface | Replace oversized pedestal collision with the flat mesh collider | Make visible geometry and collision agree |
| Castle roofs hid the player near candy pickup points | Check player and focus sightlines when correcting the camera | Keep pickup decisions and carried candy readable |
| User requested color matching and more enemy species | Introduce pink, blue, and gold gradually; vary the existing garden encounters | Add selection and movement decisions within the existing journey |

The v0.9 development log reports 283 passing automated runtime checks on October 7. They exercise real interactions, jumping routes, projectile hits, candy exchange, attack phases, and victory, using controlled positioning and some accelerated time. This verifies scripted behavior under those conditions. It does not establish that an unfamiliar player understands the tutorial, finds the game enjoyable, or finishes within the target duration. No independent human playtest is currently recorded.

The next playtest should start at the title screen with only the game's instructions. Record time to first lantern activation, missed jumps, aiming problems, whether dash is discovered, whether the two boss damage conditions are understood, deaths, total completion time, and player comments. Each observed problem should lead to a focused revision and a repeat of the affected section. Keeping the observation, change, reason, and retest together will provide the evidence required by the assignment.

### References

Salen Tekinbas, Katie, and Eric Zimmerman. Rules of Play Game Design Fundamentals. MIT Press, 2003. Chapters 14, 15, 18, and 20 discuss the four schemas used in this document. Publisher: https://mitpress.mit.edu/9780262240451/rules-of-play/

Reading copy: https://openlab.citytech.cuny.edu/aujimtec3175fall2021/files/2017/08/1-rules-of-play-game-design-fundamentals.pdf

Walter, Charles. Design Schemas. Course lecture slide supplied October 8 2026. The slide emphasizes Emergent Systems, Systems of Uncertainty, Cybernetic Systems, and Systems of Conflict, and skips Information Theory Systems and Game Theory Systems.

Walter, Charles. Project 2 CSCI 5499B 5999B. Course assignment, 2026. Assignment requirements and grading table, pages 1 to 3.

Project evidence: docs/GAME_DESIGN.md, docs/PLAYTEST.md, ASSET_CREDITS.md, the gameplay scripts, and Git commits 9efa147 and 8ff4752, reviewed October 8 2026.

AI use disclosure: Codex assisted with implementation, procedural game assets, synthesized audio, validation, and drafting this document. Mixamo and Quaternius created the credited third-party models and animation clips.
