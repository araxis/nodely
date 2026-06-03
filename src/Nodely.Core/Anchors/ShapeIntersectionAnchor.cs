using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Anchors;

/// <summary>
/// An anchor that attaches a link to a node at the point where the line toward the other endpoint crosses
/// the node's shape boundary.
/// </summary>
public sealed class ShapeIntersectionAnchor : Anchor
{
    /// <summary>Creates an anchor on <paramref name="model"/>.</summary>
    public ShapeIntersectionAnchor(NodeModel model) : base(model) => Node = model;

    /// <summary>The node this anchor attaches to.</summary>
    public NodeModel Node { get; }

    /// <inheritdoc />
    public override Point? GetPosition(BaseLinkModel link, Point[] route)
    {
        if (Node.Size == null)
            return null;

        var isTarget = link.Target == this;
        var nodeCenter = Node.GetBounds()!.Center;

        Point? pt = route.Length > 0
            ? route[isTarget ? route.Length - 1 : 0]
            : GetOtherPosition(link, isTarget);

        if (pt is null)
            return null;

        var line = new Line(pt, nodeCenter);
        var intersections = Node.GetShape().GetIntersectionsWithLine(line);
        return GetClosestPointTo(intersections, pt);
    }

    /// <inheritdoc />
    public override Point? GetPlainPosition() => Node.GetBounds()?.Center;
}
