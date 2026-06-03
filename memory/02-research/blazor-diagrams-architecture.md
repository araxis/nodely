# Research — Blazor.Diagrams architecture (source-first map)

Verified 2026-06-02 against the upstream repo `Blazor-Diagrams/Blazor.Diagrams` (branch `develop`) via
the GitHub contents API and raw file fetches, and nuget.org. Latest published `Z.Blazor.Diagrams` =
**3.0.4.1** (2026-03-02). This is the knowledge Nodely is ported from.

## Two layers, cleanly separated

```
src/
  Blazor.Diagrams.Core/        <- UI-AGNOSTIC "brain"  (NuGet: Z.Blazor.Diagrams.Core)
  Blazor.Diagrams/             <- Blazor RENDERING layer (NuGet: Z.Blazor.Diagrams)
  Blazor.Diagrams.Algorithms/  <- optional graph algorithms
  Directory.Build.props
```

### `Blazor.Diagrams.Core` — the brain (this is ~all of Nodely.Core)

- **Dependencies:** only `SvgPathProperties`. **No `Microsoft.AspNetCore.Components` / Blazor.**
  (Verified by reading `Blazor.Diagrams.Core.csproj`.)
- **Subfolders:** `Anchors`, `Behaviors`, `Controls`, `Events`, `Extensions`, `Geometry`, `Layers`,
  `Models`, `Options`, `PathGenerators`, `Positions`, `Routers`, `Utils`.
- **Top-level files:** `Diagram.cs` (abstract engine), `Behavior.cs` (base), `Delegates.cs`,
  `DiagramsException.cs`, `MouseEventButton.cs`.

#### `Diagram.cs` public surface (the contract Nodely.Core must reproduce)

- **Events** (`Action<...>`): `PointerDown/Move/Up/Enter/Leave`, `PointerClick`, `PointerDoubleClick`
  (all `Action<Model?, PointerEventArgs>`); `KeyDown(Action<KeyboardEventArgs>)`;
  `Wheel(Action<WheelEventArgs>)`; `SelectionChanged(Action<SelectableModel>)`; and parameterless
  `PanChanged`, `ZoomChanged`, `ContainerChanged`, `Changed`.
- **Input seam** (called by the renderer): `TriggerPointerDown/Move/Up/Enter/Leave`,
  `TriggerPointerClick/DoubleClick(Model?, PointerEventArgs)`, `TriggerKeyDown(KeyboardEventArgs)`,
  `TriggerWheel(WheelEventArgs)`.
- **Properties:** `Options` (abstract — only abstract member), `Nodes`, `Links`, `Groups` (layers),
  `Controls`, `Container` (Rectangle), `Pan`, `Zoom`, `OrderedSelectables`, `SuspendRefresh`,
  `SuspendSorting`.
- **Methods:** selection (`SelectModel/UnselectModel/UnselectAll/GetSelectedModels`); behaviors
  (`RegisterBehavior<T>/GetBehavior<T>/UnregisterBehavior<T>`); view (`SetZoom/SetPan/UpdatePan/
  ZoomToFit/SetContainer`); coordinates (`GetRelativeMousePoint/GetRelativePoint/GetScreenPoint`);
  ordering (`SendToBack/SendToFront/RefreshOrders`); refresh (`Batch/Refresh`).

#### `Models/`

`NodeModel`, `PortModel`, `PortAlignment`, `LinkModel`, `LinkLabelModel`, `LinkVertexModel`,
`LinkMarker`, `GroupModel`, plus `Models/Base/` (Model, SelectableModel, MovableModel, etc.).

#### `Events/` — Core's OWN event types (NOT Blazor's)

`PointerEventArgs`, `MouseEventArgs`, `WheelEventArgs`, `KeyboardEventArgs`, `TouchEventArgs`.
This is why the brain is UI-agnostic: the Blazor layer maps `Microsoft.AspNetCore.Components.Web.*`
events into these. **Nodely maps Avalonia events into the equivalent Nodely-neutral types instead.**

#### `Geometry/`

`Point`, `Size`, `Rectangle`, `Line`, `Ellipse`, `BezierSpline`, `IShape`, `Shapes`. Pure math —
ports almost verbatim and is the first thing we build + test.

#### `Behaviors/`

`SelectionBehavior`, `DragMovablesBehavior`, `DragNewLinkBehavior`, `PanBehavior`, `ZoomBehavior`,
`KeyboardShortcutsBehavior` (+ `KeyboardShortcutsDefaults`), `VirtualizationBehavior`,
`EventsBehavior`, `DebugEventsBehavior`. Each is an interaction state machine fed by `Trigger*`.

#### `Routers/` and `PathGenerators/`

- Routers: `Router` (base), `NormalRouter`, `OrthogonalRouter`.
- Path generators: `PathGenerator` (base), `StraightPathGenerator`, `SmoothPathGenerator`,
  `PathGeneratorResult`. Upstream emits **SVG path strings** (via `SvgPathProperties`).
  **Nodely divergence:** emit neutral `PathData` commands instead (see ADR-0002 / avalonia-mapping-notes).

### `Blazor.Diagrams` — the rendering layer (this is what Nodely.Avalonia REPLACES)

- `BlazorDiagram.cs` (concrete `Diagram`), `BlazorDiagramsException.cs`, `_Imports.razor`, `Options/`,
  `Models/`, `Extensions/`, `wwwroot/`.
- **Components/**
  - `DiagramCanvas.razor` (+ `.cs`) — the canvas host.
  - **Renderers/**: `NodeRenderer.cs`, `PortRenderer.cs`, `LinkRenderer.cs`, `LinkLabelRenderer.cs`,
    `LinkVertexRenderer.cs`, `GroupRenderer.cs`.
  - **Widgets/**: `GridWidget`, `NavigatorWidget`, `SelectionBoxWidget` (each `.razor/.cs/.css`).
  - `NodeWidget.razor`, `SvgNodeWidget.razor`, `LinkWidget.razor` (+ `.cs`), `GroupNodes.razor`,
    `DefaultGroupWidget.razor`, `DefaultLinkLabelWidget.razor`.
  - **Controls/** — the Controls-layer adornments.
- **`wwwroot/script.js`** — JS interop, exported as `window.ZBlazorDiagrams`:
  - `observe/unobserve` using **`ResizeObserver`** -> calls back `OnResize(bounds)` (node sizing).
  - `getBoundingClientRect(el)` (canvas origin/size).
  - a **`MutationObserver`** + window **scroll** listener to re-fire `OnResize` when canvases move.
  - **All of this is replaced natively by Avalonia layout** — see avalonia-mapping-notes.md.
- CSS: `style.min.css`, `default.styles.min.css` — replaced by a C# `NodelyTheme`.

## Implications for the port

1. `Nodely.Core` ≈ `Blazor.Diagrams.Core` minus the SVG-string coupling, with neutral events and
   neutral `PathData`. Same `Diagram` contract and `Trigger*` seam.
2. `Nodely.Avalonia` ≈ `Blazor.Diagrams` package, re-expressed with Avalonia controls + `DrawingContext`
   + layout, with **no JS** and **no CSS files** (C# theme instead).
3. The optional `Algorithms` package maps to `Nodely.Algorithms`.
