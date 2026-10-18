# Lanterns of the Lost Dream

Game Design Document

Roucheng Ou

## Part One High Concept Document

### High Concept Statement

Lanterns of the Lost Dream is a short 3D game about restoring a Halloween dream. Players light lanterns to open paths, fight flying nightmares with a bow, and use colored candy in puzzles and the final boss fight.

### Genre and World

The game is a 3D action platformer with shooting and simple light puzzles. I used floating islands, ruined gardens, a mushroom marsh, and a pumpkin castle for the four chapters. The scenery is mostly dark purple, with warm lanterns and teal paths that stand out from the background. I wanted the Halloween setting to feel strange and playful, with some danger.

### Player Role and Primary Gameplay

The player controls Timmy, an explorer who is trying to repair a broken dream. The camera follows him in third person, and the player controls his movement directly. Along the way, the player jumps between platforms, aims the bow, collects candy, and dodges attacks. I wanted progress to be easy to see, so lighting a lantern brings part of the path back into view.

### Expected Player

I designed the game for people who enjoy short action games and already know basic keyboard and mouse controls. For me, player-centred design means giving the player a chance to learn from a mistake. Checkpoints let the player try a jump again without losing every finished task, and candy supplies refill after missed throws. However, I know the route myself, so a section that feels clear to me may still confuse someone playing for the first time. This is why I need feedback from new players.

### Platform and Scope

I made the prototype in Unity 6000.6.2f1 with four 3D chapters. It uses a keyboard and mouse and is made for one player. There is no cooperative or competitive multiplayer. Apple Silicon macOS is the platform that has been tested.

### Progression

I start with movement and basic tools in The First Light. Shattered Gardens adds branching routes and runes that must be hit in order. In Witchlight Marsh, the player uses candy again, this time in potion recipes, while dealing with moving platforms and mist. The Pumpkin Keep brings the bow, dodging, and candy matching together in the final fight. Defeating the Pumpkin King ends the game. I am aiming for a playtime of five to ten minutes.

## Part Two Game Script

### Background Information

On Halloween night, Timmy has a strange dream. He finds himself on a floating island with a bow in his hands. Everything looks like Halloween decorations, but the dangers feel real. Unsure whether he is still dreaming, he follows the lantern-lit paths, hoping to find a way out.

### What the Game Teaches

I want the player to learn how to judge where to move and when to act. The first chapter introduces the main tools, and the later chapters use those same tools in puzzles, hazards, and combat. This gives the player a chance to practise an action before using it in a harder situation.

| Action | First use | Later use |
| --- | --- | --- |
| Lanterns | Open the first path | Open crossings after local tasks |
| Jumping | Wide platforms | Raised and drifting platforms |
| Bow | Rune and dragon | Fight and solve ordered runes |
| Candy | Break a pink seal | Break seals, brew, match the boss |
| Warnings | Enemy attacks | Mist, bombs, charges and portals |
| Dash | Shift instructions | Dodge; wait for cooldown |

The changing challenges in It Takes Two offer a useful comparison. For this smaller project, the question is how to make a familiar tool useful again without adding another control. Arrows become a way to solve a rune order, while candy becomes a potion ingredient. That is the kind of variety I want. Dash still needs a safe practice task beyond its on-screen hint, and playtests must check whether the teaching works.

### Controls and Feedback

| Input | Action |
| --- | --- |
| WASD / arrow keys | Move relative to camera |
| Mouse | Look around; aim with center reticle |
| Left click | Shoot; hold to repeat |
| E | Light lantern; recover memory; pick up / swap candy |
| Right click / Q | Throw candy |
| Space / Shift | Jump / dash from ground |
| Esc / R | Pause / restart |

I used It Takes Two as a reference for the lower third-person camera. I wanted the player to see Timmy and the next platform at the same time, so it is easier to judge a jump. A close camera can also hide danger, so the view pulls back in the boss arena. The player can still turn it with the mouse. This is a single-player game.

The screen shows health, current tasks, nearby interactions, and when the dash is ready. I also use changes in the world to show progress. A lit lantern reveals a path, the garden runes show how far the player has reached in the sequence, and the marsh pots show the next ingredient. Candy has names and symbols as well as colors, and the boss has a separate sign showing whether its shell is open. The player moves the mouse to turn the camera and aim at the center reticle, uses the wheel to zoom, and presses F to reset the view.

### Main Rules

