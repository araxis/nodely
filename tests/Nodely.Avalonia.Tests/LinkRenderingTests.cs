using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Nodely.Avalonia.Controls;
using Nodely.Models;
using Nodely.Models.Base;
using Shouldly;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Tests;

public class LinkRenderingTests
{
    private static (Window Window, NodelyDiagram Diagram, PortModel P1, PortModel P2) SetupTwoPorts(bool withLink)
    {
        var diagram = new NodelyDiagram();
        var n1 = diagram.Nodes.Add(new NodeModel(new NodelyPoint(50, 60)) { Title = "A" });
        var n2 = diagram.Nodes.Add(new NodeModel(new NodelyPoint(320, 60)) { Title = "B" });
        var p1 = n1.AddPort(PortAlignment.Right);
        var p2 = n2.AddPort(PortAlignment.Left);
        if (withLink)
            diagram.Links.Add(new LinkModel(p1, p2));

        var canvas = new DiagramCanvas { Diagram = diagram };
        var window = new Window { Width = 600, Height = 400, Content = canvas };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, diagram, p1, p2);
    }

    [AvaloniaFact]
    public void Ports_are_initialized_with_positions_after_layout()
    {
        var (_, _, p1, p2) = SetupTwoPorts(withLink: false);

        p1.Initialized.ShouldBeTrue();
        p2.Initialized.ShouldBeTrue();
        p1.Size.Width.ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public void Link_between_ports_generates_a_path_after_layout()
    {
        var (_, diagram, _, _) = SetupTwoPorts(withLink: true);

        var link = diagram.Links.Single();
        link.PathGeneratorResult.ShouldNotBeNull();
        link.PathGeneratorResult!.FullPath.Operations.Count.ShouldBeGreaterThan(1);
    }

    [AvaloniaFact]
    public void Clicking_a_link_selects_it_and_clicking_empty_space_clears_it()
    {
        var (window, diagram, p1, p2) = SetupTwoPorts(withLink: true);
        var link = diagram.Links.Single();

        // Port centers (pan 0, zoom 1); the smooth path between two horizontally-aligned ports runs through
        // their shared Y, so the midpoint lies on the link.
        var c1 = new Point(p1.Position.X + 6, p1.Position.Y + 6);
        var c2 = new Point(p2.Position.X + 6, p2.Position.Y + 6);
        var mid = new Point((c1.X + c2.X) / 2, (c1.Y + c2.Y) / 2);

        window.MouseDown(mid, MouseButton.Left);
        window.MouseUp(mid, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        link.Selected.ShouldBeTrue();

        var empty = new Point(mid.X, mid.Y + 140);
        window.MouseDown(empty, MouseButton.Left);
        window.MouseUp(empty, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        link.Selected.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Clicking_a_vertex_handle_selects_the_vertex()
    {
        var diagram = new NodelyDiagram();
        var n1 = diagram.Nodes.Add(new NodeModel(new NodelyPoint(50, 60)) { Title = "A" });
        var n2 = diagram.Nodes.Add(new NodeModel(new NodelyPoint(320, 200)) { Title = "B" });
        var link = diagram.Links.Add(new LinkModel(n1.AddPort(PortAlignment.Right), n2.AddPort(PortAlignment.Left)));
        var vertex = link.AddVertex(new NodelyPoint(200, 110));
        link.Refresh();

        var window = new Window { Width = 600, Height = 400, Content = new DiagramCanvas { Diagram = diagram } };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var handle = new Point(vertex.Position.X, vertex.Position.Y); // pan 0, zoom 1 -> screen == diagram
        window.MouseDown(handle, MouseButton.Left);
        window.MouseUp(handle, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        vertex.Selected.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Deleting_a_selected_vertex_is_undoable()
    {
        var diagram = new NodelyDiagram();
        var n1 = diagram.Nodes.Add(new NodeModel(new NodelyPoint(50, 60)) { Title = "A" });
        var n2 = diagram.Nodes.Add(new NodeModel(new NodelyPoint(320, 200)) { Title = "B" });
        var link = diagram.Links.Add(new LinkModel(n1.AddPort(PortAlignment.Right), n2.AddPort(PortAlignment.Left)));
        var vertex = link.AddVertex(new NodelyPoint(200, 110));
        link.Refresh();
        var canvas = new DiagramCanvas { Diagram = diagram };

        var window = new Window { Width = 600, Height = 400, Content = canvas };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        diagram.SelectModel(vertex, unselectOthers: true);

        canvas.DeleteModels(new Model[] { vertex });
        link.Vertices.ShouldNotContain(vertex);

        canvas.Undo();
        link.Vertices.ShouldContain(vertex);
    }

    [AvaloniaFact]
    public void Default_target_marker_flows_through_the_canvas_to_marker_geometry_data()
    {
        var diagram = new NodelyDiagram();
        diagram.Options.Links.DefaultTargetMarker = LinkMarker.Arrow;
        var n1 = diagram.Nodes.Add(new NodeModel(new NodelyPoint(50, 60)) { Title = "A" });
        var n2 = diagram.Nodes.Add(new NodeModel(new NodelyPoint(320, 60)) { Title = "B" });
        var link = diagram.Links.Add(new LinkModel(n1.AddPort(PortAlignment.Right), n2.AddPort(PortAlignment.Left)));

        var window = new Window { Width = 600, Height = 400, Content = new DiagramCanvas { Diagram = diagram } };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        link.EffectiveTargetMarker.ShouldBe(LinkMarker.Arrow);
        link.PathGeneratorResult!.TargetMarkerAngle.ShouldNotBeNull();
        link.PathGeneratorResult!.TargetMarkerPosition.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public void Dragging_from_a_port_to_another_creates_an_attached_link()
    {
        var (window, diagram, p1, p2) = SetupTwoPorts(withLink: false);

        // Port centers in screen coords (pan 0, zoom 1; port is 12px, top-left at port.Position).
        var c1 = new Point(p1.Position.X + 6, p1.Position.Y + 6);
        var c2 = new Point(p2.Position.X + 6, p2.Position.Y + 6);

        window.MouseDown(c1, MouseButton.Left);
        diagram.Links.Count.ShouldBe(1); // ongoing link added immediately

        window.MouseMove(new Point((c1.X + c2.X) / 2, (c1.Y + c2.Y) / 2));
        window.MouseUp(c2, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        var link = diagram.Links.Single();
        link.IsAttached.ShouldBeTrue();
        p2.Links.ShouldContain(link);
    }
}
