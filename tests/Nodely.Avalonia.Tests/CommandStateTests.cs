using Avalonia.Headless.XUnit;
using Nodely.Avalonia.Controls;
using Nodely.Geometry;
using Nodely.Models;
using Shouldly;

namespace Nodely.Avalonia.Tests;

public class CommandStateTests
{
    [AvaloniaFact]
    public void Command_state_updates_when_selection_changes()
    {
        var diagram = new NodelyDiagram();
        var canvas = new DiagramCanvas { Diagram = diagram };
        var node = diagram.Nodes.Add(new NodeModel(new Point(0, 0)) { Title = "Node" });
        var changes = 0;
        canvas.CommandStateChanged += () => changes++;

        canvas.HasSelection.ShouldBeFalse();
        canvas.CanCopySelection.ShouldBeFalse();
        canvas.CanDeleteSelection.ShouldBeFalse();

        diagram.SelectModel(node, unselectOthers: true);

        canvas.HasSelection.ShouldBeTrue();
        canvas.CanCopySelection.ShouldBeTrue();
        canvas.CanCutSelection.ShouldBeTrue();
        canvas.CanDeleteSelection.ShouldBeTrue();
        canvas.CanDuplicateSelection.ShouldBeTrue();
        changes.ShouldBeGreaterThan(0);

        diagram.UnselectAll();

        canvas.HasSelection.ShouldBeFalse();
        canvas.CanCopySelection.ShouldBeFalse();
        canvas.CanDeleteSelection.ShouldBeFalse();
        changes.ShouldBeGreaterThan(1);
    }

    [AvaloniaFact]
    public void Clipboard_and_readonly_state_update_command_state()
    {
        var diagram = new NodelyDiagram();
        var canvas = new DiagramCanvas { Diagram = diagram };
        var node = diagram.Nodes.Add(new NodeModel(new Point(0, 0)) { Title = "Copy me" });
        var changes = 0;
        canvas.CommandStateChanged += () => changes++;

        diagram.SelectModel(node, unselectOthers: true);
        canvas.CopySelection();

        canvas.CanPasteClipboard.ShouldBeTrue();
        changes.ShouldBeGreaterThan(0);

        canvas.IsReadOnly = true;

        canvas.CanPasteClipboard.ShouldBeFalse();
        canvas.CanCutSelection.ShouldBeFalse();
        canvas.CanDeleteSelection.ShouldBeFalse();

        canvas.IsReadOnly = false;

        canvas.CanPasteClipboard.ShouldBeTrue();
        canvas.CanDeleteSelection.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Grouping_state_tracks_selection_and_membership()
    {
        var diagram = new NodelyDiagram();
        diagram.Options.Groups.Enabled = true;
        var canvas = new DiagramCanvas { Diagram = diagram };
        var first = diagram.Nodes.Add(new NodeModel(new Point(0, 0)) { Size = new Size(80, 40) });
        var second = diagram.Nodes.Add(new NodeModel(new Point(120, 0)) { Size = new Size(80, 40) });
        var changes = 0;
        canvas.CommandStateChanged += () => changes++;

        diagram.SelectModel(first, unselectOthers: true);
        diagram.SelectModel(second, unselectOthers: false);

        canvas.CanGroupSelection.ShouldBeTrue();
        canvas.CanUngroupSelection.ShouldBeFalse();

        canvas.GroupSelection();

        canvas.CanGroupSelection.ShouldBeFalse();
        canvas.CanUngroupSelection.ShouldBeTrue();
        changes.ShouldBeGreaterThan(0);

        canvas.UngroupSelection();

        canvas.CanGroupSelection.ShouldBeTrue();
        canvas.CanUngroupSelection.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Diagram_swap_refreshes_state_and_detaches_old_selection_events()
    {
        var first = new NodelyDiagram();
        var firstNode = first.Nodes.Add(new NodeModel(new Point(0, 0)));
        var staleNode = first.Nodes.Add(new NodeModel(new Point(120, 0)));
        var second = new NodelyDiagram();
        var canvas = new DiagramCanvas { Diagram = first };
        var changes = 0;
        canvas.CommandStateChanged += () => changes++;

        first.SelectModel(firstNode, unselectOthers: true);
        canvas.HasSelection.ShouldBeTrue();

        canvas.Diagram = second;

        canvas.HasSelection.ShouldBeFalse();
        canvas.CanDeleteSelection.ShouldBeFalse();
        changes.ShouldBeGreaterThan(0);

        var afterSwap = changes;
        first.SelectModel(staleNode, unselectOthers: false);

        changes.ShouldBe(afterSwap);
    }
}
