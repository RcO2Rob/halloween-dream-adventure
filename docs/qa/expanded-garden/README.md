# Expanded garden validation

The final exact-source macOS development build passed **589 full-game integration checks** and **33 focused ending checks**. The full run exercises all three scenes, all new second-chapter gameplay, the physical chapter-two exit, the boss fight and celebration, revisiting the garden, and retry reset. It also checks six camera-facing labels at four orbit angles. Source and scene hashes match the local project; the ad-hoc Mac signature passed strict verification.

`map-audit.json` records 3,790 preserved existing components and disk/loaded-scene backup locations. The four original garden islands and their props are preserved. The Orchard and Terrace fence groups were refreshed to leave openings for the new branches. Tutorial and castle scenes remain byte-identical to their pre-iteration snapshots.

`integration-results.json`, `ending-results.json`, `checks.log`, `build.log`, `build-validation.json` and `source-sha256.json` retain final evidence. Screenshots show actual play states and an orthographic layout review (fog disabled for that review capture only). The isolated development app was used for validation; prior project release app/ZIP snapshots were not replaced.

Earlier failed test iterations are retained under `iterations/`. The first test attempted two candy throws faster than the unchanged throw cooldown. The second completed the expanded chapter and boss fight, then failed an existing fireworks continuation assertion. Its observation was extended to cover a full allowed late rocket cadence and diagnostic values were added; final focused/full checks passed without changing fireworks gameplay code.

No human duration or first-time navigation result is claimed. The 4–6 minute first-exploration duration is a design target for owner playtesting.
