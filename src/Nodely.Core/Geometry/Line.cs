namespace Nodely.Geometry;

/// <summary>A finite line segment between two points.</summary>
public class Line
{
    /// <summary>Creates a segment from <paramref name="start"/> to <paramref name="end"/>.</summary>
    public Line(Point start, Point end)
    {
        Start = start;
        End = end;
    }

    /// <summary>The start point.</summary>
    public Point Start { get; }

    /// <summary>The end point.</summary>
    public Point End { get; }

    /// <summary>
    /// Returns the intersection point with <paramref name="line"/> if the two <em>segments</em> cross,
    /// otherwise null (parallel, collinear, or crossing only when extended).
    /// </summary>
    public Point? GetIntersection(Line line)
    {
        var pt1Dir = new Point(End.X - Start.X, End.Y - Start.Y);
        var pt2Dir = new Point(line.End.X - line.Start.X, line.End.Y - line.Start.Y);
        var det = (pt1Dir.X * pt2Dir.Y) - (pt1Dir.Y * pt2Dir.X);
        var deltaPt = new Point(line.Start.X - Start.X, line.Start.Y - Start.Y);
        var alpha = (deltaPt.X * pt2Dir.Y) - (deltaPt.Y * pt2Dir.X);
        var beta = (deltaPt.X * pt1Dir.Y) - (deltaPt.Y * pt1Dir.X);

        if (det == 0 || alpha * det < 0 || beta * det < 0)
            return null;

        if (det > 0)
        {
            if (alpha > det || beta > det)
                return null;
        }
        else
        {
            if (alpha < det || beta < det)
                return null;
        }

        return new Point(Start.X + (alpha * pt1Dir.X / det), Start.Y + (alpha * pt1Dir.Y / det));
    }

    /// <inheritdoc />
    public override string ToString() => $"Line from {Start} to {End}";
}
