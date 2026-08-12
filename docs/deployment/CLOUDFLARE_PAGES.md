# Cloudflare Pages Deployment (KBBQ Idle WebGL)

## Required Pages Settings
- Framework preset: `None`
- Root directory: `.`
- Build command: `(none)`
- Build output directory: `docs`

This repo ships prebuilt WebGL assets in `docs/Build`, so no cloud build command is required.

## Deploy Flow
1. Build WebGL locally:

```bash
./tools/build_webgl_docs.sh
```

2. Commit and push (`docs/Build/*` included).
3. Cloudflare Pages (GitHub-connected) auto-deploys from `main`.

## Pre-Deploy Gate
Run policy/deploy checks:

```bash
./tools/release_ops.sh check
```

Expected result:
- `PASS review gate`

Placeholders exist by default. After replacing them, run the strict gate:

```bash
STRICT_EXTERNAL_SCRIPT_VALUES=1 ./tools/release_ops.sh check
```

Then run:

```bash
./tools/release_ops.sh check
```

Note:
- Placeholder warnings are expected before real external script onboarding.

## Runtime Entry
- WebGL page: `docs/index.html`
- Loader verifies `docs/Build` artifacts and supports `.unityweb/.br/.gz/plain` patterns.

## Common Failures
- `Failed: build output directory not found`
  - Output directory is not `docs`. Set it to `docs`.
- `Build check failed` on play page
  - Missing WebGL artifacts in `docs/Build`.
  - Re-run `./tools/build_webgl_docs.sh` and push again.
- Black/blank canvas after update
  - Hard refresh to invalidate old cached loader/wasm/data files.
