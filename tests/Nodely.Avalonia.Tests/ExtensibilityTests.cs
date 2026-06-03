using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nodely.Avalonia.Controls;
using Nodely.Models;
using Shouldly;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Tests;

public class ExtensibilityTests
{
    private sealed class ProbeLayer : DiagramLayer { }

    private static (Window Window, DiagramCanvas Canvas, NodelyDiagram Diagram) Show(DiagramCanvas canvas)
    {
        var window = new Window { Width = 400, Height = 300, Content = canvas };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, canvas, (NodelyDiagram)canvas.Diagram!);
    }

    [AvaloniaFact]
    public void RegisterPort_renders_a_custom_control()
    {
        var diagram = new NodelyDiagram();
        var node = diagram.Nodes.Add(new NodeModel(new NodelyPoint(50, 50)) { Title = "A" });
        node.AddPort(PortAlignment.Right);

        var canvas = new DiagramCanvas { Diagram = diagram };
        canvas.RegisterPort<PortModel>(_ => new Rectangle { Width = 8, Height = 8, Tag = "custom-port" });
        Show(canvas);

        canvas.GetVisualDescendants().OfType<Rectangle>().Any(r => Equals(r.Tag, "custom-port")).ShouldBeTrue();
    }

    [AvaloniaFact]
    public void RegisterGroup_renders_a_custom_control()
    {
        var diagram = new NodelyDiagram();
        var a = diagram.Nodes.Add(new NodeModel(new NodelyPoint(40, 40)) { Title = "A" });
        var b = diagram.Nodes.Add(new NodeModel(new NodelyPoint(200, 40)) { Title = "B" });
        diagram.Groups.Group(a, b);

        var canvas = new DiagramCanvas { Diagram = diagram };
        canvas.RegisterGroup<GroupModel>(_ => new Border { Tag = "custom-group" });
        Show(canvas);

        canvas.GetVisualDescendants().OfType<Border>().Any(x => Equals(x.Tag, "custom-group")).ShouldBeTrue();
    }

    [AvaloniaFact]
    public void AddLayer_and_RemoveLayer_manage_a_custom_overlay()
    {
        var canvas = new DiagramCanvas { Diagram = new NodelyDiagram() };
        var layer = new ProbeLayer();
        canvas.AddLayer(layer);
        Show(canvas);

        layer.Owner.ShouldBeSameAs(canvas);
        canvas.GetVisualDescendants().Contains(layer).ShouldBeTrue();

        canvas.RemoveLayer(layer);
        Dispatcher.UIThread.RunJobs();
        canvas.GetVisualDescendants().Contains(layer).ShouldBeFalse();
    }

    [AvaloniaFact]
    public void RegisterAdorner_adds_a_custom_adorner_on_selection()
    {
        var diagram = new NodelyDiagram();
        var node = diagram.Nodes.Add(new NodeModel(new NodelyPoint(60, 60)) { Title = "A" });
        var canvas = new DiagramCanvas { Diagram = diagram };
        canvas.RegisterAdorner(_ => new Button { Content = "★", Tag = "custom-adorner" });
        Show(canvas);

        diagram.SelectModel(node, unselectOthers: true);
        Dispatcher.UIThread.RunJobs();

        canvas.GetVisualDescendants().OfType<Button>().Any(b => Equals(b.Tag, "custom-adorner")).ShouldBeTrue();
    }

    [AvaloniaFact]
    public void RegisterLink_and_LinkStyleResolver_resolve_for_the_type()
    {
        var canvas = new DiagramCanvas();
        LinkDrawer drawer = (_, _) => { };
        canvas.RegisterLink<LinkModel>(drawer);
        var link = new LinkModel(new NodeModel(new NodelyPoint(0, 0)), new NodeModel(new NodelyPoint(100, 0)));

        canvas.ResolveLinkDrawer(link).ShouldBe(drawer);                 // resolves by type
        new DiagramCanvas().ResolveLinkDrawer(link).ShouldBeNull();      // none registered

        canvas.ResolveLinkStyle(link).ShouldBe(LinkStyle.Default);       // no resolver -> default
        canvas.LinkStyleResolver = _ => new LinkStyle { Width = 5 };
        canvas.ResolveLinkStyle(link).Width.ShouldBe(5);
    }
}
