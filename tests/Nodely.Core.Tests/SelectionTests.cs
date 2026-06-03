using Nodely;
using Nodely.Geometry;
using Nodely.Models;
using Shouldly;
using Xunit;

namespace Nodely.Core.Tests;

public class SelectionTests
{
    private static NodelyDiagram Diagram() => new(null, registerDefaultBehaviors: false);

    [Fact]
    public void Select_all_selects_every_node_and_link()
    {
        var d = Diagram();
        var a = d.Nodes.Add(new NodeModel(new Point(0, 0)) { Size = new Size(20, 20) });
        var b = d.Nodes.Add(new NodeModel(new Point(100, 0)) { Size = new Size(20, 20) });
        var link = d.Links.Add(new LinkModel(a, b));

        d.SelectAll();

        a.Selected.ShouldBeTrue();
        b.Selected.ShouldBeTrue();
        link.Selected.ShouldBeTrue();
    }

    [Fact]
    public void Send_to_front_raises_order_above_the_others()
    {
        var d = Diagram();
        var a = d.Nodes.Add(new NodeModel(new Point(0, 0)));
        var b = d.Nodes.Add(new NodeModel(new Point(10, 0)));

        d.SendToFront(a);

        a.Order.ShouldBeGreaterThan(b.Order);
    }

    [Fact]
    public void Send_to_back_lowers_order_below_the_others()
    {
        var d = Diagram();
        var a = d.Nodes.Add(new NodeModel(new Point(0, 0)));
        var b = d.Nodes.Add(new NodeModel(new Point(10, 0)));

        d.SendToBack(b);

        b.Order.ShouldBeLessThan(a.Order);
    }

    [Fact]
    public void Node_clone_is_a_distinct_copy_of_title_size_and_position()
    {
        var node = new NodeModel(new Point(10, 20)) { Title = "X", Size = new Size(40, 30) };

        var clone = node.Clone();

        clone.ShouldNotBeSameAs(node);
        clone.Title.ShouldBe("X");
        clone.Size!.Width.ShouldBe(40);
        clone.Position.ShouldBe(new Point(10, 20));
    }
}
