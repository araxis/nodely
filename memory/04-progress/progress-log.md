# Progress log

Newest first. One entry per working session or notable change. Keep it factual: what changed, why,
what's next.

## 2026-06-05 — Network side package

- Package: added `Nodely.Avalonia.Network` as the sixth optional domain pack, starting at side-package
  version `0.1.0` while main packages remain on `0.7.0`.
- Models: router, switch, firewall, load balancer, server, client, cloud, service, and zone nodes with network
  ports and topology links for fiber, ethernet, wireless, tunnel, dependency, and blocked connections.
- Rendering/layout: one `UseNetworkNodes()` canvas extension registers device-shaped visuals, visible typed
  ports, status badges, network link glyphs, and curved topology paths; `NetworkLayout.Arrange()` provides a
  small pack-local topology layout.
- Demo/docs: gallery now includes a Network scene with editable runtime properties, theme switching, save/load,
  arrange, visible switch ports, firewall/service/cloud/server visuals, and mixed network link types; docs
  include a Network guide and package matrix updates.
- **Verified:** build 0/0; `dotnet test` -> Core 111/111 + Avalonia 56/56 + Database 7/7 + MindMap 7/7
  + Network 7/7 + StateMachine 7/7 + UML 6/6 + Workflow 5/5 on both `net8.0` and `net10.0`; `dotnet pack`
  -> main packages `0.7.0`, Database/MindMap/Network/StateMachine/UML/Workflow `0.1.0`; Network package
  inspection confirmed `lib/net8.0`, `lib/net10.0`, symbol package assets, and main-package `0.7.0`
  dependency groups; docs install dry-run and docs build passed.

## 2026-06-05 — StateMachine side package

- Package: added `Nodely.Avalonia.StateMachine` as the fifth optional domain pack, starting at side-package
  version `0.1.0` while main packages remain on `0.7.0`.
- Models: initial, state, final, choice, and note nodes with state-machine ports and transition links for
  normal, self, choice, error, and timeout transitions.
- Rendering/layout: one `UseStateMachineNodes()` canvas extension registers distinct state-machine visuals,
  visible transition ports, transition glyphs, and self-loop drawing; `StateMachineLayout.Arrange()` provides
  a small pack-local left-to-right layout.
- Demo/docs: gallery now includes a StateMachine scene with editable runtime properties, theme switching,
  save/load, arrange, self transitions, guarded transitions, timeout/error transitions, and visible ports; docs
  include a StateMachine guide and package matrix updates.
- **Verified:** build 0/0; `dotnet test` -> Core 111/111 + Avalonia 54/54 + Database 7/7 + MindMap 7/7
  + StateMachine 7/7 + UML 6/6 + Workflow 5/5 on both `net8.0` and `net10.0`; `dotnet pack` -> main
  packages `0.7.0`, Database/MindMap/StateMachine/UML/Workflow `0.1.0`; StateMachine package inspection
  confirmed `lib/net8.0`, `lib/net10.0`, symbol package assets, and main-package `0.7.0` dependency groups;
  docs install dry-run and docs build passed.

## 2026-06-05 — MindMap side package

- Package: added `Nodely.Avalonia.MindMap` as the fourth optional domain pack, starting at side-package version
  `0.1.0` while main packages remain on `0.7.0`.
- Models: root, branch, and leaf topics with topic text, notes, accent color, icon key, collapse state, side
  hints, MindMap ports, and branch/association links.
- Rendering/layout: one `UseMindMapNodes()` canvas extension registers topic visuals, visible branch ports,
  curved link styling, and collapse badges; `MindMapLayout.Arrange()` and `ApplyCollapseState()` provide the
  pack-local root/branch workflow.
- Demo/docs: gallery now includes a MindMap scene with editable runtime properties, theme switching,
  save/load, arrange, collapsed branches, branch links, and association links; docs include a MindMap guide and
  package matrix updates.
- Renderer correctness: Avalonia link and port layers now honor model visibility so collapse state hides
  dependent paths and handles.
- **Verified:** build 0/0; `dotnet test` -> Core 111/111 + Avalonia 52/52 + Database 7/7 + MindMap 7/7
  + UML 6/6 + Workflow 5/5 on both `net8.0` and `net10.0`; `dotnet pack` -> main packages `0.7.0`,
  Database/UML/Workflow `0.1.0`, and MindMap `0.1.0`; MindMap package inspection confirmed `lib/net8.0`,
  `lib/net10.0`, symbol package assets, and main-package `0.7.0` dependency groups; docs install dry-run and
  docs build passed.

## 2026-06-05 — runtime property editing

