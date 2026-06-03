---
id: custom-nodes
title: Custom nodes
sidebar_position: 1
---

# Custom nodes

The default node is a plain titled box, which is fine for a first run but rarely what you want for real. Making
a node look like your data is the most common thing you'll customize, and it comes down to one call:
`RegisterNode`. You give it a node type and a function that turns a node of that type into an Avalonia control,
and from then on Nodely renders those nodes with your control.

## Define the model

Start with a subclass of `NodeModel` that carries whatever your node actually needs to know:

```csharp
using Nodely.Models;
using Point = Nodely.Geometry.Point;

public sealed class TaskNode : NodeModel
{
    public TaskNode(Point position, string title) : base(position) => Title = title;

    public string Status { get; set; } = "Pending";
}
```

## Render it

Register a factory on the canvas. It receives your strongly-typed node and returns any control you like — here a
bordered card with the title and status stacked inside:

```csharp
canvas.RegisterNode<TaskNode>(node => new Border
{
    Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x4A, 0x6B)),
    BorderBrush = new SolidColorBrush(Color.FromRgb(0x4D, 0x9E, 0xFF)),
    BorderThickness = new Thickness(1),
    CornerRadius = new CornerRadius(6),
    Padding = new Thickness(14, 10),
    Child = new StackPanel
    {
        Children =
        {
            new TextBlock { Text = node.Title, Foreground = Brushes.White },
            new TextBlock { Text = node.Status, Foreground = Brushes.LightGray, FontSize = 11 },
        },
    },
});
```

Add a `TaskNode` to the diagram and it shows up with that card:

```csharp
diagram.Nodes.Add(new TaskNode(new Point(420, 160), "Build") { Status = "Running" });
```

## How a factory is chosen

When a node needs rendering, Nodely walks up its type hierarchy and uses the most specific factory you've
registered, falling back to the built-in box if it finds none. So you can register a factory for a base type and
override it for a subclass, and the closest match wins.

There's one nice side effect of nodes being real controls: Nodely measures your control during layout and writes
the size straight back to `NodeModel.Size`. You don't set node sizes by hand, and there's no browser-style
resize observer involved — the control's natural size *is* the node's size.

## Copying and pasting your node

Copy, cut, paste, and duplicate clone nodes through `NodeModel.Clone()`. The default copies the built-in fields;
override it so your extra data comes along:

```csharp
public override NodeModel Clone() =>
    new TaskNode(Position, Title ?? "") { Status = Status, Size = Size };
```

If you also want that data to survive a save and reload, override the snapshot hooks too — see
[Save & load](./serialization.md).

## Ports

Ports are the little anchors links attach to. You add them by side:

```csharp
node.AddPort(PortAlignment.Right);
// also Top, Bottom, Left, and the four corners
```

Each port is hit-testable, and dragging out from one starts a new link. If the default dot isn't your style,
`RegisterPort` works exactly like `RegisterNode` — see [Extensibility](./extensibility.md).
