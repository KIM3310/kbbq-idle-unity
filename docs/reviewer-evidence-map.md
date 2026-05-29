# Reviewer Evidence Map - KBBQ Idle WebGL (Unity 2022.3 LTS)

Updated: 2026-05-29

This document is the short path for a recruiter, hiring manager, technical reviewer, or buyer who wants to understand what this repository proves without wandering through every file.

## One-Line Proof

**B2C game and ad monetization.** Playable K-BBQ idle loop with WebGL hosting and ad-review path.

## Audience and Commercial Angle

| Lens | Answer |
|---|---|
| Primary reviewer | Casual game publishers, ad-supported mini-game operators, and playable portfolio reviewers. |
| Hiring signal | Can the project be explained, verified, bounded, and extended like a real product surface? |
| Buyer signal | Is there a narrow operational pain, a runnable proof path, and a risk-aware pilot shape? |
| Stack signal | Python, Docker |

## Seven-Minute Review Route

1. Read the README `Product and Review Surface` and `Reviewer Fast Path` sections.
2. Open `docs/monetization-playbook.md` to understand the buyer, offer ladder, and GTM hypothesis.
3. Run or inspect the strongest local quality gate below.
4. Inspect CI workflow definitions and test fixtures before deeper implementation review.
5. Check the risk boundaries so claims stay credible and not overextended.

## Verification Commands

| Purpose | Command |
|---|---|
| Test suite | `python -m pytest` |

## CI and Automation Surface

- .github/workflows/architecture-blueprint.yml
- .github/workflows/backend-deploy.yml
- .github/workflows/backend-ops-monitor.yml
- .github/workflows/ci.yml
- .github/workflows/dependency-review.yml
- .github/workflows/pages-auto-deploy.yml
- .github/workflows/pages.yml
- .github/workflows/production-smoke.yml
- .github/workflows/repository-health.yml
- .github/workflows/repository-surface.yml
- .github/workflows/secret-scan.yml

## Evidence Inventory

- pytest/ruff-style local verification path
- containerized delivery path
- Unity play loop works
- WebGL docs path is clear
- Backend tests pass if backend is used

## Commercialization Snapshot

| Offer | Pricing hypothesis |
|---|---|
| Ad-supported WebGL game | Ads/IAP experiments |
| Publisher prototype | $500-$3k publisher prototype |
| Playable portfolio/game-jam asset | $2k-$8k playable polish contract |

## Risk Boundaries

- No copied assets
- Ad compliance required
- Retention must be measured before scaling

## Metrics That Matter

- Session length
- Upgrade loop conversion
- Ad RPM readiness

## Review Verdict

This repository should be evaluated as part of the broader KIM3310 portfolio: it is strongest when the reviewer sees the link between a concrete implementation, a documented verification path, and a monetizable or employable operating story.