- Public API: added `EditModelCommand`, `DiagramCanvas.RunAsUndoableEdit()`, and `DiagramCanvas.RefreshVisuals()`
  so host apps can route metadata edits through the same undo/redo history as movement, layout, grouping, and
  delete.
- Demo gallery: added a side-panel property inspector that edits selected core nodes/links, sample custom
  nodes/links, Database tables/views/procedures/relationships, UML classes/interfaces/enums/relationships, and
  Workflow nodes/links at runtime.
- Save/load: gallery load now registers Database, UML, and Workflow renderers after deserializing through the
  shared registry, so edited side-package diagrams come back with pack renderers active.
- Docs: README, changelog, and Recipes now document the runtime edit pattern.
- **Verified:** `dotnet build samples\Nodely.Demo\Nodely.Demo.csproj --configuration Debug --no-restore
  --disable-build-servers /nr:false`; `dotnet test` -> Core 111/111 and Avalonia 50/50 on both `net8.0`
  and `net10.0`.

## 2026-06-05 — domain pack visual polish

- Database: upgraded table, view, and procedure renderers with object-specific headers, field rows, key/null
  badges, row-aware database ports, relationship endpoint styling, and richer gallery data.
- UML: upgraded class, interface, enum, package, and note renderers with compartments, stereotypes, flags,
  folded notes, row-aware UML ports, relationship endpoint styling, and a port-based gallery scene.
- Core/Avalonia: `PortModel.GetPortCenter()` now gives packs a stable endpoint override, port layout measures
  pack-owned port visuals, and late node renderer registration rebuilds existing node views.
- Docs/memory: README, docs index, Database guide, UML guide, changelog, progress notes, checklist, and findings
  now record the side-package visual standard.
- **Verified:** build 0/0; `dotnet test` -> Core 110/110 + Avalonia 49/49 + Database 7/7 + UML 6/6
  + Workflow 5/5 on both `net8.0` and `net10.0`; `dotnet pack` -> main packages `0.7.0`, Database
  `0.1.0`, UML `0.1.0`, and Workflow `0.1.0`; docs install dry-run and docs build passed.

## 2026-06-05 — Workflow side package

- Package: added `Nodely.Avalonia.Workflow` as the third optional domain pack, starting at side-package version
  `0.1.0`.
- Models: workflow start, end, task, decision, gateway, event, note, and workflow link types.
- Rendering: one `UseWorkflowNodes()` canvas extension registers Workflow node renderers, link styling, and
  compact workflow link markers.
- Serialization: `WorkflowNodeFactory.CreateRegistry()` restores Workflow nodes and links through
  `DiagramSerializationRegistry`.
- Workflow: package workflow now includes Workflow in the side-package map and avoids a duplicate symbol push.
- **Verified:** build 0/0; `dotnet test` -> Core 110/110 + Avalonia 48/48 + Database 6/6 + UML 5/5
  + Workflow 5/5 on both `net8.0` and `net10.0`; `dotnet pack` -> main packages `0.7.0`, Database
  `0.1.0`, UML `0.1.0`, and Workflow `0.1.0`; Workflow package inspection confirmed `lib/net8.0` and
  `lib/net10.0` assets plus dependency groups; docs install dry-run and docs build passed.

## 2026-06-04 — UML side package

- Package: added `Nodely.Avalonia.Uml` as the second optional domain pack, starting at side-package version
  `0.1.0`.
- Models: UML class, interface, enum, package, note, members, operations, parameters, and relationship links.
- Rendering: one `UseUmlNodes()` canvas extension registers UML node renderers, link styling, and UML markers.
- Serialization: `UmlNodeFactory.CreateRegistry()` restores UML nodes and relationship links through
  `DiagramSerializationRegistry`.
- Workflow: package workflow now uses a side-package map for tag and dispatch selection.
- **Verified:** build 0/0; `dotnet test` -> Core 110/110 + Avalonia 46/46 + Database 6/6 + UML 5/5
  on both `net8.0` and `net10.0`; `dotnet pack` -> main packages `0.7.0`, Database `0.1.0`, and UML
  `0.1.0`; UML package inspection confirmed `lib/net8.0` and `lib/net10.0` assets plus dependency groups;
  docs install dry-run and docs build passed.

## 2026-06-04 — extension contract redesign

- Public API: added stable `ModelKind`, model-wide extra-data hooks, `DiagramSerializationRegistry`, typed
  link-style registration, and render-context factory overloads.
- Serialization: schema version 2 now carries `Kind` and `Extra` for nodes, ports, links, and groups.
- Database: pack now restores database nodes, ports, and relationship links through one registry call, and uses
  palette-aware render context plus typed link styling.
- Packaging: main packages keep `0.7.0`; `Nodely.Avalonia.Database` starts its independent side-package line at
  `0.1.0` and packs with dependencies on main package `0.7.0`.
