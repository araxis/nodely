# Research — How Avalonia natively replaces the Blazor HTML/SVG/CSS/JS layer

The Blazor renderer leans on the browser for three things Nodely must provide natively. Avalonia has a
first-class answer for each. **All API names below must be re-verified against installed Avalonia 12.0.4
source/docs before coding** (source-first; see `01-decisions/ADR-0004`). Primary sources:
docs.avaloniaui.net, api-docs.avaloniaui.net, github.com/AvaloniaUI/Avalonia (tag v12.0.4),
samples/ControlCatalog.

## 1. Node sizing — replaces `ResizeObserver` + `OnResize`

- Blazor observes each node element's size with `ResizeObserver` and writes it back to `NodeModel.Size`.
- **Avalonia:** the layout system already measures every control. A node host control's measured/arranged
  size (`Bounds.Size` after `ArrangeOverride`, or `DesiredSize` after measure) is written to
  `NodeModel.Size`. Trigger on `LayoutUpdated`/`EffectiveViewportChanged` or in the panel's
  `ArrangeOverride`. **No JS, no observer plumbing.**
- Open item: confirm the cleanest hook to get a child's final size in a custom `Panel` (likely capture in
  `ArrangeOverride` after arranging each child). Verify in Phase 6.

## 2. Canvas origin/size — replaces `getBoundingClientRect` + `SetContainer`

- Blazor reads the canvas element's screen rect to convert between screen and diagram coordinates.
- **Avalonia:** the `DiagramCanvas` control's own `Bounds` gives size; `this.TranslatePoint(default, topLevel)`
  (or `PointToScreen`) gives origin when needed. Feed into `diagram.SetContainer(rect)` on
  `SizeChanged`/`LayoutUpdated`. Pointer event positions arrive already relative to the control
  (`e.GetPosition(canvas)`), so most coordinate math needs only Pan/Zoom — simpler than the browser case.

## 3. Drawing links/grid — replaces SVG `<path>` + CSS

- Blazor builds SVG `<path d="...">` strings (via `SvgPathProperties`) and styles with CSS.
- **Avalonia:** override `Control.Render(DrawingContext)` and build a `StreamGeometry` via
  `StreamGeometryContext` (`BeginFigure`, `LineTo`, `CubicBezierTo`, `QuadraticBezierTo`, `EndFigure`),
  then `context.DrawGeometry(brush, pen, geometry)`. This consumes the neutral `PathData` from the
  ported path generators directly — no SVG string round-trip.
- Markers (arrowheads) are small geometries drawn at endpoints using the route's tangent. Dashes/colors
  come from a `Pen` (theme/state driven), not CSS.
- **Default = retained `LinkView : Control` per link** (draws its own cached `StreamGeometry` in
  `Render`), so links keep SVG-`<path>`-style free hit-testing + state styling while staying cheap;
  virtualized. Grid/selection/navigator use a single immediate-mode control. An immediate-mode link
  *batch* renderer is the Phase-13 option for extreme density. Avalonia has **no SVG layer** and needs
  none — and the HTML-vs-SVG two-layer split collapses into one transformed coordinate space (F-006).

### Why path generators emit neutral `PathData` (not SVG strings)

- Building a `StreamGeometry` from SVG `d` strings is possible (`PathGeometry.Parse`) but wasteful and
  loses precision/structure. Emitting commands (`Move/Line/Cubic/Quad/Close`) lets us:
  - build `StreamGeometry` straight from structured data (fast, allocation-light, cacheable),
  - sample points along the path for labels/markers with our own math (drop `SvgPathProperties`),
  - keep `Nodely.Core` rendering-neutral (reusable by a future Skia/WPF backend).

## 4. Pan & zoom — replaces the CSS transform on the SVG group

- Apply a single transform to the content coordinate space: translate by `Pan`, scale by `Zoom`.
- **Avalonia:** options to verify in Phase 5 — a `RenderTransform`(`TransformGroup{Translate,Scale}`) on
  a content host, or transform inside each layer's render/arrange. Likely a shared transform owned by
  `DiagramCanvas` and consulted by every layer so node controls, link geometry, and grid stay aligned.
- Wheel zoom centers on the pointer: standard `(screen - pan)/zoom` math already in Core's
  `GetRelativeMousePoint`/`GetScreenPoint`.

## 5. Input & hit-testing — replaces per-element `@onpointerdown`

- **Avalonia:** wire `PointerPressed/Moved/Released`, `PointerEntered/Exited`, `Tapped/DoubleTapped`,
  `PointerWheelChanged`, `KeyDown` on `DiagramCanvas`. The hit-tested source control identifies the
  model (node/port/link) or `null` (empty canvas). Use `e.Pointer.Capture(control)` for drag operations
  (replaces JS pointer capture). Translate to Nodely-neutral event args and call `Trigger*`.

## 6. Templating custom nodes — replaces `RegisterComponent`

- **Avalonia:** `IDataTemplate` / `FuncDataTemplate<TModel>` resolved by model runtime type, backed by a
  small registry plus the canvas's `DataTemplates`. Idiomatic, supports bindings and the visual tree's
  hit-testing/focus. See `01-decisions/ADR-0005`.

## 7. Theming — replaces CSS files

- **Avalonia:** ship a C# `NodelyTheme : Styles` with `ControlTheme`s for each Nodely control and style
  selectors keyed to pseudo-classes/classes for model state (`:selected`, `:locked`, `.invalid`).
  Consumers add `new NodelyTheme()` to `Application.Styles`. Theme variants (light/dark) via
  `ThemeVariant` resources. Verify `ControlTheme`/`Style`/`Selector`/`PseudoClasses` APIs against source.

## 8. Virtualization & performance

- Port `VirtualizationBehavior` to cull models whose bounds fall outside `Container` (+ margin); the
  NodesLayer only realizes visible node controls. Immediate-mode layers skip offscreen draws and cache
  `StreamGeometry` per link until its route changes. Batch model mutations (`Batch`) and invalidate once.
- Measure with BenchmarkDotNet + headless render timing in Phase 13; record numbers in findings.

## Open questions to resolve via source-first during build

- Exact Avalonia 12.0.4 signatures for `StreamGeometryContext`, custom `Panel` measure/arrange size
  capture, `RenderTransform` vs. custom transform, `FuncDataTemplate` resolution order, `ControlTheme`
  application, pointer capture, and `EffectiveViewportChanged`. Confirm each before relying on it.
