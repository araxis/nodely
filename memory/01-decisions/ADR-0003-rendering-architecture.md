# ADR-0003 — Rendering architecture: hybrid (templated nodes + immediate-mode links)

- Status: **Accepted**
- Date: 2026-06-02
- Deciders: user (priorities: performance + customizability)

## Context

Blazor.Diagrams renders everything as Blazor components into an HTML/SVG tree, sizes nodes via a JS
`ResizeObserver`, finds the canvas origin via `getBoundingClientRect`, and styles via CSS. For Avalonia
we must choose how the canvas draws nodes and links. Options:

1. **Control-per-everything (templated)** — every node *and* link is an Avalonia control resolved via
   `DataTemplate`. Most uniform; richest customization; but a control per link is heavy at high counts.
2. **Immediate-mode** — one control draws the whole diagram in `Render(DrawingContext)`. Fastest, but
   we'd re-implement hit-testing, templating, editing adornments, and focus by hand. Poor customizability.
3. **Hybrid** — templated controls for the things users customize and that need rich interaction
   (nodes, ports, labels); immediate-mode drawing for high-count, draw-heavy, low-interaction visuals
   (links, grid, overview/minimap, selection box).

## Decision

**Hybrid.**

- **Nodes / ports / labels = templated Avalonia controls.**
  - A custom `DiagramCanvas` template hosts layer panels. The **NodesLayer** is a virtualizing panel
    that arranges a node host control per visible `NodeModel` at its diagram-space `Bounds`.
  - Node visual = a `DataTemplate`/`FuncDataTemplate` resolved from the node model's runtime type
    (registry-backed). Avalonia **layout measures the node**, and we write the measured size back to
    `NodeModel.Size` — this natively replaces the JS `ResizeObserver`.
  - Hit-testing, focus, pointer capture, and binding come for free from the Avalonia visual tree.
- **Links = retained, lightweight `LinkView` per link.** (Revised 2026-06-02 — see Update below.)
  - A **LinkView : Control** holds a cached `StreamGeometry` built from the rendering-neutral `PathData`
    (ADR-0002) and draws it in `Render`. Being a real visual, it gets hit-testing, `:selected`/
    `:pointerover` styling, and child labels/vertices **for free** — the faithful analogue of an SVG
    `<path>` element, minus the browser's `<canvas>` JS-interop cost (F-006).
  - **Virtualized**: only links whose bounding box intersects the viewport are realized; geometry is
    cached until the route changes.
  - Link **labels and vertices** are child/overlay controls positioned along the route.
  - Optional opt-in: a per-link `DataTemplate` for fully custom interactive link content.
  - **Scale option (Phase 13):** an immediate-mode batch renderer that draws all visible links in one
    control's `Render` for extreme density (10k+ links), behind the same `IPathGenerator`/`PathData`.
- **Grid / selection-box / navigator(overview) = immediate-mode.** Genuinely low-interaction, high-fill
  visuals where `Render(DrawingContext)` is the right native tool. These never needed SVG either.
- **Pan/zoom** = a single transform applied to the content coordinate space (translate + scale), so
  all layers share one viewport transform. Input is translated from Avalonia pointer/wheel/key events
  into Nodely-neutral events and fed to `Trigger*`.
- **Virtualization** = port `VirtualizationBehavior` (cull models outside the viewport) so the
  NodesLayer only realizes on-screen node controls; immediate-mode layers naturally skip offscreen draws.

## Consequences

- **Customizability is maximal exactly where users want it** (nodes + links), via idiomatic Avalonia
  DataTemplates and styleable, hit-testable `LinkView`s.
- **Performance is strong where it's needed**: nodes + links virtualized; grid/selection/navigator are
  single immediate-mode controls; geometry cached; an immediate-mode link batch mode for extreme density.
- We keep the seam clean: logical z-ordered layers are independent controls over a **single shared
  transform / coordinate space**, each responsible for one model kind.
- `StreamGeometry` (drawn either by a `LinkView` or in immediate mode) is the Avalonia-native analogue
  of SVG `<path>`; the neutral `PathData` from Core makes this a thin adapter.

## Update 2026-06-02 — SVG analysis (why links are retained, and the two-layer split is dropped)

Triggered by the question "is an SVG layer meaningful in Avalonia / do we need it?" Verified against
upstream source (`DiagramCanvas.razor`, `NodeRenderer.cs`, `LinkRenderer.cs`):

- Upstream uses **two parallel layers**: an `<svg class="diagram-svg-layer">` (links always; opt-in
  `SvgNodeModel` nodes/groups) and a `<div class="diagram-html-layer">` (default nodes). `NodeRenderer`
  branches `_isSvg ? "g" : "div"`.
- **Why SVG in the browser:** it's the browser's only declarative vector primitive, it fits Blazor's
  retained DOM-diffing, and it avoids `<canvas>` (which from Blazor means JS interop + manual redraw).
  Benefits: free hit-testing, CSS styling, crisp scaling.
- **Applicability to Avalonia:** **none of the SVG-specific reasons transfer**, and the decisive one is
  *reversed* — Avalonia's immediate-mode `DrawingContext` is native/cheap, so there is no canvas penalty
  to avoid. Avalonia also has **one unified visual model**, so the HTML-vs-SVG coordinate split is
  unnecessary; it collapses into a single transformed canvas (we keep z-order layers only).
- **Consequence for this ADR:** links default to a retained, lightweight, styleable, hit-testable
  `LinkView` (the faithful analogue of an SVG `<path>` element, customizability-friendly), virtualized,
  with immediate-mode batch as a documented scale option. No SVG, no two-layer split, no JS, no CSS.

See `04-progress/findings-and-learnings.md` F-006.

## Notes / source-first checks to do during Phase 5-7

- Confirm exact Avalonia 12 APIs for: custom `Panel.MeasureOverride/ArrangeOverride`, `RenderTransform`
  vs. a custom transformed `Visual`, `StreamGeometry`/`StreamGeometryContext`, `IDataTemplate`
  resolution, pointer capture (`e.Pointer.Capture`), `PointerWheelChanged`, and layout-driven size
  feedback. Verify against installed 12.0.4 source/docs before relying on signatures (ADR-0004).

## Update 2026-06-02 (Phase 7 — links shipped immediate-mode)

Implementation reconciliation: nodes/ports are retained controls (templated `NodeView`, hit-testable
`PortView`) as planned, but **links ship immediate-mode** (`LinksLayer.Render` draws all links from the
neutral `PathData`), not the retained per-link `LinkView` this ADR described. Reasons: it's simpler and
faster (the user's #1 priority is performance), and retained `Path` hit-testing with absolute-coordinate
geometry inside a transformed panel is fiddly in Avalonia. Per-link **selection** via geometry hit-testing
and **geometry caching** are deferred (Phase 8/10/13). The retained `LinkView` remains a valid future
option if rich per-link interactive content is needed. See F-020.
