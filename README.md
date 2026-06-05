# Nodely

[![Package](https://img.shields.io/nuget/v/Nodely.Avalonia?label=package)](https://www.nuget.org/packages/Nodely.Avalonia)
[![Downloads](https://img.shields.io/nuget/dt/Nodely.Avalonia?label=downloads)](https://www.nuget.org/packages/Nodely.Avalonia)
[![API](https://img.shields.io/nuget/v/Nodely.Avalonia.Api?label=api)](https://www.nuget.org/packages/Nodely.Avalonia.Api)
[![Database](https://img.shields.io/nuget/v/Nodely.Avalonia.Database?label=database)](https://www.nuget.org/packages/Nodely.Avalonia.Database)
[![MindMap](https://img.shields.io/nuget/v/Nodely.Avalonia.MindMap?label=mindmap)](https://www.nuget.org/packages/Nodely.Avalonia.MindMap)
[![Network](https://img.shields.io/nuget/v/Nodely.Avalonia.Network?label=network)](https://www.nuget.org/packages/Nodely.Avalonia.Network)
[![StateMachine](https://img.shields.io/nuget/v/Nodely.Avalonia.StateMachine?label=statemachine)](https://www.nuget.org/packages/Nodely.Avalonia.StateMachine)
[![UML](https://img.shields.io/nuget/v/Nodely.Avalonia.Uml?label=uml)](https://www.nuget.org/packages/Nodely.Avalonia.Uml)
[![Workflow](https://img.shields.io/nuget/v/Nodely.Avalonia.Workflow?label=workflow)](https://www.nuget.org/packages/Nodely.Avalonia.Workflow)

**Nodely** is a native **Avalonia** toolkit for building interactive node / graph / diagram editors — a
first-party port of the proven [Blazor.Diagrams](https://github.com/Blazor-Diagrams/Blazor.Diagrams)
architecture to Avalonia. Pan/zoom canvas, custom nodes, interactive links, groups, an overview minimap,
theming, read-only mode, serialization, undo/redo, and auto-layout — with **no SVG, no JS, no WebView**,
just Avalonia's native rendering.

> Status: **v0.7.0** main packages, with independent side packages. Engine + Avalonia UI are complete and
> tested on `net8.0` and `net10.0` (215 tests per runtime across the engine, side packages, and Avalonia
> headless UI). See
> [`CHANGELOG.md`](CHANGELOG.md) and the design notes in [`memory/`](memory/).

## Why

- **Performance first** — virtualized nodes, cached link geometry, immediate-mode grid/overview. A
  2000-node / ~4000-link graph re-routes + generates all paths in ~15 ms.
- **Customizable** — define a custom node by subclassing `NodeModel` and registering an Avalonia control.
- **Clean architecture** — a UI-agnostic engine (`Nodely.Core`) + a thin Avalonia rendering/input layer.
- **MVVM-agnostic** — works with CommunityToolkit.Mvvm, ReactiveUI, or plain objects.

## Packages

Install the main Avalonia package:

```powershell
dotnet add package Nodely.Avalonia
```

Optional packages:

```powershell
dotnet add package Nodely.Algorithms
dotnet add package Nodely.Serialization
dotnet add package Nodely.Avalonia.Api
dotnet add package Nodely.Avalonia.Database
dotnet add package Nodely.Avalonia.MindMap
dotnet add package Nodely.Avalonia.Network
dotnet add package Nodely.Avalonia.StateMachine
dotnet add package Nodely.Avalonia.Uml
dotnet add package Nodely.Avalonia.Workflow
```

Use `Nodely.Core` directly for headless engine scenarios; it is included transitively by `Nodely.Avalonia`.

| Package | Targets | What |
|---|---|---|
| [`Nodely.Core`](https://www.nuget.org/packages/Nodely.Core) | `netstandard2.0`, `net8.0`, `net10.0` | UI-agnostic engine: models, behaviors, geometry, routers, path generators, commands. |
| [`Nodely.Avalonia`](https://www.nuget.org/packages/Nodely.Avalonia) | `net8.0`, `net10.0` | Avalonia controls: `DiagramCanvas`, `DiagramNavigator`, theming, adorners. |
| [`Nodely.Avalonia.Api`](https://www.nuget.org/packages/Nodely.Avalonia.Api) | `net8.0`, `net10.0` | Optional side package: API service nodes, endpoint cards, contract nodes, typed ports, links, and arrange helpers. |
| [`Nodely.Avalonia.Database`](https://www.nuget.org/packages/Nodely.Avalonia.Database) | `net8.0`, `net10.0` | Optional side package: database table/view/procedure renderers, row-aware ports, and relationship links. |
| [`Nodely.Avalonia.MindMap`](https://www.nuget.org/packages/Nodely.Avalonia.MindMap) | `net8.0`, `net10.0` | Optional side package: root, branch, and leaf topics, branch ports, curved links, collapse state, and arrange helpers. |
| [`Nodely.Avalonia.Network`](https://www.nuget.org/packages/Nodely.Avalonia.Network) | `net8.0`, `net10.0` | Optional side package: network device renderers, typed ports, topology links, status badges, and arrange helpers. |
| [`Nodely.Avalonia.StateMachine`](https://www.nuget.org/packages/Nodely.Avalonia.StateMachine) | `net8.0`, `net10.0` | Optional side package: initial, state, final, choice, and note nodes, visible transition ports, transition links, self loops, and arrange helpers. |
| [`Nodely.Avalonia.Uml`](https://www.nuget.org/packages/Nodely.Avalonia.Uml) | `net8.0`, `net10.0` | Optional side package: compartmented UML renderers, row-aware ports, packages, notes, and relationship links. |
| [`Nodely.Avalonia.Workflow`](https://www.nuget.org/packages/Nodely.Avalonia.Workflow) | `net8.0`, `net10.0` | Optional side package: workflow start, end, task, decision, gateway, event, note nodes, and workflow links. |
| [`Nodely.Algorithms`](https://www.nuget.org/packages/Nodely.Algorithms) | `netstandard2.0`, `net8.0`, `net10.0` | Optional: traversal, connected components, layered auto-layout. |
| [`Nodely.Serialization`](https://www.nuget.org/packages/Nodely.Serialization) | `netstandard2.0`, `net8.0`, `net10.0` | Optional: versioned JSON snapshots. |

## Getting started

```csharp
using Nodely;
using Nodely.Avalonia.Controls;
using Nodely.Models;
using Point = Nodely.Geometry.Point;

var diagram = new NodelyDiagram();

var a = diagram.Nodes.Add(new NodeModel(new Point(80, 80))  { Title = "Start" });
var b = diagram.Nodes.Add(new NodeModel(new Point(360, 80)) { Title = "End" });
diagram.Links.Add(new LinkModel(a.AddPort(PortAlignment.Right), b.AddPort(PortAlignment.Left)));

var canvas = new DiagramCanvas { Diagram = diagram };   // drop into any Avalonia layout
```

Drag empty space to **pan**, scroll to **zoom**, drag a node to **move**, drag from a **port** to another to
**connect**, **Shift-drag** to marquee-select, **Delete** to remove, **Esc** to clear selection.

## Customizing nodes (the headline)

```csharp
public sealed class TaskNode : NodeModel
{
    public TaskNode(Point p, string title) : base(p) => Title = title;
    public string Status { get; set; } = "Pending";
}

canvas.RegisterNode<TaskNode>(node => new Border
{
    Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x4A, 0x6B)),
    Padding = new Thickness(14, 10),
    Child = new TextBlock { Text = $"{node.Title} — {node.Status}", Foreground = Brushes.White },
});

diagram.Nodes.Add(new TaskNode(new Point(120, 200), "Build") { Status = "Running" });
```

Custom **links** are composed: set a per-link `Router` / `PathGenerator`, or change the defaults via
`diagram.Options.Links`. Custom **ports/anchors/behaviors** are registered explicitly (no reflection scanning).

## More features

```csharp
// Theming
canvas.Palette = NodelyPalettes.Light;          // or NodelyPalettes.Dark (default)

// Read-only inspector (pan/zoom/select work; move/connect/delete blocked)
canvas.IsReadOnly = true;

// View controls
canvas.ZoomToFit();  canvas.ZoomIn();  canvas.ResetView();

// Overview minimap (bind to the same diagram, place anywhere)
var navigator = new DiagramNavigator { Diagram = diagram };

// Snap-to-grid
diagram.Options.GridSize = 24;

// Grouping
diagram.Options.Groups.Enabled = true;
diagram.Groups.Group(a, b);

// Auto-layout (Nodely.Algorithms)
Nodely.Algorithms.LayeredLayout.Arrange(diagram);

// Serialization (Nodely.Serialization)
string json = Nodely.Serialization.DiagramSerializer.Serialize(diagram);
Nodely.Serialization.DiagramSerializer.Deserialize(new NodelyDiagram(), json);

// API pack (Nodely.Avalonia.Api)
canvas.UseApiNodes();
Nodely.Avalonia.Api.ApiLayout.Arrange(diagram);
var apiRegistry = Nodely.Avalonia.Api.ApiNodeFactory.CreateRegistry();

// Database pack (Nodely.Avalonia.Database)
canvas.UseDatabaseNodes();
var registry = Nodely.Avalonia.Database.DatabaseNodeFactory.CreateRegistry();

// MindMap pack (Nodely.Avalonia.MindMap)
canvas.UseMindMapNodes();
Nodely.Avalonia.MindMap.MindMapLayout.Arrange(diagram);
var mindMapRegistry = Nodely.Avalonia.MindMap.MindMapNodeFactory.CreateRegistry();

// Network pack (Nodely.Avalonia.Network)
canvas.UseNetworkNodes();
Nodely.Avalonia.Network.NetworkLayout.Arrange(diagram);
var networkRegistry = Nodely.Avalonia.Network.NetworkNodeFactory.CreateRegistry();

// StateMachine pack (Nodely.Avalonia.StateMachine)
canvas.UseStateMachineNodes();
Nodely.Avalonia.StateMachine.StateMachineLayout.Arrange(diagram);
var stateMachineRegistry = Nodely.Avalonia.StateMachine.StateMachineNodeFactory.CreateRegistry();

// UML pack (Nodely.Avalonia.Uml)
canvas.UseUmlNodes();
var umlRegistry = Nodely.Avalonia.Uml.UmlNodeFactory.CreateRegistry();

// Workflow pack (Nodely.Avalonia.Workflow)
canvas.UseWorkflowNodes();
var workflowRegistry = Nodely.Avalonia.Workflow.WorkflowNodeFactory.CreateRegistry();

// Undo/redo (Nodely.Commands)
var history = new Nodely.Commands.UndoRedoStack();
history.Execute(new Nodely.Commands.AddNodeCommand(diagram, new NodeModel()));
history.Undo();  history.Redo();

// Toolbar state
canvas.CommandStateChanged += RefreshToolbar;
copyButton.IsEnabled = canvas.CanCopySelection;
pasteButton.IsEnabled = canvas.CanPasteClipboard;
groupButton.IsEnabled = canvas.CanGroupSelection;

// Runtime property edits
canvas.RunAsUndoableEdit(
    apply: () => { a.Title = "Renamed"; a.RefreshAll(); },
    undo: () => { a.Title = "Start"; a.RefreshAll(); });
```

## Repository layout

```
src/        Nodely.Core, Nodely.Avalonia, side packages, Nodely.Algorithms, Nodely.Serialization
samples/    Nodely.Demo (Avalonia desktop gallery), Nodely.QuickStart (minimal copyable app)
tests/      Nodely.Core.Tests, side-package tests, Nodely.Avalonia.Tests (Avalonia headless)
bench/      Nodely.Benchmarks (engine throughput)
memory/     Design decisions (ADRs), research, the development plan, progress, learnings
```

## Build & run

Building the repository requires the **.NET 10 SDK** (pinned via `global.json`). Packages ship assets for
both `net8.0` and `net10.0` Avalonia consumers; `samples/Nodely.QuickStart` targets `net8.0`.

```powershell
dotnet build Nodely.slnx
dotnet test  Nodely.slnx
dotnet run --project samples/Nodely.Demo
dotnet run --project samples/Nodely.QuickStart
dotnet pack  Nodely.slnx -c Release   # produces the NuGet packages
```

## License

MIT — see [`LICENSE`](LICENSE) and [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md).