The player begins each chapter with five health points. Getting hit or falling removes one point. A fall sends Timmy back to the checkpoint, but losing all his health means restarting the chapter. I use lanterns to connect progress with recovery: lighting one brings back a path, restores one health point, and saves a checkpoint. The player can jump and attack in the air, but cannot double jump. A dash only works on the ground and gives a short period of protection before it needs to recharge. Jumping does not give this protection.

I kept arrows unlimited because I wanted the challenge to come from aiming and choosing when to attack. Candy works differently. Timmy can carry only one piece, and throwing it uses it up. The player can press E to swap it for a different color, while a piece already being carried keeps its color. I also made the supplies refill, so a missed throw does not leave the player unable to continue.

For the final fight, I gave the Pumpkin King six health points. The player has to wait until its shell opens and throw candy that matches its color. Each correct hit removes one health point and closes the shell again. Arrows and candy of the wrong color do no damage. Pink, blue, and gold unlock in that order. When the boss reaches half health, it also starts charging at Timmy. The player cannot jump over this attack.

The exit opens only after the player finishes all the tasks in that chapter. Walking through takes Timmy straight to the next chapter with full health. The player can pause to stop the game and then continue from the same point, or restart to reset the chapter. Progress is not saved after quitting.

### Game Modes

I kept one main gameplay mode for movement, exploration, and combat, using the same camera and controls throughout. When the player gets close to an object, actions such as lighting a lantern or collecting a memory become available. Puzzles and the boss fight change what the player needs to do within this mode. The title and pause menus sit outside active play. Losing offers a retry, winning ends the game, and moving between chapters happens automatically. The table below shows these screens and states.

| Screen or state | What happens |
| --- | --- |
| Menu | Start or choose a chapter |
| Playing | Play the game |
| Paused | Stop; resume from the same point |
| Defeated | Retry chapter |
| Level Complete | Automatic fade and chapter load |
| Victory | Finish; start again |

### Level Progression

#### Stage One

Stage 1 is the teaching level. I use two lanterns to restore the route and introduce movement, jumping, and both weapons along the way. A dragon, a bow rune, and a pink candy seal give the player a chance to try combat and interaction. Once these tasks are complete, the gate opens. I wanted the player to see the path appear after lighting a lantern, so its purpose is clear.

#### Stage Two

Stage 2 is the first main level. I placed three memories across seven islands and added branching routes with combat, the Observatory's ordered bow puzzle, and the Crypt's candy seal. Hitting the wrong rune resets the bow sequence. To open the gate, the player has to collect all the memories, defeat the guardians, finish the runes, and light all six lanterns.

#### Stage Three

In Stage 3, I use colored candy as ingredients for three potion recipes. A wrong throw does not remove ingredients the pot has already accepted, and the candy supplies refill. Brewing a potion clears nearby mist and helps unlock later lanterns. I also added mist warnings and drifting platforms, so the player has to think about when to move. Leaving the marsh requires all three potions, twelve defeated guardians, and five lit lanterns.

#### Stage Four

Stage 4 is the final boss fight in the castle. I bring the earlier skills together here. The player uses arrows against two waves of reinforcements and throws matching candy at the boss while its shell is open. Bomb warnings and the charge attack at half health make the player move before trying to throw. After the Pumpkin King is defeated, the remaining dangers are cleared, and fireworks and victory music play.

### Design Schemas

#### Cybernetic Systems

I use candy supplies as an example of a cybernetic system, where feedback keeps something close to a set target. In the tutorial and garden, a supply spot refills when it is empty. In the marsh, the game also restores the color assigned to that spot. Throwing candy reduces the supply, and the game checks it and fills it again. This is negative feedback because it brings the supply back toward its target. I added it so the player can try a seal or recipe again after missing a throw.

#### Systems of Conflict

The systems of conflict in this game come from the obstacles between the player and the goal of restoring the dream. The player has to cross gaps, defeat guardians, and solve puzzles. In the garden, this means following clues through branching paths and recovering memories in order. In the marsh, it means working on recipes while dealing with mist and moving platforms. I wanted the player to think about routes and timing as well as attacking enemies, so movement and puzzles are part of the challenge too.

### Finite State Machines

I use the tables below to explain the player and enemy behavior. For the player, labels such as Grounded Idle and Bow Ready describe flags and timers in the code. They are not one state enum, and movement, shooting, and carrying candy can happen together. Flying enemies and the boss use named phase enums. Telegraph means the warning before an attack. The tables describe active play. Pausing stops gameplay updates, and restarting reloads the chapter.

