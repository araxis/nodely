using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Nodely.Avalonia;
using Nodely.Avalonia.Controls;
using Nodely.Models;
using Shouldly;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Tests;

public class ThemingReadOnlyTests
{
    private static (Window Window, DiagramCanvas Canvas, NodelyDiagram Diagram, NodeModel Node) Setup(bool readOnly = false)
    {
        var diagram = new NodelyDiagram();
        var node = diagram.Nodes.Add(new NodeModel(new NodelyPoint(100, 100)) { Title = "Node" });
        var canvas = new DiagramCanvas { Diagram = diagram, IsReadOnly = readOnly };
        var window = new Window { Width = 600, Height = 400, Content = canvas };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, canvas, diagram, node);
    }

    [AvaloniaFact]
    public void Setting_palette_updates_canvas_brushes()
    {
        var canvas = new DiagramCanvas();

        canvas.Palette = NodelyPalettes.Light;

        canvas.Background.ShouldBe(NodelyPalettes.Light.CanvasBackground);
        canvas.GridBrush.ShouldBe(NodelyPalettes.Light.GridLine);
    }

    [AvaloniaFact]
    public void Read_only_blocks_node_drag_but_allows_selection()
    {
        var (window, _, _, node) = Setup(readOnly: true);

        window.MouseDown(new Point(105, 105), MouseButton.Left);
        window.MouseMove(new Point(170, 150));
        window.MouseUp(new Point(170, 150), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        node.Position.X.ShouldBe(100, 0.001); // didn't move
        node.Selected.ShouldBeTrue();          // but selection still works
    }

    [AvaloniaFact]
    public void Resize_handle_resizes_the_node()
    {
        var (window, _, diagram, node) = Setup();

        var w0 = node.Size!.Width;
        var h0 = node.Size.Height;
        diagram.SelectModel(node, unselectOthers: true);
        Dispatcher.UIThread.RunJobs();

        // The resize handle sits at the node's bottom-right corner (pan 0, zoom 1).
        var hx = 100 + w0;
        var hy = 100 + h0;
        window.MouseDown(new Point(hx, hy), MouseButton.Left);
        window.MouseMove(new Point(hx + 40, hy + 30));
        window.MouseUp(new Point(hx + 40, hy + 30), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        node.ControlledSize.ShouldBeTrue();
        node.Size!.Width.ShouldBeGreaterThan(w0);
        node.Size.Height.ShouldBeGreaterThan(h0);
    }

    [AvaloniaFact]
    public void Escape_clears_the_selection()
    {
        var (window, canvas, diagram, node) = Setup();
        diagram.SelectModel(node, unselectOthers: true);
        node.Selected.ShouldBeTrue();

        canvas.Focus();
        window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
        Dispatcher.UIThread.RunJobs();

        node.Selected.ShouldBeFalse();
    }
}
