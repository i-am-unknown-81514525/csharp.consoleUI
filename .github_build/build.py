name: dict[str, str] = {
    "Prototype":"Prototype.exe",
    "Test": "StdinRepeat.exe"
}

import pathlib
import os, sys

arch_arg = sys.argv[1] if sys.argv else "x86"
if arch_arg == "any":
    arch_arg = "Any CPU"
elif arch_arg not in ["x86", "ARM64"]:
    arch_arg = "x86"

base = """
using ui;
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
    os.system(f"msbuild github_workdlow_build.sln -maxCpuCount:4 -p:Platform=\"{arch_arg}\" -p:OutputPath=.tmp_build")
    for file in tmp_build_dir.rglob("*.exe"):
        file.rename(build_dir / out_name)


    