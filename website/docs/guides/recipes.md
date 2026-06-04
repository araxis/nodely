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
    public TaskNode(Point position, string title) : base(position) => Title = title;
    public TaskNode(string id, Point position, string title) : base(id, position) => Title = title;

    public string Status { get; set; } = "Pending";

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
DiagramSerializer.Deserialize(loaded, json, snapshot =>
    snapshot.Kind == nameof(TaskNode)
        ? new TaskNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "")
        : new NodeModel(snapshot.Id, new Point(snapshot.X, snapshot.Y)) { Title = snapshot.Title });
```
