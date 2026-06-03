using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nodely.Avalonia.Controls;
using Nodely.Models;
using Shouldly;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Tests;

public class GroupAndAdornerTests
{
    private static (Window Window, DiagramCanvas Canvas, NodelyDiagram Diagram, NodeModel A, NodeModel B) Setup()
    {
        var diagram = new NodelyDiagram();
        var a = diagram.Nodes.Add(new NodeModel(new NodelyPoint(60, 60)) { Title = "A" });
        var b = diagram.Nodes.Add(new NodeModel(new NodelyPoint(220, 60)) { Title = "B" });
        var canvas = new DiagramCanvas { Diagram = diagram };
        var window = new Window { Width = 600, Height = 400, Content = canvas };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, canvas, diagram, a, b);
    }

    [AvaloniaFact]
    public void Group_sizes_to_its_children_after_layout()
    {
        var (_, _, diagram, a, b) = Setup();

        var group = diagram.Groups.Group(a, b);
        Dispatcher.UIThread.RunJobs();

        group.Children.Count.ShouldBe(2);
        a.Group.ShouldBeSameAs(group);
        group.Size.ShouldNotBeNull();
        group.Size!.Width.ShouldBeGreaterThan(0);
        group.Size.Height.ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public void Shift_drag_marquee_selects_enclosed_nodes()
    {
        var (window, _, _, a, b) = Setup();

        window.MouseDown(new Point(30, 30), MouseButton.Left, RawInputModifiers.Shift);
        window.MouseMove(new Point(330, 160), RawInputModifiers.Shift);
        window.MouseUp(new Point(330, 160), MouseButton.Left, RawInputModifiers.Shift);
        Dispatcher.UIThread.RunJobs();

        a.Selected.ShouldBeTrue();
        b.Selected.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Selecting_a_node_shows_a_delete_adorner_that_removes_it()
    {
        var (window, canvas, diagram, a, _) = Setup();

        diagram.SelectModel(a, unselectOthers: true);
        Dispatcher.UIThread.RunJobs();

        var buttons = canvas.GetVisualDescendants().OfType<Button>().ToList();
        buttons.Count.ShouldBe(1);

        var center = buttons[0].Bounds.Center;
        window.MouseDown(center, MouseButton.Left);
        window.MouseUp(center, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        diagram.Nodes.Contains(a).ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Adorner_follows_the_node_while_it_is_dragged()
    {
        var (window, canvas, diagram, a, _) = Setup();

        diagram.SelectModel(a, unselectOthers: true);
        Dispatcher.UIThread.RunJobs();

        // A pointer drag moves the node via its own Changed event (not Diagram.Changed); the handles must follow.
        var delete = canvas.GetVisualDescendants().OfType<Button>().Single();
        var before = Canvas.GetLeft(delete);

        window.MouseDown(new Point(70, 70), MouseButton.Left); // grab inside node A (at 60,60)
        window.MouseMove(new Point(170, 70));                  // drag +100 in X
        window.MouseUp(new Point(170, 70), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        a.Position.X.ShouldBe(160, 0.001);                    // sanity: the node actually moved
        (Canvas.GetLeft(delete) - before).ShouldBe(100, 0.5); // the adorner tracked it, not frozen in place
    }
}
