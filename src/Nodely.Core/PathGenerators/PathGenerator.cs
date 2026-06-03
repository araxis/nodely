using System;
using Nodely.Geometry;
using Nodely.Models.Base;

namespace Nodely.PathGenerators;

/// <summary>Turns a link's route + resolved endpoints into a drawable <see cref="PathGeneratorResult"/>.</summary>
public abstract class PathGenerator
{
    /// <summary>Produces the drawable path for <paramref name="link"/>.</summary>
    public abstract PathGeneratorResult GetResult(Diagram diagram, BaseLinkModel link, Point[] route, Point source, Point target);

    /// <summary>Shortens the route start by the source marker width and returns the marker angle (degrees).</summary>
    protected static double AdjustRouteForSourceMarker(Point[] route, double markerWidth)
    {
        var angleInRadians = Math.Atan2(route[1].Y - route[0].Y, route[1].X - route[0].X) + Math.PI;
        var xChange = markerWidth * Math.Cos(angleInRadians);
        var yChange = markerWidth * Math.Sin(angleInRadians);
        route[0] = new Point(route[0].X - xChange, route[0].Y - yChange);
        return angleInRadians * 180 / Math.PI;
    }

    /// <summary>Shortens the route end by the target marker width and returns the marker angle (degrees).</summary>
    protected static double AdjustRouteForTargetMarker(Point[] route, double markerWidth)
    {
        var last = route.Length - 1;
        var angleInRadians = Math.Atan2(route[last].Y - route[last - 1].Y, route[last].X - route[last - 1].X);
        var xChange = markerWidth * Math.Cos(angleInRadians);
        var yChange = markerWidth * Math.Sin(angleInRadians);
        route[last] = new Point(route[last].X - xChange, route[last].Y - yChange);
        return angleInRadians * 180 / Math.PI;
    }

    /// <summary>Builds a single array of [source, ...route, target].</summary>
    protected static Point[] ConcatRouteAndSourceAndTarget(Point[] route, Point source, Point target)
    {
        var result = new Point[route.Length + 2];
        result[0] = source;
        route.CopyTo(result, 1);
        result[result.Length - 1] = target;
        return result;
    }
}
