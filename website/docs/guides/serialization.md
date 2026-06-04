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

A snapshot captures stable model kinds, built-in fields, custom extras, links, groups, and the viewport's pan
and zoom.

## Loading

Load JSON back into a fresh diagram. If you use custom model types, give the loader a registry that rebuilds
them from stable model kinds:

```csharp
var diagram = new NodelyDiagram();

var registry = new DiagramSerializationRegistry()
    .RegisterNode(TaskNode.ModelKindKey,
        ns => new TaskNode(ns.Id, new Point(ns.X, ns.Y), ns.Title ?? ""));

DiagramSerializer.Deserialize(diagram, json, registry);
```

Your custom model should expose a stable kind key and an id-preserving constructor:

```csharp
public new const string ModelKindKey = "app.task";
public override string ModelKind => ModelKindKey;
public TaskNode(string id, Point position, string title) : base(id, position) => Title = title;
```

## Keeping custom fields

Out of the box a snapshot stores the built-in fields, not whatever extra data your model carries. To persist
that too, override the two hooks. Write your fields out as plain JSON-friendly values, and read them back in:

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
DiagramSerializer.Load(diagram, snapshot, registry);
```
