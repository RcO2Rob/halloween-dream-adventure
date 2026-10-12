# Lanterns of the Lost Dream

Game Design Document

Roucheng Ou | Project 2 | Draft 4 | October 8 2026

## Part One High Concept Document

### Game Overview

I made Lanterns of the Lost Dream as a short, single-player 3D action adventure in Unity. The player controls Timmy, who travels through a dream. He lights pumpkin lanterns to rebuild paths, shoots flying enemies with a bow, and throws candy at the Pumpkin King. I use three chapters to teach the actions, give players practice, and bring them together in the boss fight. I aim for a first playthrough of five to ten minutes.

### Concept Genre and Background

I combine fighting, jumping across platforms, and using objects in a third-person game. The camera follows Timmy. Players can turn and zoom it, and it adjusts its view during the boss fight. In my story, nightmares have broken the paths between islands. Timmy restores them with light and reaches the castle. I use purple scenery, pumpkin lights, ghosts, and candy to make the world feel like Halloween. I want it to feel fun and a little spooky.

### Expected Player

I designed this game for people who enjoy short action adventures and know basic keyboard and mouse controls. They do not need to know an earlier game or its story. I use clear platform edges, interaction messages, slight aiming help, and checkpoints to help players learn 3D movement.

### Platform and Scope

I use Unity 6000.6.2f1 and three saved 3D scenes. My tested build runs on Apple Silicon macOS. I still need to finish the required Windows and Linux releases. The game uses keyboard and mouse, with no controller support or saved progress after quitting.

## Part Two Game Script

### What the Game Teaches

I want players to learn where to move and when to act. I introduce each action with a simple task, then use it again in a harder situation.

| Action | First use | Later use |
| --- | --- | --- |
| Lanterns | Open the first path | Open later crossings |
| Jumping | Wide platforms | Smaller, higher platforms |
| Bow | Rune and dragon | Shoot while dodging |
| Candy | Break a pink seal | Match color and timing |
| Warnings | Enemy attacks | Bombs and charges |
| Dash | Shift instructions | Dodge; wait for cooldown |

For the next playtest, I want to add a safe dash practice task and show more clearly that a boss hit needs both matching candy and an open shell.

### Controls and Core Mechanics

| Input | Action |
| --- | --- |
| WASD / arrow keys | Move relative to camera |
| Mouse | Aim |
| Left click | Shoot; hold to repeat |
| E | Light lantern; pick up or swap candy |
| Right click / Q | Throw candy |
| Space / Shift | Jump / dash from ground |
| Middle drag / Alt + left drag | Turn camera |
| Wheel / F | Zoom / reset view |
| Esc | Pause / resume |
| R / Enter | Restart / start, continue, or retry |
| M | Sound on / off |

### Main Rules

I give Timmy five health points at the start of each chapter. A normal hit removes one point, then protects him for 1.05 seconds. Falling below the world costs one point even during protection and sends him to the last checkpoint. Finished tasks stay complete unless health reaches zero. Retry or restart resets the chapter. Lighting a lantern rebuilds its path, restores one health point up to five, and sets a new checkpoint.

I set the jump height to 2.2 world units. Players can move and attack in the air, but cannot double jump. Jumping does not protect them from damage. A dash protects Timmy for 0.22 seconds and has a 1.05-second cooldown. Jumping from the ground ends an active dash. A normal jump is not high enough to clear the charging boss.

I give the bow unlimited arrows and a 0.38-second cooldown. It has slight aiming help, but an arrow must touch the target to hit. Candy travels in a curve and is used up when thrown, with a 0.6-second cooldown. Timmy can carry one candy. Pressing E near a different color swaps candies and drops the old one. He cannot pick up the color he already holds.

I keep each exit closed until all tasks are finished. Stage One needs two lit lanterns, one defeated dragon, a cleared bow rune, and a broken candy seal. Stage Two needs three lit lanterns and five cleared enemy fights. A message at the gate tells players that tasks remain.

I give the Pumpkin King six health points. Arrows cannot hurt him. Candy removes one point only if its color matches and his shell is open. A correct hit closes the shell. Wrong-color candy is used up without damage or any change to the shell. A closed shell blocks all damage. I show shell status and required color separately on screen. The fight starts with pink. The first hit adds blue, and the second adds gold. Later rounds cycle through the colors already added. The required color stays the same during an attack and its open-shell period. Held or flying candy keeps its color. I also use labels and symbols: PINK [O], BLUE [<>], GOLD [*].

I set the boss arena to check its candy every four seconds and at certain phase or color changes. It aims to keep four candies on the floor, including two matching ones, in safe spots outside the boss's body. Tutorial spots refill every three seconds until the seal breaks, even when Timmy holds candy. Pausing stops the timers.

### Game Modes

I use one single-player adventure mode. I also let players choose a chapter for practice with the same rules. The game has these screen states:

| State | What happens |
| --- | --- |
| Menu | Start or choose a chapter |
| Playing | Play the game |
| Paused | Stop; resume from the same point |
| Defeated | Retry chapter |
| Level Complete | Load next chapter |
| Victory | Finish; start again |

