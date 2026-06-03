using System;
using System.Linq;
using Nodely.Geometry;
using Shouldly;
using Xunit;

namespace Nodely.Core.Tests;

public class GeometryTests
{
    [Fact]
    public void Point_dot_and_length()
    {
        new Point(3, 4).Length.ShouldBe(5.0, 1e-9);
        new Point(1, 2).Dot(new Point(3, 4)).ShouldBe(11.0);
    }

    [Fact]
    public void PathData_distance_to_a_line_segment()
    {
        var path = new PathData().MoveTo(new Point(0, 0)).LineTo(new Point(100, 0));

        path.DistanceTo(new Point(50, 0)).ShouldBe(0, 1e-9);     // on the line
        path.DistanceTo(new Point(50, 7)).ShouldBe(7, 1e-9);     // 7px above the middle
        path.DistanceTo(new Point(-10, 0)).ShouldBe(10, 1e-9);   // off the end -> distance to the endpoint
        new PathData().DistanceTo(new Point(0, 0)).ShouldBe(double.PositiveInfinity); // empty path
    }

    [Fact]
    public void PathData_length_and_point_at_distance_along_a_polyline()
    {
        var path = new PathData().MoveTo(new Point(0, 0)).LineTo(new Point(100, 0)).LineTo(new Point(100, 40));

        path.Length().ShouldBe(140, 1e-6);
        path.PointAtDistance(0)!.ShouldBe(new Point(0, 0));
        path.PointAtDistance(50)!.X.ShouldBe(50, 1e-6);
        path.PointAtDistance(50)!.Y.ShouldBe(0, 1e-6);
        path.PointAtDistance(120)!.X.ShouldBe(100, 1e-6); // 100 across + 20 down the second segment
        path.PointAtDistance(120)!.Y.ShouldBe(20, 1e-6);
        path.PointAtDistance(999)!.ShouldBe(new Point(100, 40)); // clamped to the end
        new PathData().PointAtDistance(10).ShouldBeNull();       // empty path
    }

    [Fact]
    public void PathData_distance_to_a_cubic_curve_is_near_zero_on_the_curve()
    {
        // A symmetric cubic bulging up to y=-50 at its midpoint; sample the exact midpoint (t=0.5).
        var path = new PathData().MoveTo(new Point(0, 0)).CubicTo(new Point(0, -100), new Point(100, -100), new Point(100, 0));
        var midpoint = new Point(50, -75); // B(0.5) for these control points

        path.DistanceTo(midpoint).ShouldBeLessThan(1.0);
        path.DistanceTo(new Point(50, 40)).ShouldBeGreaterThan(40); // well below the curve
    }

    [Fact]
    public void Point_lerp_midpoint()
    {
        var p = new Point(0, 0).Lerp(new Point(10, 20), 0.5);
        p.X.ShouldBe(5.0, 1e-9);
        p.Y.ShouldBe(10.0, 1e-9);
    }

    [Fact]
    public void Point_operators()
    {
        (new Point(5, 7) - new Point(2, 3)).ShouldBe(new Point(3, 4));
        (new Point(1, 1) + new Point(2, 3)).ShouldBe(new Point(3, 4));
    }

    [Fact]
    public void Point_normalize_to_unit_length()
    {
        new Point(0, 5).Normalize().ShouldBe(new Point(0, 1));
    }

    [Fact]
    public void Point_distance_and_move_along_line()
    {
        new Point(0, 0).DistanceTo(new Point(3, 4)).ShouldBe(5.0, 1e-9);

        var moved = new Point(10, 0).MoveAlongLine(new Point(0, 0), 5);
        moved.X.ShouldBe(15.0, 1e-9);
        moved.Y.ShouldBe(0.0, 1e-9);
    }

    [Fact]
    public void Rectangle_contains_point()
    {
        var r = new Rectangle(0, 0, 10, 10);
        r.Width.ShouldBe(10);
        r.Height.ShouldBe(10);
        r.ContainsPoint(new Point(5, 5)).ShouldBeTrue();
        r.ContainsPoint(new Point(11, 5)).ShouldBeFalse();
    }

