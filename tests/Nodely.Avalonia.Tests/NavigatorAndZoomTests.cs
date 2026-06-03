using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Nodely.Avalonia.Controls;
using Nodely.Models;
using Shouldly;
using NodelyPoint = Nodely.Geometry.Point;
using NodelyRect = Nodely.Geometry.Rectangle;
using NodelySize = Nodely.Geometry.Size;

namespace Nodely.Avalonia.Tests;

public class NavigatorAndZoomTests
{
    [AvaloniaFact]
    public void Zoom_to_fit_frames_far_away_content()
    {
        var diagram = new NodelyDiagram();
        var canvas = new DiagramCanvas { Diagram = diagram };
        var window = new Window { Width = 400, Height = 300, Content = canvas };
        window.Show();
        diagram.Nodes.Add(new NodeModel(new NodelyPoint(1000, 1000)) { Title = "Far" });
        Dispatcher.UIThread.RunJobs();

        canvas.ZoomToFit();
        Dispatcher.UIThread.RunJobs();

        var topLeft = diagram.GetScreenPoint(1000, 1000);
        topLeft.X.ShouldBeGreaterThanOrEqualTo(-1);
        topLeft.Y.ShouldBeGreaterThanOrEqualTo(-1);
        topLeft.X.ShouldBeLessThanOrEqualTo(400);
        topLeft.Y.ShouldBeLessThanOrEqualTo(300);
    }

    [AvaloniaFact]
    public void Clicking_the_navigator_pans_the_diagram()
    {
        var diagram = new NodelyDiagram();
        diagram.SetContainer(new NodelyRect(0, 0, 400, 300));
        diagram.Nodes.Add(new NodeModel(new NodelyPoint(0, 0)) { Title = "A" });
        diagram.Nodes.Add(new NodeModel(new NodelyPoint(600, 400)) { Title = "B" });
        foreach (var n in diagram.Nodes)
            n.Size = new NodelySize(80, 40);

        var navigator = new DiagramNavigator { Diagram = diagram };
        var window = new Window { Width = 200, Height = 150, Content = navigator };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var before = diagram.Pan;
        window.MouseDown(new Point(20, 20), MouseButton.Left);
        window.MouseUp(new Point(20, 20), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        diagram.Pan.ShouldNotBe(before);
    }
}
