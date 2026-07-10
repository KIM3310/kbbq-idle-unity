# KBBQ Idle WebGL (Unity 2022.3 LTS)

> **Curated supporting repo**
> This repository is kept as optional product proof beside the B2B flagship repositories.
> Current technical entry points: **doeon-kim-portfolio and the B2B flagship repositories**.
> Reason: Consumer gameplay work is useful as runtime evidence, but it is not the main enterprise AI story.

K-BBQ 테마의 idle/tycoon 게임 프로젝트입니다. 최신 버전은 Unity WebGL 빌드, 정적 배포 표면, 선택형 FastAPI 백엔드까지 함께 확인할 수 있도록 정리되어 있습니다.

Updated: **February 18, 2026**

## 기술 포지셔닝
- 이 저장소는 “웹에 올린 Unity 빌드”보다, 실제 플레이 루프와 배포 검증을 같이 보여주는 게임 런타임 샘플에 가깝습니다.
- 핵심 증거는 플레이 가능한 빌드, build preflight, 정적 배포 표면, 그리고 배포 전 점검 문서가 한 세트로 이어지는지입니다.
- 빠르게 보려면 WebGL surface -> core loop 설명 -> docs/Build preflight 순서로 보는 편이 좋습니다.

## 기술 스택 신호
- **Unity 2022.3 LTS:** 실제 게임 루프, WebGL 빌드, build preflight까지 한 흐름으로 확인할 수 있습니다.
- **Static hosting:** 배포 문서, 정적 호스팅 경로, fallback UI가 명확하게 정리되어 있습니다.
- **FastAPI optional backend:** 리더보드, analytics, release readiness 경로를 선택적으로 붙일 수 있습니다.

## System Overview

| Lens | Decision signal |
|---|---|
| Technical stack | Unity WebGL runtime, static hosting, optional FastAPI backend, and deterministic economy harness. |
| Product proof | The demo, workflow loop, and static proof surface show the current product direction without extra claims. |
| Review proof | Unity scene, docs front door, build preflight, deterministic economy harness, and runtime pages make the system reviewable in minutes. |
| Safety posture | Fallback behavior is explicit when WebGL artifacts are absent, and optional server paths are separated from the core playable review path. |

## Evaluation Path

- **Start here:** Open `Assets/Scenes/Main.unity`, play the grill loop, then inspect the WebGL `docs/` surface.
- **Local demo:** Build WebGL with `./tools/build_webgl_docs.sh`; optional backend review starts from `server/`.
- **Checks:** Run `python -m pytest` for the optional backend and `KBBQ/Validate Data` inside Unity for data checks.

## Service Launch Playbook

- [Service launch playbook](docs/service-launch-playbook.md) maps the repository to its product scope, evidence gates, operating boundaries, and risk controls.

## Architecture Notes

- [Review guide](docs/architecture-evidence-map.md) summarizes the system scope, first files to inspect, verification commands, and known boundaries.
- [Quality notes](docs/quality-gate.md) lists the local checks, CI surface, and release expectations for this repository.
- [Enterprise readiness notes](docs/enterprise-readiness.md) outlines security, data, operations, integration, and handoff expectations.
- [Repository positioning](docs/repository-positioning.md) explains why this repository is archived/supporting and where the current technical entry points live.

## 프로젝트 맥락
- **패밀리:** 사람 중심 / 인터랙티브 제품군
- **이 레포의 역할:** 실제 플레이 루프와 배포 검증을 같이 보여주는 인터랙티브 런타임 샘플입니다.
- **연결해서 볼 레포:** `SteadyTap`, `ecotide`, `the-savior`

## Start Here
- Primary game surface: `Assets/`, `Packages/`, `ProjectSettings/`
- WebGL publish surface: `docs/`
- Optional backend surface: `server/`
- Deterministic economy test harness: `sim/`
- Build/release helpers: `tools/`

루트에는 설계 문서와 운영 자산이 함께 있지만, 실제 게임 작업은 `Assets/`에서 시작하고 배포 검증은 `docs/` 또는 `server/`에서 이어집니다.

## First Review Move
- 실제 게임 런타임을 보려면 `Assets/Scenes/Main.unity`를 열고 Play를 누르세요. 이 경로가 기준 런타임입니다.
- `docs/`는 Cloudflare Pages용 정적 런타임 surface입니다. `docs/Build/`에 실제 WebGL 산출물이 없으면 fallback screen이 보이는 것이 정상입니다.
- `server/`는 delivery/ops 증거를 위한 선택 경로이며, 기본 플레이 루프 확인의 필수 조건은 아닙니다.

