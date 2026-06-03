using Nodely.Geometry;
using Nodely.Models.Base;
using Shouldly;
using Xunit;

namespace Nodely.Core.Tests;

// Concrete model used to exercise the abstract base classes.
file sealed class TestNode : MovableModel
{
    public TestNode() { }
    public TestNode(Point position) : base(position) { }
}

public class ModelTests
{
    [Fact]
    public void Models_get_unique_generated_ids()
    {
        new TestNode().Id.ShouldNotBeNullOrEmpty();
        new TestNode().Id.ShouldNotBe(new TestNode().Id);
    }

    [Fact]
    public void Tag_and_data_bag_store_arbitrary_values()
    {
        var node = new TestNode();
        node.Tag = "hello";
        node.Data["count"] = 3;

        node.Tag.ShouldBe("hello");
        node.Data["count"].ShouldBe(3);
        node.Data.ContainsKey("missing").ShouldBeFalse();
    }

    [Fact]
    public void Refresh_raises_changed_with_self()
    {
        var node = new TestNode();
        Model? changed = null;
        node.Changed += m => changed = m;

        node.Refresh();

        changed.ShouldBeSameAs(node);
    }

    [Fact]
    public void Visible_raises_event_only_on_actual_change()
    {
        var node = new TestNode();
        var count = 0;
        node.VisibilityChanged += _ => count++;

        node.Visible = true;  // already true -> no event
        node.Visible = false; // changed
        node.Visible = false; // same -> no event

        count.ShouldBe(1);
        node.Visible.ShouldBeFalse();
    }

    [Fact]
    public void Order_raises_event_only_on_actual_change()
    {
        var node = new TestNode();
        var count = 0;
        node.OrderChanged += _ => count++;

        node.Order = 5;
        node.Order = 5; // same -> no event
        node.Order = 7;

        count.ShouldBe(2);
        node.Order.ShouldBe(7);
    }

    [Fact]
    public void Movable_defaults_to_origin_and_moves()
    {
        new TestNode().Position.ShouldBe(Point.Zero);

        var node = new TestNode(new Point(3, 4));
        node.Position.ShouldBe(new Point(3, 4));

        node.SetPosition(10, 20);
        node.Position.ShouldBe(new Point(10, 20));
    }

    [Fact]
    public void TriggerMoved_raises_moved_with_self()
    {
        var node = new TestNode();
        MovableModel? moved = null;
        node.Moved += m => moved = m;

        node.TriggerMoved();

        moved.ShouldBeSameAs(node);
    }

    [Fact]
    public void Selected_is_settable_from_within_core()
    {
        // Selected has an internal setter; visible to tests via InternalsVisibleTo.
        var node = new TestNode { Selected = true };
        node.Selected.ShouldBeTrue();
    }
}
