"""Verify the checked-in historical build without implying a fresh Unity build."""
import hashlib
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1] / "docs"
manifest = json.loads((root / "Build/build-manifest.json").read_text())
proof = json.loads((root / "Build/integrity.json").read_text())
assert proof["buildGeneratedAtUtc"] == manifest["generatedAtUtc"]
assert set(proof["files"]) == set(manifest["files"])
for name, expected in proof["files"].items():
    path = (root / name).resolve()
    assert path.is_relative_to(root.resolve()), "artifact escapes public directory"
    data = path.read_bytes()
    assert len(data) == expected["bytes"], f"size mismatch: {name}"
    assert hashlib.sha256(data).hexdigest() == expected["sha256"], f"checksum mismatch: {name}"
    if name.endswith(".wasm"):
        assert data[:8] == b"\x00asm\x01\x00\x00\x00", "invalid WASM header"
print("Historical WebGL integrity OK; this does not prove a rebuild or gameplay execution.")
