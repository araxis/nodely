using System;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.PathGenerators;

/// <summary>Generates a smooth (cubic Bézier) path through the route points.</summary>
public class SmoothPathGenerator : PathGenerator
{
    private readonly double _margin;

    /// <summary>Creates the generator with the given curve-handle margin.</summary>
    public SmoothPathGenerator(double margin = 125) => _margin = margin;

    /// <inheritdoc />
    public override PathGeneratorResult GetResult(Diagram diagram, BaseLinkModel link, Point[] route, Point source, Point target)
    {
        route = ConcatRouteAndSourceAndTarget(route, source, target);

        if (route.Length > 2)
            return CurveThroughPoints(route, link);

        route = GetRouteWithCurvePoints(link, route);

        double? sourceAngle = null;
        double? targetAngle = null;

        var sourceMarker = link.EffectiveSourceMarker;
        var targetMarker = link.EffectiveTargetMarker;

        if (sourceMarker != null)
            sourceAngle = AdjustRouteForSourceMarker(route, sourceMarker.Width);

        if (targetMarker != null)
            targetAngle = AdjustRouteForTargetMarker(route, targetMarker.Width);

        var path = new PathData()
            .MoveTo(route[0])
            .CubicTo(route[1], route[2], route[3]);

        return new PathGeneratorResult(path, Array.Empty<PathData>(), sourceAngle, route[0], targetAngle, route[route.Length - 1]);
    }

    private PathGeneratorResult CurveThroughPoints(Point[] route, BaseLinkModel link)
    {
        double? sourceAngle = null;
        double? targetAngle = null;

        var sourceMarker = link.EffectiveSourceMarker;
        var targetMarker = link.EffectiveTargetMarker;

        if (sourceMarker != null)
            sourceAngle = AdjustRouteForSourceMarker(route, sourceMarker.Width);

        if (targetMarker != null)
            targetAngle = AdjustRouteForTargetMarker(route, targetMarker.Width);

        BezierSpline.GetCurveControlPoints(route, out var firstControlPoints, out var secondControlPoints);
        var paths = new PathData[firstControlPoints.Length];
        var fullPath = new PathData().MoveTo(route[0]);

        for (var i = 0; i < firstControlPoints.Length; i++)
        {
            var cp1 = firstControlPoints[i];
            var cp2 = secondControlPoints[i];
            fullPath.CubicTo(cp1, cp2, route[i + 1]);
            paths[i] = new PathData().MoveTo(route[i]).CubicTo(cp1, cp2, route[i + 1]);
        }

        // Todo: adjust marker positions based on the closest control points.
        return new PathGeneratorResult(fullPath, paths, sourceAngle, route[0], targetAngle, route[route.Length - 1]);
    }

    private Point[] GetRouteWithCurvePoints(BaseLinkModel link, Point[] route)
    {
        var cX = (route[0].X + route[1].X) / 2;
        var cY = (route[0].Y + route[1].Y) / 2;
        var curvePointA = GetCurvePoint(route, link.Source, route[0].X, route[0].Y, cX, cY, first: true);
        var curvePointB = GetCurvePoint(route, link.Target, route[1].X, route[1].Y, cX, cY, first: false);
        return new[] { route[0], curvePointA, curvePointB, route[1] };
    }

    private Point GetCurvePoint(Point[] route, Anchor anchor, double pX, double pY, double cX, double cY, bool first)
    {
        if (anchor is PositionAnchor)
            return new Point(cX, cY);

        if (anchor is SinglePortAnchor spa)
            return GetCurvePoint(pX, pY, cX, cY, spa.Port.Alignment);

        // Shape-based anchors use an axis-aligned curve handle.
        if (Math.Abs(route[0].X - route[1].X) >= Math.Abs(route[0].Y - route[1].Y))
            return first ? new Point(cX, route[0].Y) : new Point(cX, route[1].Y);

        return first ? new Point(route[0].X, cY) : new Point(route[1].X, cY);
    }

    private Point GetCurvePoint(double pX, double pY, double cX, double cY, PortAlignment? alignment)
    {
        var margin = Math.Min(_margin, Math.Pow(Math.Pow(pX - cX, 2) + Math.Pow(pY - cY, 2), .5));
        return alignment switch
        {
            PortAlignment.Top => new Point(pX, Math.Min(pY - margin, cY)),
            PortAlignment.Bottom => new Point(pX, Math.Max(pY + margin, cY)),
            PortAlignment.TopRight => new Point(Math.Max(pX + margin, cX), Math.Min(pY - margin, cY)),
            PortAlignment.BottomRight => new Point(Math.Max(pX + margin, cX), Math.Max(pY + margin, cY)),
            PortAlignment.Right => new Point(Math.Max(pX + margin, cX), pY),
            PortAlignment.Left => new Point(Math.Min(pX - margin, cX), pY),
            PortAlignment.BottomLeft => new Point(Math.Min(pX - margin, cX), Math.Max(pY + margin, cY)),
            PortAlignment.TopLeft => new Point(Math.Min(pX - margin, cX), Math.Min(pY - margin, cY)),
            _ => new Point(cX, cY),
        };
    }
}
