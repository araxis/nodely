# Findings & learnings

Durable lessons, gotchas, surprises, and reversed decisions. Add an entry the moment something is
non-obvious — it's the cheapest insurance we have. Tag each with a date and the phase.

## F-057 — State-machine layouts must treat cycles as normal (2026-06-06, StateMachine hotfix)

The gallery State machine scene includes a retry transition back to an earlier state. The first layout helper
assigned a higher level whenever it saw a longer path, which works for acyclic graphs but never terminates when
there is a cycle.

Decision: `StateMachineLayout.Arrange()` now assigns the first reachable level for each state and does not
re-queue already placed states. State machines are cyclic by nature, so package-local layout helpers need cycle
coverage even when they are intentionally small.

## F-056 — Reusable editor chrome belongs in an optional package (2026-06-06, Designer side package)

The runtime property editor proved the undoable metadata path, but leaving the inspector, toolbar, status bar,
and toolbox only in the demo would make every app copy the same editor chrome before customizing it. Putting
that code in the main Avalonia package would also make all consumers take a property-inspector opinion even
when they only need a canvas.

Decision: add `Nodely.Avalonia.Designer` as an optional side package for reusable editor controls. It keeps
field choices explicit through `DiagramPropertyRegistry`, uses `DiagramCanvas.RunAsUndoableEdit()` for runtime
mutations, composes with every domain package through normal `UseXNodes()` renderer registration, and lets the
desktop gallery prove the copied inspector pattern can be removed.

## F-055 — Multi-pack polish belongs in the main release line (2026-06-05, v0.8.0 polish)

After seven side packages, adding another vocabulary was less valuable than proving the existing packages
compose as one app surface. The weak spots were not package-specific models; they were shared registration,
serializer setup, demo discoverability, and clear package selection guidance.

Decision: keep side-package code and version properties unchanged for this release. Move the main package line
to `0.8.0`, add a mixed Architecture gallery scene, add composition tests across all side packages, and document
the shared canvas/serializer pattern.

## F-054 — API packs should separate endpoints, contracts, and traffic (2026-06-05, API side package)

API diagrams become hard to read when endpoints, payload shapes, auth policy, and backend operations collapse
into one generic box. The package needs distinct endpoint cards, contract rows, visible request/response/event
ports, and relationship glyphs so the flow is understandable before a user opens an inspector.

Decision: `Nodely.Avalonia.Api` keeps endpoint, contract, auth, gateway, client, service, and group visuals
inside the side package, along with typed API ports, link metadata, link glyphs, and `ApiLayout.Arrange()`.
Core stays unchanged, and future integration-style packs should keep protocol vocabulary in the pack instead
of asking the main packages for domain-specific behavior.

## F-053 — Network topology packs need device semantics in the visuals (2026-06-05, Network side package)

Network diagrams lose most of their value if routers, switches, firewalls, services, zones, and links all look
like generic boxes connected by plain paths. The package needs to show device roles, visible port groups, status,
capacity, and connection type directly in the rendered vocabulary so a topology is readable before a user opens
an inspector.

Decision: `Nodely.Avalonia.Network` owns device-shaped node renderers, switch port rows, typed network ports,
status badges, link glyphs, link metadata, and `NetworkLayout.Arrange()` inside the side package. Core stays
unchanged, and future topology-style packs should follow this pattern instead of asking the main packages for
domain-specific render APIs.

## F-052 — StateMachine self loops can stay pack-owned (2026-06-05, StateMachine side package)

State-machine self transitions need a loop shape and label placement that generic link routing does not provide.
Adding a shared self-loop path generator to core would solve one domain-specific case but would also make the
core API larger before enough apps need it.

Decision: `Nodely.Avalonia.StateMachine` keeps self-loop drawing inside its typed link drawer. Normal
transitions still use existing routers, path generators, labels, and typed link styles. This proves a pack can
own specialized link drawing without adding marker or router APIs to the main packages.

## F-051 — MindMap needs a small domain layout without changing core (2026-06-05, MindMap side package)

Mind maps have a recognizable workflow that generic graph layout does not cover well: a central root, left/right
branch sides, descendants that inherit side, and collapse state that hides branch descendants. Putting that into
core would make the engine domain-aware, but leaving it out of the side package would make the package feel like
plain boxes and lines.

Decision: `Nodely.Avalonia.MindMap` owns `MindMapLayout.Arrange()` and `ApplyCollapseState()` as pack-local
helpers. The helpers use existing model position and `Visible` state; core stays domain-neutral. This also
surfaced a renderer correctness fix: Avalonia link and port layers must honor model visibility so domain packs
can hide dependent visuals without custom layer hacks.

## F-050 — Runtime property editing needs an undoable metadata path (2026-06-05, gallery polish)

Domain packs expose meaningful mutable fields, but users need a runtime way to edit those fields in real apps.
Trying to put one universal property grid in core would force domain decisions into the shared package.

Decision: core provides the small reusable pieces: `EditModelCommand`, `DiagramCanvas.RunAsUndoableEdit()`, and
`DiagramCanvas.RefreshVisuals()`. Host apps and samples own the actual inspector UI and field choices. The
gallery inspector proves the path across core, Database, UML, Workflow, and custom demo models while preserving
undo/redo and save/load.

## F-049 — Side packages must own a real visual vocabulary (2026-06-05, domain pack polish)

A side package is not enough if it only ships new model types with generic boxes and plain links. Category
packages are valuable when they make the diagram immediately recognizable in that domain: distinct node shapes,
row-level ports, relationship-specific endpoints, labels, badges, and theme-aware styling.

Decision: future side packages must include domain-specific renderers, port roles, save/load coverage for port
metadata, and at least one gallery scene that uses those pieces together. Core should only expose the stable
extension surface; pack-owned visuals and metadata stay inside the pack.

## F-048 — Workflow stays model/render/serializer only (2026-06-05, Workflow side package)

Workflow could easily grow into swimlanes, execution state machines, timers, and layout rules. The first side
package intentionally avoids those behaviors and provides only a diagram vocabulary: nodes, links, renderer
registration, and serializer registration.

Decision: keep workflow execution and orchestration in the host app. `Nodely.Avalonia.Workflow` owns visual
workflow shapes and metadata, while core remains domain-neutral and future workflow behavior can be added only
when a concrete app contract needs it.

## F-047 — Second side package proves the contract scales (2026-06-04, UML side package)

UML stressed a different shape than Database: no domain ports, more node varieties, nested operation
parameters, and relationship markers that are better drawn by a pack-owned link drawer than by changing core
marker APIs.

Decision: keep UML side-package-only. `UmlRelationshipLink` stores labels/multiplicities through extra data,
overrides diagram default markers with zero-size markers, and `UseUmlNodes()` draws UML-specific triangle,
open-arrow, aggregation, and composition markers in the pack renderer.

Workflow/MindMap should follow the same package contract and add only their own model/render/serializer
vocabulary unless a concrete shared need appears.

## F-046 — Side packages need a stronger extension contract before public release (2026-06-04, extension audit)

The database pack proved the package pattern, but the current extension surface is too fragile to use as the
long-term template for more side packages. Node data round-trips, but port and link metadata do not; link styling
uses one global resolver property; snapshot kinds use CLR type names; and package-version overrides leak into
project-reference dependency versions.

Decision: revise the 0.7 work before merge. Add stable model-kind keys, model-wide extra-data hooks,
serializer registry support for nodes/ports/links/groups, typed composable link-style registration, render
context overloads, and per-package version properties before treating `Nodely.Avalonia.Database` as the side-pack
template. See `memory/02-research/extension-surface-investigation.md`.

Resolution: implemented in the extension surface redesign. The database pack now uses stable kinds, registry
restore for nodes/ports/links, typed link-style registration, palette-aware render context, and its own package
version.

## F-045 — Domain packs should register a vocabulary, not change the core (2026-06-04, v0.7.0)

The first optional node pack is database-focused because tables, views, procedures, columns, parameters, and
relationship links are concrete enough to test and demo without turning the core engine into a domain model.

Pattern established by `Nodely.Avalonia.Database`:

- Keep domain model types in the pack.
- Expose one canvas extension (`UseDatabaseNodes`) that registers node, port, and link renderers.
- Persist domain model data through `Model.GetExtraData` / `SetExtraData`.
- Restore domain model types through `DiagramSerializationRegistry`.
- Keep side-package versions independent from the main package group.

## F-044 — Avalonia net8.0 asset is viable with the current SDK baseline (2026-06-04, v0.6.0)

The v0.5.0 release deferred Avalonia `net8.0` compatibility because the repo was built around a .NET 10 SDK
baseline. Rechecking the restored Avalonia 12.0.4 package showed both `net8.0` and `net10.0` assets, and a
forced `net8.0` build of `Nodely.Avalonia` succeeded once restore evaluated that target graph.