### Level

#### Stage One The First Light

I start with an open island where players can try moving and shooting a still bow rune. The first lantern reveals two wide platforms with a Space sign and clear edges. I then place a moving dragon and a second lantern along the path. The last island has pink candy and a matching seal. Candy refills after missed throws, so players can keep trying.

#### Stage Two Shattered Gardens

I use a curving path with three crossings. The last one has smaller platforms placed at different positions and heights. Players meet an ember dragon, a ghost, an elder dragon, a bee, and a demon. The dragons use similar attacks. The ghost fires a slow orb, the bee spreads its shots, and the demon dives at Timmy. I want players to keep aiming while changing how they dodge.

#### Stage Three The Pumpkin Keep

I place two narrow, raised platforms at the castle entrance, which a lantern restores. Entering the courtyard wakes the Pumpkin King. I start with bombs and marked danger areas. At half health, the boss adds a charge along a marked path and gives players less time to hit back. Later rounds combine bombs and charges. Defeating him removes the remaining dangers and ends the game.

### Design Schemas from Rules of Play

I use the four design schemas from the class slide. Each schema is a way to look at how game rules shape play. I explain them in my own words using Rules of Play, chapters 14, 15, 18, and 20, then show how they fit my game.

#### Emergent Systems

Emergence means that rules working together can create ways to play that no single rule gives on its own. I combine carrying one candy, swapping colors, dodging a charge whose direction is fixed, and throwing while the face is open. Players can hold matching candy and stand where the warning points away from a pickup spot. They can also dodge first and collect candy later. I do not require one exact plan. I expect a few different tactics, but need playtests to see which ones players find.

#### Systems of Uncertainty

Uncertainty means that players do not yet know what will happen or whether they will succeed. It can come from chance, choices, or how well they play. I use random bomb positions near Timmy and random choices of safe candy spots. Players can still miss a shot or jump even when they know an attack. I use warnings and matching candy supplies to give them a fair chance to respond. New colors follow a set order, so they are not random.

#### Cybernetic Systems

A cybernetic system checks a condition, compares it with a goal, and acts on the result. Negative feedback pushes against a change and brings the system back toward its goal. Positive feedback makes a change grow. I use candy refills as a negative feedback loop. The sensor counts floor candies and colors. The comparator checks for four candies, including two matching ones. The activator adds missing candy or changes floor colors. Picking up candy lowers the supply, and the next check brings it back. Held and flying candy keeps its color. I use this loop to maintain supply, not to change difficulty based on player skill. A sound effect or half-health phase change alone is not this kind of loop.

#### Systems of Conflict

Conflict means trying to reach a goal while rules and obstacles stand in the way. I make Timmy face enemies, gaps, and a boss that can only be hurt at certain times. Health and chapter tasks show progress or failure. Carrying one candy and waiting for the dash cooldown limit the player's choices. I use warnings, shell messages, and wrong-color explanations to help players understand the challenge.

### Finite State Machines

A finite state machine, or FSM, lists states and the conditions for changing them. I keep movement, attacks, and held candy in separate groups because players can do them at the same time. The player code uses checks, timers, and flags rather than one enum for all states. Enemy and boss phases use named states stored as enums. Telegraph means the warning before an attack. These actions only run during Playing. Pause keeps their states, while restarting a chapter resets them.

#### Player Locomotion

| State | Condition | Next state or effect |
| --- | --- | --- |
| Grounded Idle | Move | Grounded Moving |
| Grounded Moving | Stop | Grounded Idle |
| Grounded, including Dashing | Space | Rising; end dash |
| Grounded Idle / Moving | Shift; cooldown ready | Dashing; protection |
| Dashing | Dash ends | Idle / Moving if on ground; else Falling |
| Rising | Rise ends or ceiling hit | Falling |
| Grounded / Dashing | Ground lost | Falling |
| Falling | Land | Grounded Idle / Moving |
| Any live state | Fall below world | Fall Recovery; lose health, return |
| Fall Recovery | Health above zero | Grounded at checkpoint |
| Any live state | Zero health | Defeated |

I track damage protection separately. After a hit, Vulnerable changes to Protected, then returns when the timer ends. A dash also gives protection. A hit does not force Timmy to stop moving.

#### Player Actions and Candy

| State | Condition | Next state or effect |
| --- | --- | --- |
| Bow Ready | Fire | Bow Cooldown; shoot |
| Bow Cooldown | Timer ends | Bow Ready |
| Empty Hands | E at candy | Holding Candy |
| Holding Candy | E at new color | Holding new color; drop old candy |
| Holding Candy | E at same color | No change |
| Holding Candy | Throw; cooldown ready | Empty Hands; throw |
| Throw Ready | Successful throw | Cooldown; Ready when timer ends |
| Interaction Ready | E at unlit lantern | Stay Ready; restore path, heal, checkpoint |

