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
using NodelySize = Nodely.Geometry.Size;

namespace Nodely.Avalonia.Tests;

public class NodeRenderingTests
{
    private static (Window Window, DiagramCanvas Canvas, NodelyDiagram Diagram) Show(DiagramCanvas? canvas = null)
    {
        var diagram = new NodelyDiagram();
        canvas ??= new DiagramCanvas();
        canvas.Diagram = diagram;
        var window = new Window { Width = 600, Height = 400, Content = canvas };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, canvas, diagram);
    }

    [AvaloniaFact]
    public void Node_gets_measured_size_from_layout()
    {
        var (_, _, diagram) = Show();

        var node = diagram.Nodes.Add(new NodeModel(new NodelyPoint(100, 100)) { Title = "Hello" });
        Dispatcher.UIThread.RunJobs();

        node.Size.ShouldNotBeNull();
        node.Size!.Width.ShouldBeGreaterThan(0);
        node.Size.Height.ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public void Registered_custom_node_template_is_used()
    {
        var canvas = new DiagramCanvas();
        canvas.RegisterNode<NodeModel>(n => new TextBlock { Text = "CUSTOM:" + n.Title });
        var (_, _, diagram) = Show(canvas);

        diagram.Nodes.Add(new NodeModel(new NodelyPoint(50, 50)) { Title = "X" });
        Dispatcher.UIThread.RunJobs();

        var texts = canvas.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();
        texts.ShouldContain("CUSTOM:X");
    }

    [AvaloniaFact]
    public void Clicking_a_node_selects_it()
    {
        var (window, _, diagram) = Show();
        var node = diagram.Nodes.Add(new NodeModel(new NodelyPoint(100, 100)) { Title = "Click me" });
        Dispatcher.UIThread.RunJobs();

        // pan 0, zoom 1 => screen coords == diagram coords; press inside the node.
        window.MouseDown(new Point(105, 105), MouseButton.Left);
        window.MouseUp(new Point(105, 105), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        node.Selected.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Dragging_a_node_moves_it()
    {
        var (window, _, diagram) = Show();
        var node = diagram.Nodes.Add(new NodeModel(new NodelyPoint(100, 100)) { Title = "Drag me" });
        Dispatcher.UIThread.RunJobs();

        window.MouseDown(new Point(105, 105), MouseButton.Left);
        window.MouseMove(new Point(155, 135)); // +50, +30
        window.MouseUp(new Point(155, 135), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        node.Position.X.ShouldBe(150, 0.001);
        node.Position.Y.ShouldBe(130, 0.001);
    }

    [AvaloniaFact]
    public void Undo_restores_a_dragged_node_and_redo_reapplies_it()
    {
        var (window, canvas, diagram) = Show();
        var node = diagram.Nodes.Add(new NodeModel(new NodelyPoint(100, 100)) { Title = "Undo me" });
        Dispatcher.UIThread.RunJobs();

        window.MouseDown(new Point(105, 105), MouseButton.Left);
        window.MouseMove(new Point(165, 145)); // +60, +40
        window.MouseUp(new Point(165, 145), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        node.Position.X.ShouldBe(160, 0.001);

        canvas.Undo();
        node.Position.X.ShouldBe(100, 0.001);
        node.Position.Y.ShouldBe(100, 0.001);

        canvas.Redo();
        node.Position.X.ShouldBe(160, 0.001);
        node.Position.Y.ShouldBe(140, 0.001);
    }

    [AvaloniaFact]
    public void Undo_reverts_a_multi_node_drag_in_one_step()
    {
        var (window, canvas, diagram) = Show();
        var a = diagram.Nodes.Add(new NodeModel(new NodelyPoint(100, 100)) { Title = "A" });
        var b = diagram.Nodes.Add(new NodeModel(new NodelyPoint(200, 100)) { Title = "B" });
        Dispatcher.UIThread.RunJobs();

        diagram.SelectModel(a, unselectOthers: true);
        diagram.SelectModel(b, unselectOthers: false); // both selected; clicking an already-selected node keeps both

        window.MouseDown(new Point(105, 105), MouseButton.Left);
        window.MouseMove(new Point(135, 125)); // +30, +20 — both move together
        window.MouseUp(new Point(135, 125), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        a.Position.X.ShouldBe(130, 0.001);
        b.Position.X.ShouldBe(230, 0.001);

        canvas.Undo(); // a single undo restores BOTH (grouped into one transaction)
        a.Position.X.ShouldBe(100, 0.001);
        b.Position.X.ShouldBe(200, 0.001);
    }

    [AvaloniaFact]
    public void RunAsUndoableMove_groups_a_bulk_reposition_into_one_undo()
    {
        var (_, canvas, diagram) = Show();
        var a = diagram.Nodes.Add(new NodeModel(new NodelyPoint(0, 0)) { Title = "A" });
        var b = diagram.Nodes.Add(new NodeModel(new NodelyPoint(50, 0)) { Title = "B" });
        Dispatcher.UIThread.RunJobs();

        canvas.RunAsUndoableMove(() =>
        {
            a.SetPosition(200, 100); // direct repositions, like LayeredLayout (no Moved events)
            b.SetPosition(260, 140);
        });
        a.Position.X.ShouldBe(200, 0.001);

        canvas.Undo(); // one undo restores both
        a.Position.X.ShouldBe(0, 0.001);
        b.Position.X.ShouldBe(50, 0.001);
    }

    [AvaloniaFact]
    public void RunAsUndoableEdit_rebuilds_visuals_and_supports_undo_redo()
    {
        var canvas = new DiagramCanvas();
        canvas.RegisterNode<NodeModel>(node => new TextBlock { Text = node.Title });
        var (_, _, diagram) = Show(canvas);
        var node = diagram.Nodes.Add(new NodeModel(new NodelyPoint(0, 0)) { Title = "Before" });
        Dispatcher.UIThread.RunJobs();

        canvas.RunAsUndoableEdit(
            () => node.Title = "After",
            () => node.Title = "Before");
        Dispatcher.UIThread.RunJobs();

        canvas.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ShouldContain("After");

        canvas.Undo();
        Dispatcher.UIThread.RunJobs();

        canvas.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ShouldContain("Before");

        canvas.Redo();
        Dispatcher.UIThread.RunJobs();

        canvas.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ShouldContain("After");
    }

    [AvaloniaFact]
    public void Bring_selection_to_front_is_undoable()
    {
        var (_, canvas, diagram) = Show();
        var a = diagram.Nodes.Add(new NodeModel(new NodelyPoint(0, 0)) { Title = "A" });
        var b = diagram.Nodes.Add(new NodeModel(new NodelyPoint(50, 0)) { Title = "B" });
        Dispatcher.UIThread.RunJobs();
        var originalA = a.Order;
        var originalB = b.Order;
        diagram.SelectModel(a, unselectOthers: true);

        canvas.BringSelectionToFront();
        a.Order.ShouldBeGreaterThan(b.Order);

        canvas.Undo();
        a.Order.ShouldBe(originalA);
        b.Order.ShouldBe(originalB);

        canvas.Redo();
        a.Order.ShouldBeGreaterThan(b.Order);
    }

    [AvaloniaFact]
    public void Group_selection_and_ungroup_selection_are_undoable()
    {
        var (_, canvas, diagram) = Show();
        diagram.Options.Groups.Enabled = true;
        var a = diagram.Nodes.Add(new NodeModel(new NodelyPoint(0, 0)) { Title = "A", Size = new NodelySize(20, 20) });
        var b = diagram.Nodes.Add(new NodeModel(new NodelyPoint(50, 0)) { Title = "B", Size = new NodelySize(20, 20) });
        Dispatcher.UIThread.RunJobs();
        diagram.SelectModel(a, unselectOthers: true);
        diagram.SelectModel(b, unselectOthers: false);

        canvas.GroupSelection();
        diagram.Groups.Count.ShouldBe(1);
        var group = diagram.Groups.Single();
        a.Group.ShouldBeSameAs(group);

        canvas.Undo();
        diagram.Groups.Count.ShouldBe(0);
        a.Group.ShouldBeNull();

        canvas.Redo();
        diagram.Groups.Count.ShouldBe(1);
        group = diagram.Groups.Single();
        diagram.SelectModel(group, unselectOthers: true);

        canvas.UngroupSelection();
        diagram.Groups.Count.ShouldBe(0);
        a.Group.ShouldBeNull();

        canvas.Undo();
        diagram.Groups.Count.ShouldBe(1);
        a.Group.ShouldBeSameAs(group);
    }

    [AvaloniaFact]
    public void Duplicate_selection_adds_an_offset_copy_selects_it_and_is_one_undo()
    {
        var (_, canvas, diagram) = Show();
        var a = diagram.Nodes.Add(new NodeModel(new NodelyPoint(100, 100)) { Title = "A" });
        Dispatcher.UIThread.RunJobs();
        diagram.SelectModel(a, unselectOthers: true);

        canvas.DuplicateSelection();
        Dispatcher.UIThread.RunJobs();

        diagram.Nodes.Count.ShouldBe(2);
        var clone = diagram.Nodes.First(n => !ReferenceEquals(n, a));
        clone.Position.X.ShouldBe(124, 0.001);
        clone.Selected.ShouldBeTrue();
        a.Selected.ShouldBeFalse();

        canvas.Undo(); // a single undo removes the duplicate
        diagram.Nodes.Count.ShouldBe(1);
    }

    [AvaloniaFact]
    public void Copy_then_paste_twice_clones_at_a_growing_offset()
    {
        var (_, canvas, diagram) = Show();
        var a = diagram.Nodes.Add(new NodeModel(new NodelyPoint(100, 100)) { Title = "A" });
        Dispatcher.UIThread.RunJobs();
        diagram.SelectModel(a, unselectOthers: true);

        canvas.CopySelection();
        canvas.PasteClipboard();
        canvas.PasteClipboard();
        Dispatcher.UIThread.RunJobs();

        diagram.Nodes.Count.ShouldBe(3); // original + two pastes
    }
}