- Workflow: package workflow now selects main packages or the database side package from tag shape or dispatch
  input.
- **Verified:** `dotnet test` -> Core 110/110 + Avalonia 44/44 + Database 6/6 on both `net8.0` and
  `net10.0`; `dotnet pack` -> main packages `0.7.0` plus database side package `0.1.0`; package inspection,
  docs install dry-run, and docs build passed.

## 2026-06-04 — extension surface investigation

- Audited the pre-public extension surface before adding more side packages.
- Confirmed database pack model and render-registration tests pass on both `net8.0` and `net10.0`.
- Confirmed database-only package dry-run can produce a side package, but current version overrides also rewrite
  main package dependency versions.
- Decision: revise PR #8 before merge so serializer registry, typed style registration, render context, and
  independent side-package versioning become the stable pack contract.

## 2026-06-04 — v0.7.0 database pack

- Package: added `Nodely.Avalonia.Database` as the first optional domain pack.
- Models: database table, view, procedure, column, parameter, port, and relationship-link types.
- Rendering: one `UseDatabaseNodes()` canvas extension registers database node, port, and link styling.
- Serialization: `DatabaseNodeFactory` restores database node types and fields through the existing snapshot
  factory path without changing the snapshot schema.
- Demo/docs: gallery database scene plus database guide, README package table, changelog, and release checklist.
- **Verified:** build 0/0; `dotnet test` -> Core 109/109 + Avalonia 42/42 + Database 5/5 on both
  `net8.0` and `net10.0`; `dotnet pack` -> five packages + symbols including `Nodely.Avalonia.Database`;
  package inspection confirmed database `lib/net8.0` and `lib/net10.0` assets with Nodely-only dependency
  groups; package feed indexes `0.6.0`; docs install dry-run and build passed.

## 2026-06-04 — v0.6.0 compatibility pass

- Packages: `Nodely.Avalonia` now ships `net8.0` and `net10.0`; Core, Algorithms, and Serialization now
  ship `netstandard2.0`, `net8.0`, and `net10.0`.
- Samples/tests: QuickStart moved to `net8.0`; Core and Avalonia test projects now run on `net8.0` and
  `net10.0`.
- Workflows/docs: CI and package workflows install both SDK lines; README, docs, changelog, and release
  checklist document the compatibility matrix.
- **Verified:** build 0/0; `dotnet test` -> Core 109/109 + Avalonia 41/41 on both `net8.0` and `net10.0`;
  `dotnet pack` -> four packages + symbols; package contents have the expected target folders and dependency
  groups; docs build green.

## 2026-06-04 — v0.5.0 adoption polish

- Public API: `DiagramCanvas` gained command-state helpers plus `CommandStateChanged` for toolbar binding.
- Samples: `Nodely.Demo` now includes workflow, state machine, inspector, and extensibility scenes with a
  command-aware toolbar; `Nodely.QuickStart` was added as a minimal copyable Avalonia app.
- Docs: recipes guide added for minimal setup, toolbar state, overlays, and save/load custom nodes.
- Version: package metadata moved to `0.5.0`; README/changelog/status updated.
- **Verified:** build 0/0; `dotnet test` -> Core 109/109 + Avalonia 41/41 (150 total); `dotnet pack` ->
  four packages + symbols; docs build green.

## 2026-06-04 — v0.4.0 hardening and editor polish

- Editor history: z-order changes, group/ungroup operations, and bend-point add/remove now have command
  coverage and route through the canvas undo stack.
- Refresh correctness: link label edits refresh their parent link, and link-to-link dependents refresh when the
  target link reroutes.
- Hardening: warnings-as-errors enabled; unsupported link factory source models now throw an argument exception.
- Docs: undo/redo and selection guides updated; release checklist guide added to the static docs site.
- Version: package metadata moved to `0.4.0`; README/changelog/status updated.
- **Verified:** build 0/0; `dotnet test` → Core 109/109 + Avalonia 37/37 (146 total).

## 2026-06-03 — Phase 14 complete: docs, gallery, packaging → MILESTONE M4 (RELEASE v0.1.0) 🎉

- Packaging: `Directory.Build.props` v0.1.0 + metadata + symbols; `dotnet pack -c Release` produces 4
  packages + 4 snupkg (Core/Avalonia/Serialization/Algorithms). README packed per-package.
- Docs: rewrote README (getting-started + custom-node + feature snippets + build/pack); added CHANGELOG (0.1.0).
- Gallery: demo is now a 3-scene switcher (Workflow, State machine + Layout button, read-only Inspector) +
  live theme toggle, all reusing one Editor(canvas + minimap + zoom/fit/layout) builder.
