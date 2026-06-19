# KBBQ Idle WebGL (Unity 2022.3 LTS)

KBBQ Idle is an idle/tycoon game project focused on a grill-management gameplay loop. The current version is tuned for Unity WebGL deployment on Cloudflare Pages.

Updated: **February 18, 2026**

## Product and System Surface

A playable Unity WebGL project that proves shipping discipline through game loop, build pipeline, and release checks.

| Lens | Definition |
|---|---|
| Technical stack | Unity 2022.3 LTS, WebGL publishing, static hosting, optional FastAPI backend, and deterministic economy tests. |
| Architecture path | Validate the demo, README, architecture notes, and quality gate before deeper workflow architecture. |
| System signal | Playable WebGL surface, Unity validation scripts, release checks, gameplay loop, responsive UI, and policy-ready static pages. |
| Safety boundary | gameplay data should stay separate from personal data. |
| Fast path | Run `tools/runtime_quality_gate.sh` or the WebGL release check before publishing. |

## What Changed In This Build
- 4 grill slots for active cooking flow
- Pixel customer queue cards with speech bubbles and requested-cut icons
- Manual serving with customer eating reaction animation
- Upgrade modal UX and grill visual tier progression
- Stronger layered sizzling audio (loop + crackle)
- Clearer UI metrics (`$ currency`, served/customers/queue)
- Responsive layout pass for desktop + mobile WebGL
- Policy-ready static pages under `docs/` for architecture workflows

## Gameplay Loop
1. Buy raw meat.
2. Place meat on a grill slot.
3. Flip at the right time.
4. Collect cooked meat.
5. Serve waiting customers.
6. Upgrade and scale throughput.

## Quick Start (Unity)
1. Open project with **Unity 2022.3.62f3**.
2. Load `Assets/Scenes/Main.unity`.
3. Press Play.

Optional editor commands:
- `KBBQ/Run Auto Setup`
- `KBBQ/Validate Data`

## WebGL Build
Generate WebGL output to `docs/`:

```bash
./tools/build_webgl_docs.sh
```

Main entry:
- `docs/index.html`

## Cloudflare Pages Deployment
Use these settings:
- Framework preset: `None`
- Root directory: `.`
- Build command: `(none)`
- Build output directory: `docs`

Run deployment architecture checks:

```bash
./tools/release_ops.sh check
```

```bash
```

## Quality Gate
Run full local validation:

```bash
tools/runtime_quality_gate.sh
```

It executes Unity checks, backend tests, and deterministic simulator tests.

## Repo Map
- Gameplay/UI code: `Assets/Scripts/`
- WebGL publish site: `docs/`
- Optional backend: `server/`
- Deterministic sim tests: `sim/`
- Ops/build scripts: `tools/`

## Related Docs
- Korean README: `README.ko.md`
- Main README: `README.md`
- Cloudflare deployment note: `docs/deployment/CLOUDFLARE_PAGES.md`
- Technical summaries: `docs/architecture/PROJECT_SUMMARY.en.md`, `docs/architecture/PROJECT_SUMMARY.ko.md`

## License
MIT (`LICENSE`)
