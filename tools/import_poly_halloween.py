"""Restore the locally acquired Poly Halloween package without importing demo code.

Usage: python3 tools/import_poly_halloween.py PACKAGE.unitypackage PROJECT_DIRECTORY
The user's official Asset Store download is required. Original GUIDs are preserved.
"""
import hashlib
import json
from pathlib import Path, PurePosixPath
import re
import sys
import tarfile


def restore(package, project):
    package, project = Path(package), Path(project).resolve()
    root = "Assets/polyperfect/Poly Halloween"
    planned, omitted = [], []
    with tarfile.open(package, "r:gz") as archive:
        members = {m.name: m for m in archive.getmembers()}
        for member in archive.getmembers():
            if not re.fullmatch(r"[0-9a-f]{32}/pathname", member.name):
                continue
            # This Unity export appends a newline and a small trailer to pathname.
            relative = archive.extractfile(member).read().decode("utf-8").splitlines()[0]
            path = PurePosixPath(relative)
            if path.is_absolute() or ".." in path.parts or "\\" in relative:
                raise ValueError("Unsafe package path")
            if relative != "Assets/polyperfect" and not relative.startswith(root):
                raise ValueError("Unexpected package root: " + relative)
            if any(part in ("Scripts", "_Demo Scenes") for part in path.parts) or path.suffix == ".unitypackage":
                omitted.append(relative)
                continue
            prefix = member.name.rsplit("/", 1)[0]
            asset_member = members.get(prefix + "/asset")
            meta_member = members.get(prefix + "/asset.meta")
            data = archive.extractfile(asset_member).read() if asset_member and asset_member.isfile() else None
            meta = archive.extractfile(meta_member).read() if meta_member and meta_member.isfile() else None
            planned.append((relative, data, meta))

    # Check everything before writing: never replace an unrelated existing file.
    for relative, data, meta in planned:
        for name, content in ((relative, data), (relative + ".meta", meta)):
            dest = project / name
            if content is not None and dest.exists() and (not dest.is_file() or dest.read_bytes() != content):
                raise FileExistsError("Existing file differs; nothing imported: " + name)
    records = []
    for relative, data, meta in planned:
        dest = project / relative
        if data is None:
            dest.mkdir(parents=True, exist_ok=True)
        else:
            dest.parent.mkdir(parents=True, exist_ok=True)
            dest.write_bytes(data)
            records.append({"path": relative, "sha256": hashlib.sha256(data).hexdigest()})
        if meta is not None:
            dest.with_name(dest.name + ".meta").write_bytes(meta)
    report = {
        "asset": "Poly Halloween Pack", "creator": "polyperfect", "package_id": 236625,
        "official_source": "https://assetstore.unity.com/packages/3d/props/poly-halloween-pack-236625",
        "acquisition": "User added the free asset to My Assets and downloaded it with Unity Package Manager",
        "license": "Standard Unity Asset Store EULA; source assets excluded from public Git",
        "package_sha256": hashlib.sha256(package.read_bytes()).hexdigest(),
        "source_files_preserved": records, "omitted_demo_code_scenes_and_pipeline_archives": omitted,
    }
    (project / root / "LOST_DREAM_SOURCE.json").write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps({"imported_source_files": len(records), "omitted_entries": len(omitted),
                      "package_sha256": report["package_sha256"]}))


if __name__ == "__main__":
    restore(*sys.argv[1:])
