# KBBQ Idle Unity — implementation and verification

Reviewed 2026-09-07. The repository and linked tests define the evidence; a preview alone does not establish production readiness.

### Compile one reward formula

Unity and the .NET regression project compile the exact same file; the simulator no longer maintains a separate copy of offline reward math.

### Zero cap means zero reward

Disabled caps, missing/future timestamps, non-finite values and overflowing rewards return zero. The hours-to-seconds multiplication uses 64-bit arithmetic.

### Preserve artifact provenance

Historical WebGL files remain intact and carry their original generation date and integrity checks; they are never relabeled as a current-source build.

## Reproduce

```sh
dotnet test sim/KbbqIdle.Sim.Tests
make verify
python3 scripts/verify_webgl_integrity.py
```

12 .NET tests pass, including zero/negative caps, invalid/overflowing income and extreme clocks. 10 backend tests and repository/architecture/monetization checks pass. The existing WebGL files match recorded hashes and the complete WASM module validates.

## Boundaries

The playable WebGL artifact was generated on 2026-02-20 and has not been rebuilt from this revision. The new shared economy fix is verified by .NET, not by a fresh Unity player build. Unity Editor/gameplay execution and release rebuild require an available licensed Unity environment.

## Attribution

This page describes capabilities visible in the repository. It does not independently establish which lines were written manually, with AI assistance, or by collaborators. The commit history and pull-request diffs preserve the implementation trail; individual/team contribution percentages have not been inferred.

[Back to the project](../README.md)
