using Avalonia.Controls;

namespace Nodely.Avalonia.Controls;

/// <summary>
/// Optional base for a custom overlay added via <see cref="DiagramCanvas.AddLayer"/>. Gives convenient access
/// to the owning canvas and its diagram; override <c>Render(DrawingContext)</c> to draw — in diagram
/// coordinates for a world-space layer (the canvas keeps the pan/zoom transform synced and repaints the layer
/// on view/structure changes), or in viewport pixels for a screen-space layer.
/// </summary>
public abstract class DiagramLayer : Control
{
    /// <summary>The canvas this layer was added to (set by <see cref="DiagramCanvas.AddLayer"/>).</summary>
    public DiagramCanvas? Owner { get; private set; }

    /// <summary>The diagram the canvas is bound to, if any.</summary>
    protected Diagram? Diagram => Owner?.Diagram;

    internal void Attach(DiagramCanvas owner) => Owner = owner;
}
