# Cloudflare Pages Deployment (KBBQ Idle WebGL)

Updated: **February 18, 2026**

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
Run policy/ad/deploy checks:

```bash
./tools/release_ops.sh check
```

Expected result:
- `PASS review gate`

## AdSense Values
Placeholders exist by default. For production values:

```bash
./tools/release_ops.sh apply-adsense <ca-pub-xxxxxxxxxxxxxxxx> <slot-id>
```

Then run:

```bash
./tools/release_ops.sh check
```

Note:
- Placeholder warnings are expected before real AdSense onboarding.

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