## What Is In This Version
- 4-slot grill gameplay loop (`load -> flip -> collect -> serve`)
- Pixel customer queue cards with speech bubbles + requested-cut icon
- Manual serving flow with customer eating reaction animation
- Upgrade modal UX + upgrade tier based grill visual changes
- Stronger layered sizzling audio (loop + crackle)
- Clearer HUD metrics (`$ currency`, served/customers/queue summary)
- Gameplay summary surfaced directly in the perf overlay
- Mobile/desktop responsive UI pass for WebGL embedding
- Cloudflare Pages + review-friendly static pages (`docs/`)

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
- `KBBQ/Validate Data`

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

Production review configuration:

```bash
```

## Quality Gates
Full local gate:

```bash
tools/runtime_quality_gate.sh
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

## Review Surface
- Unity perf overlay now exposes `kbbq-idle-architecture-pack-v1` with live tier, queue, income, and service launch posture.
- Optional backend exposes `GET /health`, `GET /meta`, `GET /readiness`, `GET /architecture-pack`, and `GET /ops/release-readiness` for delivery and ops review.
- Recommended review order: inspect backend posture, play the grill loop in Unity/WebGL, then verify service launch remains optional.

## Review Flow
- Open `/health` and `/meta` to confirm runtime posture, enabled surfaces, and next action.
- Open `/ops/release-readiness` before `/architecture-pack` to confirm launch blockers, operator rules, and release posture.
- Open `/architecture-pack` to pin gameplay loop, economy contract, and service launch posture.
- Run the Unity WebGL build or Editor scene and validate the grill -> serve -> upgrade loop.
- Read the live perf overlay before claiming delivery or service launch readiness.

## Proof Assets
- `Health Envelope` -> `/health`
- `Runtime Profile` -> `/meta`
- `Release Readiness` -> `/ops/release-readiness`
- `Review Pack` -> `/architecture-pack`
- `Economy Balance Drill` -> `/ops/economy-balance-drill`
- `Perf Overlay` -> `Assets/Scripts/UI/PerfOverlayView.cs`

## Documentation
- Korean README: `README.ko.md`
- English README: `README.en.md`
- Cloudflare deploy note: `docs/deployment/CLOUDFLARE_PAGES.md`
- Technical summaries: `docs/architecture/PROJECT_SUMMARY.ko.md`, `docs/architecture/PROJECT_SUMMARY.en.md`

## License
MIT (`LICENSE`)

## Local Verification
```bash
make verify
```

Requires Python 3.11+. If your default `python3` is older, run `make PYTHON=/path/to/python3.11 verify`.

## Repository Hygiene
- Keep runtime artifacts out of commits (`.codex_runs/`, cache folders, temporary venvs).
- Prefer running runtime commands above before opening a PR.

## Cloud + AI Architecture

- [Cloud + AI architecture blueprint](docs/cloud-ai-architecture.md)
- [Machine-readable architecture manifest](docs/architecture/blueprint.json)
- Validation command: `python3 scripts/validate_architecture_blueprint.py`

## Enterprise Productization

- [Product operating model](docs/product-operating-model.md) defines the product scope, trust boundary, operating checks, and service path for this repository.

## System Architecture

- [System architecture](docs/system-architecture.md) maps the runtime boundary, data/control flow, cloud or local deployment surface, and operating assumptions for this repository.

## Service Architecture

- [Service architecture](docs/service-architecture.md) defines the cloud resources, account information, cost controls, and production guardrails needed to turn this repo into a scoped service without publishing public financial assumptions.

<!-- search-growth-readme:start -->

## Search And Service Surface

- Public entry: free WebGL build on Pages/itch.io
- Paid boundary: cosmetic packs, ad-free mode, event boosts, and supporter bundle
- Canonical URL: https://kbbq-idle-unity.pages.dev/
- Lead capture: https://github.com/KIM3310/kbbq-idle-unity/issues/new?template=service-inquiry.yml&title=Private+workspace+inquiry%3A+KBBQ+Idle+Unity
- Machine-readable offer: [docs/service-offer.json](docs/service-offer.json)
- Search growth implementation: [docs/search-growth-implementation.md](docs/search-growth-implementation.md)
- Revenue architecture: [docs/revenue-architecture.md](docs/revenue-architecture.md)

<!-- search-growth-readme:end -->
