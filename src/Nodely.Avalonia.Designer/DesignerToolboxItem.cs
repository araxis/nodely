using System;
using Avalonia.Controls;
using Avalonia.Media;
using Nodely.Models;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Designer;

/// <summary>A toolbox action that creates one node at a requested diagram position.</summary>
public sealed class DesignerToolboxItem
{
    /// <summary>Creates a toolbox item.</summary>
    public DesignerToolboxItem(string label, Func<NodelyPoint, NodeModel> createNode)
    {
        Label = string.IsNullOrWhiteSpace(label) ? throw new ArgumentException("Label is required.", nameof(label)) : label;
        CreateNode = createNode ?? throw new ArgumentNullException(nameof(createNode));
    }

    /// <summary>Button label.</summary>
    public string Label { get; }

    /// <summary>Optional secondary text.</summary>
    public string? Detail { get; init; }

    /// <summary>Optional accent brush for the item swatch.</summary>
    public IBrush? Accent { get; init; }

    /// <summary>Optional compact visual preview shown above the item label.</summary>
    public Func<Control>? PreviewFactory { get; init; }

    /// <summary>Creates a node at the requested position.</summary>
    public Func<NodelyPoint, NodeModel> CreateNode { get; }

    /// <summary>Optional hook after the node is added, useful for ports or app metadata.</summary>
    public Action<Diagram, NodeModel>? AfterAdd { get; init; }
}
