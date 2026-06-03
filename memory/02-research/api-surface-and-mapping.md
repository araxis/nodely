# Research — Blazor.Diagrams → Nodely / Avalonia mapping tables

How each upstream concept maps to Nodely. "Core" = `Nodely.Core` (ported, UI-agnostic).
"Avalonia" = `Nodely.Avalonia` (native rendering/input). Verify exact Avalonia 12.0.4 signatures at
implementation time (source-first); names below are the design intent.

## Engine & model layer (Core — port)

| Blazor.Diagrams.Core | Nodely.Core | Notes |
|---|---|---|
| `Diagram` (abstract) | `Diagram` (abstract) | Same `Trigger*` seam, layers, view state, behavior registry. |
| `BlazorDiagram` (concrete) | `NodelyDiagram` (concrete, in Core or Avalonia) | Concrete options + default behaviors registered. |
| `NodeModel` / `PortModel` / `LinkModel` / `GroupModel` | same names | `Models/`. |
| `LinkLabelModel` / `LinkVertexModel` / `LinkMarker` / `PortAlignment` | same | |
| `Models/Base` (Model, SelectableModel, MovableModel) | same | `Changed` notification base. |
| `Anchors/*` (Single, Dynamic, ShapeIntersection, Link, Position) | same | endpoint attach strategies. |
| `Layers/*` | `NodesLayer/LinksLayer/GroupsLayer/ControlsLayer` | observable ordered collections. |
| `Options/*` | `DiagramOptions` (+ nested Links/Zoom/Grid/Constraints) | |
| `Events/*EventArgs` | neutral structs `PointerEvent/WheelEvent/KeyEvent` | populated by Avalonia layer. |
| `MouseEventButton` | `PointerButton` enum | |

## Behaviors (Core — port; driven by `Trigger*`)

| Upstream | Nodely | Maps Avalonia input via |
|---|---|---|
| `SelectionBehavior` | `SelectionBehavior` | pointer down/up + box selection |
| `DragMovablesBehavior` | `DragMovablesBehavior` | pointer down/move/up with capture |
| `DragNewLinkBehavior` | `DragNewLinkBehavior` | pointer drag from a port |
| `PanBehavior` | `PanBehavior` | pointer drag on empty canvas / middle button |
| `ZoomBehavior` | `ZoomBehavior` | `PointerWheelChanged` |
| `KeyboardShortcutsBehavior` (+Defaults) | same | `KeyDown` (Delete, Ctrl+A, etc.) |
| `VirtualizationBehavior` | `VirtualizationBehavior` | cull models outside `Container` |
| `EventsBehavior` / `DebugEventsBehavior` | same | wiring/diagnostics |

## Routing & paths (Core — port, with neutral output)

| Upstream | Nodely | Divergence |
|---|---|---|
| `Router`/`NormalRouter`/`OrthogonalRouter` | `IRouter`/`NormalRouter`/`OrthogonalRouter` | none functional |
| `PathGenerator`/`Straight`/`Smooth` | `IPathGenerator`/`Straight`/`Smooth` | emit `PathData` (commands), not SVG `d` |
| `PathGeneratorResult` | `PathData` (+ marker/label anchor points) | consumed by Avalonia `StreamGeometry` |
| `SvgPathProperties` (dep) | own bezier/line sampling | drop the dependency |

## Rendering layer (Avalonia — re-implement)

| Blazor component | Nodely.Avalonia | Technique |
|---|---|---|
| `DiagramCanvas.razor` | `DiagramCanvas : TemplatedControl` | hosts layer panels; one pan/zoom transform; input translation |
| `NodeRenderer` / `NodeWidget` / `SvgNodeWidget` | `NodesLayer : Panel` + `NodeView` | virtualizing panel; arranges node host controls by `Bounds`; DataTemplate per model type |
| `PortRenderer` | `PortView` (templated) | small templated controls positioned by alignment |
| `LinkRenderer` / `LinkWidget` (SVG `<path>`) | `LinkView : Control` (retained, per link) | draws cached `StreamGeometry` from `PathData`; hit-test + styling for free; virtualized. Immediate-mode batch = Phase-13 scale option. See F-006. |
| `LinkLabelRenderer` / `DefaultLinkLabelWidget` | `LinkLabelView` (overlay control) | positioned along route |
| `LinkVertexRenderer` | `LinkVertexThumb` (overlay control) | draggable bend points |
| `GroupRenderer` / `DefaultGroupWidget` / `GroupNodes` | `GroupView` | auto-bounds container; moves children |
| `Controls/*` (adornments) | `ControlsLayer` + adorner controls | resize handles, delete button |
| `GridWidget` | `GridWidget` (immediate-mode) | draws grid in `Render` |
| `NavigatorWidget` | `NavigatorWidget` (immediate-mode) | minimap: scaled node draw + viewport rect + click-to-pan |
| `SelectionBoxWidget` | `SelectionBoxWidget` (immediate-mode) | marquee rectangle |
| `wwwroot/script.js` (ResizeObserver/getBoundingClientRect) | **none** | Avalonia layout: node `Bounds` -> `NodeModel.Size`; canvas `Bounds` -> `SetContainer` |
| `style.min.css` / `default.styles.min.css` | `NodelyTheme : Styles` (C#) | `ControlTheme`s + state classes |

## Input event mapping (Avalonia → Nodely-neutral → `Trigger*`)

| Avalonia event | Nodely call |
|---|---|
| `PointerPressed` | `TriggerPointerDown(model, PointerEvent)` (+ `e.Pointer.Capture`) |
| `PointerMoved` | `TriggerPointerMove(model, PointerEvent)` |
| `PointerReleased` | `TriggerPointerUp(model, PointerEvent)` |
| `PointerEntered`/`Exited` | `TriggerPointerEnter/Leave` |
| `Tapped`/`DoubleTapped` | `TriggerPointerClick/DoubleClick` |
| `PointerWheelChanged` | `TriggerWheel(WheelEvent)` |
| `KeyDown` | `TriggerKeyDown(KeyEvent)` |

`model` is resolved by Avalonia hit-testing (which node/port/link control was hit), or `null` for the
empty canvas — replacing Blazor's per-element `@onpointerdown` wiring.

## Optional packages

| Upstream | Nodely |
|---|---|
| `Blazor.Diagrams.Algorithms` | `Nodely.Algorithms` (auto-layout, traversal, components) |
| (snapshot/serialization patterns) | `Nodely.Serialization` (versioned `DiagramSnapshot` + System.Text.Json) |
