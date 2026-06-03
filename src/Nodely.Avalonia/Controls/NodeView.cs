using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Nodely.Models;

namespace Nodely.Avalonia.Controls;

/// <summary>
/// Hosts the templated visual for a single <see cref="NodeModel"/> and draws a selection outline. The
/// content is resolved from the canvas' node template registry (or a built-in default).
/// </summary>
internal sealed class NodeView : Decorator
{
    private readonly DiagramCanvas _owner;

    public NodeView(NodeModel node, DiagramCanvas owner)
    {
        _owner = owner;
        Node = node;
        Child = owner.BuildNodeContent(node);
    }

    /// <summary>The node this view represents.</summary>
    public NodeModel Node { get; }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Node.Selected)
            context.DrawRectangle(null, new Pen(_owner.Palette.Selection, 2), new Rect(Bounds.Size).Inflate(1.5));
    }
}
