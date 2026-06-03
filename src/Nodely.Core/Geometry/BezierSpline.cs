using System;

namespace Nodely.Geometry;

/// <summary>
/// Computes Bezier control points for a smooth spline through a set of knot points.
/// </summary>
/// <remarks>
/// Algorithm by Oleg V. Polikarpotchkin / Peter Lee — "Draw a smooth curve through a set of 2D points
/// with Bezier primitives" (https://www.codeproject.com/KB/graphics/BezierSpline.aspx). Control points
/// are chosen so the resulting spline has two continuous derivatives at the knot points. Ported here as
/// first-party Nodely code; see <c>memory/01-decisions/ADR-0002-core-strategy.md</c>.
/// </remarks>
public static class BezierSpline
{
    /// <summary>
    /// Computes the open-ended Bezier spline control points for the given <paramref name="knots"/>.
    /// </summary>
    /// <param name="knots">The knot points the spline must pass through (at least two).</param>
    /// <param name="firstControlPoints">Output: the first control point of each of the knots.Length - 1 segments.</param>
    /// <param name="secondControlPoints">Output: the second control point of each segment.</param>
    /// <exception cref="ArgumentNullException"><paramref name="knots"/> is null.</exception>
    /// <exception cref="ArgumentException">Fewer than two knots were supplied.</exception>
    public static void GetCurveControlPoints(Point[] knots, out Point[] firstControlPoints, out Point[] secondControlPoints)
    {
        if (knots == null)
            throw new ArgumentNullException(nameof(knots));

        int n = knots.Length - 1;
        if (n < 1)
            throw new ArgumentException("At least two knot points are required.", nameof(knots));

        if (n == 1)
        {
            // Special case: a single segment is a straight line.
            firstControlPoints = new Point[1];
            // 3P1 = 2P0 + P3
            firstControlPoints[0] = new Point((2 * knots[0].X + knots[1].X) / 3, (2 * knots[0].Y + knots[1].Y) / 3);

            secondControlPoints = new Point[1];
            // P2 = 2P1 - P0
            secondControlPoints[0] = new Point(2 * firstControlPoints[0].X - knots[0].X, 2 * firstControlPoints[0].Y - knots[0].Y);
            return;
        }

        // Right-hand-side vector.
        double[] rhs = new double[n];

        // Set right-hand-side X values.
        for (int i = 1; i < n - 1; ++i)
            rhs[i] = 4 * knots[i].X + 2 * knots[i + 1].X;
        rhs[0] = knots[0].X + 2 * knots[1].X;
        rhs[n - 1] = (8 * knots[n - 1].X + knots[n].X) / 2.0;
        double[] x = GetFirstControlPoints(rhs);

        // Set right-hand-side Y values.
        for (int i = 1; i < n - 1; ++i)
            rhs[i] = 4 * knots[i].Y + 2 * knots[i + 1].Y;
        rhs[0] = knots[0].Y + 2 * knots[1].Y;
        rhs[n - 1] = (8 * knots[n - 1].Y + knots[n].Y) / 2.0;
        double[] y = GetFirstControlPoints(rhs);

        firstControlPoints = new Point[n];
        secondControlPoints = new Point[n];
        for (int i = 0; i < n; ++i)
        {
            firstControlPoints[i] = new Point(x[i], y[i]);
            if (i < n - 1)
                secondControlPoints[i] = new Point(2 * knots[i + 1].X - x[i + 1], 2 * knots[i + 1].Y - y[i + 1]);
            else
                secondControlPoints[i] = new Point((knots[n].X + x[n - 1]) / 2, (knots[n].Y + y[n - 1]) / 2);
        }
    }

    /// <summary>Solves the tridiagonal system for one coordinate of the first control points.</summary>
    private static double[] GetFirstControlPoints(double[] rhs)
    {
        int n = rhs.Length;
        double[] x = new double[n];   // Solution vector.
        double[] tmp = new double[n]; // Temp workspace.

        double b = 2.0;
        x[0] = rhs[0] / b;
        for (int i = 1; i < n; i++) // Decomposition and forward substitution.
        {
            tmp[i] = 1 / b;
            b = (i < n - 1 ? 4.0 : 3.5) - tmp[i];
            x[i] = (rhs[i] - x[i - 1]) / b;
        }

        for (int i = 1; i < n; i++)
            x[n - i - 1] -= tmp[n - i] * x[n - i]; // Back-substitution.

        return x;
    }
}
