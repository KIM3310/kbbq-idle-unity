# KBBQ Idle WebGL (Unity 2022.3 LTS)

K-BBQ 테마의 idle/tycoon 게임 프로젝트입니다. 최신 버전은 Unity WebGL + Cloudflare Pages 배포를 기준으로 플레이 가능하도록 구성되어 있습니다.

Updated: **February 18, 2026**

## What Is In This Version
- 4-slot grill gameplay loop (`load -> flip -> collect -> serve`)
- Pixel customer queue cards with speech bubbles + requested-cut icon
- Manual serving flow with customer eating reaction animation
- Upgrade modal UX + upgrade tier based grill visual changes
- Stronger layered sizzling audio (loop + crackle)
- Clearer HUD metrics (`$ currency`, served/customers/queue summary)
- Mobile/desktop responsive UI pass for WebGL embedding
- Cloudflare Pages + AdSense review-friendly static pages (`docs/`)

## Core Gameplay Loop
1. Buy raw meat.
2. Place meat on one of 4 grill slots.
3. Flip at the right timing.
4. Collect cooked meat to inventory.
5. Serve waiting customers for tips/combo.
6. Buy upgrades and repeat for higher throughput.

## Quick Start (Unity)
1. Open with **Unity 2022.3.62f3**.
2. Open scene: `Assets/Scenes/Main.unity`.
3. Press Play.

Optional editor helpers:
- `KBBQ/Run Auto Setup`
- `KBBQ/Validate Data (Portfolio)`

## WebGL Build
Build to `docs/`:

```bash
./tools/build_webgl_docs.sh
```

Output path:
- `docs/index.html`
- `docs/Build/*`

## Cloudflare Pages Deploy
Required settings:
- Framework preset: `None`
- Root directory: `.`
- Build command: `(none)`
- Build output directory: `docs`

Pre-deploy review gate:

```bash
./tools/release_ops.sh check
```

AdSense value injection (production):

```bash
./tools/release_ops.sh apply-adsense <ca-pub-xxxxxxxxxxxxxxxx> <slot-id>
```

## Quality Gates
Full local gate:

```bash
tools/portfolio_quality_gate.sh
```

This validates:
- Unity checks (`tools/ci_unity_checks.sh`)
- backend tests (`server/tests`)
- deterministic sim tests (`sim/`)

## Repository Guide
- Game code: `Assets/Scripts/`
- WebGL publish site: `docs/`
- Optional backend: `server/`
- Sim tests: `sim/`
- Ops/build tools: `tools/`

## Documentation
- Korean README: `README.ko.md`
- English README: `README.en.md`
- Cloudflare deploy note: `CLOUDFLARE_PAGES.md`
- Technical summaries: `PROJECT_SUMMARY.ko.md`, `PROJECT_SUMMARY.en.md`
- Service execution logs: `docs/SPECKIT_SERVICE_100.ko.md`, `docs/COT_EXECUTION_LOG.ko.md`

## License
MIT (`LICENSE`)
