# ADR-0002 — Core "brain" strategy: first-party port

- Status: **Accepted**
- Date: 2026-06-02
- Deciders: user ("rewrite from scratch", then "you decide best")

## Context

`Blazor.Diagrams.Core` (NuGet `Z.Blazor.Diagrams.Core`, latest 3.0.4.1, MIT) is the UI-agnostic engine
behind Blazor.Diagrams. Source-first verification (see `02-research/blazor-diagrams-architecture.md`):

- Its **only** dependency is `SvgPathProperties` (pure .NET path math). **No Blazor dependency.**
- It defines its **own** event types (`Events/PointerEventArgs`, `WheelEventArgs`, `KeyboardEventArgs`,
  `MouseEventArgs`, `TouchEventArgs`) — not `Microsoft.AspNetCore.Components.Web` ones.
- It owns: `Models`, `Behaviors`, `Geometry`, `Anchors`, `Routers`, `PathGenerators`, `Layers`,
  `Controls`, `Positions`, `Options`, `Utils`, and the abstract `Diagram` class with the `Trigger*`
  input seam.

Three options for the brain: (A) depend on `Z.Blazor.Diagrams.Core` as-is; (B) vendor/fork its MIT
source; (C) clean-room rewrite.

## Decision

**First-party port (transliteration).** We rewrite the Core as native `Nodely.Core`:

- Keep the **proven architecture and algorithms** (the layer/model/behavior/router/path-generator
  design is battle-tested; we are not re-deriving graph math blindly — we follow the upstream design,
  with citations, and prove each piece with our own tests).
- Use **our own namespace and a clean Avalonia-friendly public API** — no `Z.Blazor.*` types leak to
  end users. This matters for the customizability/ease goals: users see a coherent native API.
- Make path generators **rendering-neutral**: emit a `PathData` command list (Move/Line/Cubic/Quad),
  not an SVG `d` string. Drop the `SvgPathProperties` dependency; do point-sampling (for labels/markers)
  with our own bezier/line math. See `02-research/avalonia-mapping-notes.md`.
- Make event args **framework-neutral structs** populated by the Avalonia layer; keep the same
  `Trigger*` seam so behaviors port almost verbatim.

This is what "rewrite from scratch" means in practice here: a faithful, independent re-implementation
— **not** a blind clean-room with no reference (which would be slow and bug-prone), and **not** a thin
dependency wrapper (which would leak browser-flavored types and block divergence).

## Consequences

- **Independence**: no upstream package dependency; we can refine APIs, naming, and performance freely.
- **Clean API**: end users program against `Nodely.*` only.
- **Rendering-neutral by construction**: `Nodely.Core` knows nothing about Avalonia *or* SVG, so a
  future `Nodely.Wpf`/`Nodely.Skia` could reuse it.
- **Cost**: we own the brain's correctness. Mitigation: port subsystem-by-subsystem (geometry ->
  models -> engine -> behaviors -> routers/paths), each with unit tests, citing upstream source.
- **Upstream drift**: we won't auto-get upstream fixes. Mitigation: keep `02-research/` mapping current;
  periodically diff upstream for notable fixes.
- **License hygiene**: Blazor.Diagrams is MIT. We re-implement rather than copy verbatim; where we do
  adapt code closely, we retain attribution per MIT in NOTICE/THIRD-PARTY-NOTICES.

## Alternatives considered

- **Depend on `Z.Blazor.Diagrams.Core`** — lowest effort/risk, but leaks browser-flavored event types,
  blocks the rendering-neutral path-data improvement, and couples our public API to upstream naming.
- **Pure clean-room** — maximal independence but needlessly re-derives proven geometry/router math;
  higher risk, slower. The port keeps the design knowledge while staying first-party.
