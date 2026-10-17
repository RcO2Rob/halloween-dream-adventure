# v0.14.0 release verification

- Unity 6000.6.2f1 macOS arm64 export; embedded typography and four scenes validated.
- Exact packaged application passed 720 rendered standalone journey checks across all four chapters, recipes, traversal, combat, automatic transitions, Boss stages and victory cleanup. See `runtime-results.json`.
- Application signature verified deeply/strictly; all 259 packaged files matched after playing and after extracting the ZIP. See `package-manifest.json` and `archive-validation.json`.
- Source review confirms removal of the R restart binding/HUD hint and Boss-only floating candy labels.
- Saved-map hashes stayed unchanged during export.
- Current font, music and CC0 art notices are included inside the application. Licensed raw Mixamo and Poly Halloween assets remain excluded from public Git.
- Earlier verification iterations are retained. The lantern audit now includes inactive variant parents; automatic transition checks validate foreground play or the intended background pause, then explicitly resume only the opt-in automation. These are test-harness corrections, not changes to ordinary gameplay.
- Previous packaged apps/ZIPs retained locally. Unrelated in-progress GDD edits left outside this release commit.
- No independent human playtest, Windows/Linux export or notarization is claimed.
