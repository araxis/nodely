using Nodely.Geometry;
using Nodely.Models;
using Shouldly;
using Xunit;

namespace Nodely.Core.Tests;

public class GroupTests
{
    [Fact]
    public void Group_bounds_fit_children_plus_padding()
    {
        var a = new NodeModel(new Point(0, 0)) { Size = new Size(50, 50) };
        var b = new NodeModel(new Point(100, 0)) { Size = new Size(50, 50) };

        var group = new GroupModel(new[] { a, b }, padding: 30);

        group.Children.Count.ShouldBe(2);
        group.Position.ShouldBe(new Point(-30, -30));
        group.Size.ShouldBe(new Size(210, 110)); // (150 + 2*30) x (50 + 2*30)
        a.Group.ShouldBeSameAs(group);
    }

    [Fact]
    public void Moving_group_moves_children()
    {
        var a = new NodeModel(new Point(0, 0)) { Size = new Size(50, 50) };
        var b = new NodeModel(new Point(100, 0)) { Size = new Size(50, 50) };
        var group = new GroupModel(new[] { a, b }, padding: 30);

        group.SetPosition(group.Position.X + 10, group.Position.Y + 20);

        a.Position.X.ShouldBe(10, 1e-9);
        a.Position.Y.ShouldBe(20, 1e-9);
    }

    [Fact]
    public void Ungroup_detaches_children()
    {
        var a = new NodeModel(new Point(0, 0)) { Size = new Size(50, 50) };
        var group = new GroupModel(new[] { a });
        a.Group.ShouldBeSameAs(group);

        group.Ungroup();

        a.Group.ShouldBeNull();
        group.Children.ShouldBeEmpty();
    }

    [Fact]
    public void Group_layer_helper_creates_and_adds_group()
    {
        var d = new NodelyDiagram();
        var a = d.Nodes.Add(new NodeModel(new Point(0, 0))) ;
        var b = d.Nodes.Add(new NodeModel(new Point(50, 0)));
        a.Size = new Size(20, 20);
        b.Size = new Size(20, 20);

        var group = d.Groups.Group(a, b);

        d.Groups.Count.ShouldBe(1);
        group.Children.Count.ShouldBe(2);
    }
}
