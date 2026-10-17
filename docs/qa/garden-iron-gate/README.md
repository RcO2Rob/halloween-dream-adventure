# Opening iron exit gate

The exact saved second scene and current sources passed **225 targeted garden integration checks** on macOS. All three scenes also passed build validation and strict ad-hoc application signature verification.

The test completes the actual three-memory adventure, keeping the iron gate closed for partial progress. It walks into the closed doorway with the real CharacterController, verifies its ground cue stays dark, obtains the final memory through normal interactions, observes both leaves opening and freezing on pause, then verifies the blocker is removed before the emissive trail activates. Standing behind the door without approaching from the front cannot auto-complete the chapter. A real front-to-back walk follows the lit floor through the opening and enters the chapter-completion state. Retry reloads the garden.

The native Poly Halloween prefab is retained with its two original hinge transforms. Animated leaf colliders are disabled; the dedicated passage blocker stays enabled until the full opening is clear. The static frame retains mesh collision. Teal stepping lights, gold arrows and one local point light provide the ground cue without adding movement/projectile colliders.

`map-audit.json` records disk and loaded-scene backups, an unchanged exit anchor (2, 0, 81), and 5,427 preserved unrelated components. Only the exit subtree changes in the map. Tutorial and castle scene files remain byte-identical. `source-sha256.json` matches the exact isolated development build; previous release app/ZIP snapshots are separate artifacts.

Screenshots were visually inspected in closed and fully open states. The targeted checks establish functional behavior; no independent first-time participant navigation result is claimed.
