"""Preserve the previous app, add notices, sign and archive a Unity macOS export."""
import argparse
from datetime import datetime
import hashlib
import json
from pathlib import Path
import plistlib
import shutil
import subprocess
from zoneinfo import ZoneInfo

ROOT = Path(__file__).resolve().parents[1]


def sha256(path):
    h=hashlib.sha256()
    with path.open('rb') as f:
        for block in iter(lambda:f.read(1024*1024),b''): h.update(block)
    return h.hexdigest()


def package(source,version):
    destination=ROOT/'Builds/macOS/Lanterns of the Lost Dream.app'
    if source.resolve()==destination.resolve(): raise ValueError('Source must be an isolated export')
    info=plistlib.loads((source/'Contents/Info.plist').read_bytes())
    if info['CFBundleShortVersionString']!=version: raise ValueError('Export version mismatch')
    executable=source/'Contents/MacOS'/info['CFBundleExecutable']
    architecture=subprocess.check_output(['file',str(executable)],text=True).strip()
    if 'arm64' not in architecture: raise ValueError('Expected an arm64 export')
    # Never replace an app while its executable is running.
    running=subprocess.check_output(['ps','-A','-o','comm='],text=True)
    if any(str(destination/'Contents/MacOS') in line for line in running.splitlines()):
        raise RuntimeError('Close the current exported game before packaging')
    stamp=datetime.now(ZoneInfo('America/Chicago')).strftime('%Y%m%d-%H%M%S')
    if destination.exists():
        backup=ROOT/'Builds/archive'/('before-v'+version+'-'+stamp)
        backup.mkdir(parents=True);shutil.move(str(destination),backup/destination.name)
    destination.parent.mkdir(parents=True,exist_ok=True)
    shutil.copytree(source,destination,symlinks=True)
    notices=destination/'Contents/Resources/ThirdPartyNotices';notices.mkdir(parents=True,exist_ok=True)
    sources={
        'ASSET_CREDITS.md':ROOT/'ASSET_CREDITS.md',
        'Griffy-OFL.txt':ROOT/'Assets/ThirdParty/Fonts/Griffy/OFL.txt',
        'AveriaSansLibre-OFL.txt':ROOT/'Assets/ThirdParty/Fonts/AveriaSansLibre/OFL.txt',
        'GeneralUser-GS-LICENSE.txt':ROOT/'Assets/ThirdParty/Music/GeneralUser-GS-LICENSE.txt',
        'KayKit-CC0.txt':ROOT/'Assets/ThirdParty/KayKit/Spooktober/License.txt',
        'Quaternius-CC0.txt':ROOT/'Assets/ThirdParty/Quaternius/UltimateMonsters/License.txt',
    }
    for name,path in sources.items(): shutil.copy2(path,notices/name)
    for command in [
        ['codesign','--force','--deep','--sign','-','--timestamp=none','--preserve-metadata=entitlements',str(destination)],
        ['codesign','--verify','--deep','--strict',str(destination)]]:
        subprocess.run(command,check=True,capture_output=True,text=True)
    archive=ROOT/'Builds'/f'Lanterns-of-the-Lost-Dream-v{version}-macOS-arm64.zip'
    if archive.exists():
        backup=ROOT/'Builds/archive'/('existing-package-'+stamp);backup.mkdir(parents=True,exist_ok=True)
        shutil.move(str(archive),backup/archive.name)
    subprocess.run(['ditto','-c','-k','--sequesterRsrc','--keepParent',str(destination),str(archive)],check=True)
    qa=ROOT/'docs/qa'/('v'+version);qa.mkdir(parents=True,exist_ok=True)
    files={str(path.relative_to(destination)):sha256(path) for path in sorted(destination.rglob('*')) if path.is_file()}
    record={'version':version,'platform':'macOS arm64','app':str(destination.relative_to(ROOT)),
            'archive':str(archive.relative_to(ROOT)),'archive_bytes':archive.stat().st_size,
            'archive_sha256':sha256(archive),'codesign_deep_strict':True,'file_count':len(files),'files':files,
            'license_notices':list(sources),'independent_human_playtest':False}
    (qa/'package-manifest.json').write_text(json.dumps(record,indent=2)+'\n')
    print(json.dumps({k:v for k,v in record.items() if k!='files'},indent=2))


if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--source',type=Path,required=True);parser.add_argument('--version',required=True)
    args=parser.parse_args();package(args.source,args.version)
