---
id: database
title: Database pack
sidebar_position: 10
---

# Database pack

`Nodely.Avalonia.Database` is an optional node pack for schema diagrams. It keeps database-specific models and
renderers out of the core packages while giving apps a ready-made vocabulary for tables, views, procedures, and
relationships.

## Install

```bash
dotnet add package Nodely.Avalonia.Database
```

The package targets `net8.0` and `net10.0` and depends on `Nodely.Avalonia` and `Nodely.Serialization`.

## Register the renderers

Call `UseDatabaseNodes()` after creating the canvas:

```csharp
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Database;

var canvas = new DiagramCanvas { Diagram = diagram };
canvas.UseDatabaseNodes();
```

That one call registers the table, view, procedure, port, and relationship styling.

## Build a schema diagram

```csharp
using Nodely;
using Nodely.Avalonia.Database;
using Nodely.Models;
using Point = Nodely.Geometry.Point;

var diagram = new NodelyDiagram();

var customers = diagram.Nodes.Add(new DatabaseTableNode(new Point(80, 100), "Customers", "sales"));
customers.Columns.Add(new DatabaseColumn("CustomerId", "int", isPrimaryKey: true, isNullable: false));
customers.Columns.Add(new DatabaseColumn("Email", "nvarchar(180)"));

var orders = diagram.Nodes.Add(new DatabaseTableNode(new Point(380, 100), "Orders", "sales"));
orders.Columns.Add(new DatabaseColumn("OrderId", "int", isPrimaryKey: true, isNullable: false));
orders.Columns.Add(new DatabaseColumn("CustomerId", "int", isNullable: false) { IsForeignKey = true });

var outPort = customers.AddPort(new DatabasePortModel(customers, PortAlignment.Right));
var inPort = orders.AddPort(new DatabasePortModel(orders, PortAlignment.Left));

diagram.Links.Add(new DatabaseRelationshipLink(outPort, inPort, RelationshipKind.OneToMany));
```

Tables and views expose mutable `Columns` collections. Procedures expose mutable `Parameters`.

## Save and load

The pack uses the existing node extra-data hooks, so the snapshot schema does not change. Pass
`DatabaseNodeFactory.Create` when loading:

```csharp
string json = DiagramSerializer.Serialize(diagram);

var loaded = new NodelyDiagram();
DiagramSerializer.Deserialize(loaded, json, DatabaseNodeFactory.Create);
```

The factory restores `DatabaseTableNode`, `DatabaseViewNode`, and `DatabaseProcedureNode` instances with their
schema, object name, columns, and parameters.
