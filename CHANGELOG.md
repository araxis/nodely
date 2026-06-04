# Changelog

All notable changes to Nodely are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/), and the project aims to follow
[Semantic Versioning](https://semver.org/).

## [Nodely.Avalonia.Uml 0.1.0] - 2026-06-04

First UML side-package release.

### Added
- **UML package:** `Nodely.Avalonia.Uml` adds class, interface, enum, package, and note nodes.
- **UML relationships:** association, inheritance, realization, dependency, aggregation, and composition links
  include label and multiplicity metadata.
- **Renderer registration:** `DiagramCanvas.UseUmlNodes()` registers UML node renderers, link styling, and UML
  relationship markers in one call.
- **Serialization registration:** `DiagramSerializationRegistry.UseUmlNodes()` restores UML nodes and links with
  stable model-kind keys.
- **Package workflow:** side-package selection now uses a package map so future side packages can be added
  without duplicating tag/dispatch branch logic.
- **Demo and docs:** the desktop gallery and documentation site include a structural UML scene and guide.

## [0.7.0] - 2026-06-04

Database pack and extension-contract release.

### Added
- **Database package:** `Nodely.Avalonia.Database` adds table, view, and procedure nodes, database ports, and
  relationship/dependency links.
- **Renderer registration:** `DiagramCanvas.UseDatabaseNodes()` registers database node, port, and link styling
  in one call.
- **Serialization registry:** `DiagramSerializationRegistry` restores custom node, port, link, and group kinds
  with stable model-kind keys and model-wide extra-data hooks.
- **Typed link styling:** `DiagramCanvas.RegisterLinkStyle<TLink>()` replaces the single global style resolver
  with composable type-based registrations.
- **Render context:** node, port, and group factories can receive a canvas context for palette-aware rendering.
- **Independent side-package versioning:** `Nodely.Avalonia.Database` now has its own package version line and
  tag-triggered publish path, starting at `0.1.0` while depending on the main `0.7.0` packages.
- **Demo scene:** the gallery now includes a database diagram with tables, a view, a stored procedure,
  relationship links, dependency links, save/load, theme switching, and zoom-to-fit.
- **Docs:** README and the documentation site now include the database package and usage guide.

## [0.6.0] - 2026-06-04

Compatibility release. No C# API changes.

### Changed
- **Target framework assets:** `Nodely.Avalonia` now ships `net8.0` and `net10.0` assets.
- **Engine package assets:** `Nodely.Core`, `Nodely.Algorithms`, and `Nodely.Serialization` now ship
  `netstandard2.0`, `net8.0`, and `net10.0` assets.
- **QuickStart runtime:** `samples/Nodely.QuickStart` now targets `net8.0` so the minimal copyable app matches
  the lowest supported Avalonia runtime.
- **Build validation:** CI and package workflows install both .NET 8 and .NET 10 before build, test, and pack.
- **Docs:** README and documentation now include the package target matrix and compatibility release checks.

## [0.5.0] - 2026-06-04

Adoption polish release. No breaking API changes.

### Added
- **Command-state helpers:** `DiagramCanvas` now exposes `HasSelection`, selection/clipboard/grouping
  availability properties, and `CommandStateChanged` so toolbars can stay in sync without inspecting the
  diagram graph.
- **QuickStart sample:** a tiny C#-only Avalonia app with one canvas, custom nodes, ports, links, a theme
  toggle, and zoom-to-fit.
- **Recipes docs:** copyable recipes for minimal setup, command-state toolbars, custom overlays, and
  save/load custom nodes.

### Changed
- **Demo gallery:** the desktop gallery now has workflow, state machine, read-only inspector, and extensibility
  scenes, plus a command-aware toolbar using the public canvas state helpers.
- **Sample coverage:** the solution now builds both sample apps as part of normal build validation.

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
  nodes.
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

[0.7.0]: https://github.com/araxis/nodely/releases/tag/v0.7.0
[0.6.0]: https://github.com/araxis/nodely/releases/tag/v0.6.0
[0.5.0]: https://github.com/araxis/nodely/releases/tag/v0.5.0
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