- **Verified:** build 0/0; `dotnet test` → Core 76/76 + Avalonia 20/20 (96 total); `dotnet pack` → 8 packages.
- ✅ **MILESTONE M4 — v0.1.0 released.** All 15 phases (0-14) complete; tagged `v0.1.0`.

### Project complete
Nodely is a faithful, independent Avalonia port of Blazor.Diagrams: a UI-agnostic engine (`Nodely.Core`,
netstandard2.0 + net10.0) + native Avalonia UI (`Nodely.Avalonia`) + optional `Serialization`/`Algorithms`.
Built phase-by-phase, source-grounded, every divergence recorded (F-001…F-026). 16 commits, 96 tests, no SVG/JS.

## 2026-06-03 — Phase 13 complete: performance pass (caching, virtualization, benchmark numbers)

- `LinksLayer` caches the built `StreamGeometry` per link (rebuild only when the link changes).
- `NodesLayer` honors `Model.Visible` (collapses off-screen `NodeView`s when `VirtualizationBehavior` runs).
- `bench/Nodely.Benchmarks` Stopwatch harness documents engine throughput (2000 nodes / ~4000 links):
  re-route + smooth-bezier paths ~15 ms, layered layout ~31 ms, serialize/deserialize ~61 ms (1.26 MB) — F-025.
- **Verified:** all 96 tests still green after the optimizations (correctness preserved).
- **Next:** Phase 14 — docs + sample gallery + NuGet packaging → MILESTONE M4 (release).

## 2026-06-03 — Phase 12 complete: algorithms (graph queries + layered auto-layout)

- `Nodely.Algorithms`: `DiagramGraph` (edges from links via anchor→node resolution; BFS/DFS; connected
  components over directed/undirected adjacency) and `LayeredLayout` (longest-path layering via Kahn's
  algorithm; horizontal/vertical; cycles degrade gracefully). Reference-equality node sets (nodes are unique).
- **Verified:** build 0/0 (both TFMs); `dotnet test` → **Core 76/76** (components, BFS, layered chain +
  sibling separation) + Avalonia 20/20.
- **Next:** Phase 13 — performance pass: BenchmarkDotNet + virtualization/caching/batching tuning, document
  a scale target.

## 2026-06-03 — Phase 11 complete: serialization + commands/undo-redo

- `Nodely.Serialization`: versioned `DiagramSnapshot` DTOs + `DiagramSerializer` (Diagram↔snapshot↔JSON via
  System.Text.Json). Endpoints capture anchor kind (Port/Node/Position); custom nodes round-trip via `Kind`
  + an optional load factory. Round-trip is byte-identical JSON.
- `Nodely.Commands` (in Core): `IDiagramCommand` + `UndoRedoStack`; Add/Remove/Move node + Add/Remove link.
  `RemoveNodeCommand` captures cascaded links and restores them on undo.
- netstandard2.0: conditional `System.Text.Json` 8.0.5 + a second `IsExternalInit` polyfill; non-generic
  `Enum.Parse`. Added a `GroupModel(string id, …)` ctor so groups keep their ids across a round-trip (F-024).
- **Verified:** build 0/0 (both TFMs); `dotnet test` → **Core 72/72** (round-trip, structure, add/move/remove
  undo-redo) + Avalonia 20/20.
- **Next:** Phase 12 — `Nodely.Algorithms`: traversal/connected-components + an auto-layout.

## 2026-06-03 — Phase 10 complete: theming, read-only, resize, a11y → MILESTONE M3 (production UX)

- Theming: `NodelyPalette` (light/dark) + `DiagramCanvas.Palette`; all built-in visuals (grid, links,
  default node, ports, groups, selection) read it; palette change rebuilds retained views. Demo "Theme" button.
- Read-only: `DiagramCanvas.IsReadOnly` unregisters the mutating behaviors (drag-move + drag-link), hides
  adorners, and blocks keyboard edits — pan/zoom/select still work. Demo "Lock" button.
- Resize handles: made `NodeModel.ControlledSize` settable; AdornersLayer adds a bottom-right resize handle
  (manual-drag Border) per selected node alongside the delete button. Completes the Phase-8 deferral.
- Accessibility: Escape clears selection, arrow keys nudge selected nodes by the grid step, basic
  `DiagramCanvasAutomationPeer`.
- **Verified:** build 0/0; `dotnet test` → Core 67/67 + **Avalonia 20/20** (palette updates brushes,
  read-only blocks drag, resize handle resizes, Escape clears). Details in F-023.
- ✅ **MILESTONE M3 reached** — production-grade UX: themeable, read-only inspector mode, resize, accessible.
- **Next:** Phase 11 — serialization (versioned JSON snapshots in `Nodely.Serialization`) + a command layer
  with undo/redo.

