# Phase checklist

Tick items as completed. Mirrors `development-plan.md`. `[ ]` todo, `[~]` in progress, `[x]` done.

## Post-M4 — side package roadmap
- [x] Add `Nodely.Avalonia.Uml` package targeting `net8.0;net10.0`.
- [x] Add structural UML nodes and relationship links.
- [x] Add `UseUmlNodes()` renderer registration, serializer registration, and `UmlNodeFactory`.
- [x] Generalize side-package package workflow selection with a package map.
- [x] Add UML demo scene, documentation guide, and focused tests.
- [x] Add `Nodely.Avalonia.Workflow` as the next independent side package.
- [x] Add Workflow start, end, task, decision, gateway, event, note nodes, and workflow links.
- [x] Add `UseWorkflowNodes()` renderer registration, serializer registration, and `WorkflowNodeFactory`.
- [x] Add Workflow demo scene, documentation guide, and focused tests.
- [x] Upgrade Database and UML pack visuals with domain-specific ports, row-aware endpoints, and richer demo scenes.
- [x] Add `Nodely.Avalonia.MindMap` as the third independent side package.
- [x] Add `Nodely.Avalonia.StateMachine` as the next independent side package.
- [x] Add StateMachine initial, state, final, choice, note nodes, transition ports, transition links, and self-loop rendering.
- [x] Add `UseStateMachineNodes()` renderer registration, serializer registration, and `StateMachineNodeFactory`.
- [x] Add StateMachine demo scene, documentation guide, runtime inspector fields, and focused tests.
- [x] Add `Nodely.Avalonia.Network` as the next independent side package.
- [x] Add Network device nodes, typed ports, topology links, status metadata, link glyphs, and arrange helper.
- [x] Add `UseNetworkNodes()` renderer registration, serializer registration, and `NetworkNodeFactory`.
- [x] Add Network demo scene, documentation guide, runtime inspector fields, and focused tests.

## Post-M4 — v0.7.0 database pack
- [x] Add `Nodely.Avalonia.Database` package targeting `net8.0;net10.0`.
- [x] Add database nodes, columns, parameters, ports, and relationship links.
- [x] Add `UseDatabaseNodes()` renderer registration and `DatabaseNodeFactory`.
- [x] Add database demo scene and documentation guide.
- [x] Final validation: build, test, pack, package inspection, docs build, wording scan.

## Post-M4 — extension surface redesign
- [x] Audit side-package extension and versioning surface before public database-pack merge.
- [x] Add stable model-kind and model-wide extra-data hooks.
- [x] Add serializer registry and version 2 snapshots for nodes, ports, links, and groups.
- [x] Replace global link-style resolver with typed composable link-style registrations.
- [x] Add render-context factory overloads for palette-aware pack renderers.
- [x] Split main and side-package versions plus selective package workflow.
- [x] Revise database pack to use the finalized side-pack contract.

## Post-M4 — v0.6.0 compatibility pass
- [x] Add `net8.0` package assets for `Nodely.Avalonia`.
- [x] Add explicit `net8.0` assets for Core, Algorithms, and Serialization while retaining `netstandard2.0`.
- [x] Retarget QuickStart to `net8.0` and multi-target test projects to `net8.0;net10.0`.
- [x] Update CI/package workflows to install both .NET 8 and .NET 10.
- [x] Final validation: build, test, pack, package inspection, docs build, wording scan.

## Phase 0 — Foundation & scaffolding ✅ (2026-06-02)
- [x] `Nodely.slnx` + all 7 projects created (ADR-0001 layout; note: `.slnx`, not `.sln` — F-010)
- [x] `Directory.Build.props` / `Directory.Packages.props` (Avalonia 12.0.4 pinned) / `.editorconfig` / `.gitignore` / `.gitattributes` / `nuget.config` / `global.json` (SDK 10.0.300)
- [x] `git init`, MIT LICENSE, THIRD-PARTY-NOTICES, README
- [x] GitHub Actions CI workflow (build + test) — added; not yet run on a remote (no remote pushed)
- [x] Demo app builds + hosts placeholder `DiagramCanvas`; control verified by headless test (GUI launch = manual `dotnet run`)
- [x] DoD: `dotnet build` 0 warn/0 err; `dotnet test` green (Core 2/2, Avalonia headless 1/1) locally
- Deviations recorded: net10.0 not net8.0 (F-007); Avalonia.Diagnostics dropped (F-008); xUnit v3 (F-009)

## Phase 1 — Geometry & model primitives ✅ (2026-06-02)
- [x] Geometry types + tests (Point/Size/Rectangle/Line/Ellipse/BezierSpline/IShape/Shapes) — `Nodely.Geometry`
- [x] Base models (Model/SelectableModel/MovableModel + IHasBounds/IHasShape) + `Changed`/`OrderChanged`/`Moved` — `Nodely.Models.Base`
- [x] `Layer<T>` + events + tests (decoupled from Diagram via `IModelBatcher` — F-012)
- [x] Neutral input types (PointerEvent/WheelEvent/KeyboardEvent/PointerButton) — `Nodely.Events`
- [x] DoD: `dotnet build` 0/0 (incl. netstandard2.0); `dotnet test` green — **Core 30/30**, Avalonia 1/1
- Notes: netstandard2.0 polyfills (F-011); upstream ref cloned at `D:\Projects\_refs` (F-013)

