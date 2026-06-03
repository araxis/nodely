# Nodely — Development Plan

A phased, vertical-slice plan. Each phase has a **Goal**, **Work**, and an explicit **Definition of Done
(DoD)** that is testable. We do not start a phase until the previous DoD is green. Source-first applies
throughout: verify upstream behavior and Avalonia 12.0.4 APIs before coding, and cite findings in
`02-research/` and `04-progress/`.

**Stack (ADR-0004):** Avalonia 12.0.4, .NET 8 LTS (Core/Algorithms/Serialization multi-target
`netstandard2.0;net8.0`), C#-only UI, xUnit + Shouldly, Avalonia.Headless.XUnit, MVVM-agnostic core.

**Architecture:** first-party ported brain `Nodely.Core` (ADR-0002) + hybrid Avalonia renderer
`Nodely.Avalonia` (ADR-0003) + DataTemplate-based customization (ADR-0005).

## Execution principle — slice first

Phases 1–4 build the headless brain bottom-up because nodes/links are meaningless without it. To avoid a
"big bang" integration risk, we insert a **Tracer Bullet** right after Phase 2: a throwaway-grade thin
slice (minimal engine + minimal canvas + one node + pan) to prove the whole stack wires together early.
Then we return to deepen the brain (Phases 3–4) and build each render phase (5+) as a runnable slice.

## Milestones

- **M1 — Headless brain (end of Phase 4):** complete, tested, UI-agnostic `Nodely.Core`.
- **M2 — Interactive editor MVP (end of Phase 7):** pan/zoom/select/drag + custom nodes + interactive
  link creation, visible and headless-tested.
- **M3 — Production-grade UX (end of Phase 10):** groups, widgets, theming, accessibility, read-only.
- **M4 — Release v0.1.0 (end of Phase 14):** serialization, undo/redo, algorithms, docs, gallery, NuGet.

---

## Phase 0 — Foundation & scaffolding

- **Goal:** a buildable, testable, CI-green solution with an empty Avalonia canvas window.
- **Work:**
  - `Nodely.sln`; projects per ADR-0001 (`Nodely.Core`, `Nodely.Avalonia`, `Nodely.Algorithms`,
    `Nodely.Serialization`, `Nodely.Demo`, `Nodely.Core.Tests`, `Nodely.Avalonia.Tests`).
  - `Directory.Build.props` (nullable enable, latest LangVersion, deterministic, SourceLink, XML docs,
    warnings-as-errors in CI), `Directory.Packages.props` (central versions, pin Avalonia 12.0.4),
    `.editorconfig`, `.gitignore`.
  - `git init`; MIT `LICENSE`; `THIRD-PARTY-NOTICES` (Blazor.Diagrams MIT attribution); root `README`.
  - GitHub Actions: build + test (+ pack on tag).
  - `Nodely.Demo` opens a window hosting an (empty) `DiagramCanvas` placeholder with a background.
- **DoD:** `dotnet build` and `dotnet test` pass locally and in CI; demo app launches showing an empty
  canvas surface; repo committed.

## Phase 1 — Core geometry & model primitives

- **Goal:** the pure, tested foundation of the brain.
- **Work (port from `Blazor.Diagrams.Core/Geometry` + `Models/Base`):**
  - Geometry: `Point`, `Size`, `Rectangle`, `Line`, `Ellipse`, `BezierSpline`, `IShape`, `Shapes`
    (intersections, bounds, containment, distance, bezier sampling).
  - Base models: `Model` (Id + `Changed`), `SelectableModel`, `MovableModel`.
  - `Layer<T>` observable ordered collection (Added/Removed/order events).
  - Neutral input types: `PointerEvent`, `WheelEvent`, `KeyEvent`, `PointerButton`.
- **DoD:** xUnit tests cover geometry math (line/line + line/rect + shape intersections, bounds,
  bezier point-at-t) and layer add/remove/reorder/notify; all green. No UI references.

## Phase 2 — Diagram engine (no rendering)

- **Goal:** a working in-memory diagram you can drive entirely from code/tests.
- **Work (port `Diagram.cs`, `Models`, `Anchors`, `Positions`, `Options`, `Layers`):**
  - `Diagram` base: Nodes/Links/Groups/Controls layers; Pan/Zoom/Container; `Trigger*` seam;
    selection; ordering (`SendToBack/Front`); `Batch/Refresh`, `SuspendRefresh/Sorting`; behavior registry.
  - `NodeModel`, `PortModel`, `PortAlignment`, `LinkModel`, `GroupModel`, `LinkLabelModel`,
    `LinkVertexModel`, `LinkMarker`.
  - Anchors: `SinglePortAnchor`, `DynamicAnchor`, `ShapeIntersectionAnchor`, `LinkAnchor`, `PositionAnchor`.
  - `DiagramOptions` (+ Links/Zoom/Grid/Constraints); concrete `NodelyDiagram` with default options.
  - Coordinate transforms: `GetRelativeMousePoint/GetRelativePoint/GetScreenPoint`, `SetZoom/SetPan/ZoomToFit`.
