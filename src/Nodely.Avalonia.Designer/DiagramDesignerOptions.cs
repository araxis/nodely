using System;
using System.Collections.Generic;
using Nodely.Avalonia;
using Nodely.Avalonia.Controls;

namespace Nodely.Avalonia.Designer;

/// <summary>Options for <see cref="DiagramDesignerShell"/>.</summary>
public sealed class DiagramDesignerOptions
{
    /// <summary>Initial palette.</summary>
    public NodelyPalette Palette { get; init; } = NodelyPalettes.Dark;

    /// <summary>Initial read-only state.</summary>
    public bool IsReadOnly { get; init; }

    /// <summary>Property descriptors used by the inspector.</summary>
    public DiagramPropertyRegistry PropertyRegistry { get; init; } = DiagramPropertyRegistry.CreateDefault();

    /// <summary>Toolbox sections.</summary>
    public IEnumerable<DesignerToolboxSection> ToolboxSections { get; init; } = Array.Empty<DesignerToolboxSection>();

    /// <summary>Hook for node/port/link renderer registration.</summary>
    public Action<DiagramCanvas>? ConfigureCanvas { get; init; }

    /// <summary>Optional layout command.</summary>
    public Action<DiagramCanvas, Diagram>? LayoutAction { get; init; }

    /// <summary>Whether to show the toolbox.</summary>
    public bool ShowToolbox { get; init; } = true;

    /// <summary>Whether to show the property inspector.</summary>
    public bool ShowInspector { get; init; } = true;

    /// <summary>Whether to show the navigator.</summary>
    public bool ShowNavigator { get; init; } = true;

    /// <summary>Whether to show the command bar.</summary>
    public bool ShowCommandBar { get; init; } = true;

    /// <summary>Whether to show the status bar.</summary>
    public bool ShowStatusBar { get; init; } = true;

    /// <summary>Inspector column width.</summary>
    public double InspectorWidth { get; init; } = 340;

    /// <summary>Toolbox column width.</summary>
    public double ToolboxWidth { get; init; } = 220;
}