## Phase 2 — Diagram engine (no rendering) ✅ (2026-06-02)
- [x] `Diagram` base (layers/view/Trigger*/selection/ordering/batch/behavior registry) + `IModelBatcher`
- [x] Models (Node/Port/Link/BaseLink/Group/Label/Vertex/Marker/Alignment + ILinkable)
- [x] Anchors: Anchor/SinglePort/ShapeIntersection/Position (Dynamic+Link deferred → Phase 7)
- [x] `DiagramOptions` (+ Zoom/Links/Groups/Constraints/Virtualization) + concrete `NodelyDiagram`
- [x] Router/PathGenerator abstract bases + neutral `PathData`/`PathGeneratorResult` (SVG-free seam)
- [x] Controls plumbing (Control/ControlsType/ControlsContainer/ControlsLayer); widgets → Phase 8
- [x] Coordinate transforms + ZoomToFit + ordering
- [x] DoD: build 0/0 (net10.0 + netstandard2.0); tests green — **Core 51/51** (incl. 21 engine/link/group)
- Notes: links attach + anchors resolve now; concrete routing/paths → Phase 4 (F-014); ns2.0 fixes (F-015)

## Tracer bullet (integration checkpoint)
- [ ] Minimal canvas hosts one node + pan via Trigger*
- [ ] Learnings recorded in findings

## Phase 3 — Behaviors ✅ (2026-06-02)
- [x] Selection / DragMovables / DragNewLink / Pan / Zoom
- [x] KeyboardShortcuts (+Defaults) / Virtualization / Events / DebugEvents (+ `KeysUtils`)
- [x] Registered (in order) in `NodelyDiagram`; `registerDefaultBehaviors` flag
- [x] DoD: synthetic-input behavior tests green — **Core 60/60** (9 new: select/pan/zoom/drag-move/drag-link/delete/click/virtualize)
- Note: marquee box-select is a Phase 9 widget, not a behavior (F-016); ns2.0 fixes (Math.Clamp/KVP/ValueTask)

## Phase 4 — Routers & path generators (M1) ✅ (2026-06-02)
- [x] `Router` base + `NormalRouter` + `OrthogonalRouter` (A* over a sparse grid)
- [x] `PathGenerator` base + `StraightPathGenerator` + `SmoothPathGenerator` → neutral `PathData`
- [x] Dropped `SvgPathProperties` entirely (own bezier/line via `PathData` + `BezierSpline`)
- [x] Wired `DiagramLinkOptions` defaults (Normal + Smooth); links now produce `Route`/`PathGeneratorResult`
- [x] netstandard2.0: `PriorityQueue` polyfill, KVP access, explicit indices (F-017)
- [x] DoD: routing tests green — **Core 66/66** (Normal/Orthogonal routes, Straight/Smooth path ops, markers)
- ✅ **MILESTONE M1 — headless brain complete** (UI-agnostic engine done & tested on both TFMs)

## Phase 5 — Canvas shell: pan/zoom + grid ✅ (2026-06-02)
- [x] `DiagramCanvas : Panel` (viewport) hosting an immediate-mode `GridLayer : Control` (pan/zoom-aware)
- [x] Avalonia input → neutral events → `Trigger*` (pointer capture; `PointerUpdateKind`→`PointerButton`)
- [x] `SetContainer` from `ArrangeOverride` (Left/Top = 0; positions already canvas-relative — F-018)
- [x] Demo hosts a real `NodelyDiagram` (drag empty space to pan, wheel to zoom)
- [x] DoD: headless tests green — **Avalonia 4/4** (container-from-layout, drag-pans, wheel-zooms) + Core 66/66
- Note: `Panel.Render` is sealed → grid lives in a child layer (F-018); demo window is a manual `dotnet run`

## Phase 6 — Node rendering & templating ✅ (2026-06-02)
- [x] `NodesLayer : Panel` + `NodeView : Decorator` under a pan/zoom `RenderTransform` (culling → Phase 13)
- [x] `RegisterNode<T>(Func<T,Control>)` type-keyed registry + built-in default template
- [x] Measured size → `NodeModel.Size` (size feedback in `MeasureOverride`; replaces ResizeObserver)
- [x] Selection outline (`NodeView.Render`) + drag-move; capture-safe node hit-resolution via `GetVisualsAt`
- [x] DoD: custom node in ~5-10 lines (demo `TaskNode`); tests green — **Avalonia 8/8** (size/template/click/drag)
- Note: F-019; render virtualization + DataTemplate integration deferred

