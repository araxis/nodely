using System;
using System.Collections.Generic;

namespace Nodely.Geometry;

/// <summary>An axis-aligned ellipse in diagram space.</summary>
public class Ellipse : IShape
{
    /// <summary>Creates an ellipse from its center and radii.</summary>
    public Ellipse(double cx, double cy, double rx, double ry)
    {
        Cx = cx;
        Cy = cy;
        Rx = rx;
        Ry = ry;
    }

    /// <summary>Center X.</summary>
    public double Cx { get; }

    /// <summary>Center Y.</summary>
    public double Cy { get; }

    /// <summary>Horizontal radius.</summary>
    public double Rx { get; }

    /// <summary>Vertical radius.</summary>
    public double Ry { get; }

    /// <summary>Returns the points where the ellipse boundary crosses the segment <paramref name="line"/>.</summary>
    public IEnumerable<Point> GetIntersectionsWithLine(Line line)
    {
        var a1 = line.Start;
        var a2 = line.End;
        var dir = new Point(line.End.X - line.Start.X, line.End.Y - line.Start.Y);
        var diff = a1.Subtract(Cx, Cy);
        var mDir = new Point(dir.X / (Rx * Rx), dir.Y / (Ry * Ry));
        var mDiff = new Point(diff.X / (Rx * Rx), diff.Y / (Ry * Ry));

        var a = dir.Dot(mDir);
        var b = dir.Dot(mDiff);
        var c = diff.Dot(mDiff) - 1.0;
        var d = b * b - a * c;

        if (d > 0)
        {
            var root = Math.Sqrt(d);
            var ta = (-b - root) / a;
            var tb = (-b + root) / a;

            if ((ta >= 0 && 1 >= ta) || (tb >= 0 && 1 >= tb))
            {
                if (0 <= ta && ta <= 1)
                    yield return a1.Lerp(a2, ta);

                if (0 <= tb && tb <= 1)
                    yield return a1.Lerp(a2, tb);
            }
        }
        else
        {
            var t = -b / a;
            if (0 <= t && t <= 1)
            {
                yield return a1.Lerp(a2, t);
            }
        }
    }

    /// <summary>Returns the boundary point at angle <paramref name="a"/> (degrees).</summary>
    public Point? GetPointAtAngle(double a)
    {
        var t = Math.Tan(a / 360 * Math.PI);
        var px = Rx * (1 - Math.Pow(t, 2)) / (1 + Math.Pow(t, 2));
        var py = Ry * 2 * t / (1 + Math.Pow(t, 2));
        return new Point(Cx + px, Cy + py);
    }
}
