using Nodely.Geometry;
using Nodely.Models.Base;

namespace Nodely.Models;

/// <summary>A user-draggable bend point on a link.</summary>
public class LinkVertexModel : MovableModel
{
    /// <summary>Creates a vertex on <paramref name="parent"/>.</summary>
    public LinkVertexModel(BaseLinkModel parent, Point? position = null) : base(position)
    {
        Parent = parent;
    }

    /// <summary>The link this vertex belongs to.</summary>
    public BaseLinkModel Parent { get; }

    /// <inheritdoc />
    public override void SetPosition(double x, double y)
    {
        base.SetPosition(x, y);
        Refresh();
        Parent.Refresh();
    }
}