## 2026-06-03 — Phase 9 complete: navigator minimap, zoom controls, snap-to-grid

- `DiagramNavigator` (public): an overview minimap bound to the diagram — draws all nodes at fit-scale + the
  viewport rectangle; click/drag pans the main diagram. Reusable; place it anywhere.
- Zoom controls on `DiagramCanvas`: `ZoomToFit`, `ZoomIn`/`ZoomOut` (zoom-about-center), `ResetView`; demo
  wires them to a toolbar.
- Snap-to-grid: already an engine feature (`DragMovablesBehavior` + `Options.GridSize`); demo enables it at
  24px (matching the visual grid). Two separate knobs documented (F-022).
- Demo: now a Grid with the canvas + a bottom-right minimap + a top-right zoom toolbar; nodes snap when dragged.
- **Verified:** build 0/0; `dotnet test` → **Core 67/67** (+ snap) + **Avalonia 16/16** (zoom-to-fit frames
  content, clicking the navigator pans). Details in F-022.
- **Next:** Phase 10 — theming (C# `NodelyTheme`, light/dark), accessibility, read-only mode, and the
  deferred node resize handles. Reaches **Milestone M3 (production-grade UX)**.

## 2026-06-03 — Phase 8 complete: groups, marquee selection, delete adorner

- `GroupsLayer`/`GroupView`: render groups as auto-sized container boxes below links/nodes; groups are
  draggable (move children) and selectable (`ResolveModelAt` resolves them). No size feedback — group size
  is model-driven.
- `SelectionBoxLayer` + canvas marquee: Shift-drag on empty space draws a screen-space rectangle; on
  release it converts to diagram coords and selects every node/group whose bounds intersect (in a Batch).
  Panning is suppressed while Shift is held.
- `AdornersLayer : Canvas` (screen-space, fixed size): a ✕ delete button per selected node; click removes
  it. Canvas pointer handlers now guard on `e.Handled` so adorner clicks don't also pan/select.
- Demo: the two task nodes are grouped; drag the group, Shift-drag to marquee-select, select a node for
  the delete adorner.
- **Verified:** build 0/0; `dotnet test` → Core 66/66 + **Avalonia 14/14** (group sizes to children,
  marquee selects both nodes, delete adorner removes a node). Details in F-021.
- **Deferred:** node resize handles → Phase 10 (needs settable `ControlledSize`).
- **Next:** Phase 9 — widgets: navigator/overview minimap, snap-to-grid, zoom-to-fit + zoom controls.

## 2026-06-02 — Phase 7 complete: ports & links + interactive creation → MILESTONE M2 (editor MVP)

- `LinksLayer : Control` (immediate-mode) draws every link's generated path under the pan/zoom transform,
  via `PathDataGeometry.ToGeometry` (neutral `PathData` → Avalonia `StreamGeometry`).
- `PortsLayer : Panel` hosts a hit-testable `PortView` (Ellipse) per port; in `ArrangeOverride` it computes
  and **initializes** each port's diagram-space position/size from the node's bounds + alignment, then
  refreshes attached links so they generate their path (links resolve only once ports are `Initialized`).
- `DiagramCanvas.ResolveModelAt` now returns a `PortModel` when a port is hit (z-order grid<links<nodes<
  ports). Dragging from a port starts a link (`DragNewLinkBehavior`); `GetVisualsAt` is capture-safe so
  dropping on a port attaches the link.
- Decisions: links ship **immediate-mode** (performance-first), not the retained `LinkView` ADR-0003
  described — ADR updated, reconciliation in F-020. Markers/labels/vertices/link-selection deferred.
- Demo: Start → Build/Deploy with ports + links; drag a port to a port to add a link.
- **Verified:** build 0/0; `dotnet test` → Core 66/66 + **Avalonia 11/11** (ports initialized, link path
  generated, drag-from-port-creates-attached-link).
- ✅ **MILESTONE M2 reached** — interactive editor MVP: nodes + ports + links + drag-to-connect on screen.
- **Next:** Phase 8 — groups, marquee selection box, and the controls/adorners layer (resize/delete).

## 2026-06-02 — Phase 6 complete: node rendering & templating (the customizability headline)

- `NodesLayer : Panel` hosts one `NodeView : Decorator` per node under a shared pan/zoom `RenderTransform`
  (`Scale(zoom)` then `Translate(pan)`, origin TopLeft). Node visuals come from a `RegisterNode<T>`
  type-keyed template registry (most-derived wins) with a built-in default; a custom node is ~5-10 lines.
- Layout-driven **size feedback**: measured `NodeView.DesiredSize` → `NodeModel.Size` (replaces the browser
  ResizeObserver; setter equality guard prevents loops).
- Capture-safe **node hit-resolution** via `GetVisualsAt(point)` → walk to `NodeView.Node`, so pressing a
  node carries the right model into `Trigger*` (selects/drags it); empty canvas still pans. `GridLayer` is
  hit-test-transparent. Selection outline drawn in `NodeView.Render`.
- Demo shows a default node + a custom `TaskNode` (title + status) registered in a few lines.
- **Verified:** build 0/0; `dotnet test` → Core 66/66 + **Avalonia 8/8** (size feedback, custom template
  resolves, click selects a node, drag moves a node). Details in F-019.
- **Next:** Phase 7 — ports & links rendering + interactive link creation (drag port→port draws a routed,
  styled link with markers/labels). Reaches **Milestone M2 (interactive editor MVP)**.

## 2026-06-02 — Phase 5 complete: Avalonia canvas shell (pan/zoom + grid) — brain on screen

- `Nodely.Avalonia`: real `DiagramCanvas : Panel` (viewport) hosting an immediate-mode `GridLayer : Control`
  that draws a pan/zoom-aware background grid. Avalonia pointer/wheel/key input is translated into neutral
  `PointerEvent`/`WheelEvent`/`KeyboardEvent` and fed to `Trigger*`; pointer captured on press;
  `SetContainer` reported from `ArrangeOverride`.
- Demo now hosts a real `NodelyDiagram` — drag empty space to pan, wheel to zoom.
- Key learnings (F-018): `Panel.Render` is sealed → custom drawing must be a child `Control` layer (the
  layered structure later phases need anyway); fixed a ctor property-changed/`_grid`-null ordering bug;
  Avalonia coordinates are already canvas-relative so `Container` Left/Top = 0 (no getBoundingClientRect math).
- **Verified:** build 0/0; `dotnet test` → Core 66/66 + **Avalonia 4/4** (container-from-layout, drag-pans,
  wheel-zooms via headless input). First on-screen vertical slice works.
- **Next:** Phase 6 — node rendering & templating: `NodesLayer` virtualizing panel + `NodeView` hosts,
  DataTemplate registry (`RegisterNode<T>`), measured size → `NodeModel.Size`, drag-move on screen.

## 2026-06-02 — Phase 4 complete: routers & path generators → MILESTONE M1 (headless brain)

- `Nodely.Routers`: `NormalRouter` (vertices) + `OrthogonalRouter` (A* over a sparse grid built from node
  edges, penalizing direction changes; falls back to NormalRouter for unattached/non-port links).
- `Nodely.PathGenerators`: `StraightPathGenerator` (+ corner radius) and `SmoothPathGenerator` (cubic
  Bézier via `BezierSpline`), both emitting **neutral `PathData`** — `SvgPathProperties` is fully dropped.
- Wired `DiagramLinkOptions` defaults (Normal + Smooth); links added to a diagram now compute a real
  `Route` + `PathGeneratorResult` (marker angles/positions included).
- netstandard2.0: added a `PriorityQueue` binary-heap polyfill, fixed KVP deconstruction + index-from-end;
  made `SmoothPathGenerator`'s curve-point branch forward-compatible for the deferred Dynamic/Link anchors
  (F-017).
- **Verified:** build 0/0 (net10.0 + netstandard2.0); `dotnet test` → **Core 66/66** (6 new routing tests:
  Normal route = vertices, Straight = Move+Line, Smooth = Move+Cubic, target-marker shortening+angle,
  link-in-diagram generates a path, Orthogonal route is axis-aligned), headless 1/1.
- ✅ **MILESTONE M1 reached** — the entire UI-agnostic engine is complete, independent of any UI, and tested.
- **Next:** Phase 5 — the Avalonia `DiagramCanvas` shell (pan/zoom transform + grid, Avalonia input →
  neutral events → `Trigger*`, `SetContainer` from layout). First on-screen vertical slice.

## 2026-06-02 — Phase 3 complete: interaction behaviors (headless, driven by Trigger*)

- Ported all interaction behaviors into `Nodely.Behaviors`: `SelectionBehavior`, `DragMovablesBehavior`,
  `DragNewLinkBehavior`, `PanBehavior`, `ZoomBehavior`, `EventsBehavior` (click/double-click synthesis),
  `KeyboardShortcutsBehavior` (+ `KeyboardShortcutsDefaults`: Delete/Group), `VirtualizationBehavior`,
  `DebugEventsBehavior` (opt-in), plus `Nodely.Utils.KeysUtils`.
- Registered the defaults (in upstream order, minus Controls→Phase 8) in `NodelyDiagram`; added a
  `registerDefaultBehaviors` flag.
- Adapted to neutral events (`e.X`/`e.Y`/`PointerButton`) and netstandard2.0 (inline `Math.Clamp`, no
  `KeyValuePair` deconstruction, `default` for `ValueTask.CompletedTask`) — F-016.
- Marquee box-selection is a Phase 9 widget (not a behavior); noted in F-016.
- **Verified:** build 0/0 (net10.0 + netstandard2.0); `dotnet test` → **Core 60/60** (9 new behavior tests
  via synthetic pointer/key/wheel: select+ctrl-toggle, empty-unselect, pan, wheel-zoom, drag-move,
  drag-from-port-to-connect, Delete-removes, click-synthesis, virtualization), headless 1/1.
- **Next:** Phase 4 — Routers (`NormalRouter`/`OrthogonalRouter`) + PathGenerators (`Straight`/`Smooth`)
  emitting neutral `PathData` (own bezier/line sampling, no SvgPathProperties); wire the diagram defaults.
  **Reaches Milestone M1 — headless brain complete.**

## 2026-06-02 — Phase 2 complete: diagram engine (headless, no rendering)

- Ported the full engine from upstream source into `Nodely.Core`:
  - `Diagram` (abstract) + `NodelyDiagram`: node/link/group/controls layers, Pan/Zoom/Container, the
    `Trigger*` input seam (neutral events), selection, z-ordering, behavior registry, Batch/Refresh.
    Implements `IModelBatcher`; behavior registration lives in `NodelyDiagram` (OCP, not the base).
  - Models: `NodeModel`, `PortModel`, `PortAlignment`, `BaseLinkModel`/`LinkModel`, `GroupModel`,
    `LinkVertexModel`, `LinkLabelModel`, `LinkMarker` (neutral `PathData`), `ILinkable`.
  - Anchors: `Anchor`, `SinglePortAnchor`, `ShapeIntersectionAnchor`, `PositionAnchor`.
  - Options: `DiagramOptions` (+ Zoom/Links/Groups/Constraints/Virtualization), delegates.
  - Router/PathGenerator abstract bases + neutral `PathGeneratorResult`; `PathData` (SVG-free seam).
  - Concrete layers (`NodeLayer`/`LinkLayer`/`GroupLayer`), `ControlsLayer` plumbing, `Behavior` base,
    `DiagramExtensions.GetBounds`, `NodelyException`.
- Phase 2/4 cut (F-014): links attach + anchors resolve now; concrete routing/path-drawing deferred to
  Phase 4 (default router/generator null ⇒ path generation degrades gracefully). DynamicAnchor/LinkAnchor
  → Phase 7; default Controls widgets → Phase 8.
- netstandard2.0 fixes (F-015): replaced `^` index-from-end with explicit indices; conditional
  `System.Threading.Tasks.Extensions` for `ValueTask`.
- **Verified:** build 0 warn/0 err (net10.0 + netstandard2.0); `dotnet test` → **Core 51/51**, headless 1/1.
- **Next:** Phase 3 — behaviors (Selection, DragMovables, DragNewLink, Pan, Zoom, KeyboardShortcuts,
  Virtualization), driven by synthetic `Trigger*` input, registered in `NodelyDiagram`.

## 2026-06-02 — Phase 1 complete: Core geometry + model primitives (headless brain foundation)

- Cloned upstream `Blazor.Diagrams` (`develop`) to `D:\Projects\_refs\Blazor.Diagrams` and ported the
  **exact** source (not summaries) for faithfulness.
- `Nodely.Core` now contains (UI-agnostic, multi-targeting netstandard2.0;net10.0):
  - `Nodely.Geometry`: Point, Size, Rectangle, Line, Ellipse, BezierSpline, IShape, Shapes.
  - `Nodely.Models.Base`: Model, SelectableModel, MovableModel, IHasBounds, IHasShape (with
    Changed/VisibilityChanged/OrderChanged/Moved events).
  - `Nodely.Events`: framework-neutral PointerEvent, WheelEvent, KeyboardEvent, PointerButton.
  - `Layer<T>` + `IModelBatcher` (layer decoupled from Diagram for testability — F-012).
- Port deltas for netstandard2.0 recorded in F-011 (IsExternalInit polyfill, drop double.IsFinite /
  ArgumentNullException.ThrowIfNull / [JsonConstructor]).
- Removed the Phase-0 `NodelyCore` marker + its smoke test; added `InternalsVisibleTo(Nodely.Core.Tests)`.
- **Verified:** `dotnet build` 0 warn/0 err (incl. netstandard2.0); `dotnet test` → **Core 30/30**,
  Avalonia headless 1/1.
- **Next:** Phase 2 — Diagram engine (Diagram base + Trigger* seam, concrete models Node/Port/Link/Group,
  anchors, options, coordinate transforms), driven entirely from tests.

## 2026-06-02 — Phase 0 complete: solution scaffolded, builds & tests green

- Created `Nodely.slnx` with 7 projects (src: Core, Avalonia, Algorithms, Serialization; samples: Demo;
  tests: Core.Tests, Avalonia.Tests) under `src/ samples/ tests/`.
- Repo config: `Directory.Build.props` (nullable, implicit usings, analyzers, doc-gen), central package
  management (`Directory.Packages.props`), `.editorconfig`, `.gitignore`, `.gitattributes` (LF),
  `nuget.config` (nuget.org only), `global.json` (SDK 10.0.300 stable), MIT LICENSE,
  THIRD-PARTY-NOTICES, README, GitHub Actions CI.
- `Nodely.Avalonia` ships a Phase-0 placeholder `DiagramCanvas` (paints background); `Nodely.Demo` is a
  C#-only Avalonia desktop app hosting it; `Nodely.Core` has a `NodelyCore` marker.
- **Toolchain findings (verified, not guessed):** target **net10.0** (current LTS; machine has no .NET 8
  SDK) — F-007; **`.slnx`** is the new default — F-010; **xUnit v3** required by Avalonia.Headless.XUnit
  12.x (+ `OutputType=Exe`) — F-009; **Avalonia.Diagnostics** has no 12.x, dropped — F-008. ADR-0004 updated.
- **Verified:** `dotnet build` → 0 warnings / 0 errors; `dotnet test` → Core 2/2 + Avalonia headless 1/1.
- Committed as the initial Phase 0 commit.
- **Next:** Phase 1 — Core geometry & model primitives (port `Geometry/` + `Models/Base` with tests).

## 2026-06-02 — SVG-layer question → ADR-0003 link rendering revised

- User asked, before coding: is an SVG layer meaningful in Avalonia / do we need it?
- Source-verified upstream (`DiagramCanvas.razor`, `NodeRenderer.cs`, `LinkRenderer.cs`): two parallel
  layers — `<svg>` (links + opt-in SVG nodes) and HTML `<div>` (default nodes).
- Conclusion: **no SVG needed in Avalonia**; the SVG-specific reasons don't transfer and the canvas-cost
  reason is reversed (immediate-mode is native/cheap here). The HTML/SVG split collapses to one
  coordinate space.
- **Revised ADR-0003:** links default to a retained, lightweight, hit-testable, styleable `LinkView`
  per link (faithful SVG-`<path>` analogue, best for interactive custom links), virtualized; immediate
  batch mode deferred to Phase 13. Recorded as F-006; mapping docs updated.
- **Next:** still awaiting go-ahead for Phase 0 scaffolding.

## 2026-06-02 — Planning & research complete (pre-development)

- Loaded skills: `blazor-diagrams-master`, `avalonia-csharp-ui-senior`, `dotnet-vertical-slice-senior`.
- **Source-first research** against upstream `Blazor-Diagrams/Blazor.Diagrams` (branch `develop`) + nuget.org:
  - Confirmed `Blazor.Diagrams.Core` is UI-agnostic (only dep `SvgPathProperties`; own event types).
  - Mapped the `Diagram` public API + `Trigger*` input seam, Models, Behaviors, Geometry, Routers,
    PathGenerators, and the Blazor rendering layer (renderers, widgets, `wwwroot/script.js` JS interop).
  - Verified latest stable Avalonia = **12.0.4** (2026-05-28); latest `Z.Blazor.Diagrams` = 3.0.4.1.
  - Findings recorded in `02-research/`.
- **Decisions (ADRs 0001–0005):** name `Nodely`; first-party port of the Core; hybrid renderer
  (templated nodes + immediate-mode links); Avalonia 12.0.4 / .NET 8 / C#-only UI / MVVM-agnostic core;
  DataTemplate-based customization.
- **Wrote the development plan** (`03-plan/development-plan.md`) — 15 phases (0–14), 4 milestones, with a
  tracer-bullet integration checkpoint after Phase 2.
- Working dir confirmed empty (greenfield); not yet a git repo.
- **Next:** await go-ahead, then execute **Phase 0 — Foundation & scaffolding**.

### Open items carried forward
- Re-verify exact Avalonia 12.0.4 APIs at implementation time: `StreamGeometryContext`, custom `Panel`
  size capture, pan/zoom transform approach, `FuncDataTemplate` resolution, `ControlTheme`, pointer
  capture, `EffectiveViewportChanged` (tracked in `avalonia-mapping-notes.md`).
- Decide group-delete policy default (delete children vs ungroup) during Phase 8.
- Confirm `netstandard2.0` is comfortable for all Core code (e.g. nullable, spans) during Phase 1.
