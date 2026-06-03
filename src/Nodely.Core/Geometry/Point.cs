using System;

namespace Nodely.Geometry;

/// <summary>An immutable point in diagram space.</summary>
public record Point
{
    /// <summary>The origin (0, 0).</summary>
    public static Point Zero { get; } = new(0, 0);

    /// <summary>Creates a point.</summary>
    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>The X coordinate.</summary>
    public double X { get; init; }

    /// <summary>The Y coordinate.</summary>
    public double Y { get; init; }

    /// <summary>The distance from the origin to this point.</summary>
    public double Length => Math.Sqrt(Dot(this));

    /// <summary>Dot product with <paramref name="other"/>.</summary>
    public double Dot(Point other) => X * other.X + Y * other.Y;

    /// <summary>Linear interpolation toward <paramref name="other"/> by <paramref name="t"/> in [0, 1].</summary>
    public Point Lerp(Point other, double t)
        => new(X * (1.0 - t) + other.X * t, Y * (1.0 - t) + other.Y * t);

    /// <summary>Adds a scalar to both components.</summary>
    public Point Add(double value) => new(X + value, Y + value);

    /// <summary>Adds the given offsets.</summary>
    public Point Add(double x, double y) => new(X + x, Y + y);

    /// <summary>Subtracts a scalar from both components.</summary>
    public Point Subtract(double value) => new(X - value, Y - value);

    /// <summary>Subtracts the given offsets.</summary>
    public Point Subtract(double x, double y) => new(X - x, Y - y);

    /// <summary>Subtracts another point (vector difference).</summary>
    public Point Subtract(Point other) => new(X - other.X, Y - other.Y);

    /// <summary>Component-wise division by <paramref name="other"/>.</summary>
    public Point Divide(Point other) => new(X / other.X, Y / other.Y);

    /// <summary>Multiplies both components by a scalar.</summary>
    public Point Multiply(double value) => new(X * value, Y * value);

    /// <summary>Returns this vector scaled to unit length.</summary>
    public Point Normalize()
    {
        var length = Length;
        return new Point(X / length, Y / length);
    }

    /// <summary>Euclidean distance to <paramref name="other"/>.</summary>
    public double DistanceTo(Point other) => Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));

    /// <summary>Euclidean distance to the given coordinates.</summary>
    public double DistanceTo(double x, double y) => Math.Sqrt(Math.Pow(X - x, 2) + Math.Pow(Y - y, 2));

    /// <summary>Moves this point a further <paramref name="dist"/> along the ray from <paramref name="from"/>.</summary>
    public Point MoveAlongLine(Point from, double dist)
    {
        var x = X - from.X;
        var y = Y - from.Y;
        var angle = Math.Atan2(y, x);
        var xOffset = Math.Cos(angle) * dist;
        var yOffset = Math.Sin(angle) * dist;
        return new Point(X + xOffset, Y + yOffset);
    }

    /// <summary>Vector subtraction.</summary>
    public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);

    /// <summary>Vector addition.</summary>
    public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);

    /// <summary>Deconstructs into (x, y).</summary>
    public void Deconstruct(out double x, out double y)
    {
        x = X;
        y = Y;
    }
}
