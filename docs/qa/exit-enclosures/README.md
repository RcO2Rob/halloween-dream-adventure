# Connected exits and matching tutorial trees

Both chapter-one and chapter-two iron gates now connect to the existing island side rails. New front wings close the gaps beside each gate, with continuous collision matching the 2.8-metre railing height. Rear side railings retain a 4.7-metre central opening aligned with the doorway, keeping the lit route visible through the opened leaves. The original gate anchors remain (0, 0, 68) and (2, 0, 81).

All eighteen tutorial tree anchors now use the garden's two imported dead-tree models. Earlier round-canopy renderers and sparse imported duplicates are hidden. Original trunk collision and gameplay positions are retained. New tree art adds no collision and stays outside targeting raycasts. `initial-map-audit.json` retains the initial tree migration and its disk/loaded-scene backup. The latest `map-audit.json` records the exit-only geometry corrections, preserving 2,843 unrelated components in chapter one and 5,427 in chapter two. The castle scene stays byte-identical to its pre-iteration snapshot.

## Verification boundaries

A full three-chapter run passed **637 checks**, including both actual gate crossings, tutorial supply recovery, garden objectives, the boss fight and ending. That report and its source hashes are under `full-before-rear-opening/` and belong to the layout before the final visual corrections.

The **final exact-source build passed 265 targeted checks**. These exercise both chapters' enclosure walk and jump blocking, both rear-side barriers, mesh bounds leaving the doorway visibly clear, tutorial tree coverage and nonblocking art, all real garden objectives, gate animation/pause, floor cue activation, actual walk-through completion and retry reset. All three scenes passed build validation, and the development Mac app passed strict ad-hoc signature verification. `source-sha256.json` matches the final local sources and maps.

Visual inspection caught what physics checks alone missed: the inherited fence normalizer assumed every run lay along world Z, stretching transverse mesh panels into the doorway. It now measures along the actual run direction. Only the new exit enclosure subtrees were rebuilt; existing island side rails remain unchanged. Renderer-bound checks now guard the visible doorway as well as its physical collision. Earlier failed/editor or visually rejected iterations are retained under `iterations/`.

Closed/enclosed and open-lit screenshots, plus tutorial tree views, were visually reviewed. The final open doorway is visibly clear. No independent participant navigation result or human playtime is claimed. Previous release app/ZIP snapshots remain separate from the isolated validation build.
