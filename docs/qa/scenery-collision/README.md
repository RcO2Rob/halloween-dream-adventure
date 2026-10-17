# Scenery collision verification

- Isolated Unity 6000.6.2f1 macOS build and signature verification passed.
- 109 focused runtime checks passed; `runtime-results.json`.
- 195 model instances, including alternate lantern skins, use 205 fitted mesh surfaces.
- 59 character-clearance positions passed: spawns, checkpoints, jump entries/exits, gate approaches and candy supplies.
- Editor audit preserved all original transforms, renderers and unrelated gameplay components; only relevant old physics components changed.
- Running the configuration twice did not duplicate shapes.
- Four saved chapters copied back only after verifying workspace hashes still matched task-start backups.
- Copies of the actual tree/pumpkin were used on a clear temporary floor to isolate contacts from fences, enemies and floor edges. The controller may slide around rounded props; genuine side contacts, projectile stops and pumpkin jumping were verified.
- Source assets and saved maps updated locally. Earlier packaged releases remain separate; no GitHub push.
- No independent human playtest claimed.
- Source/document whitespace checks passed. Unity-generated scene YAML retains its standard empty-field trailing spaces; saved scene bytes match the validated export.
