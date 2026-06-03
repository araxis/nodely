using System;
using System.Collections.Generic;
using System.Linq;

namespace Nodely.Geometry;

/// <summary>An axis-aligned rectangle in diagram space.</summary>
public class Rectangle : IShape
{
    /// <summary>The empty rectangle at the origin.</summary>
    public static Rectangle Zero { get; } = new(0, 0, 0, 0);

    /// <summary>The width.</summary>
    public double Width { get; }

    /// <summary>The height.</summary>
    public double Height { get; }

    /// <summary>The top (minimum Y) edge.</summary>
    public double Top { get; }

    /// <summary>The right (maximum X) edge.</summary>
    public double Right { get; }

    /// <summary>The bottom (maximum Y) edge.</summary>
    public double Bottom { get; }

    /// <summary>The left (minimum X) edge.</summary>
    public double Left { get; }

    /// <summary>Creates a rectangle from its four edges.</summary>
    public Rectangle(double left, double top, double right, double bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
        Width = Math.Abs(Left - Right);
        Height = Math.Abs(Top - Bottom);
    }

    /// <summary>Creates a rectangle from a top-left position and a size.</summary>
    public Rectangle(Point position, Size size)
    {
        Left = position.X;
        Top = position.Y;
        Right = Left + size.Width;
        Bottom = Top + size.Height;
        Width = size.Width;
        Height = size.Height;
    }

    /// <summary>True if this rectangle overlaps <paramref name="r"/> (touching edges excluded).</summary>
    public bool Overlap(Rectangle r)
        => Left < r.Right && Right > r.Left && Top < r.Bottom && Bottom > r.Top;

    /// <summary>True if this rectangle intersects <paramref name="r"/>.</summary>
    public bool Intersects(Rectangle r)
    {
        var thisX = Left;
        var thisY = Top;
        var thisW = Width;
        var thisH = Height;
        var rectX = r.Left;
        var rectY = r.Top;
        var rectW = r.Width;
        var rectH = r.Height;
        return rectX < thisX + thisW && thisX < rectX + rectW && rectY < thisY + thisH && thisY < rectY + rectH;
    }

    /// <summary>Returns a copy grown by the given amounts on each axis.</summary>
    public Rectangle Inflate(double horizontal, double vertical)
        => new(Left - horizontal, Top - vertical, Right + horizontal, Bottom + vertical);

    /// <summary>Returns the smallest rectangle containing both this and <paramref name="r"/>.</summary>
    public Rectangle Union(Rectangle r)
    {
        var x1 = Math.Min(Left, r.Left);
        var x2 = Math.Max(Left + Width, r.Left + r.Width);
        var y1 = Math.Min(Top, r.Top);
        var y2 = Math.Max(Top + Height, r.Top + r.Height);
        return new(x1, y1, x2, y2);
    }

    /// <summary>True if the rectangle contains <paramref name="point"/> (edges inclusive).</summary>
    public bool ContainsPoint(Point point) => ContainsPoint(point.X, point.Y);

    /// <summary>True if the rectangle contains the given coordinates (edges inclusive).</summary>
    public bool ContainsPoint(double x, double y)
        => x >= Left && x <= Right && y >= Top && y <= Bottom;

    /// <summary>Returns the points where the rectangle's borders cross <paramref name="line"/>.</summary>
    public IEnumerable<Point> GetIntersectionsWithLine(Line line)
    {
        var borders = new[]
        {
            new Line(NorthWest, NorthEast),
            new Line(NorthEast, SouthEast),
            new Line(SouthWest, SouthEast),
            new Line(NorthWest, SouthWest)
        };

        for (var i = 0; i < borders.Length; i++)
        {
            var intersectionPt = borders[i].GetIntersection(line);
            if (intersectionPt != null)
                yield return intersectionPt;
        }
    }

    /// <summary>Returns the border point along the ray from the center at angle <paramref name="a"/> (degrees).</summary>
    public Point? GetPointAtAngle(double a)
    {
        var vx = Math.Cos(a * Math.PI / 180);
        var vy = Math.Sin(a * Math.PI / 180);
        var px = Left + Width / 2;
        var py = Top + Height / 2;
        double? t1 = (Left - px) / vx; // left
        double? t2 = (Right - px) / vx; // right
        double? t3 = (Top - py) / vy; // top
        double? t4 = (Bottom - py) / vy; // bottom
        var t = new[] { t1, t2, t3, t4 }
            .Where(n => n.HasValue && !double.IsNaN(n.Value) && !double.IsInfinity(n.Value) && n.Value > 0)
            .DefaultIfEmpty(null)
            .Min();
        if (t == null) return null;

        var x = px + t.Value * vx;
        var y = py + t.Value * vy;
        return new Point(x, y);
    }

    /// <summary>The center point.</summary>
    public Point Center => new(Left + Width / 2, Top + Height / 2);

    /// <summary>The top-right corner.</summary>
    public Point NorthEast => new(Right, Top);

    /// <summary>The bottom-right corner.</summary>
    public Point SouthEast => new(Right, Bottom);

    /// <summary>The bottom-left corner.</summary>
    public Point SouthWest => new(Left, Bottom);

    /// <summary>The top-left corner.</summary>
    public Point NorthWest => new(Left, Top);

    /// <summary>The mid-point of the right edge.</summary>
    public Point East => new(Right, Top + Height / 2);

    /// <summary>The mid-point of the top edge.</summary>
    public Point North => new(Left + Width / 2, Top);

    /// <summary>The mid-point of the bottom edge.</summary>
    public Point South => new(Left + Width / 2, Bottom);

    /// <summary>The mid-point of the left edge.</summary>
    public Point West => new(Left, Top + Height / 2);

    /// <summary>Value equality across all edges and size.</summary>
    public bool Equals(Rectangle? other)
        => other != null && Left == other.Left && Right == other.Right && Top == other.Top &&
           Bottom == other.Bottom && Width == other.Width && Height == other.Height;

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as Rectangle);

    /// <inheritdoc />
    public override int GetHashCode() => (Left, Top, Right, Bottom).GetHashCode();

    /// <inheritdoc />
    public override string ToString()
        => $"Rectangle(width={Width}, height={Height}, top={Top}, right={Right}, bottom={Bottom}, left={Left})";
}
