using System;
using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models.Base;

namespace Nodely.Anchors;

/// <summary>
/// Strategy for where a link endpoint attaches. Decouples "what a link connects to" (a port, node, free
/// point, …) from "the exact point it touches".
/// </summary>
public abstract class Anchor
{
    /// <summary>Creates an anchor optionally bound to a linkable model.</summary>
    protected Anchor(ILinkable? model = null) => Model = model;

    /// <summary>The model this anchor attaches to, if any.</summary>
    public ILinkable? Model { get; }

    /// <summary>Resolves the attachment point given the link and its route.</summary>
    public abstract Point? GetPosition(BaseLinkModel link, Point[] route);

    /// <summary>Resolves a representative point independent of any route (e.g. a center).</summary>
    public abstract Point? GetPlainPosition();

    /// <summary>Resolves the attachment point with an empty route.</summary>
    public Point? GetPosition(BaseLinkModel link) => GetPosition(link, Array.Empty<Point>());

    /// <summary>The plain position of the link's other endpoint.</summary>
    protected static Point? GetOtherPosition(BaseLinkModel link, bool isTarget)
    {
        var anchor = isTarget ? link.Source : link.Target;
        return anchor.GetPlainPosition();
    }

    /// <summary>The point in <paramref name="points"/> closest to <paramref name="point"/>.</summary>
    protected static Point? GetClosestPointTo(IEnumerable<Point?> points, Point point)
    {
        var minDist = double.MaxValue;
        Point? minPoint = null;

        foreach (var pt in points)
        {
            if (pt == null)
                continue;

            var dist = pt.DistanceTo(point);
            if (dist < minDist)
            {
                minDist = dist;
                minPoint = pt;
            }
        }

        return minPoint;
    }
}
