# Glossary

Shared vocabulary so code, docs, and conversations line up. Terms mirror Blazor.Diagrams where
sensible; divergences are flagged.

- **Diagram** — the root engine object. Owns layers (Nodes, Links, Groups, Controls), view state
  (Pan, Zoom, Container), the behavior registry, selection, and the input seam (`Trigger*`). Lives in
  `Nodely.Core`, UI-agnostic. The Avalonia layer binds a `DiagramCanvas` control to a `Diagram`.
- **Layer** — an ordered, observable collection of one model kind (e.g. `NodesLayer`, `LinksLayer`).
  Raises Added/Removed/order-changed so the renderer can react.
- **Model** — base of everything with an `Id` and a `Changed` notification. `SelectableModel` adds
  selection; `MovableModel` adds position.
- **Node (`NodeModel`)** — a positioned, sized box on the canvas. Has Ports. Rendered by a templated
  Avalonia control resolved from the node's runtime type.
- **Port (`PortModel`)** — a typed connection point on a node (input/output, top/right/bottom/left,
  semantic ids). Links attach to ports. Has an `Alignment`.
- **Anchor** — strategy for *where on a node/port/link a link endpoint attaches*: `SinglePortAnchor`,
  `DynamicAnchor`, `ShapeIntersectionAnchor`, `LinkAnchor`, `PositionAnchor`. Decouples "what a link
  connects to" from "the exact point it touches."
- **Link (`LinkModel`)** — a connection between two endpoints (ports, nodes, or free points). Has a
  Router, a PathGenerator, Vertices, Labels, and Markers.
- **Vertex (`LinkVertexModel`)** — a user-draggable bend point on a link.
- **Label (`LinkLabelModel`)** — text/content placed along a link at a relative position.
- **Marker (`LinkMarker`)** — an arrowhead/decoration at a link end (e.g. `Arrow`, `Circle`).
- **Group (`GroupModel`)** — a container node whose bounds auto-fit its children; moving it moves children.
- **Router** — computes the *route* (the ordered set of waypoints) a link takes between endpoints:
  `NormalRouter` (direct + vertices), `OrthogonalRouter` (right-angle).
- **PathGenerator** — turns a route into a drawable *path*: `StraightPathGenerator`,
  `SmoothPathGenerator` (bezier). **Nodely divergence:** generators emit a neutral `PathData`
  (move/line/cubic/quad commands), not an SVG `d` string, so Avalonia can build a `StreamGeometry`
  directly. See `02-research/avalonia-mapping-notes.md`.
- **Behavior** — an interaction state machine driven by the diagram's input events. Examples:
  `SelectionBehavior`, `DragMovablesBehavior`, `DragNewLinkBehavior`, `PanBehavior`, `ZoomBehavior`,
  `KeyboardShortcutsBehavior`, `VirtualizationBehavior`. Registered explicitly (no reflection scanning).
- **Widget** — an optional on-canvas overlay: `GridWidget`, `NavigatorWidget` (overview/minimap),
  `SelectionBoxWidget`. In Nodely most widgets are immediate-mode rendered.
- **Control (Controls layer)** — small interactive adornments attached to a selected model
  (resize handles, delete button). Distinct from Avalonia "controls"; capitalized "Controls layer"
  means this concept.
- **Container** — the diagram's viewport rectangle in screen space. In Blazor it came from
  `getBoundingClientRect`; in Nodely it comes from Avalonia layout (`Bounds`). Feeds coordinate math.
- **Pan / Zoom** — viewport translation/scale. Applied as a single transform over the canvas content.
- **`Trigger*` (input seam)** — `TriggerPointerDown/Move/Up/Enter/Leave/Click/DoubleClick`,
  `TriggerWheel`, `TriggerKeyDown`. The Avalonia layer translates Avalonia pointer/key/wheel events
  into Nodely-neutral event args and calls these.
- **Tracer bullet** — a thin end-to-end slice (minimal Core + canvas + one node + pan) built early to
  de-risk the whole stack before deepening each layer. See `03-plan/development-plan.md`.
- **DataTemplate registration** — the Avalonia-native equivalent of Blazor's `RegisterComponent`:
  map a node/link model type to an Avalonia `DataTemplate`/`FuncDataTemplate` to customize rendering.
