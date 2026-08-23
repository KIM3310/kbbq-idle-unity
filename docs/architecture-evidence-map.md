# Review Guide - KBBQ Idle WebGL (Unity 2022.3 LTS)

Updated: 2026-05-30

This repository is now curated as supporting material. Use it only after the flagship enterprise AI, operations, security, data, and runtime reliability projects have established the main story.

## Summary

| Field | Notes |
|---|---|
| Repository | `kbbq-idle-unity` |
| Status | Supporting (active; not a flagship) |
| Lane | B2C gameplay prototype |
| Technical stack | Unity WebGL, static hosting, optional FastAPI backend, deterministic simulation tests. |
| Why it moved back | Consumer gameplay work distracts from the enterprise AI and infrastructure story. |
| Current successor | doeon-kim-portfolio and the B2B flagship repositories |

## Open First

1. Start with the successor repositories named above.
2. Use this repository only for optional domain breadth or historical product exploration.
3. Check `docs/repository-positioning.md` before presenting it externally.
4. Keep its active supporting (not flagship) status visible in any external writeup or technical walkthrough.

## Evidence

- Unity play loop works
- WebGL docs path is clear
- Backend tests pass if backend is used

## Architecture Notes

| Possible next step | Working scope assumption | Scope |
|---|---|---|
| WebGL gameplay prototype | Optional economy experiments | Scoped after review. |
| Publisher prototype | Scope after product intake | Scoped after review. |
| Playable game-jam asset | Scope after product intake | Scoped after review. |

## Boundaries

- No copied assets
- Platform policy review required
- Retention must be measured before scaling
- Consumer gameplay work distracts from the enterprise AI and infrastructure story.
- Do not present this as a current flagship or maintained product surface.

## Useful Metrics

- Session length
- Upgrade loop clarity
- Session-quality readiness
