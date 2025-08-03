name: dict[str, str] = {
    "Prototype":"Prototype.exe",
    "Test": "StdinRepeat.exe",
    "FracParseTest": "FracParseTest.exe",
    "ButtonTest": "ButtonTest.exe",
    "GroupItemTest": "MultiButtonTest.exe",
    "InputFieldTest": "InputFieldTest.exe",
    "MultiFieldTest": "MultiFieldTest.exe",
}

import pathlib
import shutil
import os, sys

arch_arg = sys.argv[1] if len(sys.argv) >= 2 else "x64"
if arch_arg == "any":
    arch_arg = "Any CPU"
elif arch_arg not in ["x64", "ARM64"]:
    print(f"Architecture {arch_arg} is unknown, default to x64")
    arch_arg = "x64"

base = """
using ui.test;
internal class Program {
    public static void Main(string[] _) => [CLSNAME].Setup();
}
"""
cwd = pathlib.Path(os.getcwd())
if (cwd / "ui").is_dir():
    cwd = cwd / "ui"
if (cwd / ".github_build").is_dir():
    cwd = cwd / ".github_build"

os.chdir(cwd.as_posix())

build_dir = cwd / ".build"
build_dir.mkdir(exist_ok=True)
tmp_build_dir = cwd / ".tmp_build"

for cls_name, out_name in name.items():
    with open("File.cs", "w") as fp:
        content = base.replace("[CLSNAME]", cls_name)
        fp.write(content)
    os.system("rm -rf obj || true")
    os.system("rm -rf bin || true")
    os.system("rm -rf .tmp_build || true")
    tmp_build_dir.mkdir(exist_ok=True)
    status = os.system(f"msbuild github_workflow_build.sln -maxCpuCount:4 -p:Platform=\"{arch_arg}\" -p:OutputPath=\"{tmp_build_dir.as_posix()}\"") # `-restore` for future me if nuget package is installed
    if status != 0:
        raise ChildProcessError(f"Failed to compile with non-zero status code: {status}")
    for file in tmp_build_dir.glob("*.exe"):
        shutil.move(file, build_dir / out_name)
        print(f"Moved {file.as_posix()} -> {(build_dir / out_name).as_posix()}")
    for file in tmp_build_dir.glob("*.dll"):
        if (build_dir / file.name).exists():
            continue
        shutil.move(file, build_dir / file.name)
        print(f"Moved {file.as_posix()} -> {(build_dir / file.name).as_posix()}")
