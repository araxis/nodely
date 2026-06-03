using System.Linq;
using Nodely.Algorithms;
using Nodely.Geometry;
using Nodely.Models;
using Shouldly;
using Xunit;

namespace Nodely.Core.Tests;

public class AlgorithmsTests
{
    private static NodelyDiagram Diagram() => new(null, registerDefaultBehaviors: false);

    private static NodeModel Node(NodelyDiagram d, double x = 0, double y = 0)
    {
        var n = d.Nodes.Add(new NodeModel(new Point(x, y)));
        n.Size = new Size(40, 40);
        return n;
    }

    [Fact]
    public void Connected_components_groups_linked_nodes()
    {
        var d = Diagram();
        var a = Node(d); var b = Node(d);
        var c = Node(d); var e = Node(d);
        d.Links.Add(new LinkModel(a, b));
        d.Links.Add(new LinkModel(c, e));

        var components = DiagramGraph.ConnectedComponents(d);

        components.Count.ShouldBe(2);
        components.All(comp => comp.Count == 2).ShouldBeTrue();
    }

    [Fact]
    public void Bfs_visits_reachable_nodes()
    {
        var d = Diagram();
        var a = Node(d); var b = Node(d); var c = Node(d);
        d.Links.Add(new LinkModel(a, b));
        d.Links.Add(new LinkModel(b, c));

        var order = DiagramGraph.Bfs(d, a);

        order.ShouldContain(a);
        order.ShouldContain(b);
        order.ShouldContain(c);
        order[0].ShouldBeSameAs(a);
    }

    [Fact]
    public void Layered_layout_places_a_chain_in_increasing_layers()
    {
        var d = Diagram();
        var a = Node(d, 999, 999); var b = Node(d, 0, 0); var c = Node(d, -50, 30);
        d.Links.Add(new LinkModel(a, b));
        d.Links.Add(new LinkModel(b, c));

        LayeredLayout.Arrange(d, new LayeredLayoutOptions { Horizontal = true });

        a.Position.X.ShouldBeLessThan(b.Position.X);
        b.Position.X.ShouldBeLessThan(c.Position.X);
    }

    [Fact]
    public void Layered_diagram_layout_implements_the_pluggable_IDiagramLayout()
    {
        var d = Diagram();
        var a = Node(d, 999, 999); var b = Node(d, 0, 0); var c = Node(d, -50, 30);
        d.Links.Add(new LinkModel(a, b));
        d.Links.Add(new LinkModel(b, c));

        IDiagramLayout layout = new LayeredDiagramLayout(new LayeredLayoutOptions { Horizontal = true });
        layout.Arrange(d);

        a.Position.X.ShouldBeLessThan(b.Position.X);
        b.Position.X.ShouldBeLessThan(c.Position.X);
    }

    [Fact]
    public void Layered_layout_separates_siblings_in_the_same_layer()
    {
        var d = Diagram();
        var root = Node(d); var left = Node(d); var right = Node(d);
        d.Links.Add(new LinkModel(root, left));
        d.Links.Add(new LinkModel(root, right));

        LayeredLayout.Arrange(d, new LayeredLayoutOptions { Horizontal = true });

        // siblings share a layer (same x) but are offset across (different y)
        left.Position.X.ShouldBe(right.Position.X);
        left.Position.Y.ShouldNotBe(right.Position.Y);
    }

    [Fact]
    public void Layered_layout_handles_cycles_instead_of_collapsing_to_one_column()
    {
        // A state machine: Idle -> Running -> {Done, Error}, with Error -> Idle closing the loop.
        // Before cycle-breaking, no node had in-degree 0, so every node collapsed to layer 0 (one column).
        var d = Diagram();
        var idle = Node(d); var running = Node(d); var done = Node(d); var error = Node(d);
        d.Links.Add(new LinkModel(idle, running));
        d.Links.Add(new LinkModel(running, done));
        d.Links.Add(new LinkModel(running, error));
        d.Links.Add(new LinkModel(error, idle)); // back edge -> cycle

        LayeredLayout.Arrange(d, new LayeredLayoutOptions { Horizontal = true });

        idle.Position.X.ShouldBeLessThan(running.Position.X);     // the chain spreads across layers...
        running.Position.X.ShouldBeLessThan(done.Position.X);
        done.Position.X.ShouldBe(error.Position.X);               // Done & Error branch from Running -> same layer
        done.Position.Y.ShouldNotBe(error.Position.Y);            // ...but are separated within it

        var distinctColumns = new[] { idle, running, done, error }.Select(n => n.Position.X).Distinct().Count();
        distinctColumns.ShouldBeGreaterThan(1);                   // no longer a single column
    }

    [Fact]
    public void Layered_layout_orders_a_layer_to_reduce_crossings()
    {
        // Two roots each feed one child; barycenter ordering should line children up under their parents
        // (a1 over b1, a2 over b2) rather than crossing the two edges.
        var d = Diagram();
        var a1 = Node(d); var a2 = Node(d);
        var b1 = Node(d); var b2 = Node(d);
        d.Links.Add(new LinkModel(a1, b1));
        d.Links.Add(new LinkModel(a2, b2));

        LayeredLayout.Arrange(d, new LayeredLayoutOptions { Horizontal = false }); // layers stack vertically

        // Within each layer the two nodes keep a consistent cross-axis order so the edges don't cross.
        var parentsAscending = a1.Position.X < a2.Position.X;
        var childrenAscending = b1.Position.X < b2.Position.X;
        childrenAscending.ShouldBe(parentsAscending);
    }
}