Decision for v0.6.0: keep `global.json` pinned to the stable .NET 10 SDK for repository builds, but ship
`net8.0` and `net10.0` assets for `Nodely.Avalonia`. Add explicit `net8.0` assets to the engine packages
too, while retaining `netstandard2.0` for broad headless reuse. Tests run on both app runtimes so the package
matrix stays honest.

## F-001 — The brain is already UI-agnostic (2026-06-02, research)

`Blazor.Diagrams.Core` has no Blazor dependency and defines its own pointer/key/wheel event types. This
is *the* reason a faithful Avalonia port is tractable: the Blazor-specific surface is only the rendering
package + a tiny JS file. **Implication:** invest in a clean `Nodely.Core` first; the Avalonia work is a
focused adapter, not a rewrite of graph logic.

## F-002 — Replace SVG path strings with neutral `PathData` (2026-06-02, design)

Upstream path generators emit SVG `d` strings via `SvgPathProperties`. For Avalonia we want
`StreamGeometry`, and for reuse we want Core to stay rendering-neutral. **Decision:** path generators
emit a structured command list (`Move/Line/Cubic/Quad/Close`) plus point/tangent sampling done with our
own math; the Avalonia layer turns commands into a `StreamGeometry`. Drops the `SvgPathProperties`
dependency and keeps Core backend-agnostic. (ADR-0002.)

## F-003 — Avalonia layout replaces three browser APIs (2026-06-02, design)

- `ResizeObserver` (node sizing) → Avalonia measures controls; write final size to `NodeModel.Size`.
- `getBoundingClientRect` (canvas origin) → control `Bounds` / `TranslatePoint`; feed `SetContainer`.
- SVG `<path>` + CSS → `Render(DrawingContext)` + `StreamGeometry` + `Pen`/`ControlTheme`.
No JS interop and no CSS files are needed. (avalonia-mapping-notes.md.) **To verify in code** against
Avalonia 12.0.4 before relying on exact signatures.

## F-004 — "Rewrite from scratch" reframed as "first-party port" (2026-06-02, decision)

User initially leaned "rewrite Core from scratch," then delegated ("you decide best"). A literal
clean-room would needlessly re-derive proven geometry/router math at high risk. We adopt a faithful
first-party port: keep the upstream design + algorithms (cited), rewrite as native `Nodely.*` with a
clean Avalonia-friendly API and neutral path output. Independent and low-leakage, but de-risked. (ADR-0002.)

## F-005 — Customization must feel like idiomatic Avalonia (2026-06-02, decision)

User's top UX goal: easy custom nodes/links. Avalonia users already know DataTemplates/styles/classes.
So the customization API is DataTemplate-first (`RegisterNode<T>` + `FuncDataTemplate`), links are
composed from small replaceable pieces (router/path/style) with a full-template escape hatch, and there
is **no reflection scanning**. (ADR-0005.)

## F-006 — Avalonia needs no SVG layer; links should be retained, not immediate-mode (2026-06-02, design)

Verified upstream (`DiagramCanvas.razor`, `NodeRenderer.cs`, `LinkRenderer.cs`): Blazor.Diagrams renders
**two parallel layers** — an `<svg>` layer (links always; opt-in `SvgNodeModel`) and an HTML `<div>`
layer (default nodes). **Why SVG:** it is the browser's declarative vector primitive, fits Blazor's
retained DOM-diffing, and avoids `<canvas>` (which from Blazor = JS interop + manual redraw).

**None of these reasons apply to Avalonia, and the decisive one is reversed:** Avalonia has no SVG in its
pipeline (it draws vectors via `Geometry`/`StreamGeometry` on Skia); immediate-mode `DrawingContext` is
native and cheap, so the canvas penalty that pushed Blazor to SVG doesn't exist. Avalonia's single unified
visual model also makes the HTML-vs-SVG coordinate split unnecessary — it collapses to one transformed
canvas (we keep only z-ordered logical layers).

**Two consequences:**
1. Drop the SVG framing and the two-layer split entirely. Map SVG `<path>` → Avalonia `StreamGeometry`;
   HTML `<div>` node → templated control; everything in one coordinate space.
