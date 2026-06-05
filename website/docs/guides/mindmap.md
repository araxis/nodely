---
id: mindmap
title: MindMap pack
sidebar_position: 12
---

# MindMap pack

`Nodely.Avalonia.MindMap` is an optional side package for mind-map editors. It provides topic models,
pack-owned renderers, branch ports, curved branch/association links, collapse state, and a small arrange helper.

## Install

```bash
dotnet add package Nodely.Avalonia.MindMap
```

## Register renderers

Call `UseMindMapNodes()` after creating the canvas:

```csharp
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.MindMap;

var canvas = new DiagramCanvas { Diagram = diagram };
canvas.UseMindMapNodes();
```

## Create topics and links

```csharp
using Nodely.Avalonia.MindMap;
using Nodely.Models;
using Point = Nodely.Geometry.Point;

var root = diagram.Nodes.Add(new MindMapRootNode(new Point(0, 0), "Launch plan")
{
    Notes = "Keep the first release focused",
    IconKey = "plan",
    AccentColor = "#4D9EFF",
});

var adoption = diagram.Nodes.Add(new MindMapBranchNode(new Point(0, 0), "Adoption")
{
    Side = MindMapTopicSide.Right,
    AccentColor = "#37A779",
});

var docs = diagram.Nodes.Add(new MindMapLeafNode(new Point(0, 0), "Docs recipes")
{
    AccentColor = "#D89C35",
});

var rootOut = root.AddPort(new MindMapPortModel(root, PortAlignment.Right, MindMapPortRole.Branch, "out"));
var adoptionIn = adoption.AddPort(new MindMapPortModel(adoption, PortAlignment.Left, MindMapPortRole.Branch, "in"));
var adoptionOut = adoption.AddPort(new MindMapPortModel(adoption, PortAlignment.Right, MindMapPortRole.Branch, "out"));
var docsIn = docs.AddPort(new MindMapPortModel(docs, PortAlignment.Left, MindMapPortRole.Branch, "in"));

diagram.Links.Add(new MindMapLink(rootOut, adoptionIn, MindMapLinkKind.Branch)
{
    Label = "scope",
    AccentColor = "#37A779",
});

diagram.Links.Add(new MindMapLink(adoptionOut, docsIn, MindMapLinkKind.Branch)
{
    AccentColor = "#D89C35",
});
```

## Arrange and collapse

`MindMapLayout.Arrange()` places the root at the configured center. First-level `Auto` branches alternate
left/right; explicit left/right sides are honored; descendants inherit their parent side.

```csharp
MindMapLayout.Arrange(diagram, new MindMapLayoutOptions
{
    OriginX = 0,
    OriginY = 0,
    LevelSpacing = 260,
});
```

Collapsed topics hide descendants and branch links by setting `Visible` on the affected models:

```csharp
adoption.Collapsed = true;
MindMapLayout.ApplyCollapseState(diagram);
canvas.RefreshVisuals();
```

For a toolbar action, route arrange through the canvas history:

```csharp
canvas.RunAsUndoableMove(() => MindMapLayout.Arrange(diagram));
canvas.RefreshVisuals();
canvas.ZoomToFit();
```

## Association links

Use association ports and `MindMapLinkKind.Association` for secondary relationships:

```csharp
var source = adoption.AddPort(new MindMapPortModel(adoption, PortAlignment.Bottom, MindMapPortRole.Association));
var target = docs.AddPort(new MindMapPortModel(docs, PortAlignment.Top, MindMapPortRole.Association));

diagram.Links.Add(new MindMapLink(source, target, MindMapLinkKind.Association)
{
    Label = "supports",
    AccentColor = "#9779CD",
});
```

## Save and load

Register the MindMap serializer vocabulary when loading:

```csharp
using Nodely.Avalonia.MindMap;
using Nodely.Serialization;

var json = DiagramSerializer.Serialize(diagram);

var loaded = new NodelyDiagram();
DiagramSerializer.Deserialize(loaded, json, MindMapNodeFactory.CreateRegistry());
```

The registry restores topic fields, port roles, link kind, labels, accent metadata, side hints, and collapse state.
