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

That one call registers compartmented UML nodes, UML-specific ports, relationship styling, and pack-owned
relationship markers.

## Visual vocabulary

The UML pack owns its own visual language:

- classes, interfaces, and enums render as typed compartments with stereotypes, flags, members, operations, and
  literals;
- packages render with a package tab, and notes render with a folded corner;
- association, inheritance, realization, dependency, aggregation, and composition ports use role-specific
  shapes;
- named ports can attach to a matching member, operation, or enum literal row;
- relationship links draw UML-specific markers through the pack renderer.

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

var customerPort = customer.AddPort(new UmlPortModel(
    customer,
    PortAlignment.Right,
    UmlPortKind.Realization,
    "Id"));
var repositoryPort = repository.AddPort(new UmlPortModel(
    repository,
    PortAlignment.Left,
    UmlPortKind.Realization,
    "Get"));

var link = diagram.Links.Add(new UmlRelationshipLink(
    customerPort,
    repositoryPort,
    UmlRelationshipKind.Realization)
{
    Label = "implements",
    SourceMultiplicity = "1",
    TargetMultiplicity = "1",
});
```

The first release includes class, interface, enum, package, and note nodes plus association, inheritance,
realization, dependency, aggregation, and composition links. Port names are optional; when a name matches a
member, operation, or literal, the port aligns with that row during layout.

## Save and load

Register the UML serializer vocabulary when loading:

```csharp
using Nodely.Avalonia.Uml;
using Nodely.Serialization;

var json = DiagramSerializer.Serialize(diagram);

var loaded = new NodelyDiagram();
DiagramSerializer.Deserialize(loaded, json, UmlNodeFactory.CreateRegistry());
```

The registry restores UML nodes, ports, relationship links, labels, multiplicities, and custom
member/operation data.