#### Player Locomotion

| State | Condition | Next state or effect |
| --- | --- | --- |
| Grounded Idle | Move | Grounded Moving |
| Grounded Moving | Stop | Grounded Idle |
| Grounded, including Dashing | Space | Rising; end dash |
| Grounded Idle / Moving | Shift; cooldown ready | Dashing; protection |
| Dashing | Dash ends | Idle / Moving if on ground; else Falling |
| Rising | Rise ends or ceiling hit | Falling |
| Grounded / Dashing | Ground lost | Falling; active dash lasts until its timer ends |
| Falling | Land | Grounded Idle / Moving |
| Any live state | Player y < -8 | Fall Recovery; lose health, return |
| Fall Recovery | Health above zero | Return to checkpoint; settle onto ground |
| Any live state | Zero health | Defeated |

I track damage protection separately, so a hit does not stop Timmy from moving. Hits give 1.05 seconds of protection, while dashing gives protection for its own duration. Fall Recovery is an immediate damage-and-teleport action. If the fall takes his last health point, he is still moved to the checkpoint, but the game stays in Defeated until a retry. The jump poses have separate Takeoff, Rising, Falling, Landing and None stages. After victory, Timmy can settle onto the ground and continue his idle animation.

#### Player Actions and Candy

| State | Condition | Next state or effect |
| --- | --- | --- |
| Bow Ready | Fire | Bow Cooldown; shoot |
| Bow Cooldown | Timer ends | Bow Ready |
| Empty Hands | E at candy | Holding Candy |
| Holding Candy | E at new color | Hold new color; floor pickup takes old color |
| Holding Candy | Same-color candy is unavailable | Keep held candy |
| Holding Candy | Throw; cooldown ready | Empty Hands; throw |
| Throw Ready | Successful throw | Cooldown; Ready when timer ends |
| Interaction Ready | E at nearby unlocked lantern / ready memory | Ready; restore path or recover memory |

Candy can be pink, blue, or gold. Carrying it does not stop players from using the bow.

#### Flying Enemy Behavior

| Transition | Condition or effect |
| --- | --- |
| Patrol to Telegraph | Player within 18 units of home; timer ends; warn |
| Telegraph to Attack | Warning ends; shoot or dive |
| Attack to Recover | Ranged: 0.15s passes; demon: target reached or 1.6s passes |
| Recover to Patrol | Recovery timer ends; demon also reaches home |
| Any live phase to Defeated | Health at or below zero; stop attacks and warnings |

I use the same sequence of states for dragons, ghosts, bees, and demons, but their attacks are different. Dragons and ghosts give a 0.9-second warning, bees give 1.15 seconds, and demons give 1.3 seconds. A ghost fires a slow orb, while a bee fires three shots that spread out. A demon dives along a fixed path and can hit once, then returns without causing contact damage.

#### Pumpkin King Behavior

| State | Condition | Next state |
| --- | --- | --- |
| Dormant | Lantern lit; Timmy enters the arena | Telegraph |
| Telegraph | Warning ends | Bombardment |
| Bombardment | Bomb round ends above half health | Exposed |
| Exposed | Opening ends or hit leaves health above half | Telegraph |
| Exposed | Matching hit leaves half health | Charge Telegraph |
| Bombardment | Bomb round ends at half health or below | Charge Telegraph |
| Charge Telegraph | Warning ends | Charging |
| Charging | Charge ends | Recovering |
| Recovering | Recovery ends; or nonfatal matching hit away from home | Returning |
| Returning | Home reached | Telegraph |
| Exposed / Recovering | Final matching hit | Defeated |

I use a warning before each attack so the player has time to react. The boss first cycles between bombs and an open shell. At half health, it adds a charge, with another opening while it recovers. Only matching candy can hurt it during these openings. It summons enemies at the start and again at half health, so the player has to handle them while avoiding the boss's attacks.

### Animation and Audio

I used Mixamo clips for Timmy's idle, running, bow aiming, and throwing animations. I also added poses for taking off, being in the air, and landing, and lined up the bow with the shot. The enemies use Quaternius clips for flying, attacking, and dying. The boss moves to show when an attack is coming and when the player can hit it. The bow still does not have a full draw-and-release animation, and the candy leaves Timmy's hand when the throw input is pressed.

