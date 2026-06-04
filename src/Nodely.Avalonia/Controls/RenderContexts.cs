using Nodely.Models;

namespace Nodely.Avalonia.Controls;

/// <summary>Context passed to control factories registered on a <see cref="DiagramCanvas"/>.</summary>
public sealed class DiagramRenderContext
{
    internal DiagramRenderContext(DiagramCanvas canvas) => Canvas = canvas;

    /// <summary>The canvas that owns the rendered model.</summary>
    public DiagramCanvas Canvas { get; }

    /// <summary>The diagram currently bound to the canvas, if any.</summary>
    public Diagram? Diagram => Canvas.Diagram;

    /// <summary>The canvas palette at render time.</summary>
    public NodelyPalette Palette => Canvas.Palette;

    /// <summary>Whether the canvas is currently read-only.</summary>
    public bool IsReadOnly => Canvas.IsReadOnly;
}
