# Review Guide - KBBQ Idle WebGL (Unity 2022.3 LTS)

Updated: 2026-05-30

Use this page as the short path through the repository. It keeps the review grounded in the code, docs, commands, and boundaries that are already present.

## Summary

| Field | Notes |
|---|---|
| Lane | B2C game and ad monetization |
| Core idea | Playable K-BBQ idle loop with WebGL hosting and ad-review path. |
| Primary reader | Casual game publishers, ad-supported mini-game operators, and playable portfolio reviewers. |
| Stack | Python, Docker |

## Open First

1. Start with the README fast path and architecture section.
2. Open `docs/monetization-playbook.md` only when reviewing the product or service angle.
3. Check the commands below before making claims about quality.
4. Skim the CI workflows and fixture data before deeper implementation review.
5. Read the boundaries section before presenting the project externally.

## Checks

| Purpose | Command |
|---|---|
| Test suite | `python -m pytest` |

## CI

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

## Evidence

- pytest/ruff-style local verification path
- containerized delivery path
- Unity play loop works
- WebGL docs path is clear
- Backend tests pass if backend is used

## Commercial Notes

| Possible offer | Working price assumption |
|---|---|
| Ad-supported WebGL game | Ads/IAP experiments |
| Publisher prototype | $500-$3k publisher prototype |
| Playable portfolio/game-jam asset | $2k-$8k playable polish contract |

## Boundaries

- No copied assets
- Ad compliance required
- Retention must be measured before scaling

## Useful Metrics

- Session length
- Upgrade loop conversion
- Ad RPM readiness