- **DoD:** headless tests: create nodes + ports, connect with a link, link endpoints resolve to anchor
  points; select/unselect; pan/zoom math + coordinate round-trips; `ZoomToFit` frames known content;
  ordering works. Zero rendering.

## Tracer Bullet (integration checkpoint, after Phase 2)

- **Goal:** prove the stack end-to-end before deepening.
- **Work:** minimal `DiagramCanvas` that hosts one hard-coded node control over a pan transform and feeds
  `PointerPressed/Moved/Released` into `Trigger*`. Throwaway quality; not the final renderer.
- **DoD:** demo shows one node; dragging the empty canvas pans it. Captured learnings → findings file.
  (This de-risks Phases 5–6; code may be discarded/rewritten.)

## Phase 3 — Behaviors (interaction state machines)

- **Goal:** all default interactions, still UI-agnostic and unit-testable.
- **Work (port `Behaviors/`):** `SelectionBehavior`, `DragMovablesBehavior`, `DragNewLinkBehavior`,
  `PanBehavior`, `ZoomBehavior`, `KeyboardShortcutsBehavior` (+Defaults), `VirtualizationBehavior`,
  `EventsBehavior`, `DebugEventsBehavior`. Registered explicitly on `NodelyDiagram`.
- **DoD:** tests drive synthetic pointer/key/wheel sequences through `Trigger*`: drag moves a node;
  drag from a port creates a link; marquee selects; wheel zooms about the pointer; Delete removes
  selection; virtualization marks offscreen models. All without Avalonia rendering.

## Phase 4 — Routers & path generators (neutral output) — **M1**

- **Goal:** link routing + drawable path data, rendering-neutral.
- **Work (port `Routers/` + `PathGenerators/`, diverging to `PathData`):**
  - `IRouter`, `NormalRouter`, `OrthogonalRouter`.
  - `IPathGenerator`, `StraightPathGenerator`, `SmoothPathGenerator` → emit `PathData`
    (Move/Line/Cubic/Quad/Close); `PathData` exposes point/tangent sampling for labels + markers.
  - Replace `SvgPathProperties` with own bezier/line sampling.
- **DoD:** tests assert route waypoints and `PathData` command sequences for straight/smooth/orthogonal
  links across sample port pairs and with vertices; marker tangents + label anchor points correct.
  **M1 reached: `Nodely.Core` is complete, independent, and fully tested.**

## Phase 5 — Avalonia canvas shell: pan/zoom + grid (first real slice)

- **Goal:** a pannable, zoomable, gridded canvas bound to a `Diagram`.
- **Work (`Nodely.Avalonia`):** `DiagramCanvas : TemplatedControl` hosting layer hosts under one
  pan/zoom transform; Avalonia input → neutral events → `Trigger*` (with pointer capture); canvas
  `Bounds` → `SetContainer`; immediate-mode `GridWidget`. Source-verify transform/input APIs first.
- **DoD:** demo shows an infinite grid that pans (drag) and zooms (wheel, centered on pointer); headless
  test asserts a drag pans and a wheel event changes `Zoom` and re-centers. First end-to-end vertical slice.

## Phase 6 — Node rendering & templating (customization headline)

- **Goal:** nodes render, move, and are trivially customizable.
- **Work:** `NodesLayer : Panel` (virtualizing) arranging `NodeView` host controls by `NodeModel.Bounds`;
  DataTemplate registry keyed by node model type (`RegisterNode<T>` + `FuncDataTemplate`/`IDataTemplate`);
  default node template; measured size → `NodeModel.Size` (replaces ResizeObserver); selection visuals;
  drag-move via Phase 3 behaviors.
- **DoD:** demo renders default nodes plus a **user-defined custom node in ≤10 lines** (model + template);
  drag-move updates the model; resizing content updates `Size`. Headless tests: template resolves by type;
  moving a node updates position; offscreen nodes are virtualized.

## Phase 7 — Ports & links rendering + interactive link creation — **M2**

- **Goal:** connect nodes by dragging; links route and render; labels/vertices/markers work.
- **Work:** `PortView` templated controls positioned by alignment; `LinksLayer : Control` immediate-mode
  (`StreamGeometry` from `PathData`, cached per route, dirty-redraw); `LinkLabelView` + `LinkVertexThumb`
  overlays; arrowhead markers; `DragNewLinkBehavior` wiring; invalid-connection feedback hook; optional
  opt-in per-link template.
- **DoD:** demo — drag from a port to another port/node creates a `LinkModel`; links route
  (normal/orthogonal) and render (straight/smooth); labels show; vertices drag; markers point correctly;
  custom link style (brush/dash/markers) applies. Headless test: port→port drag yields a link with correct
  source/target/anchors. **M2 reached: interactive editor MVP.**

