using System;
using System.Linq;
using Nodely.Geometry;
using Nodely.Models;
using Shouldly;
using Xunit;

namespace Nodely.Core.Tests;

file sealed class TestBehavior : Behavior
{
    public TestBehavior(Diagram diagram) : base(diagram) { }
    public bool Disposed { get; private set; }
    public override void Dispose() => Disposed = true;
}

public class EngineTests
{
    [Fact]
    public void Adding_nodes_assigns_increasing_order()
    {
        var d = new NodelyDiagram();
        var a = d.Nodes.Add(new NodeModel());
        var b = d.Nodes.Add(new NodeModel());

        d.Nodes.Count.ShouldBe(2);
        d.OrderedSelectables.Count.ShouldBe(2);
        a.Order.ShouldBe(1);
        b.Order.ShouldBe(2);
    }

    [Fact]
    public void Select_then_unselect_raises_selection_changed_twice()
    {
        var d = new NodelyDiagram();
        var n = d.Nodes.Add(new NodeModel());
        var changes = 0;
        d.SelectionChanged += _ => changes++;

        d.SelectModel(n, unselectOthers: true);
        n.Selected.ShouldBeTrue();
        d.GetSelectedModels().ShouldContain(n);

        d.UnselectModel(n);
        n.Selected.ShouldBeFalse();
        changes.ShouldBe(2);
    }

    [Fact]
    public void Unselect_all_clears_selection()
    {
        var d = new NodelyDiagram();
        var a = d.Nodes.Add(new NodeModel());
        var b = d.Nodes.Add(new NodeModel());
        d.SelectModel(a, false);
        d.SelectModel(b, false);
        d.GetSelectedModels().Count().ShouldBe(2);

        d.UnselectAll();

        d.GetSelectedModels().ShouldBeEmpty();
    }

    [Fact]
    public void Pan_and_zoom_update_state()
    {
        var d = new NodelyDiagram();

        d.SetPan(30, 40);
        d.Pan.ShouldBe(new Point(30, 40));

        d.UpdatePan(10, -5);
        d.Pan.ShouldBe(new Point(40, 35));

        d.SetZoom(2);
        d.Zoom.ShouldBe(2.0);
    }

    [Fact]
    public void Set_zoom_clamps_to_minimum()
    {
        var d = new NodelyDiagram();
        d.SetZoom(0.01); // below the default minimum of 0.1
        d.Zoom.ShouldBe(d.Options.Zoom.Minimum);
    }

    [Fact]
    public void Set_zoom_to_zero_throws()
    {
        var d = new NodelyDiagram();
        Should.Throw<ArgumentException>(() => d.SetZoom(0));
    }

    [Fact]
    public void Coordinate_transforms_round_trip()
    {
        var d = new NodelyDiagram();
        d.SetContainer(new Rectangle(0, 0, 800, 600));
        d.SetPan(50, 20);
        d.SetZoom(2);

        d.GetRelativeMousePoint(250, 120).ShouldBe(new Point(100, 50));
        d.GetScreenPoint(100, 50).ShouldBe(new Point(250, 120));
    }

    [Fact]
    public void Coordinate_methods_throw_without_container()
    {
        var d = new NodelyDiagram();
        Should.Throw<NodelyException>(() => d.GetRelativeMousePoint(0, 0));
    }

    [Fact]
    public void Zoom_to_fit_frames_nodes_inside_container()
    {
        var d = new NodelyDiagram();
        d.SetContainer(new Rectangle(0, 0, 200, 200));
        var n = d.Nodes.Add(new NodeModel(new Point(100, 100)));
        n.Size = new Size(50, 50);

        d.ZoomToFit();

        d.Zoom.ShouldBeGreaterThan(0);
        var topLeft = d.GetScreenPoint(100, 100);
        var bottomRight = d.GetScreenPoint(150, 150);
        topLeft.X.ShouldBeGreaterThanOrEqualTo(-0.01);
        topLeft.Y.ShouldBeGreaterThanOrEqualTo(-0.01);
        bottomRight.X.ShouldBeLessThanOrEqualTo(200.01);
        bottomRight.Y.ShouldBeLessThanOrEqualTo(200.01);
    }

    [Fact]
    public void Send_to_front_and_back_reorder()
    {
        var d = new NodelyDiagram();
        var a = d.Nodes.Add(new NodeModel());
        d.Nodes.Add(new NodeModel());
        var c = d.Nodes.Add(new NodeModel());

        d.SendToFront(a);
        d.GetMaxOrder().ShouldBe(a.Order);

        d.SendToBack(c);
        d.GetMinOrder().ShouldBe(c.Order);
    }

    [Fact]
    public void Batch_refreshes_only_once()
    {
        var d = new NodelyDiagram();
        var refreshes = 0;
        d.Changed += () => refreshes++;

        d.Batch(() =>
        {
            d.Refresh();
            d.Refresh();
        });

        refreshes.ShouldBe(1);
    }

    [Fact]
    public void Behavior_registry_register_get_unregister()
    {
        var d = new NodelyDiagram();
        var behavior = new TestBehavior(d);

        d.RegisterBehavior(behavior);
        d.GetBehavior<TestBehavior>().ShouldBeSameAs(behavior);

        Should.Throw<NodelyException>(() => d.RegisterBehavior(new TestBehavior(d)));

        d.UnregisterBehavior<TestBehavior>();
        d.GetBehavior<TestBehavior>().ShouldBeNull();
        behavior.Disposed.ShouldBeTrue();
    }
}