## Phase 7 — Ports & links + interactive creation (M2) ✅ (2026-06-02)
- [x] `PortsLayer` + `PortView` (hit-testable dots; positions/sizes/Initialized computed from node bounds)
- [x] `LinksLayer` immediate-mode (`StreamGeometry` from `PathData` via `PathDataGeometry`)
- [x] DragNewLink wiring (port hit-resolution via `GetVisualsAt`, capture-safe) — drop-on-port attaches
- [x] DoD: drag-to-connect + tests green — **Avalonia 11/11** (ports init, link path, drag-creates-link)
- ✅ **MILESTONE M2 — interactive editor MVP** (nodes + ports + links + drag-to-connect, on screen)
- [~] Labels / vertices / markers / link-selection / geometry caching → deferred (F-020); links are
  immediate-mode (ADR-0003 updated)

## Phase 8 — Groups, selection box, controls layer ✅ (2026-06-03)
- [x] `GroupsLayer`/`GroupView` (auto-bounds container; draggable→moves children; selectable; resolvable)
- [x] `SelectionBoxLayer` marquee (Shift-drag; screen-space draw, diagram-coord selection of nodes/groups)
- [x] `AdornersLayer` (screen-space ✕ delete button per selected node; `e.Handled` guard)
- [x] DoD: group/marquee/adorner tests green — **Avalonia 14/14** (group sizes, marquee selects, delete removes)
- [~] Node **resize handles** deferred → Phase 10 (needs settable `ControlledSize`); F-021

## Phase 9 — Widgets: navigator/snap/zoom controls ✅ (2026-06-03)
- [x] `DiagramNavigator` minimap (nodes + viewport rect; click/drag to pan) — public, reusable
- [x] Snap-to-grid (engine `Options.GridSize` via `DragMovablesBehavior`; demo sets 24)
- [x] Zoom-to-fit + zoom in/out/reset on `DiagramCanvas` (zoom-about-center) + demo toolbar
- [x] DoD: navigator/snap/zoom tests green — Core 67/67 + **Avalonia 16/16** (snap, zoom-to-fit, nav pan)
- Note: visual grid (`DiagramCanvas.GridSize`) and snap grid (`Options.GridSize`) are separate knobs (F-022)

## Phase 10 — Theming, accessibility, read-only (M3) ✅ (2026-06-03)
- [x] `NodelyPalette` (light/dark) via `DiagramCanvas.Palette`; wired through all built-in visuals
- [x] Read-only mode blocks move/connect/delete (unregister mutating behaviors) — pan/zoom/select stay
- [x] Node **resize handles** (settable `ControlledSize`) — completes the Phase-8 deferral
- [x] Keyboard a11y: Escape clears selection, arrow-key nudge, `DiagramCanvasAutomationPeer`
- [x] DoD: theming/read-only/resize/keyboard tests green — Core 67/67 + **Avalonia 20/20**
- ✅ **MILESTONE M3 — production-grade UX** (themeable, read-only inspector, resize, accessible)

## Phase 11 — Serialization + commands/undo-redo ✅ (2026-06-03)
- [x] Versioned `DiagramSnapshot` DTOs + `DiagramSerializer` (System.Text.Json); `Version` + node `Kind`/factory hook
- [x] `UndoRedoStack` + commands (Add/Remove/Move node, Add/Remove link) in `Nodely.Commands`
- [x] DoD: round-trip (byte-identical JSON) + undo/redo tests green — **Core 72/72**
- Notes: F-024 (JSON-string round-trip compare; ns2.0 STJ package + polyfill; `GroupModel` id ctor); UI
  save/load + undo wiring is a Phase-14 polish

## Phase 12 — Algorithms (optional) ✅ (2026-06-03)
- [x] `DiagramGraph`: edges, BFS/DFS, connected components (directed/undirected adjacency)
- [x] `LayeredLayout` auto-layout (longest-path layering via Kahn; horizontal/vertical)
- [x] DoD: algorithm tests green — Core 76/76 (components, BFS, layered chain + sibling separation)

## Phase 13 — Performance pass ✅ (2026-06-03)
- [x] Link `StreamGeometry` caching (rebuild only on link change); node-visibility virtualization in `NodesLayer`
- [x] `bench/Nodely.Benchmarks` Stopwatch harness (engine hot paths + serialization)
- [x] DoD: documented numbers — 2000 nodes/~4000 links: re-route+paths ~15 ms, layout ~31 ms, ser/deser ~61 ms (F-025)

## Phase 14 — Docs, gallery, packaging (M4) ✅ (2026-06-03)
- [x] Gallery: 3-scene switcher (Workflow, State machine + auto-layout, read-only Inspector) + theme toggle
- [x] XML docs throughout; README getting-started + customization; CHANGELOG (0.1.0)
- [x] `dotnet pack -c Release` → 4 packages + 4 symbol packages (snupkg); README packed per-package
- [x] DoD: packages build; tag `v0.1.0` — ✅ **MILESTONE M4 — RELEASE v0.1.0**
- Note: SourceLink/repo URL are placeholders until a git remote exists (F-026)
