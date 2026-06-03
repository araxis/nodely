using Nodely.Anchors;
using Nodely.Events;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;
using Nodely.Options;
using Shouldly;
using Xunit;

namespace Nodely.Core.Tests;

public class BehaviorTests
{
    private static PointerEvent P(double x, double y, PointerButton button = PointerButton.Left,
        bool ctrl = false, bool shift = false)
        => new(x, y, button, ctrl, shift, false, false);

    [Fact]
    public void Pointer_down_selects_node_and_ctrl_toggles()
    {
        var d = new NodelyDiagram();
        var n = d.Nodes.Add(new NodeModel(new Point(0, 0))) ;
        n.Size = new Size(50, 50);

        d.TriggerPointerDown(n, P(10, 10));
        n.Selected.ShouldBeTrue();

        d.TriggerPointerDown(n, P(10, 10, ctrl: true)); // ctrl-click an already-selected model unselects it
        n.Selected.ShouldBeFalse();
    }

    [Fact]
    public void Pointer_down_on_empty_canvas_unselects_all()
    {
        var d = new NodelyDiagram();
        var n = d.Nodes.Add(new NodeModel());
        d.SelectModel(n, false);

        d.TriggerPointerDown(null, P(10, 10));

        n.Selected.ShouldBeFalse();
    }

    [Fact]
    public void Dragging_empty_canvas_pans()
    {
        var d = new NodelyDiagram();
        d.SetContainer(new Rectangle(0, 0, 800, 600));

        d.TriggerPointerDown(null, P(100, 100));
        d.TriggerPointerMove(null, P(130, 120));

        d.Pan.ShouldBe(new Point(30, 20));
    }

    [Fact]
    public void Wheel_zooms_in()
    {
        var d = new NodelyDiagram();
        d.SetContainer(new Rectangle(0, 0, 800, 600));
        var before = d.Zoom;

        d.TriggerWheel(new WheelEvent(400, 300, 0, 1, 0, false, false, false, false));

        d.Zoom.ShouldBeGreaterThan(before);
    }

    [Fact]
    public void Dragging_a_node_moves_it_by_the_zoom_adjusted_delta()
    {
        var d = new NodelyDiagram();
        var n = d.Nodes.Add(new NodeModel(new Point(0, 0)));
        n.Size = new Size(50, 50);

        d.TriggerPointerDown(n, P(10, 10)); // selection runs first, then drag captures it
        d.TriggerPointerMove(n, P(40, 30)); // delta (30, 20) at zoom 1
        d.TriggerPointerUp(n, P(40, 30));

        n.Position.ShouldBe(new Point(30, 20));
    }

    [Fact]
    public void Dragging_from_a_port_creates_and_attaches_a_link()
    {
        var d = new NodelyDiagram();
        d.SetContainer(new Rectangle(0, 0, 800, 600));
        var n1 = d.Nodes.Add(new NodeModel(new Point(0, 0))); n1.Size = new Size(40, 40);
        var n2 = d.Nodes.Add(new NodeModel(new Point(200, 0))); n2.Size = new Size(40, 40);
        var p1 = n1.AddPort(new PortModel(n1, PortAlignment.Right, new Point(40, 16), new Size(8, 8)));
        var p2 = n2.AddPort(new PortModel(n2, PortAlignment.Left, new Point(200, 16), new Size(8, 8)));
        p1.Initialized = true;
        p2.Initialized = true;

        d.TriggerPointerDown(p1, P(40, 20));
        d.Links.Count.ShouldBe(1); // ongoing link added immediately

        d.TriggerPointerMove(null, P(150, 20));
        d.TriggerPointerUp(p2, P(200, 20));

        var link = d.Links.Single();
        link.IsAttached.ShouldBeTrue();
        link.Target.ShouldBeOfType<SinglePortAnchor>().Port.ShouldBeSameAs(p2);
        p2.Links.ShouldContain(link);
    }

    [Fact]
    public void CanConnect_false_discards_a_dragged_link()
    {
        var d = new NodelyDiagram();
        d.SetContainer(new Rectangle(0, 0, 800, 600));
        d.Options.Links.CanConnect = (_, _) => false; // reject every connection
        var n1 = d.Nodes.Add(new NodeModel(new Point(0, 0))); n1.Size = new Size(40, 40);
        var n2 = d.Nodes.Add(new NodeModel(new Point(200, 0))); n2.Size = new Size(40, 40);
        var p1 = n1.AddPort(new PortModel(n1, PortAlignment.Right, new Point(40, 16), new Size(8, 8)));
        var p2 = n2.AddPort(new PortModel(n2, PortAlignment.Left, new Point(200, 16), new Size(8, 8)));
        p1.Initialized = true; p2.Initialized = true;

        d.TriggerPointerDown(p1, P(40, 20));
        d.TriggerPointerMove(null, P(150, 20));
        d.TriggerPointerUp(p2, P(200, 20));

        d.Links.Count.ShouldBe(0); // rejected, and RequireTarget -> discarded
    }

    [Fact]
    public void SnapPosition_snaps_a_dragged_node()
    {
        var d = new NodelyDiagram();
        d.Options.SnapPosition = _ => new Point(50, 50); // snap everything to (50,50)
        var n = d.Nodes.Add(new NodeModel(new Point(0, 0))); n.Size = new Size(20, 20);

        d.TriggerPointerDown(n, P(5, 5));
        d.TriggerPointerMove(n, P(40, 30));

        n.Position.ShouldBe(new Point(50, 50));
    }

    [Fact]
    public void CanDrag_false_prevents_a_node_move()
    {
        var d = new NodelyDiagram();
        d.Options.CanDrag = _ => false;
        var n = d.Nodes.Add(new NodeModel(new Point(0, 0))); n.Size = new Size(20, 20);

        d.TriggerPointerDown(n, P(5, 5));
        d.TriggerPointerMove(n, P(40, 30));

        n.Position.ShouldBe(new Point(0, 0)); // never recorded as draggable
    }

    [Fact]
    public void Delete_shortcut_removes_selected_node()
    {
        var d = new NodelyDiagram();
        var n = d.Nodes.Add(new NodeModel());
        n.Size = new Size(10, 10);
        d.SelectModel(n, false);

        d.TriggerKeyDown(new KeyboardEvent("Delete", "Delete", false, false, false, false));

        d.Nodes.Count.ShouldBe(0);
    }

    [Fact]
    public void Pointer_down_then_up_without_move_synthesizes_a_click()
    {
        var d = new NodelyDiagram();
        var n = d.Nodes.Add(new NodeModel());
        n.Size = new Size(10, 10);
        Model? clicked = null;
        d.PointerClick += (m, _) => clicked = m;

        d.TriggerPointerDown(n, P(5, 5));
        d.TriggerPointerUp(n, P(5, 5));

        clicked.ShouldBeSameAs(n);
    }

    [Fact]
    public void Virtualization_hides_offscreen_nodes()
    {
        var options = new DiagramOptions();
        options.Virtualization.Enabled = true; // OnNodes defaults to true
        var d = new NodelyDiagram(options);
        d.SetContainer(new Rectangle(0, 0, 200, 200));

        var onscreen = d.Nodes.Add(new NodeModel(new Point(10, 10))); onscreen.Size = new Size(20, 20);
        var offscreen = d.Nodes.Add(new NodeModel(new Point(1000, 1000))); offscreen.Size = new Size(20, 20);

        d.UpdatePan(0, 0); // triggers a visibility recheck

        onscreen.Visible.ShouldBeTrue();
        offscreen.Visible.ShouldBeFalse();
    }
}