2. **Revised ADR-0003:** links default to a retained, lightweight, styleable, hit-testable `LinkView`
   per link (the faithful analogue of an SVG `<path>` element, best for "easy interactive custom links"),
   virtualized, with an immediate-mode batch renderer as a Phase-13 scale option. Grid/selection/navigator
   stay immediate-mode. (Originally I'd defaulted links to immediate-mode; this question corrected that.)

## F-007 — Target net10.0 (current LTS), not net8.0 (2026-06-02, Phase 0)

Dev machine has .NET 10 SDK (10.0.300 stable) + .NET 11 SDK (preview); no .NET 8 SDK. .NET 10 is the
current LTS (GA Nov 2025). Runtimes present: 8.0.27, 9.0.16, 10.0.8, 11-preview. **Changed primary TFM
to `net10.0`** (brain multi-targets `netstandard2.0;net10.0`). Pinned SDK via `global.json`
(`10.0.300`, `rollForward latestMinor`, `allowPrerelease false`) so we don't accidentally build on the
.NET 11 preview SDK. Supersedes the net8.0 choice in ADR-0004's original draft.

## F-008 — Avalonia.Diagnostics has no 12.x release (2026-06-02, Phase 0)

`Avalonia` core is 12.0.4, but `Avalonia.Diagnostics` (the F12 inspector) tops out at **11.3.17** — it
is NOT version-locked to core. Restoring it at 12.0.4 fails (`NU1102`). It's optional dev tooling, so we
removed it from the demo for now and will re-add when a 12.x publishes. Lesson: don't assume every
`Avalonia.*` satellite shares the core version; verify each.

## F-009 — Avalonia 12 headless testing uses xUnit v3 (2026-06-02, Phase 0)

`Avalonia.Headless.XUnit` 12.0.4 depends on `xunit.v3.extensibility.core` 3.2.2 — it's **xUnit v3**, not
v2. Pinning v2 (`xunit` 2.9.2) built fine but produced "No test is available" at run time (the v2 runner
can't discover v3 `[AvaloniaFact]` tests). Fix: `xunit.v3` 3.2.2 + `xunit.runner.visualstudio` 3.1.5 +
`Microsoft.NET.Test.Sdk` 17.12.0, and **v3 test projects require `<OutputType>Exe</OutputType>`**.
`using Xunit;` / `[Fact]` are unchanged in v3, so test code didn't change.

## F-010 — `dotnet new sln` defaults to `.slnx` on .NET 10 (2026-06-02, Phase 0)

The .NET 10 SDK created `Nodely.slnx` (new XML solution format), not `Nodely.sln`. `dotnet
build/test Nodely.sln` fails with `MSB1009`. All tooling (CI, README, local commands) references
`Nodely.slnx`. Supported by `dotnet` CLI and VS 2022 17.10+.

## F-011 — netstandard2.0 port adjustments (2026-06-02, Phase 1)

Multi-targeting `Nodely.Core` to `netstandard2.0` (for broad reach) required these deltas vs. upstream:
- **`IsExternalInit` polyfill** (`Compat/IsExternalInit.cs`, guarded `#if !NET5_0_OR_GREATER`) so C# records
  / `init` setters compile on netstandard2.0.
- **Replace `double.IsFinite(x)`** (netstandard2.1+) with `!double.IsNaN(x) && !double.IsInfinity(x)` in
  `Rectangle.GetPointAtAngle`.
- **Drop `ArgumentNullException.ThrowIfNull`** (.NET 6+) — rely on nullable annotations; explicit
  `if (x is null) throw` where needed.
- **Drop `[JsonConstructor]`** from `Rectangle` — serialization lives in `Nodely.Serialization` (Phase 11)
  via DTOs, keeping Core free of `System.Text.Json` on netstandard2.0.
- Added `Equals(object)`/`GetHashCode` overrides to `Rectangle` (upstream only had `Equals(Rectangle?)`).

## F-012 — Layer<T> decoupled from Diagram via IModelBatcher (2026-06-02, Phase 1)

Upstream `BaseLayer<T>` calls `Diagram.Batch(...)` directly, coupling the layer to the engine. To keep
Phase 1 independently unit-testable, Nodely's `Layer<T>` takes an **optional `IModelBatcher`**
(null ⇒ apply immediately). The concrete `Diagram` will implement `IModelBatcher` in Phase 2. Deliberate
DIP improvement; behavior is identical once a batcher is supplied. Renamed `BaseLayer<T>` ⇒ `Layer<T>`.

## F-013 — Upstream reference source cloned locally (2026-06-02, Phase 1)

Exact upstream source (for faithful porting, not summaries) is shallow-cloned at
`D:\Projects\_refs\Blazor.Diagrams` (branch `develop`), **outside** the Nodely repo (not committed).
Reuse it for Phase 2+ ports; refresh with `git -C D:\Projects\_refs\Blazor.Diagrams pull` if needed.

## F-014 — Phase 2/4 boundary and engine decoupling choices (2026-06-02, Phase 2)

Porting the engine surfaced several deliberate seam/scoping decisions:
- **Links attach in Phase 2; routing/path-drawing is Phase 4.** `BaseLinkModel.GeneratePath` degrades
  gracefully: `DiagramLinkOptions.DefaultRouter`/`DefaultPathGenerator` are **nullable and null** for now,
  so links still attach and their **anchors resolve real positions** (tested), but `Route`/
  `PathGeneratorResult` stay null until Phase 4 wires the concrete `NormalRouter`/`SmoothPathGenerator`.
- **Behavior registration moved out of the base `Diagram`.** Upstream's base ctor hard-codes
  `RegisterBehavior(new SelectionBehavior(this))` etc. Nodely's base only owns the registry; the concrete
  `NodelyDiagram` registers defaults (in Phase 3). Keeps the base open/closed and Phase-2-buildable.
- **`PathData` + `PathGeneratorResult` defined now as the neutral, SVG-free seam** (realizes F-002):
  path generators will emit `PathData` (Move/Line/Cubic/Quad/Close), not SVG strings. `LinkMarker` is
  likewise neutral `PathData`, not an SVG `d` string.
- **Deferred to keep Phase 2 focused:** `DynamicAnchor` + `LinkAnchor` → Phase 7; the default Controls
  widgets (resize/delete) + `ControlsBehavior` → Phase 8 (only the `ControlsLayer`/`Control` plumbing is
  here); the circle `LinkMarker` (needs arc) → Phase 7.

## F-015 — More netstandard2.0 gotchas: index-from-end and ValueTask (2026-06-02, Phase 2)

- The **index-from-end operator** (`route[^1]`, `_orderedSelectables[^1]`) needs `System.Index`, absent on
  netstandard2.0. Replaced all `^` index usages with explicit `arr[arr.Length - 1]` indexing.
- **`ValueTask`/`ValueTask<T>`** (used by `DiagramConstraintsOptions`) isn't built into netstandard2.0;
  added a **conditional** `System.Threading.Tasks.Extensions` 4.5.4 package reference for that TFM only,
  and used `new ValueTask<bool>(true)` rather than `ValueTask.FromResult` (the static helper isn't in the
  polyfill). net10.0 has both built in. (Builds with net10.0 + netstandard2.0, 0 warnings.)

## F-016 — Behavior porting notes (2026-06-02, Phase 3)

- **Registration order matters.** `SelectionBehavior` is registered *before* `DragMovablesBehavior` so a
  pointer-down selects the node first, then the drag captures the (now-selected) set. Order preserved from
  upstream in `NodelyDiagram` (minus `ControlsBehavior` → Phase 8). Added a `registerDefaultBehaviors` flag.
- **netstandard2.0 again:** `Math.Clamp` (ns2.1+) inlined as `x < min ? min : (x > max ? max : x)` in
  `ZoomBehavior`; `KeyValuePair` tuple-deconstruction (`foreach (var (k,v) in dict)`) isn't available, so
  `DragMovablesBehavior` uses `kvp.Key`/`kvp.Value`; `ValueTask.CompletedTask` → `default`.
- **`async void` keyboard handler** (`KeyboardShortcutsBehavior.OnDiagramKeyDown`) is faithful to upstream
  and fine for tests because the default delete constraints are synchronously-completed `ValueTask<bool>`,
  so the handler runs to completion before `TriggerKeyDown` returns.
- **Marquee/box selection is NOT a behavior** — upstream `SelectionBehavior` only does click-selection; the
  drag-rectangle marquee lives in the `SelectionBoxWidget` (rendering), so it's **Phase 9**, not here.
- Events are framework-neutral: behaviors read `e.X`/`e.Y`/`e.Button`(`PointerButton`) — the Avalonia layer
  will populate these (Phase 5).

## F-017 — Routers & path generators: SVG-free, and the orthogonal A* on netstandard2.0 (2026-06-02, Phase 4) — M1

- **`SvgPathProperties` is fully gone.** `StraightPathGenerator`/`SmoothPathGenerator` now build neutral
  `PathData` (`MoveTo`/`LineTo`/`QuadTo`/`CubicTo`) — a 1:1 swap for `SvgPath`'s `Add*` methods. This
  realizes ADR-0002/F-002: `Nodely.Core` emits structured path commands, not SVG `d` strings, so the
  Avalonia layer (Phase 7) builds `StreamGeometry` directly.
- **Defaults wired:** `DiagramLinkOptions.DefaultRouter = new NormalRouter()`,
  `DefaultPathGenerator = new SmoothPathGenerator()`. So links added to a diagram now produce a real
  `Route` + `PathGeneratorResult` (the Phase-2 graceful-degradation path is no longer hit by default).
- **`OrthogonalRouter` hit three netstandard2.0 gaps:** `PriorityQueue<,>` is .NET 6+ only → added a small
  binary-heap **polyfill** (`Compat/PriorityQueuePolyfill.cs`, `#if !NET6_0_OR_GREATER`, internal, in
  `System.Collections.Generic`); `KeyValuePair` deconstruction → `kvp.Key`/`kvp.Value`; `result[^1]`/`[^2]`
  → explicit indices. Renamed the internal `Node` → `RouteNode` to avoid confusion with diagram nodes.
- **`SmoothPathGenerator` forward-compat:** upstream's curve-point branch lists
  `ShapeIntersectionAnchor or DynamicAnchor or LinkAnchor` and throws otherwise. Since Dynamic/Link anchors
  are deferred (Phase 7), I made the final branch the axis-aligned default (no throw), so they'll work when
  added later.
- **Milestone M1 reached:** the entire UI-agnostic engine (geometry → models → engine → behaviors →
  routing/paths) is complete, independent, and tested (Core 66/66 on net10.0 + netstandard2.0, 0 warnings).

## F-018 — Avalonia canvas: Panel.Render is sealed → layered children (2026-06-02, Phase 5)

- **`Panel.Render` is sealed** in Avalonia (it draws `Background`). A `Panel` can't do custom drawing
  directly. Resolution: `DiagramCanvas : Panel` hosts a child **`GridLayer : Control`** (Control.Render is
  overridable, `IsHitTestVisible = false` so input passes through). This is the layered structure later
  phases want anyway (Links/Nodes layers become more children). Background hit-testing comes from the
  Panel's `Background`.
- **Property-changed during construction:** setting `GridBrush` in the ctor raised `OnPropertyChanged`,
  whose handler invalidated `_grid` before it was assigned → NRE. Fix: create `_grid` first, and use
  `_grid?.` defensively.
- **Coordinate mapping is simpler than the browser.** Avalonia's `e.GetPosition(canvas)` is already
  canvas-relative, so we `SetContainer(new Rectangle(0, 0, w, h))` (Left/Top = 0) and pass the position
  straight through — no `getBoundingClientRect` offset math (confirms F-003).
- **Input → neutral events:** Avalonia pointer/wheel/key → `PointerEvent`/`WheelEvent`/`KeyboardEvent`
  via `PointerUpdateKind`→`PointerButton`, `KeyModifiers`, `e.Delta`. Pointer captured on press. For
  Phase 5 the hit-model is always `null` (empty canvas) → drives panning; real hit-testing arrives with
  node controls in Phase 6.
- **Headless input tests** use `Avalonia.Headless.HeadlessWindowExtensions` (`MouseDown/Move/Up`,
  `MouseWheel`) on the `Window` + `Dispatcher.UIThread.RunJobs()`; assert `diagram.Pan`/`Zoom`/`Container`.
- Letter-key shortcut casing (Avalonia `Key.G` → "G" vs upstream "g") is deferred to Phase 10; Delete works.

## F-019 — Node rendering, templating, and pan/zoom transform (2026-06-02, Phase 6)

- **Layered structure:** `DiagramCanvas : Panel` hosts `GridLayer` (bottom) and `NodesLayer` (top).
  `NodesLayer : Panel` hosts one `NodeView : Decorator` per node, arranged at the node's diagram-space
  position, under a shared **`RenderTransform`** = `TransformGroup { Scale(zoom), Translate(pan) }` with
  `RenderTransformOrigin = TopLeft` → `p_screen = p_diagram*zoom + pan` (matches the grid math).
- **Size feedback (replaces ResizeObserver):** `NodesLayer.MeasureOverride` measures each view with
  `Size.Infinity` and writes `view.DesiredSize` back to `NodeModel.Size` (the setter's equality guard
  prevents a layout loop). `ControlledSize` nodes are measured at their fixed size instead.
- **Custom-node API:** `DiagramCanvas.RegisterNode<TNode>(Func<TNode, Control>)` — a type-keyed registry
  resolved by walking the node's runtime type up to `NodeModel` (most-derived wins), with a built-in
  default. A custom node is ~5–10 lines (subclass + register). No assembly scanning.
- **Model hit-resolution is capture-safe:** the canvas captures the pointer on press (for smooth drag),
  which makes `e.Source` the canvas on move/up. So we resolve the hit node positionally via
  `this.GetVisualsAt(point)` → walk visual parents → `NodeView.Node`, on press AND release (so a tap on a
  node both selects and click-detects). `GridLayer.IsHitTestVisible = false` lets input fall through.
- **Deferred:** render-virtualization culling (skip off-screen `NodeView`s; honor `VirtualizationBehavior`'s
  `Model.Visible`) → Phase 13; optional Avalonia `IDataTemplate`/`FuncDataTemplate` integration alongside
  the registry → later. Selection outline is a simple `NodeView.Render` ring for now (themed in Phase 10).

## F-020 — Ports & links rendering + interactive creation (2026-06-02, Phase 7) — M2

- **Links: immediate-mode (revises ADR-0003's retained default).** `LinksLayer : Control` draws every
  link's `PathGeneratorResult.FullPath` via `PathDataGeometry.ToGeometry` (neutral `PathData` →
  `StreamGeometry`) under the shared pan/zoom transform. Chosen over retained per-link `Path` controls
  because (a) it's simpler/faster — aligns with the user's #1 priority, performance — and (b) retained
  `Path` hit-testing with absolute-coord geometry in a transformed panel is fiddly. **Per-link selection
  (geometry hit-test) is deferred** (Phase 8/10). ADR-0003 updated with this reconciliation.
- **Ports: retained hit-testable dots.** `PortsLayer : Panel` hosts a `PortView : Decorator` (an
  `Ellipse`) per port. In `ArrangeOverride` it computes each port's diagram-space position from the
  node's bounds + alignment, sets `Position`/`Size`/`Initialized`, and calls `port.RefreshLinks()` on
  first-init/move so attached links generate their path (links only resolve once ports are `Initialized`).
- **Hit-resolution extended:** `ResolveModelAt` returns a `PortModel` when a `PortView` is hit (ports are
  the top z-layer: grid < links < nodes < ports), so dragging from a port starts a link
  (`DragNewLinkBehavior`); empty canvas still pans. `GetVisualsAt` is capture-safe, so drop-on-port on
  release resolves the target port even while the canvas holds pointer capture.
- **Gotcha:** the type `Avalonia.Media.Geometry` collides with the `Nodely.Geometry` *namespace* in files
  that `using Nodely.Geometry;` — aliased as `AvGeometry`.
- **Deferred:** arrowhead markers, link labels, draggable vertices, link selection, geometry caching
  (rebuild per render for now) → later phases / Phase 13.

<!-- Template for new entries:
## F-00X — <short title> (YYYY-MM-DD, phase)
What we expected, what we found, why it matters, what we changed.
-->

## F-043 — Adoption polish needs command state, not more editor features (2026-06-04, v0.5.0)

The next useful release boundary is helping consumers wire real apps quickly:

- **Toolbar friction:** the canvas already had commands (`CopySelection`, `GroupSelection`, `Undo`, etc.), but
  callers had to inspect selection, clipboard, grouping, history, and read-only state themselves. Added
  read-only command-state helpers and one `CommandStateChanged` event.
- **Samples as documentation:** `Nodely.Demo` now shows practical app scenes and the command-aware toolbar;
  `Nodely.QuickStart` is intentionally tiny so a new consumer can copy the whole app.
- **Docs recipes:** recipes cover the adoption paths that were too small for full guides: minimal app, toolbar
  state, overlay layer, and custom-node save/load.
- **Tests:** Avalonia +4 for selection, clipboard/read-only, grouping, and diagram-swap command state.

## F-042 — History polish and refresh hardening (2026-06-04, v0.4.0)

The next release is a hardening pass rather than a feature wave. The useful boundary was "make existing editor
actions behave like durable editor actions":

- **Undoable z-order:** `SendToFront`/`SendToBack` mutate several `Order` values, so the command stores full
  before/after order snapshots (`SetModelOrdersCommand`) instead of trying to infer a single delta.
- **Undoable grouping:** group removal should ungroup by default in the canvas, leaving child nodes on the
  diagram. `RemoveGroupCommand` captures children before `GroupLayer.Remove` calls `Ungroup()`, then restores
  membership on undo.
- **Undoable bend points:** add/remove vertex commands preserve the vertex instance and original index, so redo
  and undo keep route order stable.
- **Refresh fix:** link labels now refresh their parent link when content, distance, or offset changes. Link
  refresh also cascades to link-to-link dependents, guarded by a local `_refreshing` flag so cyclic link anchors
  cannot recurse forever.
- **Hardening:** warnings-as-errors is now on across the solution; the default link factory throws a precise
  argument exception for unsupported source models.
- **Tests:** Core +6, Avalonia +3. Current count: Core **109**, Avalonia **37** (146 total).

## F-041 — Extensibility seams wave (lean framework, not features) (2026-06-03, per user direction)

User wants the lib kept lean and customization maximized (see [[working-style-decide-and-document]]). Added 10
extension seams instead of built-in features; reference impls go in the demo, not core.

- **Render hooks (Avalonia, parity with `RegisterNode`):** `RegisterLink<TLink>(LinkDrawer)` — immediate-mode
  drawer with a `LinkRenderContext` (`Geometry`/`Path`/`Palette`/`IsSelected`/`Result` + `DrawDefault()`);
  `RegisterPort<TPort>` and `RegisterGroup<TGroup>` (views now call `owner.BuildPortContent/BuildGroupContent`).
- **Superseded by F-046:** the single link-style resolver became typed `RegisterLinkStyle<TLink>` registrations
  so side packages compose without chaining a global property.
- **Custom layers:** `DiagramCanvas.AddLayer(Control, worldSpace=true)` / `RemoveLayer` + `DiagramLayer` base
  (exposes `Owner`/`Diagram`). World-space layers share the pan/zoom transform; inserted just below adorners;
  updated in OnView/OnStructure changed. The "master seam" — any overlay, user-built.
- **`RegisterAdorner(Func<NodeModel,Control?>)`** — screen-space adorners anchored at the node's top-left
  (selection toolbars/badges); positioned by `AdornersLayer`.
- **Validation delegates (Core, OCP):** `DiagramLinkOptions.CanConnect` (wired into `DragNewLinkBehavior` drop
  + snap), `DiagramOptions.CanDrag` + `SnapPosition` (wired into `DragMovablesBehavior`).
- **`IDiagramLayout`** (Algorithms) + `LayeredDiagramLayout` wrapper — pluggable layouts.
- **Superseded by F-046:** serialization extras moved from node-only hooks to model-wide hooks, and registry
  restore covers nodes, ports, links, and groups.
- **Model metadata bag:** `Model.Tag` (object?) + lazy `Model.Data` dictionary — attach data without subclassing.
- **Behavior/input API:** thin `RegisterBehavior/GetBehavior/UnregisterBehavior` pass-throughs on the canvas
  (the `Behavior` + `Trigger*` seams already existed). Added `InternalsVisibleTo` so tests can hit
  `ResolveLinkDrawer/Style`.
- **Docs:** new "Extensibility" guide (the seam map); site builds green.
- **Tests:** Core +6 (metadata, IDiagramLayout, extras, 3× validation); Avalonia +5 (port/group templates,
  AddLayer/RemoveLayer, adorner, link drawer/style resolution). Core **103**, Avalonia **34** (137). 0/0 both TFMs.

## F-040 — Documentation site + GitHub Pages pipeline (2026-06-03, docs)

- **Site shape:** lean docs-only setup in `website/`, dark-default theme tuned to the blue accent. Curated
  guides: intro homepage, getting-started, custom-nodes, links/markers/labels/vertices, selection & clipboard,
  undo/redo, serialization, layout, theming, architecture — all written from the verified APIs.
- **Pipeline:** `.github/workflows/docs.yml` — Node 20 + `npm ci` + `npm run build`, deployed via the official
  GitHub Pages actions (`upload-pages-artifact` + `deploy-pages`), triggered on `website/**`. One-time repo
  setup: **Settings → Pages → Source: "GitHub Actions"**.
- **Markup note:** wrap C# generics like `RegisterNode<TNode>` in backticks in prose so the docs build treats
  them as code, not markup. Local build is green before publishing.
- `package-lock.json` is committed (CI uses `npm ci`); `node_modules/` and `build/` are gitignored.

## F-039 — Circle link marker (last deferred marker shape) (2026-06-03, post-0.1.0 backlog 7)

`LinkMarker` remarked the circle was deferred (needs arc support). `PathData` has no arc command, so
`NewCircle(diameter)` approximates a circle with **four cubic Béziers** (κ = 0.5522847498 × r): link end at the
origin, circle spanning x∈[0, diameter], y∈[-r, r]. The figure is `Close()`d so the generic marker renderer
fills it as a disc — **no rendering change needed** (markers already draw any `PathData` filled, F-029).
`LinkMarker.Circle` default; demo shows it as a source marker on the workflow Deploy link.
- **Test:** `Circle_marker_is_a_closed_path_spanning_its_diameter` (bbox 0..10 × -5..5, ends with Close). Core **97**.
- **Backlog complete:** all 7 post-0.1.0 items (layout-undo, select-all/z-order, context menu, clipboard,
  save/load, dynamic+link anchors, circle marker) done; nothing left deferred from the original plan.

## F-038 — Dynamic + Link anchors (the last deferred anchors) (2026-06-03, post-0.1.0 backlog 6)

Phase 2 deferred `DynamicAnchor`/`LinkAnchor` (checklist "Dynamic+Link deferred → Phase 7"). Implemented now.

- **`DynamicAnchor(model, anchors)`:** holds candidate anchors; `GetPosition` resolves each and returns the one
  closest (`GetClosestPointTo`) to the link's other endpoint — so a link snaps to whichever side/port faces the
  connected model. `isTarget = link.Target == this` (the established pattern). Falls back to the first candidate
  before the other end is known.
- **`LinkAnchor(link)`:** link-to-link — resolves to the **middle of the target link's path** via
  `PathData.PointAtDistance(Length/2)` (the helpers added in F-031), falling back to the endpoint midpoint
  before the path exists. `Model = link` (a `BaseLinkModel` is `ILinkable`), so the diagram registers the
  dependent link on the target.
- **Superseded by F-042:** live link-to-link refresh now cascades from the refreshed target link to its
  dependents.
- **Tests:** `Dynamic_anchor_picks_the_candidate_nearest_the_other_endpoint`,
  `Link_anchor_resolves_to_the_middle_of_the_target_link`. Core **96**. 0/0 both TFMs.

## F-037 — Demo Save/Load via the serializer (2026-06-03, post-0.1.0 backlog 5)

- **Demo:** top-bar Save/Load. Save → `DiagramSerializer.Serialize(currentDiagram)` into a field; Load → new
  `NodelyDiagram`, deserialize, then rebuild the host with `Editor(loaded)`.
- **Superseded by F-046:** load now uses stable model-kind keys and `DiagramSerializationRegistry`, so CLR type
  names are no longer the persistence key.
- **Limitations:** custom fields not in the snapshot (e.g. `TaskNode.Status`) aren't persisted; options
  (arrows/groups) reset to defaults on load (demo re-sets the arrow marker).
- **Tests:** Core `Custom_node_kind_round_trips_via_the_factory_preserving_id_and_links`. Core **94**, Avalonia
  **29** (123). 0/0 both TFMs.

## F-036 — Context menu + clipboard (copy/cut/paste/duplicate) (2026-06-03, post-0.1.0 backlog 3-4)

- **Context menu:** `DiagramCanvas` hosts a `ContextMenu` rebuilt on `ContextRequested` — the handler selects
  the model under the cursor (via `TryGetPosition` + `ResolveModelAt`), then lists Delete / Duplicate / Bring
  to front / Send to back (when there's a selection and not read-only) + Select all / Zoom to fit always.
- **Clipboard:** `NodeModel.Clone()` is **virtual** — subclasses override to copy their data (the demo's
  `TaskNode` copies `Status`). This is the extensibility seam for copy/paste of custom nodes (no reflection,
  no serializer dependency). `Clone()` copies own data only — no ports/links.
- **Canvas ops + keys:** `CopySelection` (Ctrl+C, stores clones), `CutSelection` (Ctrl+X, copy+delete),
  `PasteClipboard` (Ctrl+V, growing +24 offset, re-clones the templates so repeat-paste works),
  `DuplicateSelection` (Ctrl+D / menu). Adds go through one history `Transaction()` and select only the copies,
  so paste/duplicate is one undo step. **Limitation:** links between copied nodes aren't copied (nodes only).
- **Gotcha:** `NodeModel.Title` is `string?`; `new TaskNode(Position, Title)` warned CS8604 → `Title ?? ""`.
- **Tests:** Core `Node_clone_is_a_distinct_copy...`; Avalonia `Duplicate_selection...` (offset + select + one
  undo) and `Copy_then_paste_twice...`. Core **93**, Avalonia **29** (122). 0/0 both TFMs.

## F-035 — Editor ops: layout-undo, select-all, z-order (2026-06-03, post-0.1.0 backlog 1-2)

- **Layout undo:** auto-layout moves via `SetPosition` (no `Moved` event), so the history can't observe them.
  Added `DiagramHistory.RecordApplied(cmd)` (record an already-applied command) + `DiagramCanvas
  .RunAsUndoableMove(mutate)` which snapshots positions, runs the bulk mutation in a `Transaction()`, and
  records a `MoveNodeCommand` per changed node → one undo step. Demo Layout button uses it.
- **Select-all:** `Diagram.SelectAll()` (nodes + links + groups, batched); `Ctrl+A` in the canvas (works in
  read-only too — selection is allowed there).
- **Z-order:** `Diagram.SendToFront/SendToBack` existed but weren't visual — `NodesLayer` now binds each
  `NodeView.ZIndex` to `node.Order` (updated on `OrderChanged`). Canvas exposes `BringSelectionToFront/
  SendSelectionToBack` (wired to the context menu next). Z-order is **not** undoable yet (noted).
- **Tests:** Core `Select_all...`, `Send_to_front...`, `Send_to_back...`; Avalonia
  `RunAsUndoableMove_groups_a_bulk_reposition_into_one_undo`. Core **92**, Avalonia **27** (119). 0/0 both TFMs.

## F-034 — Undo transactions: group a gesture into one step + cancel discarded drag-new-links (2026-06-03, post-0.1.0 feature)

Follow-up to F-033, resolving its two noted issues.

- **Transactions in `DiagramHistory`:** `BeginTransaction`/`EndTransaction` (depth-counted) + a `Transaction()`
  `IDisposable` scope. While open, auto-recorded edits buffer instead of pushing; `EndTransaction` commits the
  buffer as one `CompositeCommand` (or the single command). `Changed` fires once on commit, so toolbars don't
  flicker mid-gesture.
- **Canvas wires it to the pointer gesture:** `BeginTransaction` in `OnPointerPressed` (after the double-click
  early-return), `EndTransaction` in `OnPointerReleased` inside a `finally` so begin/end stay balanced on the
  `e.Handled` early path. Result: a **multi-node drag is one undo step** (was one-per-node, the F-033 wart).
  Empty gestures (plain clicks, pans) buffer nothing → no-op commit.
- **Transient drag-new-link fix:** F-033 recorded `AddLinkCommand` the instant the ongoing link was added on
  pointer-down; if the drag was discarded (no target, `RequireTarget`), the link was removed but the command
  lingered → phantom link on redo. Now `OnLinkRemoved` cancels a buffered add for a link added+removed *within
  the same transaction* (tracked in `_bufferedLinkAdds`), so a discarded drag leaves no history.
- **Tests:** Core `Transaction_groups_multiple_moves_into_one_undo_step`,
  `Transaction_cancels_a_link_added_then_removed_within_it`; Avalonia `Undo_reverts_a_multi_node_drag_in_one_step`
  (select 2, drag, one `Undo()` restores both). Core **89**, Avalonia **26** (115 total). 0/0 both TFMs.
- **Superseded by F-035/F-042:** bulk layout moves, vertex add/remove, group/ungroup, and z-order changes now
  have canvas history paths.

## F-033 — Undo/redo wired into the editor via an observer DiagramHistory (2026-06-03, post-0.1.0 feature)

`UndoRedoStack` + commands existed (Phase 11) but nothing recorded or triggered them (F-024 noted the wiring
as deferred).

- **Design — `DiagramHistory` (Core), an observer with a re-entrancy guard:**
  - Auto-records **adds** (`Nodes.Added`→`AddNodeCommand`, `Links.Added`→`AddLinkCommand`) and **completed
    moves** (`MovableModel.Moved` + a `lastPosition` map → `MoveNodeCommand(from,to)`). Drag works because
    `SetPosition` doesn't raise `Moved`; only `DragMovables.TriggerMoved` (pointer-up) does — so one move
    command per drag, with the correct pre-drag `from`.
  - **Deletions** can't be observed after the fact (the link cascade is gone by the `Nodes.Removed` event), so
    they're recorded by calling `history.Execute(RemoveNodeCommand)` at the delete site — `RemoveNodeCommand`
    captures the node's links in `Execute` before removing.
  - **`IsApplying` guard:** set during Execute/Undo/Redo so the Added/Moved observers don't re-record the
    changes that undo/redo themselves produce (the bug that otherwise makes undo create new history). Verified
    by a test (`CanUndo==false`/`CanRedo==true` after undoing a delete).
  - **`Push` added to `UndoRedoStack`** — records an already-applied command without re-executing (Execute
    re-runs; observed user actions are already done).
  - **Baseline:** history is created after the diagram is populated (canvas sets it in the `Diagram` change
    handler), so initial scene content isn't undoable. `CompositeCommand` groups multi-select delete into one undo.
- **Canvas wiring:** `DiagramCanvas` owns/disposes the history per diagram, exposes `Undo/Redo/CanUndo/CanRedo`
  + `HistoryChanged`, handles **Ctrl+Z / Ctrl+Y / Ctrl+Shift+Z** and **Delete/Back** in `OnKeyDown` (Delete now
  routes through history instead of forwarding to the core `KeyboardShortcuts` behavior — avoids a double
  delete), and the adorner ✕ deletes via `DeleteModels` too. Demo gains Undo/Redo toolbar buttons.
- **Superseded by F-034/F-035/F-042:** multi-node drag, bulk layout moves, vertex add/remove, group/ungroup,
  and z-order changes now have grouped canvas history paths.
- **Tests:** Core `History_records_a_completed_move...`, `..._link_add...`, `..._delete_via_execute_is_undoable
  _and_the_restore_is_not_re_recorded`; Avalonia `Undo_restores_a_dragged_node_and_redo_reapplies_it` (drag via
  mouse, `canvas.Undo()/Redo()`). Ctrl+Z key mapping is build-verified, not headless-tested. Core **87**,
  Avalonia **25** (112 total). 0/0 both TFMs.

## F-032 — Draggable link vertices (bend points): mostly free via existing behaviors (2026-06-03, post-0.1.0 feature)

Last deferred link feature. `LinkVertexModel` + `AddVertex` existed but vertices weren't rendered/interactive.

- **Key realization (minimal new code):** `LinkVertexModel : MovableModel` and its `SetPosition` calls
  `Parent.Refresh()`; `NormalRouter` (the default) routes through `link.Vertices`. And `NodelyDiagram`
  registers `SelectionBehavior` *before* `DragMovablesBehavior`. So if a vertex resolves as the model under
  the cursor, a single click-drag selects it then drags it then reroutes — entirely through existing behaviors.
  I only had to (a) draw handles and (b) make `ResolveModelAt` return the vertex.
- **New `VerticesLayer`** (immediate-mode, like `LinksLayer`): draws a small square handle at each vertex of
  every link that has vertices (handles shown always, *not* gated on selection — clicking a vertex deselects
  its link, so a selection-gated handle would vanish mid-grab). Subscribes to `Links.Added/Removed` + each
  `link.Changed` to repaint as vertices move/add/remove. Geometric `HitTest(point, zoom)` (radius scaled by
  zoom). Layer sits above ports, below adorners.
- **Hit priority** (in `ResolveModelAt`): ports/nodes > **vertex** > link > group — so you grab the handle, not
  the line it sits on.
- **Add/remove:** `OnPointerPressed` with `e.ClickCount == 2` — double-click a vertex removes it; double-click a
  `Segmentable` link inserts a vertex at the click, choosing the insert index by **least added detour**
  (`dist(a,p)+dist(p,b)-dist(a,b)` over route segments) so multi-vertex order stays correct.
- **Demo:** workflow links are `Segmentable`; the Start→Build link seeds one bend point.
- **Tests:** Core `Link_routes_through_its_vertices_and_reroutes_when_a_vertex_moves`; Avalonia
  `Clicking_a_vertex_handle_selects_the_vertex` (single-click — reliable). **Double-click add/remove is NOT
  headless-tested** — headless doesn't synthesize `ClickCount==2` reliably (cf. F-030's headless limits); logic
  is simple + build-verified, needs manual check. Core **84**, Avalonia **24** (108 total). 0/0 both TFMs.
- **Superseded by F-042:** selected vertices can now be removed through the canvas delete path and undone.
  All original deferred link features (markers, selection, labels, vertices) are now done.

## F-031 — Link labels rendered along the path (2026-06-03, post-0.1.0 feature)

`LinkLabelModel` + `BaseLinkModel.AddLabel(...)` existed but labels were never drawn (deferred since F-020).

- **Positioning (Core, testable):** added `PathData.Length()` and `PathData.PointAtDistance(distance)` — both
  built on a shared private `Flatten(curveSamples)` poly-line (cubic sampler extracted to `CubicSample`, reused
  by the F-030 `DistanceTo` too). `PointAtDistance` walks accumulated segment lengths and lerps within the
  segment; clamps to the ends; null for an empty path.
- **`LinkLabelModel.Distance` semantics** (honored in `LinksLayer.ResolveLabelDistance`): `[0,1]` = fraction of
  length, `> 1` = absolute from start, `< 0` = absolute from end, `null` = midpoint. Plus an optional pixel
  `Offset`.
- **Rendering:** `LinksLayer.DrawLabel` draws a rounded chip (`DrawRectangle(brush, null, rect, 4, 4)`) +
  centered `FormattedText` (`DrawText`) at the resolved point, in diagram space, so labels scale with zoom like
  nodes. New themeable palette brushes `LabelBackground`/`LabelForeground` — added as **non-required** `init`
  props with defaults so existing `NodelyPalette` constructions (tests) don't break; built-ins set proper
  dark/light values.
- **Avalonia API (12.0.4):** `new FormattedText(text, CultureInfo, FlowDirection, Typeface.Default, emSize,
  brush)`, `DrawingContext.DrawText(FormattedText, Point)`, and `DrawRectangle(..., radiusX, radiusY)` all
  compile/work.
- **Demo:** state-machine transitions now carry labels (`start`/`ok`/`fail`/`reset`) via `.AddLabel(...)`.
- **Superseded by F-042:** changing label content, distance, or offset now refreshes the parent link.
- **Tests:** Core `PathData_length_and_point_at_distance_along_a_polyline`. Rendering itself isn't pixel-tested
  (headless has no text rendering — see F-030); positioning math is covered in Core. Core **83**, Avalonia **23** (106 total). 0/0 both TFMs.
- **Still deferred:** draggable **vertices/bend points** (model exists, not interactive).

## F-030 — Link selection + hit-testing; headless has no geometry math so don't use `StrokeContains` (2026-06-03, post-0.1.0 feature)

Links were `IsHitTestVisible = false` (deferred since F-020) — you couldn't click one to select/delete it.

- **Design:** links are immediate-mode (not visuals), so I added geometric hit-testing rather than visual
  hit-testing, and routed it through the *existing* behavior system. `DiagramCanvas.ResolveModelAt` now, after
  the node/port/group visual walk, calls `LinksLayer.HitTest(diagramPoint, zoom)`. Because the result feeds the
  same `TriggerPointerDown`, selection + delete come for free: `SelectionBehavior` already selects any
  `SelectableModel`, and `KeyboardShortcutsDefaults.DeleteSelection` already removes a selected `BaseLinkModel`.
  Selected links render with `Palette.Selection` + a thicker pen (and the arrowhead matches).
- **Hit-priority:** ports/nodes > **link** > group. A group is a background container, so a click on a thin
  link crossing a group should pick the link; nodes/ports still win. Implemented by remembering the group hit
  during the visual walk and only returning it if no link is hit.
- **THE GOTCHA:** first attempt used Avalonia `Geometry.StrokeContains(pen, point)` on the cached
  `StreamGeometry`. The headless test failed — the click never selected. Cause: the test harness uses
  `UseHeadless(new AvaloniaHeadlessPlatformOptions())` with default `UseHeadlessDrawing = true`, whose geometry
  impl is a **no-op stub** → `StrokeContains`/`FillContains` return false. It'd work on a real Skia backend, but
  hit-testing must not depend on the render backend (and shouldn't be untestable headless).
- **Fix / better design:** added `PathData.DistanceTo(point, curveSamples=16)` in Core — flattens Béziers
  (quadratics promoted to cubics) and returns min point-to-segment distance. Pure math, backend-independent,
  unit-tested in Core (no headless needed). `HitTest` is now `FullPath.DistanceTo(point) <= max(width/2,
  HitTolerance/zoom)` — tolerance divided by zoom so the clickable band stays a constant ~6px on screen.
  Bonus: hit-testing no longer needs to build/keep Avalonia geometry.
- **Lesson:** headless Avalonia (default options) has **no real drawing/geometry** — anything relying on
  `StrokeContains`, `FillContains`, pixel capture, or measured render output won't work unless you set
  `UseHeadlessDrawing = false` (needs Skia). Prefer pure-math, Core-testable logic for hit-testing/geometry.
- **Tests:** Core `PathData_distance_to_a_line_segment` + `..._cubic_curve...`; Avalonia
  `Clicking_a_link_selects_it_and_clicking_empty_space_clears_it`. Core **82**, Avalonia **23** (105 total). 0/0 both TFMs.
- **Still deferred:** link **labels** and draggable **vertices/bend points** (models exist, not rendered/interactive).

## F-029 — Link arrowheads/markers: Core already supported them, only rendering was missing (2026-06-03, post-0.1.0 feature)

User reviewing the demo wanted direction-readable links. Directed diagrams (workflow, state machine) had no
arrowheads.

- **What already existed in Core:** `LinkMarker` (neutral `PathData` shapes — `Arrow`, `Square`),
  `BaseLinkModel.SourceMarker/TargetMarker`, and both path generators already shortened the route for the
  marker and returned `Source/TargetMarkerAngle` + `Source/TargetMarkerPosition` in `PathGeneratorResult`.
  The **only** gap was that the Avalonia `LinksLayer` never drew them (deferred in F-020), and markers
  defaulted to null so nothing showed.
- **What I added:**
  1. `DiagramLinkOptions.DefaultSourceMarker/DefaultTargetMarker` (null by default), plus
     `BaseLinkModel.EffectiveSourceMarker/EffectiveTargetMarker` = `per-link ?? diagram default` — mirrors the
     existing `Router ?? DefaultRouter` / `PathGenerator ?? DefaultPathGenerator` resolution. One line
     (`Options.Links.DefaultTargetMarker = LinkMarker.Arrow`) now arrows an entire diagram; per-link still wins.
     Generators read the *effective* marker so the global default also drives route-shortening + angle.
  2. `PathDataGeometry.ToGeometry(data, filled)` — `filled:true` begins a filled, closed figure so the
     arrowhead triangle fills (link routes stay `filled:false`, open-stroked).
  3. `LinksLayer.DrawMarker` — marker shape is defined at the origin pointing +X; draw it with
     `Matrix.CreateRotation(angle) * Matrix.CreateTranslation(pos)` via `context.PushTransform(...)`, in diagram
     space (the layer's pan/zoom RenderTransform then maps to screen). Local-space marker geometry is cached
     per `LinkMarker` instance (Arrow is a singleton → one cache entry shared by all links).
- **Avalonia API check (12.0.4):** `DrawingContext.PushTransform(Matrix)` (returns disposable) and
  `Matrix.CreateRotation`/`CreateTranslation(double,double)` all compile and work — confirmed by build.
- **Orientation reasoning:** target angle = `atan2(target - prevPoint)`; the endpoint is pulled back by the
  marker width, and the arrow tip (`x = width`) lands exactly on the real endpoint pointing into the node.
- **Demo:** all three scenes set `DefaultTargetMarker = Arrow` (before adding links, so the path picks it up).
- **Tests:** Core `Default_target_marker_option_applies...` + `Per_link_marker_overrides_the_diagram_default`;
  Avalonia `Default_target_marker_flows_through_the_canvas...`. Core **80**, Avalonia **22** (102 total). 0/0 both TFMs.
- **Lesson:** the neutral `PathData` seam paid off again — markers were a render-only add because Core stayed
  rendering-agnostic and pre-computed angle/position. Still deferred: link **labels**, **vertices**, and
  link **selection/hit-testing** (`LinksLayer` is `IsHitTestVisible = false`).

## F-028 — Auto-layout collapsed cyclic graphs into one column; rewrote as Sugiyama-lite (2026-06-03, post-0.1.0 fix)

Spotted from a user screenshot of the "State machine" scene after pressing **Layout**: all four nodes
(`Idle/Running/Done/Error`) stacked in a single vertical column with overlapping links.

- **Root cause:** the original `LayeredLayout` did longest-path layering with Kahn's algorithm directly on
  the link graph. A state machine is **cyclic** (`Idle→Running→Error→Idle`), so *no* node has in-degree 0,
  Kahn's queue starts empty, the loop never runs, and every node keeps `layer = 0`. With `Horizontal=true`
  that put them all at the same x, stacked in y. The old "cycles degrade gracefully (stay at layer 0)" note
  was wrong when the *whole* graph is one cycle — it degrades to useless.
- **Fix — four-stage Sugiyama-lite** (still one self-contained static class, netstandard2.0-safe):
  1. **Break cycles** → iterative DFS; edges pointing at a node currently on the DFS stack (back edges) are
     dropped, giving a DAG. (Iterative, not recursive, to stay safe on deep graphs.)
  2. **Layer** → longest-path via Kahn on the DAG (now every node is reachable from a source).
  3. **Order within layers** → barycenter heuristic, down+up sweeps × `CrossingIterations` (default 4), to
     reduce edge crossings / line children under parents. Stable `OrderBy` preserves ties.
  4. **Place** → size-aware spacing (uses `NodeModel.Size`, falls back to default W/H), each layer **centered
     on a shared spine** so single-node layers form a straight centerline. `LayerSpacing`/`NodeSpacing` are now
     edge-to-edge **gaps** (were absolute steps); defaults retuned 160/90 → 120/40 to match prior density.
- **Result:** the state machine now reads `Idle → Running → {Done, Error}` left-to-right, Done/Error stacked
  in the last column, the `Error→Idle` back edge routing around. Demo's **Layout** button now also
  `ZoomToFit()`s (layout centers on the origin, which would otherwise be partly off-screen).
- **Lesson:** any layering pass over a user diagram must assume cycles (state machines, ret/ loops,
  dependency graphs) and break them *before* layering — never rely on the graph being a DAG.
- **Tests:** `Layered_layout_handles_cycles_instead_of_collapsing_to_one_column` and
  `Layered_layout_orders_a_layer_to_reduce_crossings`. Core suite now **78** (was 76). Existing chain/sibling
  layout tests still green. Both TFMs build 0/0.

## F-027 — Adorners froze mid-drag: node moves raise `node.Changed`, not `Diagram.Changed` (2026-06-03, post-0.1.0 fix)

Spotted from a user screenshot: selecting a node then **dragging** it left the delete-✕ and resize handle
floating at the node's *old* screen spot while the node view followed the pointer.

- **Root cause:** `NodeModel.SetPosition` (the per-pointer-move drag path via `DragMovablesBehavior`) calls
  the *node's* `Refresh()` → raises `node.Changed`, but never `Diagram.Refresh()`/`Diagram.Changed`.
  `NodesLayer`/`LinksLayer`/`PortsLayer` subscribe per-node so they tracked the drag; `AdornersLayer` only
  repositioned on `Diagram.Changed` (`OnStructureChanged`) and pan/zoom (`OnViewChanged`) — neither fires
  mid-drag. So adorners were the only overlay that didn't follow. (Arrow-key nudge goes through
  `Diagram.Batch→Refresh`, so it *did* reposition — which is why only pointer-drag exposed it.)
- **Fix:** `AdornersLayer` now subscribes each adorned node's own `Changed` event (added/removed in
  `Rebuild`, so no leak) and repositions just that node's handles live. Math was already correct and
  identical to `NodesLayer` (`pos*zoom+pan`); the bug was purely a *missing reposition trigger*, not a
  coordinate error.
- **Lesson / watch-list:** any future screen-space overlay keyed to a model must subscribe to the **model's**
  change event, not only the diagram-level `Changed` — drag mutations are intentionally node-local for perf.
- **Test:** `GroupAndAdornerTests.Adorner_follows_the_node_while_it_is_dragged` (drag +100px → handle shifts
  +100px). Avalonia headless suite now **21** (was 20).

## F-026 — Docs, gallery, packaging → v0.1.0 (2026-06-03, Phase 14) — M4

- **Packaging:** `Directory.Build.props` sets `Version 0.1.0`, package metadata, `IncludeSymbols` + `snupkg`,
  and packs the repo README into each package. `dotnet pack -c Release` produces 4 packages + 4 symbol
  packages (`Nodely.Core/.Avalonia/.Serialization/.Algorithms`). SourceLink/repo URL are placeholders until
  a git remote exists (noted in the props + CHANGELOG).
- **Docs:** rewrote `README.md` (getting-started + custom-node + feature snippets + build/pack commands);
  added `CHANGELOG.md` (0.1.0).
- **Gallery:** the demo is now a 3-scene switcher — Workflow (nodes/links/groups/ports), State machine
  (node-to-node transitions + a "Layout" button using `LayeredLayout`), and a read-only Inspector — plus a
  live theme toggle. Each scene reuses one `Editor(...)` builder (canvas + minimap + zoom/fit/layout toolbar).
- **Milestone M4 reached — v0.1.0:** 15 phases (0-14) complete; 96 tests green (Core 76 + Avalonia headless
  20; both engine TFMs build clean); packages produced; tagged `v0.1.0`. Every divergence documented (F-001…F-026).

## F-025 — Performance pass: numbers + optimizations (2026-06-03, Phase 13)

- **Optimizations:** `LinksLayer` now **caches the built `StreamGeometry` per link**, invalidated only when
  that link changes (route change) — no rebuild-per-render. `NodesLayer` honors `Model.Visible`: when
  `VirtualizationBehavior` toggles a node off-screen, its `NodeView.IsVisible` collapses (skips
  measure/arrange) — wires the Phase-3 virtualization into the UI.
- **Benchmark harness** (`bench/Nodely.Benchmarks`, Stopwatch, Release, net10.0) — documented engine
  throughput for **2000 nodes / ~4000 links**:
  - add 2000 nodes: ~3 ms
  - add ~4000 links (each generates its first path): ~23 ms
  - regenerate all ~4000 link paths (route + smooth bezier): **~15 ms** (~3.8 µs/link)
  - layered layout of 2000 nodes: ~31 ms
  - serialize (1.26 MB JSON): ~61 ms; deserialize: ~61 ms
- **Scale target met:** the headless engine handles a 2000-node / 4000-link graph with full re-routing in
  ~15 ms — comfortably interactive. (Used a Stopwatch harness rather than full BenchmarkDotNet runs to get
  real numbers fast; BenchmarkDotNet could be added later for statistical rigor.)
- Existing 96 tests still green after the optimizations (correctness preserved).

## F-024 — Serialization + undo/redo (2026-06-03, Phase 11)

- **`Nodely.Serialization`:** versioned `DiagramSnapshot` DTOs (init-only records) + `DiagramSerializer`
  (`ToSnapshot`/`Load`/`Serialize`/`Deserialize`, System.Text.Json). Endpoints capture the anchor kind
  (Port id / Node id / free Position). Custom node types round-trip via `Kind` + an optional
  `Func<NodeSnapshot, NodeModel>` factory on load.
- **Round-trip test compares JSON strings, not records** — record value-equality does *not* deep-compare
  `List<T>` members (reference equality), so `snapshot1 == snapshot2` is false even with equal content.
  Comparing `Serialize(original) == Serialize(loaded)` is the robust check (and it's byte-identical).
- **netstandard2.0 again:** `System.Text.Json` is framework on net10 but a **conditional package** on ns2.0
  (pinned 8.0.5); and `Nodely.Serialization` needed its **own** `IsExternalInit` polyfill (the one in Core
  is internal to Core). Also used the non-generic `Enum.Parse(typeof(T), s)` (the generic `Enum.Parse<T>`
  is ns2.1+).
- **Core change:** added a `GroupModel(string id, …)` ctor so groups round-trip with their ids.
- **Commands/undo-redo (`Nodely.Commands` in Core):** `IDiagramCommand` + `UndoRedoStack` (execute pushes
  undo + clears redo). Commands: Add/Remove/Move node, Add/Remove link. `RemoveNodeCommand` captures the
  node's links before removal (which cascades them out) and re-adds them on undo — restoring both node and
  links. UI is not yet wired to the stack (a Phase-14 polish); the layer is ready for consumers.

## F-023 — Theming, read-only, resize, a11y (2026-06-03, Phase 10) — M3

- **Theming via a `NodelyPalette`** (light/dark): `DiagramCanvas.Palette` (styled property, default Dark).
  Immediate-mode layers (grid, links) read it live in `Render`; retained views (node default, port, group,
  selection outline) read it at creation, so a palette change calls `Rebuild()` on the node/port/group
  layers to recreate views. Canvas `Background`/`GridBrush` come from the palette. Custom nodes
  (`RegisterNode`) own their colors. Avalonia's `ThemeVariant` resource system was *not* used — a single
  palette object is simpler and keeps the colors in one place.
- **Read-only mode** (`DiagramCanvas.IsReadOnly`): implemented by **unregistering/re-registering** the two
  mutating behaviors (`DragMovablesBehavior`, `DragNewLinkBehavior`) via the diagram's behavior registry —
  selection/pan/zoom stay, move/connect stop. Adorners are hidden and keyboard edits aren't forwarded
  (Delete blocked). Clean use of the registry.
- **Resize handles** (the Phase-8 deferral): `NodeModel.ControlledSize` is now **settable** so a resized
  size survives the layout size-feedback (`NodesLayer` measures `ControlledSize` nodes at their fixed size).
  The handle is a screen-space `Border` with manual pointer-drag (reliable + visible vs. a `Thumb`) at the
  node's bottom-right; resize sets `ControlledSize=true` + `Size += delta/zoom` (min 20).
- **Accessibility (minimal):** Escape clears the selection; arrow keys nudge selected nodes (by the grid
  step); a basic `DiagramCanvasAutomationPeer` (custom control type) exposes the surface.
- **Gotcha:** headless `KeyPress` needs the full signature `KeyPress(Key, RawInputModifiers, PhysicalKey,
  string? keySymbol)` in Avalonia 12 — pass the physical key and a `null` symbol.

## F-022 — Navigator, zoom controls, snap-to-grid (2026-06-03, Phase 9)

- **`DiagramNavigator`** (public, reusable): a `Control` minimap bound to the same diagram. Renders every
  node at a fit-to-content scale plus the current viewport rectangle (viewport diagram bounds =
  `[-Pan/Zoom, (Container-Pan)/Zoom]`); click/drag pans the main diagram by centering the clicked content
  point in the viewport (`SetPan(W/2 - gx*Zoom, …)`). Confirms a plain leaf `Control` is hit-testable
  across its bounds by default (only `Panel`/`Border` need a `Background` for hit-testing — which is why
  `GridLayer` had to opt out with `IsHitTestVisible=false`).
- **Zoom controls** on `DiagramCanvas`: `ZoomToFit` (delegates to `Diagram.ZoomToFit`), `ZoomIn`/`ZoomOut`
  (zoom-about-viewport-center: `panNew = c - (c - panOld)*ratio`, clamped to the zoom min/max), `ResetView`.
  The demo wires them to a small toolbar.
- **Snap-to-grid was already in the engine** — `DragMovablesBehavior.ApplyGridSize` snaps when
  `Options.GridSize` is set (Phase 3). Phase 9 just exercises/demos it. Note: there are **two** grid knobs —
  the *visual* grid (`DiagramCanvas.GridSize`, default 24) and the *snap* grid (`Options.GridSize`); the
  demo sets both to 24 to align them. (Unifying them is a possible later convenience.)

## F-021 — Groups, marquee, and adorners (2026-06-03, Phase 8)

- **Groups:** `GroupsLayer`/`GroupView` mirror the nodes layer but with **no size feedback** — a group's
  `Size` is model-driven (auto-fits children). Groups sit below links/nodes (z-order grid < groups < links
  < nodes < ports < adorners < selection-box). A group only sizes once its children have measured sizes;
  since `GroupsLayer` arranges before `NodesLayer` in a pass, the group lands correctly on the *next* pass
  (child `SizeChanged` → group `UpdateDimensions` → `Changed` → re-arrange). `ResolveModelAt` now resolves
  groups too, so a group is draggable (moves children) and selectable.
- **Marquee:** Shift-drag on empty canvas. Panning is already suppressed when Shift is held
  (`PanBehavior`). The canvas tracks the drag in **screen space** (drawn by `SelectionBoxLayer`), and on
  release converts the rectangle to diagram coords via `GetRelativeMousePoint` and selects every node/group
  whose `GetBounds()` intersects it (in a `Batch`).
- **Adorners (delete):** `AdornersLayer : Canvas` is **screen-space** (fixed-size, doesn't scale with
  zoom) — repositioned on pan/zoom/structure changes. It shows a ✕ `Button` at each selected node's
  top-right; clicking removes the node. Key fix: the canvas pointer handlers now **guard on `e.Handled`**
  so an adorner button click doesn't also pan/select (the `Button` marks the press handled before it
  bubbles to the canvas's class handler).
- **Deferred:** node **resize handles** → Phase 10 (they need `ControlledSize` to be settable so the
  resized size survives the node-size feedback in `NodesLayer`; cleaner to do with theming). The Core
  `ControlsLayer`/`Control` abstraction stays available for custom user adorners later.
