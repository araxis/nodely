using System;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.PathGenerators;
using Nodely.Routers;
using Shouldly;
using Xunit;

namespace Nodely.Core.Tests;

public class RoutingTests
{
    private static PortModel InitPort(NodeModel node, PortAlignment alignment, Point position, Size size)
    {
        var port = node.AddPort(new PortModel(node, alignment, position, size));
        port.Initialized = true;
        return port;
    }

    [Fact]
    public void Normal_router_returns_the_links_vertices()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var n1 = d.Nodes.Add(new NodeModel(new Point(0, 0))); n1.Size = new Size(10, 10);
        var n2 = d.Nodes.Add(new NodeModel(new Point(200, 0))); n2.Size = new Size(10, 10);
        var link = d.Links.Add(new LinkModel(n1, n2));
        link.AddVertex(new Point(50, 50));

        var route = new NormalRouter().GetRoute(d, link);

        route.Length.ShouldBe(1);
        route[0].ShouldBe(new Point(50, 50));
    }

    [Fact]
    public void Straight_generator_emits_move_then_line()
    {
        var d = new NodelyDiagram();
        var link = new LinkModel(new PositionAnchor(new Point(0, 0)), new PositionAnchor(new Point(100, 0)));

        var result = new StraightPathGenerator().GetResult(d, link, Array.Empty<Point>(), new Point(0, 0), new Point(100, 0));

        var ops = result.FullPath.Operations;
        ops.Count.ShouldBe(2);
        ops[0].Command.ShouldBe(PathCommand.MoveTo);
        ops[0].Point!.ShouldBe(new Point(0, 0));
        ops[1].Command.ShouldBe(PathCommand.LineTo);
        ops[1].Point!.ShouldBe(new Point(100, 0));
    }

    [Fact]
    public void Smooth_generator_emits_move_then_cubic()
    {
        var d = new NodelyDiagram();
        var link = new LinkModel(new PositionAnchor(new Point(0, 0)), new PositionAnchor(new Point(100, 0)));

        var result = new SmoothPathGenerator().GetResult(d, link, Array.Empty<Point>(), new Point(0, 0), new Point(100, 0));

        var ops = result.FullPath.Operations;
        ops.Count.ShouldBe(2);
        ops[0].Command.ShouldBe(PathCommand.MoveTo);
        ops[1].Command.ShouldBe(PathCommand.CubicTo);
        ops[1].Point!.ShouldBe(new Point(100, 0));
    }

    [Fact]
    public void Target_marker_shortens_route_and_reports_angle()
    {
        var d = new NodelyDiagram();
        var link = new LinkModel(new PositionAnchor(new Point(0, 0)), new PositionAnchor(new Point(100, 0)))
        {
            TargetMarker = LinkMarker.Arrow // width 10
        };

        var result = new StraightPathGenerator().GetResult(d, link, Array.Empty<Point>(), new Point(0, 0), new Point(100, 0));

        result.TargetMarkerAngle.ShouldNotBeNull();
        result.TargetMarkerAngle!.Value.ShouldBe(0, 1e-9); // pointing along +X
        result.TargetMarkerPosition.ShouldBe(new Point(90, 0)); // shortened by the 10px marker width
    }

    [Fact]
    public void Link_added_to_diagram_generates_a_path_with_defaults()
    {
        var d = new NodelyDiagram();
        var n1 = d.Nodes.Add(new NodeModel(new Point(0, 0))); n1.Size = new Size(40, 40);
        var n2 = d.Nodes.Add(new NodeModel(new Point(200, 0))); n2.Size = new Size(40, 40);
        var p1 = InitPort(n1, PortAlignment.Right, new Point(40, 16), new Size(8, 8));
        var p2 = InitPort(n2, PortAlignment.Left, new Point(200, 16), new Size(8, 8));

        var link = d.Links.Add(new LinkModel(p1, p2));

        link.Route.ShouldNotBeNull();
        link.PathGeneratorResult.ShouldNotBeNull();
        link.PathGeneratorResult!.FullPath.Operations.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Orthogonal_router_produces_axis_aligned_waypoints()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var n1 = d.Nodes.Add(new NodeModel(new Point(0, 0))); n1.Size = new Size(40, 40);
        var n2 = d.Nodes.Add(new NodeModel(new Point(200, 100))); n2.Size = new Size(40, 40);
        var p1 = InitPort(n1, PortAlignment.Right, new Point(40, 16), new Size(8, 8));
        var p2 = InitPort(n2, PortAlignment.Left, new Point(200, 116), new Size(8, 8));
        var link = d.Links.Add(new LinkModel(p1, p2));

        var route = new OrthogonalRouter().GetRoute(d, link);

        route.Length.ShouldBeGreaterThan(0);
        for (var i = 1; i < route.Length; i++)
        {
            var prev = route[i - 1];
            var cur = route[i];
            var axisAligned = Math.Abs(prev.X - cur.X) < 1e-6 || Math.Abs(prev.Y - cur.Y) < 1e-6;
            axisAligned.ShouldBeTrue($"segment {i} is not axis-aligned: {prev} -> {cur}");
        }
    }
}
