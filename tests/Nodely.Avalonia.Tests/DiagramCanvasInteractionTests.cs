using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Nodely.Avalonia.Controls;
using Shouldly;

namespace Nodely.Avalonia.Tests;

public class DiagramCanvasInteractionTests
{
    private static (Window Window, NodelyDiagram Diagram) Setup()
    {
        var diagram = new NodelyDiagram();
        var canvas = new DiagramCanvas { Diagram = diagram };
        var window = new Window { Width = 400, Height = 300, Content = canvas };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, diagram);
    }

    [AvaloniaFact]
    public void Layout_reports_the_container_to_the_diagram()
    {
        var (_, diagram) = Setup();

        diagram.Container.ShouldNotBeNull();
        diagram.Container!.Width.ShouldBeGreaterThan(0);
        diagram.Container.Height.ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public void Dragging_empty_canvas_pans_the_diagram()
    {
        var (window, diagram) = Setup();

        window.MouseDown(new Point(100, 100), MouseButton.Left);
        window.MouseMove(new Point(130, 120));
        window.MouseUp(new Point(130, 120), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        diagram.Pan.X.ShouldBe(30, 0.001);
        diagram.Pan.Y.ShouldBe(20, 0.001);
    }

    [AvaloniaFact]
    public void Wheel_zooms_the_diagram()
    {
        var (window, diagram) = Setup();
        var before = diagram.Zoom;

        window.MouseWheel(new Point(200, 150), new Vector(0, 1));
        Dispatcher.UIThread.RunJobs();

        diagram.Zoom.ShouldBeGreaterThan(before);
    }
}
