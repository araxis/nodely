# Changelog

All notable changes to Nodely are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/), and the project aims to follow
[Semantic Versioning](https://semver.org/).

## [0.4.0] - 2026-06-04

Hardening and editor polish release. No breaking API changes.

### Added
- **Undoable editor polish:** z-order changes, group/ungroup operations, and link bend-point add/remove now
  route through the canvas history where the canvas exposes those actions.
- **Canvas grouping helpers:** `GroupSelection`, `UngroupSelection`, and `ToggleGroupingSelection` give
  toolbar/menu authors direct entry points for grouping workflows.
- **Release checklist docs:** the documentation site now includes a release checklist for version bumps,
  validation, tagging, package publishing, and Pages verification.

### Changed
- **Refresh correctness:** editing a link label now refreshes its parent link immediately, and link-to-link
  anchors refresh dependent links when the target link reroutes.
- **Build hardening:** warnings are treated as errors across the solution.

### Fixed
- Deleting a selected bend point through the canvas is now undoable.
- Link factory failures now report unsupported source models with an argument exception instead of an
  implementation placeholder.

## [0.3.0] - 2026-06-03

Extensibility wave: a lean set of seams so you can build features yourself, rather than growing the library.
No breaking API changes.

### Added
- **Render hooks:** `RegisterLink<T>` (custom immediate-mode link drawer via `LinkRenderContext` +
  `DrawDefault()`), `RegisterPort<T>`, and `RegisterGroup<T>` — links/ports/groups are now as customizable as
  nodes. Plus `LinkStyleResolver` for quick stroke/width/dash overrides.
- **Custom layers:** `DiagramCanvas.AddLayer` / `RemoveLayer` + a `DiagramLayer` base — add any overlay (rulers,
  guides, heatmaps, annotations) in world or screen space.
- **Selection adorners:** `RegisterAdorner` for per-node toolbars/badges/handles.
- **Custom interactions:** `RegisterBehavior` / `GetBehavior` / `UnregisterBehavior` on the canvas, plus
  interaction-rule delegates `DiagramLinkOptions.CanConnect`, `DiagramOptions.CanDrag`, and
  `DiagramOptions.SnapPosition`.
- **Pluggable layout:** `IDiagramLayout` (+ `LayeredDiagramLayout`).
- **Model data:** `Model.Tag` and a lazy `Model.Data` bag — attach data without subclassing.
- **Custom-field persistence:** `NodeModel.GetExtraData` / `SetExtraData` carry custom node fields through
  save/load (snapshots gained an `Extra` map).
- Documentation site gained an **Extensibility** guide (the seam map).

[0.4.0]: https://github.com/araxis/nodely/releases/tag/v0.4.0
[0.3.0]: https://github.com/araxis/nodely/releases/tag/v0.3.0

## [0.2.0] - 2026-06-03

Editor and interaction wave on top of 0.1.0. No breaking API changes.

### Added
- **Link visuals & editing:** arrowheads/markers (`Default{Source,Target}Marker` option; arrow, square, and
  circle shapes), link **selection** (click to select, Delete to remove — backend-independent
  `PathData.DistanceTo` hit-testing), link **labels** along the path, and draggable **vertices/bend points**
  (double-click a link to add, a vertex to remove).
- **Undo/redo:** `DiagramHistory` records adds, drags, link-adds, and deletes; **Ctrl+Z / Ctrl+Y**; gesture
  **transactions** (a multi-node drag or auto-layout is one undo step); discarded drag-new-links are cancelled.
- **Clipboard & duplication:** **Ctrl+C/X/V/D** and a right-click **context menu** (delete, duplicate,
  bring-to-front, send-to-back, select-all, zoom-to-fit). `NodeModel.Clone()` is the override seam for copying
  custom nodes.
- **Selection & order:** **Ctrl+A** select-all; visible z-order (`SendToFront`/`SendToBack` bound to `ZIndex`).
- **Anchors:** `DynamicAnchor` (snaps to the nearest candidate facing the other end) and `LinkAnchor`
  (link-to-link, at the target link's midpoint).
- **Auto-layout:** cycle-aware Sugiyama-style `LayeredLayout` (cycle-breaking → layering → barycenter crossing
  reduction → size-aware, spine-centered placement) — state machines lay out instead of collapsing.
- **Demo:** Save/Load (JSON round-trip), Undo/Redo buttons, one-undo Layout, labeled state machine, a seeded
  bend point, and a circle source marker.

### Fixed
- Selected-node delete/resize adorners now track the node during a live drag (no longer freeze at the
  pre-drag position).

[0.2.0]: https://github.com/araxis/nodely/releases/tag/v0.2.0

## [0.1.0] - 2026-06-03

First release. A native Avalonia diagram/graph editor toolkit — a first-party port of the
Blazor.Diagrams architecture, with **no SVG, no JS, no WebView**.

### Core engine (`Nodely.Core`, UI-agnostic, netstandard2.0 + net10.0)
- Geometry primitives (Point, Size, Rectangle, Line, Ellipse, BezierSpline, Shapes).
- Models: nodes, ports, links, groups, labels, vertices, markers; anchors
  (single-port, shape-intersection, position).
- Diagram engine: layers, pan/zoom/container, coordinate transforms, selection, z-ordering,
  the `Trigger*` input seam, batching, and a behavior registry.
- Behaviors: selection, drag-move, drag-new-link, pan, zoom, keyboard shortcuts, virtualization,
  click/double-click synthesis.
- Routers (Normal, Orthogonal A*) and path generators (Straight, Smooth) emitting a neutral
  `PathData` (no SVG strings).
- `Nodely.Commands`: undo/redo stack + add/remove/move-node and add/remove-link commands.

### Avalonia UI (`Nodely.Avalonia`, net10.0)
- `DiagramCanvas` with grid, nodes, ports, links, groups, adorners, and a marquee overlay.
- Custom nodes via `RegisterNode<T>` + Avalonia controls; layout-driven node sizing.
- Interactive link creation by dragging from a port; group/marquee selection; delete + resize adorners.
- `DiagramNavigator` overview minimap; zoom-to-fit / zoom in-out / reset.
- Light/dark `NodelyPalette` theming; read-only inspector mode; basic keyboard accessibility.

### Optional packages
- `Nodely.Serialization`: versioned JSON snapshots (round-trips byte-identically).
- `Nodely.Algorithms`: graph traversal/components and a layered auto-layout.

### Performance
- 2000 nodes / ~4000 links: full re-route + smooth-bezier path generation ~15 ms; layered layout
  ~31 ms; serialize/deserialize ~61 ms (1.26 MB JSON) on a desktop dev machine.

[0.1.0]: https://github.com/araxis/nodely/releases/tag/v0.1.0