## Phase 8 — Groups, selection box, controls layer

- **Goal:** grouping, marquee selection, and on-model adornments.
- **Work:** `GroupView` (auto-bounds container; moving group moves children; configurable delete =
  delete-children vs ungroup); immediate-mode `SelectionBoxWidget`; `ControlsLayer` adorners (resize
  handles, delete button) ported from upstream Controls.
- **DoD:** demo — marquee multi-select; group/ungroup; move group moves children; resize handle resizes a
  node; delete-control deletes. Tests: group bounds fit children; membership persists; nested groups
  (if enabled) behave.

## Phase 9 — Widgets: navigator/overview, snap-to-grid, zoom controls

- **Goal:** navigation aids and precision placement.
- **Work:** immediate-mode `NavigatorWidget` (minimap: scaled node draw + viewport rect + click/drag to
  pan); snap-to-grid behavior + option; `ZoomToFit` button + zoom in/out/reset controls; formalize grid
  as a toggleable widget.
- **DoD:** demo — minimap reflects content and navigates the viewport; snap toggles and quantizes
  movement; zoom-to-fit frames all content; zoom buttons work. Tests: zoom-to-fit math, snap quantization,
  navigator viewport↔pan mapping.

## Phase 10 — Theming, accessibility, read-only — **M3**

- **Goal:** production-grade look, keyboard access, and first-class read-only mode.
- **Work:** C# `NodelyTheme : Styles` with `ControlTheme`s + state classes (`:selected/:locked/.invalid`);
  light/dark variants; read-only/locked mode blocking edit commands and drag/connect; keyboard nav +
  focus order + `AutomationPeer`s for nodes/canvas; respect reduced-motion.
- **DoD:** theme added via one line; light/dark verified in demo; read-only blocks all mutation;
  keyboard can select/move/delete a node; basic automation peers exposed. Headless tests for read-only
  blocking + keyboard interactions. **M3 reached.**

## Phase 11 — Serialization + commands/undo-redo

- **Goal:** persist diagrams and support reversible editing.
- **Work:** `Nodely.Serialization` — versioned `DiagramSnapshot`/`NodeSnapshot`/`LinkSnapshot`/
  `GroupSnapshot`/`ViewportSnapshot` (+ `Metadata`) with System.Text.Json round-trip and a migration hook;
  command layer (`AddNode/Connect/Delete/Duplicate/Align/ZoomToFit/...`) + undo/redo stack over commands.
- **DoD:** round-trip test (serialize→deserialize→structurally equal); undo/redo of move/add/delete/group;
  version-bump migration test. Demo: save/load + undo/redo buttons.

## Phase 12 — Algorithms (optional package)

- **Goal:** auto-layout and graph utilities on demand.
- **Work:** `Nodely.Algorithms` — traversal, connected components, and at least one auto-layout
  (layered/tree; force-directed optional). Verify approach against upstream Algorithms + literature.
- **DoD:** auto-layout arranges a sample graph without overlaps; unit tests on small graphs; demo button
  "auto-layout".

## Phase 13 — Performance pass & large-graph virtualization

- **Goal:** hit and document interactive performance at scale.
- **Work:** BenchmarkDotNet micro-benchmarks (path build, route, hit-test) + headless render-timing;
  tune virtualization, geometry caching, batched refresh, allocation hot-spots; dirty-region link redraw.
- **DoD:** documented target met — e.g. **2,000 nodes / 3,000 links pan+zoom at interactive frame rate on
  a desktop dev machine** (record actual numbers + method in findings). Regression benchmarks committed.

## Phase 14 — Docs, sample gallery, packaging & release — **M4**

- **Goal:** ship v0.1.0.
- **Work:** sample gallery slices (workflow builder, ERD/database designer, state machine, mind map,
  read-only inspector); XML docs on public API; getting-started + customization guides; `dotnet pack`
  for `Nodely.Core/.Avalonia/.Algorithms/.Serialization` with SourceLink/symbols; SemVer + CHANGELOG;
  tag `v0.1.0`.
- **DoD:** `dotnet pack` produces all packages; gallery demonstrates every shipped feature; getting-started
  doc verified by a fresh consuming project; v0.1.0 tagged. **M4 reached.**

---

## Cross-cutting "definition of done" (every phase)

- New public API has XML docs and is MVVM-agnostic.
- New behavior has unit tests (brain) and/or headless tests (Avalonia); failure/edge paths covered.
- No reflection scanning; all extension points registered explicitly.
- `dotnet build` warnings-as-errors clean; `dotnet test` green in CI.
- Source-first: any non-obvious upstream/Avalonia behavior verified and cited in `02-research/` or
  `04-progress/findings-and-learnings.md`.
- `04-progress/progress-log.md` updated; `phase-checklist.md` ticked.