I added original music for exploration, the marsh, the boss fight, and the ending. The boss music changes at half health. Separate sounds give feedback for actions and warn about danger. Pausing also pauses the audio, and M mutes both music and sound effects. The visual warnings still work when the sound is off. Quaternius Ultimate Monsters assets are CC0. Mixamo assets follow Adobe's terms for use in a project, so I keep the raw Mixamo files out of the public repository. I used Codex to help and listed the sources in ASSET_CREDITS.md.

### Development and Iteration

I used iterative design by changing the game to address a problem, then checking it again. I kept earlier builds and failed checks locally so I could look back at them. Feedback from the project owner, automated checks, and tests with independent players tell me different things about the game. The table below connects the main development changes with their Git commits.

| Stage or problem | My change | Commit |
| --- | --- | --- |
| Initial concept | Defined the dream-restoration idea | 9efa147 |
| Tutorial candy could run out | Added refills for more practice | 8ff4752 |
| Walking skipped jumps | Narrowed and offset platforms | 8ff4752 |
| Bow pointed sideways | Fixed the aim pose | 8ff4752 |
| Boss faced away | Turned its face toward players | 8ff4752 |
| Pedestal blocked candy | Used a collider matching its shape | 8ff4752 |
| Roofs hid players and pickups | Adjusted the camera | 8ff4752 |
| Waves and music | Added reinforcements and background music | 755090a |
| Scenery and start screen | Added Halloween scenery and a cover menu | 20549ff |
| Chapter two ended too quickly | Added branches, memories and puzzles | 205631c |

A player reported that Timmy could walk straight through objects in the game world. This makes it difficult to know which objects are supposed to block the way. I plan to check the models, add collision to the objects that should block movement, and ask the player to try those areas again.

When I expanded the garden, I had to think about whether I was making the level better or just longer. The new branches, memories, and puzzles give the player more ways to use the same tools, but they may also make the route harder to follow. Adding more features does not tell me whether the level is more enjoyable. The v0.13 log records 719 passing automated checks for progression and rules. I still need more player tests to find out whether the route is clear and the level is enjoyable.

For the next playtest, at least three new players should start with only the game's instructions. I will record task understanding, missed jumps, aiming, recipe mistakes, boss hits, and finish time, then change the main problems and retest. The four-chapter planning budget is about 9.5 to 13 minutes, which can exceed the assignment's five-to-ten-minute target. Cutting a repeated encounter may help more than adding another feature. Timed human runs should guide that choice; these estimates are not measured playtimes.

### References

Salen Tekinbas, Katie, and Eric Zimmerman. Rules of Play Game Design Fundamentals. MIT Press, 2003. Chapters 18 and 20. https://mitpress.mit.edu/9780262240451/rules-of-play/

[Rules of Play reading copy.](https://openlab.citytech.cuny.edu/aujimtec3175fall2021/files/2017/08/1-rules-of-play-game-design-fundamentals.pdf)

Walter, Charles. Project 2 CSCI 5499B 5999B. Course assignment, 2026, pages 1 to 3.

Walter, Charles. Fundamentals of Game Design. Course slides based on Ernest Adams. Relevant slides: 9 to 11, 21 to 26, 42 to 43, 47, 55, and 80 to 84.

Hazelight Studios and Electronic Arts. It Takes Two features. [https://www.ea.com/games/it-takes-two/it-takes-two/features](https://www.ea.com/games/it-takes-two/it-takes-two/features)

Adobe. Mixamo. Timmy player character and humanoid animation clips. https://www.mixamo.com/

Quaternius. Ultimate Monsters. Dragon, Dragon_Evolved, Ghost, Armabee, and Demon models and animations. https://quaternius.com/packs/ultimatemonsters.html

polyperfect. Poly Halloween Pack. Environment models including pumpkins, dead trees, fences, gates, gravestones, mushrooms, and cauldrons. Unity Asset Store. https://assetstore.unity.com/packages/3d/props/poly-halloween-pack-236625

Lousberg, Kay. KayKit: Spooktober Seasonal Pack 1.1 (Legacy). Candy model candyA. OpenGameArt. https://opengameart.org/content/kaykit-spooktober-seasonal-pack-11

Project evidence: docs/GAME_DESIGN.md, docs/WITCHLIGHT_MARSH.md, docs/PLAYTEST.md, ASSET_CREDITS.md, current gameplay scripts, Git history through 205631c, and docs/qa/four-chapters/build-validation.json. Reviewed October 16, 2026.

AI use: I used Codex to help with code, project assets, generated audio, tests, and this document. Mixamo and Quaternius made the credited models and animation clips.
