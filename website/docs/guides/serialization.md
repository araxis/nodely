---
id: serialization
title: Save & load (serialization)
sidebar_position: 5
---

# Save & load

`Nodely.Serialization` turns a diagram into a versioned JSON snapshot and back again. The round-trip is exact —
serialize, reload, serialize again, and you get byte-for-byte the same JSON — which makes it safe to store and to
diff.

```bash
dotnet add package Nodely.Serialization
```

## Saving

```csharp
using Nodely.Serialization;

string json = DiagramSerializer.Serialize(diagram);
// write it to a file, a database, wherever
```

A snapshot captures the node ids, their kinds, positions, sizes, titles, and ports; the links and their bend
points; the groups; and the viewport's pan and zoom.

## Loading

Load JSON back into a fresh diagram. If you use custom node types, give the loader a factory that rebuilds them
from each snapshot — and make sure it keeps the snapshot's id, because that's how links and groups find their
nodes again:

```csharp
var diagram = new NodelyDiagram();

DiagramSerializer.Deserialize(diagram, json, ns => ns.Kind == nameof(TaskNode)
    ? new TaskNode(ns.Id, new Point(ns.X, ns.Y), ns.Title ?? "")
    : new NodeModel(ns.Id, new Point(ns.X, ns.Y)));
```

The factory keys off the kind, which is just the type's name, so your custom node needs an id-preserving
constructor:

```csharp
public TaskNode(string id, Point position, string title) : base(id, position) => Title = title;
```

## Keeping custom fields

Out of the box a snapshot stores the built-in fields, not whatever extra data your node carries. To persist that
too, override the two hooks on your node. Write your fields out as plain JSON-friendly values, and read them back
in:

```csharp
public override IReadOnlyDictionary<string, object?> GetExtraData() =>
    new Dictionary<string, object?> { ["Status"] = Status };

public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
{
    if (data.TryGetValue("Status", out var value) && value is string status)
        Status = status;
}
```

The values you read back are already plain CLR types — strings, numbers, booleans — not raw JSON, so there's
nothing to unwrap.

## The lower-level API

If you'd rather work with the snapshot object than a string, `ToSnapshot` and `Load` are right there:

```csharp
DiagramSnapshot snapshot = DiagramSerializer.ToSnapshot(diagram);
DiagramSerializer.Load(diagram, snapshot, nodeFactory);
```
