# Search Growth Implementation - KBBQ Idle Unity

This repository now exposes a search-readable service surface in addition to the system architecture. The implementation is designed to support organic discovery, AI answer surfaces, and a free-to-paid service path without committing to paid infrastructure first.

## Implemented Surface

| Surface | Path |
| --- | --- |
| Machine-readable offer | [docs/service-offer.json](./service-offer.json) |
| Revenue architecture | [docs/revenue-architecture.md](./revenue-architecture.md) |
| System architecture | [docs/system-architecture.md](./system-architecture.md) |
| Public canonical URL | https://kbbq-idle-unity.pages.dev/ |
| Lead capture URL | https://kim3310-doeon-kim-portfolio.pages.dev/?offer=kbbq-idle-unity&inquiry=consumer-prototype-customization#private-inquiry |
| Commercial route | https://kim3310-doeon-kim-portfolio.pages.dev/?offer=kbbq-idle-unity#service-offers |

## Search Positioning

- Primary query: KBBQ Idle Unity Korean BBQ game
- Secondary queries: KBBQ Idle Unity demo; KBBQ Idle Unity system architecture; KBBQ Idle Unity game tool; Korean BBQ idle game with progression, leaderboard, events, and collectible content service
- Public entry point: free WebGL build on Pages/itch.io
- Paid boundary: private prototype customization for branded WebGL review builds, content planning, and monetization-readiness handoff

## Conversion Boundary

The public surface stays crawlable and free. Paid value starts when a visitor wants private data, saved history, branded export packs, customer-specific connectors, recurring reports, or implementation support.

## Deployment Notes

- Keep the sitemap and robots file aligned with the final production domain.
- Submit the canonical URL and sitemap in Google Search Console after the domain is connected.
- The lead-capture path is the central private inquiry route for the `consumer-prototype-customization` lane; no self-serve checkout is configured.
- Ads and IAP remain disabled by default. Public copy should describe monetization-readiness work, not live ad inventory, active mobile checkout, or verified purchase fulfillment.
- Keep exact free-tier quotas out of public promises because provider limits change.