I track three possible candy colors. Holding candy does not stop players from using the bow.

#### Flying Enemy Behavior

| Transition | Condition or effect |
| --- | --- |
| Patrol to Telegraph | In range; timer ends; set target and warn |
| Telegraph to Attack | Warning ends; shoot or dive |
| Attack to Recover | Shots end; dive ends or times out |
| Recover to Patrol | Recovery ends; demon home |
| Any live phase to Defeated | Zero health; stop attacks and warnings |

I use the same state order for flying enemies, but change their attacks. Dragon and ghost warnings last 0.9 seconds, the bee's 1.15, and the demon's 1.3. The ghost fires one orb at five units per second. The bee fires three shots at 7.5 units per second with 18-degree gaps. The demon dives along a fixed path at ten units per second, can hit once, and cannot deal contact damage on its return.

#### Pumpkin King Behavior

| State | Condition | Next state |
| --- | --- | --- |
| Dormant | Lantern lit; enter courtyard | Telegraph |
| Telegraph | Warning ends | Bombardment |
| Bombardment | Bombs end; health above three | Exposed for four seconds |
| Exposed | Time ends or hit leaves more than three health | Telegraph |
| Exposed | Matching hit leaves three health | Charge Telegraph immediately |
| Bombardment | Bombs end; health at most three | Charge Telegraph |
| Charge Telegraph | 1.6-second warning ends | Charging |
| Charging | Charge ends | Recovering for 2.4 seconds |
| Recovering | Time ends or nonfatal matching hit | Returning |
| Returning | Home reached | Telegraph |
| Exposed / Recovering | Matching hit leaves zero health | Defeated |

I open the shell only in Exposed and Recovering. I track the required color separately, and a wrong-color hit does not change the state. Above half health, the boss drops three bombs. Later rounds use four with shorter delays. All bombs explode before the charge warning. The charge moves at 15 units per second along the direction set when the warning starts and can hit once. During recovery, the face turns toward Timmy. Returning closes the shell.

### Animation and Audio

I use Mixamo clips for Timmy's idle, run, bow aim, and throw. The aim clip affects selected body parts. Code adds jump and landing poses and lines up the chest, arms, and bow with the target. The controller moves Timmy. I use Quaternius flight, attack, and death clips for enemies and code to move the boss. The bow does not yet have a full draw-and-release animation. Candy launches on input, not at an animation's release point.

I use generated background sound and event sounds for lanterns, weapons, pickups, damage, attacks, movement, and results. Visual warnings still work with sound off. Quaternius Ultimate Monsters assets are CC0. Mixamo assets follow Adobe's project-use terms, and I leave raw Mixamo FBX files and extracted textures out of the public source repository. I used Codex help for scenery, equipment, the boss, colors, and audio. I keep source and credit records in ASSET_CREDITS.md.

### Development and Iteration

I have two local Git commits: 9efa147 for the concept on October 5 and 8ff4752 for v0.9 on October 7. I kept v0.1 to v0.8 builds and test records locally, but they are not separate commits. I made these changes after development checks and requests:

| Problem or request | My change |
| --- | --- |
| Tutorial candy could run out | I added refills for more practice |
| Walking skipped jumps | I narrowed and offset platforms |
| Bow pointed sideways | I fixed the aim pose |
| Boss faced away | I turned its face toward players |
| Pedestal blocked candy | I used a collider matching its shape |
| Roofs hid players and pickups | I adjusted the camera |
| More colors and enemy types | I added colors in steps and varied attacks |

My v0.9 log lists 283 passing automated checks. These use set positions and sometimes faster timers to check game behavior. I have no recorded playtest with an independent player yet. I still need to check whether new players understand the game, find it fair and fun, and finish in the target time.

For my next playtest, I want players to start at the title screen and use only the game's instructions. I will record their first lantern use, missed jumps, aiming problems, dash use, understanding of boss hits, deaths, finish time, and comments. I will use those results to choose changes, test them again, and record the changes in GitHub as the assignment requires.

### References

Salen Tekinbas, Katie, and Eric Zimmerman. Rules of Play Game Design Fundamentals. MIT Press, 2003. Chapters 14, 15, 18, and 20. https://mitpress.mit.edu/9780262240451/rules-of-play/

Reading copy: https://openlab.citytech.cuny.edu/aujimtec3175fall2021/files/2017/08/1-rules-of-play-game-design-fundamentals.pdf

Walter, Charles. Design Schemas. Course slide supplied October 8 2026. Covers the four schemas above and skips Information Theory Systems and Game Theory Systems.

Walter, Charles. Project 2 CSCI 5499B 5999B. Course assignment, 2026, pages 1 to 3.

Project evidence: docs/GAME_DESIGN.md, docs/PLAYTEST.md, ASSET_CREDITS.md, gameplay scripts, and commits 9efa147 and 8ff4752, reviewed October 8 2026.

AI use: I used Codex to help with code, project assets, generated audio, tests, and this document. Mixamo and Quaternius made the credited models and animation clips.