    [Fact]
    public void Rectangle_intersect_overlap_union_inflate()
    {
        var a = new Rectangle(0, 0, 10, 10);
        var b = new Rectangle(5, 5, 15, 15);
        var far = new Rectangle(100, 100, 110, 110);

        a.Intersects(b).ShouldBeTrue();
        a.Overlap(b).ShouldBeTrue();
        a.Intersects(far).ShouldBeFalse();

        var u = a.Union(b);
        (u.Left, u.Top, u.Right, u.Bottom).ShouldBe((0, 0, 15, 15));

        var inflated = a.Inflate(2, 3);
        (inflated.Left, inflated.Top, inflated.Right, inflated.Bottom).ShouldBe((-2, -3, 12, 13));
    }

    [Fact]
    public void Rectangle_corners()
    {
        var r = new Rectangle(0, 0, 10, 20);
        r.Center.ShouldBe(new Point(5, 10));
        r.NorthWest.ShouldBe(new Point(0, 0));
        r.SouthEast.ShouldBe(new Point(10, 20));
        r.East.ShouldBe(new Point(10, 10));
    }

    [Fact]
    public void Rectangle_intersections_with_line()
    {
        var r = new Rectangle(0, 0, 10, 10);
        var line = new Line(new Point(-5, 5), new Point(15, 5));

        var pts = r.GetIntersectionsWithLine(line).ToList();

        pts.Count.ShouldBe(2);
        pts.ShouldContain(new Point(0, 5));
        pts.ShouldContain(new Point(10, 5));
    }

    [Fact]
    public void Rectangle_point_at_angle_zero_is_east()
    {
        var p = new Rectangle(0, 0, 10, 10).GetPointAtAngle(0);
        p.ShouldNotBeNull();
        p!.X.ShouldBe(10, 1e-9);
        p.Y.ShouldBe(5, 1e-9);
    }

    [Fact]
    public void Line_intersection_crossing_and_parallel()
    {
        var a = new Line(new Point(0, 0), new Point(10, 10));
        var b = new Line(new Point(0, 10), new Point(10, 0));
        a.GetIntersection(b).ShouldBe(new Point(5, 5));

        var h1 = new Line(new Point(0, 0), new Point(10, 0));
        var h2 = new Line(new Point(0, 5), new Point(10, 5));
        h1.GetIntersection(h2).ShouldBeNull();
    }

    [Fact]
    public void Line_intersection_outside_segment_is_null()
    {
        var a = new Line(new Point(0, 0), new Point(1, 1));
        var b = new Line(new Point(5, 0), new Point(5, 10)); // only crosses the extension
        a.GetIntersection(b).ShouldBeNull();
    }

    [Fact]
    public void Ellipse_intersections_with_line()
    {
        var circle = new Ellipse(0, 0, 10, 10);
        var line = new Line(new Point(-20, 0), new Point(20, 0));

        var pts = circle.GetIntersectionsWithLine(line).ToList();

        pts.Count.ShouldBe(2);
        pts.ShouldContain(new Point(-10, 0));
        pts.ShouldContain(new Point(10, 0));
    }

    [Fact]
    public void Shapes_factories_from_position_and_size()
    {
        Shapes.Rectangle(new Point(0, 0), new Size(10, 10)).ShouldBeOfType<Rectangle>();

        var circle = Shapes.Circle(new Point(0, 0), new Size(10, 10)).ShouldBeOfType<Ellipse>();
        (circle.Cx, circle.Cy, circle.Rx, circle.Ry).ShouldBe((5, 5, 5, 5));
    }

    [Fact]
    public void BezierSpline_two_knots_is_straight_line()
    {
        var knots = new[] { new Point(0, 0), new Point(30, 0) };

        BezierSpline.GetCurveControlPoints(knots, out var first, out var second);

        first.Length.ShouldBe(1);
        second.Length.ShouldBe(1);
        first[0].X.ShouldBe(10, 1e-9);  // 3P1 = 2P0 + P3
        second[0].X.ShouldBe(20, 1e-9); // P2 = 2P1 - P0
    }

    [Fact]
    public void BezierSpline_throws_on_single_knot()
    {
        Should.Throw<ArgumentException>(() =>
            BezierSpline.GetCurveControlPoints(new[] { new Point(0, 0) }, out _, out _));
    }
}
