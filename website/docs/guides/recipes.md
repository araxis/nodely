---
id: recipes
title: Recipes
sidebar_position: 9
---

# Recipes

These are small copyable patterns for the first things most apps wire around a diagram canvas.

## Minimal app

The smallest useful setup is a diagram, two nodes, two ports, one link, and a canvas.

```csharp
using Nodely;
using Nodely.Avalonia.Controls;
using Nodely.Models;
using Point = Nodely.Geometry.Point;

var diagram = new NodelyDiagram();
diagram.Options.GridSize = 24;
diagram.Options.Links.DefaultTargetMarker = LinkMarker.Arrow;

var left = diagram.Nodes.Add(new NodeModel(new Point(120, 180)) { Title = "Input" });
var right = diagram.Nodes.Add(new NodeModel(new Point(420, 180)) { Title = "Output" });

diagram.Links.Add(new LinkModel(
    left.AddPort(PortAlignment.Right),
    right.AddPort(PortAlignment.Left)));

var canvas = new DiagramCanvas { Diagram = diagram };
canvas.ZoomToFit();
```

For a complete tiny desktop app, see `samples/Nodely.QuickStart`.

## Toolbar command state

`DiagramCanvas` exposes command-state helpers so toolbars do not need to inspect the model graph themselves.
Refresh buttons from `CommandStateChanged`.

```csharp
var copy = new Button { Content = "Copy" };
var paste = new Button { Content = "Paste" };
var group = new Button { Content = "Group" };
var undo = new Button { Content = "Undo" };

copy.Click += (_, _) => canvas.CopySelection();
paste.Click += (_, _) => canvas.PasteClipboard();
group.Click += (_, _) => canvas.GroupSelection();
undo.Click += (_, _) => canvas.Undo();

void RefreshButtons()
{
    copy.IsEnabled = canvas.CanCopySelection;
    paste.IsEnabled = canvas.CanPasteClipboard;
    group.IsEnabled = canvas.CanGroupSelection;
    undo.IsEnabled = !canvas.IsReadOnly && canvas.CanUndo;
}

canvas.CommandStateChanged += RefreshButtons;
RefreshButtons();
```

The state helpers are read-only. They reflect selection, clipboard, history, read-only mode, and diagram swaps.

## Runtime property inspector

Host apps can edit selected-node or selected-link metadata through the same undo/redo stack used by drag,
layout, grouping, and delete. Use `RunAsUndoableEdit` for the reversible mutation; it rebuilds the canvas after
apply and undo. Refresh the edited model inside the action when link paths, labels, or dependent links need to
recompute.

```csharp
void Rename(NodeModel node, string nextTitle)
{
    var previousTitle = node.Title;
    if (previousTitle == nextTitle)
        return;

    canvas.RunAsUndoableEdit(
        apply: () =>
        {
            node.Title = nextTitle;
            node.RefreshAll();
        },
        undo: () =>
        {
            node.Title = previousTitle;
            node.RefreshAll();
        });
}
```

For links, edit the concrete link model the same way:

```csharp
void ChangeWidth(LinkModel link, double nextWidth)
{
    var previousWidth = link.Width;
    canvas.RunAsUndoableEdit(
        apply: () =>
        {
            link.Width = nextWidth;
            link.Refresh();
        },
        undo: () =>
        {
            link.Width = previousWidth;
            link.Refresh();
        });
}
```

The desktop gallery includes a side-panel inspector that edits core, API, Database, MindMap, Network,
StateMachine, UML, Workflow, and sample custom model fields at runtime.

For a reusable panel instead of app-local inspector code, use `Nodely.Avalonia.Designer`:

```csharp
using Nodely.Avalonia.Designer;

var registry = DiagramPropertyRegistry.CreateDefault()
    .Register<TaskNode>(
        DiagramProperty.Text<TaskNode>(
            "Status",
            node => node.Status,
            (node, value) => node.Status = value ?? string.Empty,
            "Task"));

var inspector = new DiagramPropertyInspector
{
    Canvas = canvas,
    Diagram = diagram,
    Registry = registry,
};
```

## Custom overlay

Use `DiagramLayer` for overlays that should pan and zoom with the diagram, such as rulers, guides, heatmaps, or
validation marks.

```csharp
public sealed class GuideLayer : DiagramLayer
{
    public override void Render(DrawingContext context)
    {
        if (Diagram is null)
            return;

        var pen = new Pen(Brushes.DeepSkyBlue, 1, DashStyle.Dash);
        foreach (var node in Diagram.Nodes)
            if (node.Size is { } size)
            {
                var y = node.Position.Y + size.Height + 24;
                context.DrawLine(pen,
                    new Avalonia.Point(node.Position.X, y),
                    new Avalonia.Point(node.Position.X + size.Width, y));
            }
    }
}

canvas.AddLayer(new GuideLayer()); // world-space by default
```

Pass `worldSpace: false` when the overlay should stay pinned to the viewport.

## Save and load custom nodes

Custom nodes should preserve their id on load, and use the extra-data hooks for fields that are not built in.

```csharp
public sealed class TaskNode : NodeModel
{
    public new const string ModelKindKey = "app.task";

    public TaskNode(Point position, string title) : base(position) => Title = title;
    public TaskNode(string id, Point position, string title) : base(id, position) => Title = title;

    public string Status { get; set; } = "Pending";

    public override string ModelKind => ModelKindKey;

    public override IReadOnlyDictionary<string, object?> GetExtraData() =>
        new Dictionary<string, object?> { ["Status"] = Status };

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Status", out var value) && value is string status)
            Status = status;
    }
}
```

```csharp
string json = DiagramSerializer.Serialize(diagram);

var loaded = new NodelyDiagram();
var registry = new DiagramSerializationRegistry()
    .RegisterNode(TaskNode.ModelKindKey,
        snapshot => new TaskNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? ""));

DiagramSerializer.Deserialize(loaded, json, registry);
```
