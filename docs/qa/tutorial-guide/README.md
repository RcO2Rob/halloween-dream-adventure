# First Light tutorial guidance

Chapter one now has a golden ground trail, a ring around the current target, and a compact instruction panel. The trail follows these lessons: first lantern, bow practice, round-platform crossing, flying monster, second lantern, candy pickup, candy seal, and exit.

Guidance reads the saved scene references at runtime. It does not regenerate maps. A small ground search avoids solid scenery; explicit jump links follow the existing round platforms, and arrows are drawn only on actual ground. Opening exit colliders are checked again while the door moves. Trail meshes have no collision and do not intercept aiming rays. Guidance is hidden during the introduction, menus, pause, and defeat, and is created only in chapter one.

## Verification

- Current runtime sources compile with Unity 6000.6.2f1's bundled Roslyn compiler: zero errors. The 133 existing deprecated object-search API warnings remain.
- The final rendered standalone run passed 86 checks using `--smoke-test --tutorial-guide-smoke`. It covers real bow hits, the defeated monster, real candy throws, missed throws and replenishment, fall recovery, the opening gate, restart, and chapter scope.
- Every sampled arrow in the focused checks has a ground surface underneath and sufficient character clearance. Tutorial visuals add no colliders.
- The instruction panel was checked at 1440x900, 1280x720, and 1280x960. The existing font overflow audit reported no overflow.
- At the time of the focused tutorial run, all four saved scene hashes matched their pre-change values. The existing release application's signature also remained valid.

The test used an isolated copy of the v0.14.0 macOS player and asset bundle with the newly compiled runtime assembly. This validates the new code against the existing saved assets without occupying the open Unity project. It is not a full Unity rebuild or a new exported release. The focused validation did not replace the existing app/ZIP.

The subsequent source checkpoint includes owner-authored position adjustments to two transforms and three prefab roots in the saved garden scene. `sync-scene-validation.json` verifies that its serialized objects and asset references are unchanged. These later garden positions were not included in the focused tutorial runtime run. Chapter three's enemy names and attack text are hidden, with health bars retained.

`runtime-results.json` records the final run. `source-validation.json` identifies the tested source and assembly hashes. Earlier check iterations are retained under `iterations/`; the first headless run exposed an overly broad test query that included the chapter's own colliders, and the first rendered run preceded the exit-collider cache fix. Selected final screenshots show the initial trail, jump lesson, monster lesson, and open exit.
