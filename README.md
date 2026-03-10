# KBBQ Idle WebGL (Unity 2022.3 LTS)

K-BBQ 테마의 idle/tycoon 게임 프로젝트입니다. 최신 버전은 Unity WebGL 빌드와 GitHub/Cloudflare reviewer surface까지 함께 검토할 수 있도록 정리되어 있습니다.

Updated: **February 18, 2026**

## 포트폴리오 포지셔닝
- 이 저장소는 게임 플레이 루프와 배포 준비도를 함께 보여주는 WebGL 포트폴리오입니다.
- 핵심 증거는 실제 플레이 가능한 빌드, build preflight, reviewer surface, 그리고 배포 문서입니다.
- 빠른 검토 경로: Reviewer Surface -> Review Flow -> docs/Build preflight 순서로 보면 됩니다.

## 커리어 시그널
- **인터랙티브/런타임 엔지니어 관점:** 실제 게임 루프, WebGL 빌드, build preflight까지 한 흐름으로 검토할 수 있습니다.
- **플랫폼/클라우드 아키텍트 관점:** reviewer surface, 배포 문서, 정적 호스팅 경로가 명확하게 정리되어 있습니다.
- **제품/필드 관점:** 바로 플레이 가능한 결과물이 있어서 슬라이드보다 설득력이 높습니다.

## Start Here
- Primary game surface: `Assets/`, `Packages/`, `ProjectSettings/`
- WebGL publish surface: `docs/`
- Optional backend surface: `server/`
- Deterministic economy test harness: `sim/`
- Build/release helpers: `tools/`

루트에는 리뷰 문서와 운영 자산이 함께 있지만, 실제 게임 작업은 보통 `Assets/`에서 시작하고 배포 검증은 `docs/` 또는 `server/`로 이어집니다.

## What Is In This Version
- 4-slot grill gameplay loop (`load -> flip -> collect -> serve`)
- Pixel customer queue cards with speech bubbles + requested-cut icon
- Manual serving flow with customer eating reaction animation
- Upgrade modal UX + upgrade tier based grill visual changes
- Stronger layered sizzling audio (loop + crackle)
- Clearer HUD metrics (`$ currency`, served/customers/queue summary)
- Gameplay summary surfaced directly in the perf overlay
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

## Repository Map
- Game code: `Assets/Scripts/`
- WebGL publish site: `docs/`
- Optional backend: `server/`
- Sim tests: `sim/`
- Ops/build tools: `tools/`
- Supporting review docs: `README.ko.md`, `README.en.md`, `docs/ops/RUNBOOK.md`, `docs/deployment/CLOUDFLARE_PAGES.md`

## Docs Map
- `README.md`: primary project overview and verification flow
- `README.ko.md`, `README.en.md`: language-specific summaries
- `docs/ops/RUNBOOK.md`: release/ops handoff notes
- `docs/deployment/CLOUDFLARE_PAGES.md`: WebGL publishing notes for Pages
- `server/README.md`: backend-only setup and API notes

## Reviewer Surface
- Unity perf overlay now exposes `kbbq-idle-review-pack-v1` with live tier, queue, income, and monetization posture.
- Optional backend exposes `GET /health`, `GET /meta`, `GET /readiness`, and `GET /review-pack` for delivery and ops review.
- Recommended review order: inspect backend posture, play the grill loop in Unity/WebGL, then verify monetization remains optional.

## Review Flow
- Open `/health` and `/meta` to confirm runtime posture, enabled surfaces, and next action.
- Open `/review-pack` to pin gameplay loop, economy contract, and monetization posture.
- Run the Unity WebGL build or Editor scene and validate the grill -> serve -> upgrade loop.
- Read the live perf overlay before claiming delivery or monetization readiness.

## Proof Assets
- `Health Envelope` -> `/health`
- `Runtime Profile` -> `/meta`
- `Review Pack` -> `/review-pack`
- `Perf Overlay` -> `Assets/Scripts/UI/PerfOverlayView.cs`

## Documentation
- Korean README: `README.ko.md`
- English README: `README.en.md`
- Cloudflare deploy note: `docs/deployment/CLOUDFLARE_PAGES.md`
- Technical summaries: `docs/review/PROJECT_SUMMARY.ko.md`, `docs/review/PROJECT_SUMMARY.en.md`

## License
MIT (`LICENSE`)

## Local Verification
```bash
python3 -m venv .venv
source .venv/bin/activate
python -m pip install -U pip
python -m pip install -e ".[dev]"
tools/portfolio_quality_gate.sh
```

## Repository Hygiene
- Keep runtime artifacts out of commits (`.codex_runs/`, cache folders, temporary venvs).
- Prefer running verification commands above before opening a PR.
