using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Shouldly;
using Xunit;

namespace Nodely.Core.Tests;

public class LinkTests
{
    private static PortModel InitPort(NodeModel node, PortAlignment alignment, Point position, Size size)
    {
        var port = node.AddPort(new PortModel(node, alignment, position, size));
        port.Initialized = true;
        return port;
    }

    [Fact]
    public void Link_between_ports_is_attached_and_wires_links()
    {
        var d = new NodelyDiagram();
        var n1 = d.Nodes.Add(new NodeModel(new Point(0, 0)));
        var n2 = d.Nodes.Add(new NodeModel(new Point(100, 0)));
        n1.Size = new Size(20, 20);
        n2.Size = new Size(20, 20);
        var p1 = InitPort(n1, PortAlignment.Right, new Point(20, 6), new Size(8, 8));
        var p2 = InitPort(n2, PortAlignment.Left, new Point(96, 6), new Size(8, 8));

        var link = d.Links.Add(new LinkModel(p1, p2));

        link.IsAttached.ShouldBeTrue();
        p1.Links.ShouldContain(link);
        p2.Links.ShouldContain(link);
    }

    [Fact]
    public void Single_port_anchor_resolves_to_point_on_port_shape()
    {
        var node = new NodeModel(new Point(0, 0)) { Size = new Size(20, 20) };
        var port = InitPort(node, PortAlignment.Right, new Point(100, 100), new Size(8, 8));
        var other = new NodeModel(new Point(200, 100)) { Size = new Size(20, 20) };
        var otherPort = InitPort(other, PortAlignment.Left, new Point(200, 100), new Size(8, 8));

        var link = new LinkModel(port, otherPort);
        var pos = link.Source.GetPosition(link);

        // Port circle: center (104,104) radius 4; alignment Right -> angle 0 -> (108,104).
        pos.ShouldNotBeNull();
        pos!.X.ShouldBe(108, 1e-9);
        pos.Y.ShouldBe(104, 1e-9);
    }

    [Fact]
    public void Shape_intersection_anchor_resolves_to_node_boundary()
    {
        var n1 = new NodeModel(new Point(0, 0)) { Size = new Size(10, 10) };
        var n2 = new NodeModel(new Point(100, 0)) { Size = new Size(10, 10) };

        var link = new LinkModel(n1, n2);

        link.Source.GetPlainPosition().ShouldBe(new Point(5, 5));  // node center
        link.Source.GetPosition(link).ShouldBe(new Point(10, 5));  // right edge facing n2
    }

    [Fact]
    public void Position_anchor_makes_link_unattached()
    {
        var n1 = new NodeModel(new Point(0, 0)) { Size = new Size(10, 10) };

        var link = new LinkModel(new ShapeIntersectionAnchor(n1), new PositionAnchor(new Point(50, 50)));

        link.IsAttached.ShouldBeFalse();
        link.Target.GetPlainPosition().ShouldBe(new Point(50, 50));
    }

    [Fact]
    public void Default_target_marker_option_applies_and_resolves_a_marker_angle()
    {
        var d = new NodelyDiagram();
        d.Options.Links.DefaultTargetMarker = LinkMarker.Arrow;
        var n1 = d.Nodes.Add(new NodeModel(new Point(0, 0)) { Size = new Size(20, 20) });
        var n2 = d.Nodes.Add(new NodeModel(new Point(200, 0)) { Size = new Size(20, 20) });
        var p1 = InitPort(n1, PortAlignment.Right, new Point(20, 6), new Size(8, 8));
        var p2 = InitPort(n2, PortAlignment.Left, new Point(196, 6), new Size(8, 8));

        var link = d.Links.Add(new LinkModel(p1, p2));

        link.TargetMarker.ShouldBeNull();                       // nothing set per-link...
        link.EffectiveTargetMarker.ShouldBe(LinkMarker.Arrow);  // ...resolved from the diagram default
        link.PathGeneratorResult.ShouldNotBeNull();
        link.PathGeneratorResult!.TargetMarkerAngle.ShouldNotBeNull();
        link.PathGeneratorResult!.TargetMarkerPosition.ShouldNotBeNull();
    }

