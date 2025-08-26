# Project Roadmap (2025)

This roadmap outlines the planned directions for dotnet.push. Timelines are approximate and may change based on feedback and capacity.

## Vision
- Simple, reliable .NET library for APNs (iOS) and FCM (Android).
- Modern runtimes first (.NET 8/9), minimal dependencies, clear APIs, solid docs.

## Near-term (0–3 months)
- FCM HTTP v1 API support (OAuth2 service account) in addition to legacy server key endpoint.
- APNs options surface: expose topic, collapse-id, expiration, priority as first-class parameters.
- Cancellation/timeout and retry/backoff guidance; better error mapping and messages.
- Logging hooks (ILogger) and basic metrics counters.
- Samples refresh and small integration test(s) for happy-path calls.

## Mid-term (3–6 months)
- Connection reuse/health improvements and token refresh ergonomics for APNs.
- Strongly-typed payload builders (iOS/Android) with validation.
- Topic/multicast helpers and request batching utilities.
- Improved exception model and result types with actionable details.
- CI enhancements (matrix for .NET 8/9) and publishing automation.

## Long-term (6–12 months)
- Pluggable transports/config abstractions to enable advanced scenarios.
- Advanced telemetry (EventCounters/OpenTelemetry) and redaction-safe logs.
- Expanded samples (Minimal API, Worker Service) and docs (troubleshooting, cookbook).

## Non-goals (for now)
- Push provider dashboard/UX.
- Device token registration backend.

## Versioning & releases
- SemVer. Minor versions for additive features; major for breaking API changes.
- Target frameworks: .NET 8/9. Older TFMs are not planned.

## Proposing items
Open an issue with [Roadmap] in the title describing motivation, scope, and expected users.
