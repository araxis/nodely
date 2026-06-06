---
title: Designer controls
---

# Designer controls

`Nodely.Avalonia.Designer` is an optional package for reusable editor chrome around a `DiagramCanvas`. It
contains a ready-to-compose shell plus standalone controls for the common surfaces apps otherwise copy into
each editor: toolbox, command bar, property inspector, navigator host, and status bar.

Use it when you want runtime property editing, node stencils, common commands, and package-specific renderers
without rebuilding those panels in every app.

## Install

```powershell
dotnet add package Nodely.Avalonia.Designer
```

Add any domain packages your editor needs as usual:

```powershell
dotnet add package Nodely.Avalonia.Database
dotnet add package Nodely.Avalonia.Uml
dotnet add package Nodely.Serialization
```

## Compose a shell

`DiagramDesignerShell` owns the canvas and wires the optional chrome. Use `ConfigureCanvas` for renderer
registrations from core app code or side packages.

```csharp
using Avalonia.Media;
using Nodely;
using Nodely.Avalonia;
using Nodely.Avalonia.Database;
using Nodely.Avalonia.Designer;
using Nodely.Models;
using Point = Nodely.Geometry.Point;

var diagram = new NodelyDiagram();
diagram.Nodes.Add(new DatabaseTableNode(new Point(120, 120), "Orders", "sales"));

var shell = new DiagramDesignerShell(diagram, new DiagramDesignerOptions
{
    Palette = NodelyPalettes.Dark,
    ConfigureCanvas = canvas => canvas.UseDatabaseNodes(),
    PropertyRegistry = DiagramPropertyRegistry.CreateDefault(),
});
```

The shell exposes `Canvas`, `Diagram`, `Palette`, and `Refresh()` so host windows can still coordinate theme
switches, save/load, or external model changes.

## Register editable fields

The inspector is descriptor-based. It never guesses through reflection, so apps decide which runtime fields are
safe to edit and how each edit refreshes visuals.

```csharp
var properties = DiagramPropertyRegistry.CreateDefault()
    .Register<DatabaseTableNode>(
        DiagramProperty.Text<DatabaseTableNode>(
            "Schema",
            node => node.Schema,
            (node, value) => node.Schema = string.IsNullOrWhiteSpace(value) ? null : value.Trim(),
            "Database"),
        DiagramProperty.Color<DatabaseTableNode>(
            "Accent",
            node => node.AccentColor,
            (node, value) => node.AccentColor = value,
            "Database"),
        DiagramProperty.Collection<DatabaseTableNode, DatabaseColumn>(
            "Columns",
            node => node.Columns,
            _ => new DatabaseColumn("new_column", "text"),
            column => $"{column.Name}: {column.DataType}",
            "Columns",
            "Add column"));

var shell = new DiagramDesignerShell(diagram, new DiagramDesignerOptions
{
    ConfigureCanvas = canvas => canvas.UseDatabaseNodes(),
    PropertyRegistry = properties,
});
```

Edits run through `DiagramCanvas.RunAsUndoableEdit()`, then refresh the edited model and current visuals. That
means text fields, colors, booleans, enums, numbers, and collection add/remove actions participate in the same
undo/redo stack as move, delete, grouping, and layout.

## Add toolbox stencils

Toolbox sections create real node models. The shell places new nodes near the current viewport center and
selects them after insertion.

```csharp
var toolbox = new[]
{
    new DesignerToolboxSection(
        "Database",
        new[]
        {
            new DesignerToolboxItem(
                "Table",
                position => new DatabaseTableNode(position, "NewTable", "dbo"))
            {
                Detail = "Schema table",
                Accent = Brushes.SteelBlue,
            },
        }),
};

var shell = new DiagramDesignerShell(diagram, new DiagramDesignerOptions
{
    ConfigureCanvas = canvas => canvas.UseDatabaseNodes(),
    ToolboxSections = toolbox,
});
```

Use `AfterAdd` when a stencil needs follow-up setup after insertion, such as registering app-local metadata.
The toolbox runs that hook once for the created model so undo/redo reuses the same model cleanly.

## Use individual controls

The shell is convenient, but each surface is public and can be placed in your own layout:

```csharp
var canvas = new DiagramCanvas { Diagram = diagram };
canvas.UseDatabaseNodes();

var inspector = new DiagramPropertyInspector
{
    Canvas = canvas,
    Diagram = diagram,
    Registry = properties,
};

var commandBar = new DiagramCommandBar { Canvas = canvas };
var statusBar = new DiagramStatusBar { Canvas = canvas, Diagram = diagram };
var navigator = new DiagramNavigator { Diagram = diagram };
```

This is the recommended replacement for copied runtime-inspector patterns in samples or host apps: keep the
editing policy in descriptors, keep the chrome in the Designer package, and keep domain visuals in their
domain packages.
