using System;
using System.Collections.Generic;

namespace Nodely.Geometry;

/// <summary>The kind of a single <see cref="PathOperation"/> in a <see cref="PathData"/>.</summary>
public enum PathCommand
{
    /// <summary>Begin a new sub-path at a point.</summary>
    MoveTo,

    /// <summary>Straight line to a point.</summary>
    LineTo,

    /// <summary>Cubic Bézier curve (two control points) to a point.</summary>
    CubicTo,

    /// <summary>Quadratic Bézier curve (one control point) to a point.</summary>
    QuadTo,

    /// <summary>Close the current sub-path.</summary>
    Close
}

/// <summary>A single drawing operation within a <see cref="PathData"/>.</summary>
public readonly record struct PathOperation(
    PathCommand Command,
    Point? Point = null,
    Point? Control1 = null,
    Point? Control2 = null);

/// <summary>
/// A rendering-neutral description of a path as an ordered list of move/line/curve/close operations.
/// This is the seam that lets path generators (Phase 4) stay UI-agnostic: the Avalonia layer turns a
/// <see cref="PathData"/> into a <c>StreamGeometry</c>, with no SVG path strings involved
/// (see <c>memory/01-decisions/ADR-0002-core-strategy.md</c>).
/// </summary>
public sealed class PathData
{
    private readonly List<PathOperation> _operations;

    /// <summary>Creates an empty path.</summary>
    public PathData() => _operations = new List<PathOperation>();

    /// <summary>Creates a path from existing operations.</summary>
    public PathData(IEnumerable<PathOperation> operations) => _operations = new List<PathOperation>(operations);

    /// <summary>The ordered operations that make up the path.</summary>
    public IReadOnlyList<PathOperation> Operations => _operations;

    /// <summary>Begins a new sub-path at <paramref name="point"/>.</summary>
    public PathData MoveTo(Point point)
    {
        _operations.Add(new PathOperation(PathCommand.MoveTo, point));
        return this;
    }

    /// <summary>Adds a straight line to <paramref name="point"/>.</summary>
    public PathData LineTo(Point point)
    {
        _operations.Add(new PathOperation(PathCommand.LineTo, point));
        return this;
    }

    /// <summary>Adds a cubic Bézier curve with two control points ending at <paramref name="end"/>.</summary>
    public PathData CubicTo(Point control1, Point control2, Point end)
    {
        _operations.Add(new PathOperation(PathCommand.CubicTo, end, control1, control2));
        return this;
    }

    /// <summary>Adds a quadratic Bézier curve with one control point ending at <paramref name="end"/>.</summary>
    public PathData QuadTo(Point control, Point end)
    {
        _operations.Add(new PathOperation(PathCommand.QuadTo, end, control));
        return this;
    }

    /// <summary>Closes the current sub-path.</summary>
    public PathData Close()
    {
        _operations.Add(new PathOperation(PathCommand.Close));
        return this;
    }

    /// <summary>
    /// A conservative bounding box over every referenced point (including control points). Returns
    /// <see cref="Rectangle.Zero"/> for an empty path.
    /// </summary>
    public Rectangle GetBBox()
    {
        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;
        var any = false;

        foreach (var op in _operations)
        {
            foreach (var p in new[] { op.Point, op.Control1, op.Control2 })
            {
                if (p is null)
                    continue;

                any = true;
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }
        }

        return any ? new Rectangle(minX, minY, maxX, maxY) : Rectangle.Zero;
    }

    /// <summary>
    /// The shortest distance from <paramref name="point"/> to the drawn path, flattening Bézier curves into
    /// <paramref name="curveSamples"/> straight segments each. Backend-independent (pure math), so it drives
    /// link hit-testing without depending on any rendering surface. Returns
    /// <see cref="double.PositiveInfinity"/> for an empty path.
    /// </summary>
    public double DistanceTo(Point point, int curveSamples = 16)
    {
        var best = double.PositiveInfinity;
        Point? subStart = null; // first point of the current sub-path (for Close)
        Point? current = null;

        foreach (var op in _operations)
        {
            switch (op.Command)
            {
                case PathCommand.MoveTo when op.Point is { } moveTo:
                    current = moveTo;
                    subStart = moveTo;
                    break;

                case PathCommand.LineTo when current is { } from && op.Point is { } to:
                    best = Math.Min(best, SegmentDistance(point, from, to));
                    current = to;
                    break;

                case PathCommand.CubicTo
                    when current is { } p0 && op.Control1 is { } c1 && op.Control2 is { } c2 && op.Point is { } p1:
                    best = Math.Min(best, CurveDistance(point, p0, c1, c2, p1, curveSamples));
                    current = p1;
                    break;

                case PathCommand.QuadTo when current is { } q0 && op.Control1 is { } qc && op.Point is { } q1:
                    // Promote the quadratic to a cubic so it reuses the same sampler.
                    best = Math.Min(best, CurveDistance(point, q0, Lerp(q0, qc, 2.0 / 3.0), Lerp(q1, qc, 2.0 / 3.0), q1, curveSamples));
                    current = q1;
                    break;

                case PathCommand.Close when current is { } c && subStart is { } s:
                    best = Math.Min(best, SegmentDistance(point, c, s));
                    current = s;
                    break;
            }
        }

        return best;
    }