    [Fact]
    public void Per_link_marker_overrides_the_diagram_default()
    {
        var d = new NodelyDiagram();
        d.Options.Links.DefaultTargetMarker = LinkMarker.Arrow;
        var n1 = d.Nodes.Add(new NodeModel(new Point(0, 0)) { Size = new Size(20, 20) });
        var n2 = d.Nodes.Add(new NodeModel(new Point(200, 0)) { Size = new Size(20, 20) });
        var p1 = InitPort(n1, PortAlignment.Right, new Point(20, 6), new Size(8, 8));
        var p2 = InitPort(n2, PortAlignment.Left, new Point(196, 6), new Size(8, 8));
        var link = new LinkModel(p1, p2) { TargetMarker = LinkMarker.Square };

        d.Links.Add(link);

        link.EffectiveTargetMarker.ShouldBe(LinkMarker.Square);
    }

    [Fact]
    public void Link_routes_through_its_vertices_and_reroutes_when_a_vertex_moves()
    {
        var d = new NodelyDiagram();
        var n1 = d.Nodes.Add(new NodeModel(new Point(0, 0)) { Size = new Size(20, 20) });
        var n2 = d.Nodes.Add(new NodeModel(new Point(200, 0)) { Size = new Size(20, 20) });
        var link = d.Links.Add(new LinkModel(n1, n2));

        var vertex = link.AddVertex(new Point(100, 80));
        link.Refresh();

        link.Route.ShouldNotBeNull();
        link.Route!.Length.ShouldBe(1);
        link.Route[0].ShouldBe(new Point(100, 80));

        vertex.SetPosition(120, 90); // a vertex's SetPosition refreshes its parent link
        link.Route![0].ShouldBe(new Point(120, 90));
    }

    [Fact]
    public void Circle_marker_is_a_closed_path_spanning_its_diameter()
    {
        var marker = LinkMarker.NewCircle(10);

        marker.Width.ShouldBe(10);
        var bbox = marker.Path.GetBBox();
        bbox.Left.ShouldBe(0, 1e-9);
        bbox.Right.ShouldBe(10, 1e-9);
        bbox.Top.ShouldBe(-5, 1e-9);
        bbox.Bottom.ShouldBe(5, 1e-9);
        marker.Path.Operations[marker.Path.Operations.Count - 1].Command.ShouldBe(PathCommand.Close);
    }

    [Fact]
    public void Dynamic_anchor_picks_the_candidate_nearest_the_other_endpoint()
    {
        var d = new NodelyDiagram();
        var node = d.Nodes.Add(new NodeModel(new Point(0, 0)) { Size = new Size(40, 40) });
        var other = d.Nodes.Add(new NodeModel(new Point(200, 0)) { Size = new Size(20, 20) });

        var left = new PositionAnchor(new Point(0, 20));
        var right = new PositionAnchor(new Point(40, 20));
        var dynamic = new DynamicAnchor(node, new Anchor[] { left, right });
        var link = new LinkModel(dynamic, new ShapeIntersectionAnchor(other));

        // The other node is to the right, so the right-side candidate is chosen.
        link.Source.GetPosition(link).ShouldBe(new Point(40, 20));
    }

    [Fact]
    public void Link_anchor_resolves_to_the_middle_of_the_target_link()
    {
        var d = new NodelyDiagram();
        var a = d.Nodes.Add(new NodeModel(new Point(0, 0)) { Size = new Size(20, 20) });
        var b = d.Nodes.Add(new NodeModel(new Point(100, 0)) { Size = new Size(20, 20) });
        var target = d.Links.Add(new LinkModel(a, b));
        target.PathGeneratorResult.ShouldNotBeNull();

        var mid = new LinkAnchor(target).GetPlainPosition();

        mid.ShouldNotBeNull();
        mid!.X.ShouldBeInRange(40, 80); // between the two node boundaries (~20..100)
        mid.Y.ShouldBe(10, 0.5);        // both node centers sit at y=10
    }

    [Fact]
    public void Removing_node_removes_its_port_links()
    {
        var d = new NodelyDiagram();
        var n1 = d.Nodes.Add(new NodeModel(new Point(0, 0)));
        var n2 = d.Nodes.Add(new NodeModel(new Point(100, 0)));
        n1.Size = new Size(20, 20);
        n2.Size = new Size(20, 20);
        var p1 = InitPort(n1, PortAlignment.Right, new Point(20, 6), new Size(8, 8));
        var p2 = InitPort(n2, PortAlignment.Left, new Point(96, 6), new Size(8, 8));
        d.Links.Add(new LinkModel(p1, p2));
        d.Links.Count.ShouldBe(1);

        d.Nodes.Remove(n1);

        d.Links.Count.ShouldBe(0);
    }
}
