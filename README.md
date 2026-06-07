# KBBQ Idle WebGL (Unity 2022.3 LTS)

> **Curated supporting repo**
> This repository is kept as optional proof, but it no longer leads the portfolio.
> Current front door: **doeon-kim-portfolio and the B2B flagship repositories**.
> Reason: Consumer gameplay work distracts from the enterprise AI and infrastructure portfolio signal.

K-BBQ 테마의 idle/tycoon 게임 프로젝트입니다. 최신 버전은 Unity WebGL 빌드와 GitHub/Cloudflare reviewer surface까지 함께 검토할 수 있도록 정리되어 있습니다.

Updated: **February 18, 2026**

## 포트폴리오 포지셔닝
- 이 저장소는 “웹에 올린 Unity 빌드”보다, 실제 플레이 루프와 배포 검증을 같이 보여주는 게임 런타임 포트폴리오에 가깝습니다.
- 핵심 증거는 플레이 가능한 빌드, build preflight, reviewer surface, 그리고 배포 전 점검 문서가 한 세트로 이어지는지입니다.
- 빠르게 보려면 reviewer surface -> core loop 설명 -> docs/Build preflight 순서로 보는 편이 좋습니다.

## 커리어 시그널
- **인터랙티브/런타임 엔지니어 관점:** 실제 게임 루프, WebGL 빌드, build preflight까지 한 흐름으로 검토할 수 있습니다.
- **플랫폼/클라우드 아키텍트 관점:** reviewer surface, 배포 문서, 정적 호스팅 경로가 명확하게 정리되어 있습니다.
- **제품/필드 관점:** 바로 플레이 가능한 결과물이 있어서 슬라이드보다 설득력이 높습니다.

## Product and Review Surface

| Lens | Decision signal |
|---|---|
| Reviewer | Casual WebGL game reviewers, Unity prototype reviewers, and portfolio reviewers looking for a playable loop rather than a mockup. |
| Product proof | The demo, workflow loop, and static proof surface show the current product direction without extra claims. |
| Reviewer proof | Unity scene, docs front door, build preflight, deterministic economy harness, and reviewer pages make the runtime inspectable in minutes. |
| Safety posture | Placeholder behavior is explicit when WebGL artifacts are absent, and optional server paths are separated from the core playable review path. |

## Reviewer Fast Path

- **First minute:** Open `Assets/Scenes/Main.unity`, play the grill loop, then review the WebGL `docs/` surface.
- **Local demo:** Build WebGL with `./tools/build_webgl_docs.sh`; optional backend review starts from `server/`.
- **Verification:** Run `python -m pytest` for the optional backend and `KBBQ/Validate Data (Portfolio)` inside Unity for data checks.

## Service Launch Playbook

- [Service launch playbook](docs/service-launch-playbook.md) maps the repository to review audiences, proof gates, operating boundaries, and risk controls.

## Review Notes

- [Review guide](docs/reviewer-evidence-map.md) summarizes the project angle, first files to inspect, verification commands, and known boundaries.
- [Quality notes](docs/quality-gate.md) lists the local checks, CI surface, and release expectations for this repository.
- [Enterprise readiness notes](docs/enterprise-readiness.md) outlines security, data, operations, integration, and handoff expectations.
- [Portfolio fit](docs/portfolio-fit.md) explains why this repository is archived/supporting and where the current portfolio front door lives.

## 포트폴리오 맥락
- **패밀리:** 사람 중심 / 인터랙티브 제품군
- **이 레포의 역할:** 실제 플레이 루프와 배포 검증을 같이 보여주는 인터랙티브 런타임 포트폴리오입니다.
- **연결해서 볼 레포:** `SteadyTap`, `ecotide`, `the-savior`

## Start Here
- Primary game surface: `Assets/`, `Packages/`, `ProjectSettings/`
- WebGL publish surface: `docs/`
- Optional backend surface: `server/`
- Deterministic economy test harness: `sim/`
- Build/release helpers: `tools/`

루트에는 리뷰 문서와 운영 자산이 함께 있지만, 실제 게임 작업은 `Assets/`에서 시작하고 배포 검증은 `docs/` 또는 `server/`에서 이어집니다.

## First Reviewer Move
- 실제 게임 런타임을 보려면 `Assets/Scenes/Main.unity`를 열고 Play를 누르세요. 이 경로가 기준 런타임입니다.
- `docs/`는 Cloudflare Pages용 정적 reviewer surface입니다. `docs/Build/`에 실제 WebGL 산출물이 없으면 placeholder가 보이는 것이 정상입니다.
- `server/`는 delivery/ops 증거를 위한 선택 경로이며, 기본 플레이 루프 리뷰의 필수 조건은 아닙니다.

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

Production review configuration:

```bash
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
- Unity perf overlay now exposes `kbbq-idle-review-pack-v1` with live tier, queue, income, and service launch posture.
- Optional backend exposes `GET /health`, `GET /meta`, `GET /readiness`, `GET /review-pack`, and `GET /ops/release-readiness` for delivery and ops review.
- Recommended review order: inspect backend posture, play the grill loop in Unity/WebGL, then verify service launch remains optional.

## Review Flow
- Open `/health` and `/meta` to confirm runtime posture, enabled surfaces, and next action.
- Open `/ops/release-readiness` before `/review-pack` to confirm launch blockers, operator rules, and release posture.
- Open `/review-pack` to pin gameplay loop, economy contract, and service launch posture.
- Run the Unity WebGL build or Editor scene and validate the grill -> serve -> upgrade loop.
- Read the live perf overlay before claiming delivery or service launch readiness.

## Proof Assets
- `Health Envelope` -> `/health`
- `Runtime Profile` -> `/meta`
- `Release Readiness` -> `/ops/release-readiness`
- `Review Pack` -> `/review-pack`
- `Economy Balance Drill` -> `/ops/economy-balance-drill`
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

## Cloud + AI Architecture

This repository includes a neutral cloud and AI engineering blueprint that maps the current proof surface to runtime boundaries, data contracts, model-risk controls, deployment posture, and validation hooks.

- [Cloud + AI architecture blueprint](docs/cloud-ai-architecture.md)
- [Machine-readable architecture manifest](docs/architecture/blueprint.json)
- Validation command: `python3 scripts/validate_architecture_blueprint.py`

## Enterprise Productization

- [Product operating model](docs/product-operating-model.md) defines the reviewer, trust boundary, trust boundary, operating checks, and service path for this repository.

## Service Architecture

- [Service architecture](docs/service-architecture.md) defines the cloud resources, account information, cost controls, and production guardrails needed to turn this repo into a scoped service without publishing public financial assumptions.
