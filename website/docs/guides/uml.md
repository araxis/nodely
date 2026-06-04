---
id: uml
title: UML pack
sidebar_position: 10
---

# UML pack

`Nodely.Avalonia.Uml` is an optional side package for structural UML diagrams. It keeps UML-specific models and
renderers out of the core package while using the same canvas, serializer, selection, undo, and theming surface.

## Install

```bash
dotnet add package Nodely.Avalonia.Uml
```

## Register renderers

Call `UseUmlNodes()` after creating the canvas:

```csharp
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Uml;

var canvas = new DiagramCanvas { Diagram = diagram };
canvas.UseUmlNodes();
```

## Create UML nodes

```csharp
using Nodely.Avalonia.Uml;
using Nodely.Models;
using Point = Nodely.Geometry.Point;

var customer = diagram.Nodes.Add(new UmlClassNode(new Point(120, 160), "Customer"));
customer.Stereotypes.Add("entity");
customer.Members.Add(new UmlMember("Id", "Guid"));

var repository = diagram.Nodes.Add(new UmlInterfaceNode(new Point(440, 160), "ICustomerRepository"));
repository.Operations.Add(new UmlOperation("Get", "Customer"));

var link = diagram.Links.Add(new UmlRelationshipLink(customer, repository, UmlRelationshipKind.Realization)
{
    Label = "implements",
    SourceMultiplicity = "1",
    TargetMultiplicity = "1",
});
```

The first release includes class, interface, enum, package, and note nodes plus association, inheritance,
realization, dependency, aggregation, and composition links.

## Save and load

Register the UML serializer vocabulary when loading:

```csharp
using Nodely.Avalonia.Uml;
using Nodely.Serialization;

var json = DiagramSerializer.Serialize(diagram);

var loaded = new NodelyDiagram();
DiagramSerializer.Deserialize(loaded, json, UmlNodeFactory.CreateRegistry());
```

The registry restores UML nodes, relationship links, labels, multiplicities, and custom member/operation data.