    private static double CurveDistance(Point p, Point p0, Point c1, Point c2, Point p1, int samples)
    {
        var best = double.PositiveInfinity;
        var prev = p0;
        for (var i = 1; i <= samples; i++)
        {
            var sample = CubicSample(p0, c1, c2, p1, (double)i / samples);
            best = Math.Min(best, SegmentDistance(p, prev, sample));
            prev = sample;
        }

        return best;
    }

    private static Point Lerp(Point a, Point b, double t) => new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

    private static double SegmentDistance(Point p, Point a, Point b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var lengthSq = dx * dx + dy * dy;

        double t;
        if (lengthSq <= double.Epsilon)
            t = 0; // degenerate segment: a == b
        else
        {
            t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSq;
            if (t < 0) t = 0;
            else if (t > 1) t = 1;
        }

        var ex = p.X - (a.X + t * dx);
        var ey = p.Y - (a.Y + t * dy);
        return Math.Sqrt(ex * ex + ey * ey);
    }

    /// <summary>The total drawn length of the path (Béziers flattened into <paramref name="curveSamples"/> segments each).</summary>
    public double Length(int curveSamples = 16)
    {
        var points = Flatten(curveSamples);
        var total = 0d;
        for (var i = 1; i < points.Count; i++)
            total += Distance(points[i - 1], points[i]);
        return total;
    }

    /// <summary>
    /// The point at arc-length <paramref name="distance"/> along the drawn path (clamped to the path ends), or
    /// null for an empty path. Used to place link labels.
    /// </summary>
    public Point? PointAtDistance(double distance, int curveSamples = 16)
    {
        var points = Flatten(curveSamples);
        if (points.Count == 0)
            return null;
        if (points.Count == 1 || distance <= 0)
            return points[0];

        var accumulated = 0d;
        for (var i = 1; i < points.Count; i++)
        {
            var segment = Distance(points[i - 1], points[i]);
            if (accumulated + segment >= distance)
            {
                var t = segment <= double.Epsilon ? 0 : (distance - accumulated) / segment;
                return Lerp(points[i - 1], points[i], t);
            }

            accumulated += segment;
        }

        return points[points.Count - 1];
    }

    /// <summary>Flattens the drawn path into a connected poly-line (one sub-path for the typical link).</summary>
    private List<Point> Flatten(int curveSamples)
    {
        var points = new List<Point>();
        Point? current = null;

        foreach (var op in _operations)
        {
            switch (op.Command)
            {
                case PathCommand.MoveTo when op.Point is { } moveTo:
                    current = moveTo;
                    points.Add(moveTo);
                    break;

                case PathCommand.LineTo when op.Point is { } to:
                    points.Add(to);
                    current = to;
                    break;

                case PathCommand.CubicTo
                    when current is { } p0 && op.Control1 is { } c1 && op.Control2 is { } c2 && op.Point is { } p1:
                    for (var i = 1; i <= curveSamples; i++)
                        points.Add(CubicSample(p0, c1, c2, p1, (double)i / curveSamples));
                    current = p1;
                    break;

                case PathCommand.QuadTo when current is { } q0 && op.Control1 is { } qc && op.Point is { } q1:
                    var k1 = Lerp(q0, qc, 2.0 / 3.0);
                    var k2 = Lerp(q1, qc, 2.0 / 3.0);
                    for (var i = 1; i <= curveSamples; i++)
                        points.Add(CubicSample(q0, k1, k2, q1, (double)i / curveSamples));
                    current = q1;
                    break;

                case PathCommand.Close when points.Count > 0:
                    points.Add(points[0]);
                    current = points[0];
                    break;
            }
        }

        return points;
    }

    private static Point CubicSample(Point p0, Point c1, Point c2, Point p1, double t)
    {
        var u = 1 - t;
        return new Point(
            u * u * u * p0.X + 3 * u * u * t * c1.X + 3 * u * t * t * c2.X + t * t * t * p1.X,
            u * u * u * p0.Y + 3 * u * u * t * c1.Y + 3 * u * t * t * c2.Y + t * t * t * p1.Y);
    }

    private static double Distance(Point a, Point b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
