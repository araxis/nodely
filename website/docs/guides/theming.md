---
id: theming
title: Theming
sidebar_position: 7
---

# Theming

The canvas paints itself from a `NodelyPalette` — a bundle of brushes for the grid, nodes, ports, links, groups,
selection, and label chips. Two palettes ship with Nodely, and switching between them restyles everything at
once:

```csharp
canvas.Palette = NodelyPalettes.Dark;   // the default
canvas.Palette = NodelyPalettes.Light;
```

## Rolling your own

A palette is a record, so the easiest way to make one is to take a built-in and change just the brushes you care
about:

```csharp
using Avalonia.Media;

var palette = NodelyPalettes.Dark with
{
    CanvasBackground = new SolidColorBrush(Color.FromRgb(0x12, 0x12, 0x16)),
    Selection = Brushes.MediumPurple,
    LinkStroke = new SolidColorBrush(Color.FromRgb(0xB0, 0xB0, 0xC0)),
};

canvas.Palette = palette;
```

The full set of brushes you can set is `CanvasBackground`, `GridLine`, `NodeBackground`, `NodeBorder`,
`NodeText`, `LinkStroke`, `PortFill`, `PortStroke`, `Selection`, `GroupBackground`, `GroupBorder`,
`LabelBackground`, and `LabelForeground`.

If you need styling that varies per model rather than across the whole diagram — say, links that turn red when
they're invalid — that's a job for the render hooks rather than the palette. See
[Extensibility](./extensibility.md).

## The grid

There are two grids, and it's worth knowing they're separate. One is what you see; the other is what dragging
snaps to. You can show one spacing and snap to another, or use either on its own:

```csharp
canvas.GridSize = 24;            // the visible grid
canvas.GridBrush = Brushes.DimGray;
diagram.Options.GridSize = 24;   // the snap grid
```
