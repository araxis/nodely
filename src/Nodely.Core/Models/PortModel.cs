using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models.Base;

namespace Nodely.Models;

/// <summary>A typed connection point on a node. Links attach to ports.</summary>
public class PortModel : Model, IHasBounds, IHasShape, ILinkable
{
    /// <summary>The stable serialization kind for a default port.</summary>
    public const string ModelKindKey = "port";

    private readonly List<BaseLinkModel> _links = new(4);

    /// <summary>Creates a port on <paramref name="parent"/>.</summary>
    public PortModel(NodeModel parent, PortAlignment alignment = PortAlignment.Bottom, Point? position = null,
        Size? size = null)
    {
        Parent = parent;
        Alignment = alignment;
        Position = position ?? Point.Zero;
        Size = size ?? Size.Zero;
    }

    /// <summary>Creates a port with the given id on <paramref name="parent"/>.</summary>
    public PortModel(string id, NodeModel parent, PortAlignment alignment = PortAlignment.Bottom,
        Point? position = null, Size? size = null) : base(id)
    {
        Parent = parent;
        Alignment = alignment;
        Position = position ?? Point.Zero;
        Size = size ?? Size.Zero;
    }

    /// <summary>The node this port belongs to.</summary>
    public NodeModel Parent { get; }

    /// <summary>Where the port sits on its node.</summary>
    public PortAlignment Alignment { get; }

    /// <summary>The top-left position of the port in diagram space.</summary>
    public Point Position { get; set; }

    /// <summary>The center of the port.</summary>
    public Point MiddlePosition => new(Position.X + (Size.Width / 2), Position.Y + (Size.Height / 2));

    /// <summary>The measured size of the port.</summary>
    public Size Size { get; set; }

    /// <summary>The links attached to this port.</summary>
    public IReadOnlyList<BaseLinkModel> Links => _links;

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <summary>If false, a call to <see cref="Model.Refresh"/> forces the port to update its position/size.</summary>
    public bool Initialized { get; set; }

    /// <summary>Refreshes the port and its links.</summary>
    public void RefreshAll()
    {
        Refresh();
        RefreshLinks();
    }

    /// <summary>Refreshes every link attached to this port.</summary>
    public void RefreshLinks()
    {
        foreach (var link in Links)
            link.Refresh();
    }

    /// <summary>The parent node as <typeparamref name="T"/>.</summary>
    public T GetParent<T>() where T : NodeModel => (T)Parent;

    /// <summary>The port's bounds.</summary>
    public Rectangle GetBounds() => new(Position, Size);

    /// <summary>
    /// Resolves the center point for this port on its parent. Override this for semantic ports that need
    /// to attach to a specific row, field, or shape inside the node instead of the alignment's default edge.
    /// </summary>
    public virtual Point GetPortCenter()
    {
        var nodePosition = Parent.Position;
        var nodeSize = Parent.Size ?? Size.Zero;
        var cx = nodePosition.X + nodeSize.Width / 2;
        var cy = nodePosition.Y + nodeSize.Height / 2;

        return Alignment switch
        {
            PortAlignment.Top => new Point(cx, nodePosition.Y),
            PortAlignment.TopRight => new Point(nodePosition.X + nodeSize.Width, nodePosition.Y),
            PortAlignment.Right => new Point(nodePosition.X + nodeSize.Width, cy),
            PortAlignment.BottomRight => new Point(nodePosition.X + nodeSize.Width, nodePosition.Y + nodeSize.Height),
            PortAlignment.Bottom => new Point(cx, nodePosition.Y + nodeSize.Height),
            PortAlignment.BottomLeft => new Point(nodePosition.X, nodePosition.Y + nodeSize.Height),
            PortAlignment.Left => new Point(nodePosition.X, cy),
            PortAlignment.TopLeft => new Point(nodePosition.X, nodePosition.Y),
            _ => new Point(cx, cy),
        };
    }

    /// <summary>The shape links use to attach to this port (a circle by default).</summary>
    public virtual IShape GetShape() => Shapes.Circle(this);

    /// <inheritdoc />
    public virtual bool CanAttachTo(ILinkable other)
        => other is PortModel port && port != this && !port.Locked && Parent != port.Parent;

    void ILinkable.AddLink(BaseLinkModel link) => _links.Add(link);

    void ILinkable.RemoveLink(BaseLinkModel link) => _links.Remove(link);
}
