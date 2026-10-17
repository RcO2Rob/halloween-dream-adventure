# Solid scenery

Solid trees, pumpkins (including lantern skins), tombstones, coffins, large
mushrooms, the decorative cauldron, scarecrow, book stand and timber watchtower
now use scene-owned MeshColliders matching their visible meshes. Branches and
gaps retain their shape; there is no oversized whole-tree collision box. Small
obstacles can be stepped or jumped over according to the existing controller.

`SceneryObstacle` marks each fitted prop. Children named **Scenery collision -
Tree**, **Scenery collision - Pumpkin**, etc. sit directly under the corresponding
MeshFilter with identity local transforms. Their colliders use the Default layer;
artwork keeps its existing layer. Walking, dash sweeps, reticle raycasts and
projectiles therefore use the same solid surface. No rigid-body pushing is added.

Both lantern variants have their own shapes. `LanternSkin` activates the correct
artwork and collision together; the pedestal and E interaction retain their
logic. Witch potion receivers, Boss collision, gates, fences, pickups, enemies,
platforms, checkpoints and objectives are not reconfigured. Minor visual details
such as grass, distant lights, webs and particles remain decorative.

Hidden replaced procedural trees have their old trunk colliders disabled to
avoid invisible obstacles. Original object transforms, renderers and gameplay
components are preserved; only relevant old physics components and new collider
children/markers change. The application does not rebuild scenery at runtime.

## Editing in Unity

Move, rotate or scale the complete prop/model in the saved scene: its collider
children follow automatically. Duplicating a configured scene prop includes its
collision. For a newly placed source prefab, select the model and use **Lost Dream
→ Physics → Add collision to selected props**, then save the scene. This operation
supports Undo. Existing valid shapes are reused rather than duplicated.

**Lost Dream → Physics → Apply collision to saved chapters** updates supported
prop categories across the four saved maps, without regenerating their layout.
The editor audit checks every original transform/nonphysics component and verifies
that running the operation a second time does not duplicate shapes. If using an
older art-dressing builder on new content, run this physics pass afterwards.

## Verification and snapshots

Task-start maps are preserved locally under ignored
`Builds/collision-work/saved-scenes-before`. The isolated test export is separate
from previous packaged release snapshots. `docs/qa/scenery-collision` retains the
per-chapter component audit and runtime results. `--smoke-test
--scenery-collision-smoke` checks saved mesh alignment, lantern interaction and
variant switching, all chapters' representative jump routes, and walking,
dashing, aiming, arrows and jumping using copies of actual scene props on a clear
test floor. These automated checks do not constitute independent human playtesting.

The saved pass configures 195 model instances (including the alternative lit/unlit
lantern variants) with 205 mesh surfaces. Fifty-nine spawn/checkpoint, jump entry/
exit, gate approach and candy-supply positions are checked for character-sized
clearance against the newly solid props. Movement tests allow normal sliding
around rounded props and assert actual controller side contacts on a clear floor;
finishing on the far side after sliding around a thin trunk is not treated as
passing through its mesh.
