using System;
using Nodely.Geometry;
using Nodely.Models.Base;

namespace Nodely.PathGenerators;

/// <summary>Generates a straight (poly-line) path, optionally rounding the corners by a radius.</summary>
public class StraightPathGenerator : PathGenerator
{
    private readonly double _radius;

    /// <summary>Creates the generator. A positive <paramref name="radius"/> rounds bends.</summary>
    public StraightPathGenerator(double radius = 0) => _radius = radius;

    /// <inheritdoc />
    public override PathGeneratorResult GetResult(Diagram diagram, BaseLinkModel link, Point[] route, Point source, Point target)
    {
        route = ConcatRouteAndSourceAndTarget(route, source, target);
        var last = route.Length - 1;

        double? sourceAngle = null;
        double? targetAngle = null;

        var sourceMarker = link.EffectiveSourceMarker;
        var targetMarker = link.EffectiveTargetMarker;

        if (sourceMarker != null)
            sourceAngle = AdjustRouteForSourceMarker(route, sourceMarker.Width);

        if (targetMarker != null)
            targetAngle = AdjustRouteForTargetMarker(route, targetMarker.Width);

        var paths = link.Vertices.Count > 0 ? new PathData[route.Length - 1] : null;
        var fullPath = new PathData().MoveTo(route[0]);
        double? secondDist = null;
        var lastPt = route[0];

        for (var i = 0; i < route.Length - 1; i++)
        {
            if (_radius > 0 && i > 0)
            {
                var previous = route[i - 1];
                var current = route[i];
                var next = route[i + 1];

                double firstDist = secondDist ?? (current.DistanceTo(previous) / 2);
                secondDist = current.DistanceTo(next) / 2;

                var p1 = -Math.Min(_radius, firstDist);
                var p2 = -Math.Min(_radius, secondDist.Value);

                var fp = current.MoveAlongLine(previous, p1);
                var sp = current.MoveAlongLine(next, p2);

                fullPath.LineTo(fp).QuadTo(current, sp);

                if (paths != null)
                    paths[i - 1] = new PathData().MoveTo(lastPt).LineTo(fp).QuadTo(current, sp);

                lastPt = sp;

                if (i == route.Length - 2)
                {
                    fullPath.LineTo(route[last]);

                    if (paths != null)
                        paths[i] = new PathData().MoveTo(lastPt).LineTo(route[last]);
                }
            }
            else if (_radius == 0 || route.Length == 2)
            {
                fullPath.LineTo(route[i + 1]);

                if (paths != null)
                    paths[i] = new PathData().MoveTo(route[i]).LineTo(route[i + 1]);
            }
        }

        return new PathGeneratorResult(fullPath, paths ?? Array.Empty<PathData>(), sourceAngle, route[0], targetAngle, route[last]);
    }
}
