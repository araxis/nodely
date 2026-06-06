using System;
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
        var bounds = Node.GetBounds()!;
        var nodeCenter = bounds.Center;

        Point? pt = route.Length > 0
            ? route[isTarget ? route.Length - 1 : 0]
            : GetOtherPosition(link, isTarget);

        if (pt is null)
            return null;

        if (route.Length == 0 && IsSameShapeEndpoint(link) && pt == nodeCenter)
            pt = GetSameNodeReferencePoint(bounds, isTarget);

        var line = new Line(pt, nodeCenter);
        var intersections = Node.GetShape().GetIntersectionsWithLine(line);
        return GetClosestPointTo(intersections, pt);
    }

    /// <inheritdoc />
    public override Point? GetPlainPosition() => Node.GetBounds()?.Center;

    private bool IsSameShapeEndpoint(BaseLinkModel link)
        => link.Source is ShapeIntersectionAnchor source &&
           link.Target is ShapeIntersectionAnchor target &&
           ReferenceEquals(source.Node, target.Node);

    private static Point GetSameNodeReferencePoint(Rectangle bounds, bool isTarget)
    {
        var horizontal = Math.Max(bounds.Width, 1);
        var vertical = Math.Max(bounds.Height, 1);
        return isTarget
            ? new Point(bounds.Left - horizontal, bounds.Top - vertical)
            : new Point(bounds.Right + horizontal, bounds.Top - vertical);
    }
}
