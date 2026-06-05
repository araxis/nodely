---
id: statemachine
title: StateMachine pack
sidebar_position: 13
---

# StateMachine pack

`Nodely.Avalonia.StateMachine` is an optional side package for state-machine editors. It adds focused
models, renderers, transition ports, transition links, self-loop drawing, serialization helpers, and a simple
arrange helper without changing the main packages.

## Install

```powershell
dotnet add package Nodely.Avalonia.StateMachine
dotnet add package Nodely.Serialization
```

The package targets `net8.0` and `net10.0`, and depends on `Nodely.Avalonia` plus `Nodely.Serialization`.

## Register renderers

Call `UseStateMachineNodes()` on the canvas after creating it:

```csharp
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.StateMachine;

var canvas = new DiagramCanvas
{
    Diagram = diagram,
};

canvas.UseStateMachineNodes();
```

This registers:

- initial, state, final, choice, and note node renderers
- visible state-machine port renderers
- typed transition styles
- transition drawing, including a custom self-loop drawer

## Create a diagram

```csharp
using Nodely;
using Nodely.Avalonia.StateMachine;
using Nodely.Models;
using Point = Nodely.Geometry.Point;

var diagram = new NodelyDiagram();

var start = diagram.Nodes.Add(new StateMachineInitialNode(new Point(80, 240), "Start"));
var idle = diagram.Nodes.Add(new StateMachineStateNode(new Point(300, 180), "Idle")
{
    Description = "Waiting for work",
    EntryAction = "show ready",
    ExitAction = "clear ready",
});
var route = diagram.Nodes.Add(new StateMachineChoiceNode(new Point(580, 220), "Route"));
var running = diagram.Nodes.Add(new StateMachineStateNode(new Point(850, 180), "Running")
{
    EntryAction = "start timer",
    ExitAction = "stop timer",
});
var done = diagram.Nodes.Add(new StateMachineFinalNode(new Point(1130, 220), "Done"));
```

## Add ports and transitions

Use `StateMachinePortModel` when you want visible, semantic attachment points:

```csharp
var startOut = start.AddPort(new StateMachinePortModel(start, PortAlignment.Right, StateMachinePortRole.Exit, "start"));
var idleIn = idle.AddPort(new StateMachinePortModel(idle, PortAlignment.Left, StateMachinePortRole.Entry, "entry"));
var idleOut = idle.AddPort(new StateMachinePortModel(idle, PortAlignment.Right, StateMachinePortRole.Exit, "submit"));
var routeIn = route.AddPort(new StateMachinePortModel(route, PortAlignment.Left, StateMachinePortRole.Entry, "request"));

diagram.Links.Add(new StateMachineTransitionLink(startOut, idleIn)
{
    Trigger = "created",
    Action = "initialize",
    Priority = 1,
});

diagram.Links.Add(new StateMachineTransitionLink(idleOut, routeIn, StateMachineTransitionKind.Choice)
{
    Trigger = "submit",
    Guard = "has payload",
    Action = "validate",
    Priority = 2,
});
```

Transitions can store:

- `Kind`: `Normal`, `Self`, `Choice`, `Error`, or `Timeout`
- `Trigger`
- `Guard`
- `Action`
- `Priority`
- `AccentColor`

Self transitions can connect a node to itself:

```csharp
diagram.Links.Add(new StateMachineTransitionLink(running, running, StateMachineTransitionKind.Self)
{
    Trigger = "progress",
    Guard = "more work",
    Action = "continue",
});
```

## Arrange

`StateMachineLayout.Arrange()` is intentionally small. It places reachable states in left-to-right columns
from initial states and ignores self transitions for level calculation.

```csharp
StateMachineLayout.Arrange(diagram);
canvas.ZoomToFit();
```

For an undoable toolbar action:

```csharp
canvas.RunAsUndoableMove(() => StateMachineLayout.Arrange(diagram));
canvas.RefreshVisuals();
canvas.ZoomToFit();
```

## Save and load

Use the state-machine serializer registration when restoring saved diagrams:

```csharp
using Nodely.Serialization;

var registry = StateMachineNodeFactory.CreateRegistry();

string json = DiagramSerializer.Serialize(diagram);

var loaded = new NodelyDiagram();
DiagramSerializer.Deserialize(loaded, json, registry);
```

If your app uses several side packages, compose the same registry:

```csharp
var registry = StateMachineNodeFactory.CreateRegistry()
    .UseWorkflowNodes()
    .UseMindMapNodes();
```

The StateMachine pack uses stable model-kind keys and existing extra-data hooks, so custom state-machine
metadata round-trips without a snapshot schema change.
