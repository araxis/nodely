using System.Collections.Generic;
using Nodely.Commands;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;
using Shouldly;
using Xunit;

namespace Nodely.Core.Tests;

public class CommandTests
{
    [Fact]
    public void Add_node_undo_then_redo()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var stack = new UndoRedoStack();

        stack.Execute(new AddNodeCommand(d, new NodeModel(new Point(0, 0))));
        d.Nodes.Count.ShouldBe(1);
        stack.CanUndo.ShouldBeTrue();

        stack.Undo();
        d.Nodes.Count.ShouldBe(0);
        stack.CanRedo.ShouldBeTrue();

        stack.Redo();
        d.Nodes.Count.ShouldBe(1);
    }

    [Fact]
    public void Move_node_undo_restores_position()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var node = d.Nodes.Add(new NodeModel(new Point(10, 10)));
        node.Size = new Size(20, 20);
        var stack = new UndoRedoStack();

        stack.Execute(new MoveNodeCommand(node, new Point(10, 10), new Point(100, 50)));
        node.Position.ShouldBe(new Point(100, 50));

        stack.Undo();
        node.Position.ShouldBe(new Point(10, 10));
    }

    [Fact]
    public void Set_model_orders_undo_restores_the_previous_z_order()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var a = d.Nodes.Add(new NodeModel(new Point(0, 0)));
        var b = d.Nodes.Add(new NodeModel(new Point(10, 0)));
        var before = new Dictionary<SelectableModel, int>
        {
            [a] = a.Order,
            [b] = b.Order,
        };

        d.SendToFront(a);
        var after = new Dictionary<SelectableModel, int>
        {
            [a] = a.Order,
            [b] = b.Order,
        };
        var command = new SetModelOrdersCommand(d, before, after);

        command.Undo();
        a.Order.ShouldBe(before[a]);
        b.Order.ShouldBe(before[b]);

        command.Execute();
        a.Order.ShouldBe(after[a]);
        b.Order.ShouldBe(after[b]);
    }

    [Fact]
    public void Remove_node_undo_restores_node_and_its_links()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var n1 = d.Nodes.Add(new NodeModel(new Point(0, 0))); n1.Size = new Size(20, 20);
        var n2 = d.Nodes.Add(new NodeModel(new Point(100, 0))); n2.Size = new Size(20, 20);
        var p1 = n1.AddPort(new PortModel(n1, PortAlignment.Right)); p1.Initialized = true;
        var p2 = n2.AddPort(new PortModel(n2, PortAlignment.Left)); p2.Initialized = true;
        var link = d.Links.Add(new LinkModel(p1, p2));
        var stack = new UndoRedoStack();

        stack.Execute(new RemoveNodeCommand(d, n1));
        d.Nodes.Count.ShouldBe(1);
        d.Links.Count.ShouldBe(0); // link cascaded out with the node

        stack.Undo();
        d.Nodes.Count.ShouldBe(2);
        d.Links.Count.ShouldBe(1);
        p1.Links.ShouldContain(link);
    }

    [Fact]
    public void Add_group_undo_and_redo_toggles_group_membership()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var a = d.Nodes.Add(new NodeModel(new Point(0, 0)) { Size = new Size(20, 20) });
        var b = d.Nodes.Add(new NodeModel(new Point(50, 0)) { Size = new Size(20, 20) });
        var group = new GroupModel(new[] { a, b });
        var stack = new UndoRedoStack();

        stack.Execute(new AddGroupCommand(d, group));
        d.Groups.Count.ShouldBe(1);
        a.Group.ShouldBeSameAs(group);

        stack.Undo();
        d.Groups.Count.ShouldBe(0);
        a.Group.ShouldBeNull();

        stack.Redo();
        d.Groups.Count.ShouldBe(1);
        a.Group.ShouldBeSameAs(group);
        b.Group.ShouldBeSameAs(group);
    }

    [Fact]
    public void Remove_group_undo_restores_group_without_deleting_children()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var a = d.Nodes.Add(new NodeModel(new Point(0, 0)) { Size = new Size(20, 20) });
        var b = d.Nodes.Add(new NodeModel(new Point(50, 0)) { Size = new Size(20, 20) });
        var group = d.Groups.Add(new GroupModel(new[] { a, b }));
        var stack = new UndoRedoStack();

        stack.Execute(new RemoveGroupCommand(d, group));
        d.Groups.Count.ShouldBe(0);
        d.Nodes.Count.ShouldBe(2);
        a.Group.ShouldBeNull();

        stack.Undo();
        d.Groups.Count.ShouldBe(1);
        a.Group.ShouldBeSameAs(group);
        b.Group.ShouldBeSameAs(group);
    }

    [Fact]
    public void Link_vertex_commands_restore_the_vertex_at_its_original_index()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var a = d.Nodes.Add(new NodeModel(new Point(0, 0)) { Size = new Size(20, 20) });
        var b = d.Nodes.Add(new NodeModel(new Point(200, 0)) { Size = new Size(20, 20) });
        var link = d.Links.Add(new LinkModel(a, b));
        var first = link.AddVertex(new Point(40, 20));
        var inserted = new LinkVertexModel(link, new Point(80, 40));
        var stack = new UndoRedoStack();

        stack.Execute(new AddLinkVertexCommand(link, inserted, 1));
        link.Vertices[1].ShouldBeSameAs(inserted);

        stack.Undo();
        link.Vertices.Count.ShouldBe(1);
        link.Vertices[0].ShouldBeSameAs(first);

        stack.Redo();
        stack.Execute(new RemoveLinkVertexCommand(link, inserted));
        link.Vertices.ShouldNotContain(inserted);

        stack.Undo();
        link.Vertices[1].ShouldBeSameAs(inserted);
    }

    [Fact]
    public void History_records_a_completed_move_and_undo_redo_restores_it()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var node = d.Nodes.Add(new NodeModel(new Point(10, 10)) { Size = new Size(20, 20) });
        using var history = new DiagramHistory(d);

        node.SetPosition(80, 60);
        node.TriggerMoved(); // a drag completes here

        history.CanUndo.ShouldBeTrue();
        history.Undo();
        node.Position.ShouldBe(new Point(10, 10));
        history.Redo();
        node.Position.ShouldBe(new Point(80, 60));
    }

    [Fact]
    public void History_records_a_link_add_and_undo_redo_toggles_it()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var a = d.Nodes.Add(new NodeModel(new Point(0, 0)) { Size = new Size(20, 20) });
        var b = d.Nodes.Add(new NodeModel(new Point(100, 0)) { Size = new Size(20, 20) });
        using var history = new DiagramHistory(d);

        d.Links.Add(new LinkModel(a, b));
        d.Links.Count.ShouldBe(1);
        history.CanUndo.ShouldBeTrue();

        history.Undo();
        d.Links.Count.ShouldBe(0);
        history.Redo();
        d.Links.Count.ShouldBe(1);
    }

    [Fact]
    public void History_delete_via_execute_is_undoable_and_the_restore_is_not_re_recorded()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var a = d.Nodes.Add(new NodeModel(new Point(0, 0))); a.Size = new Size(20, 20);
        var b = d.Nodes.Add(new NodeModel(new Point(100, 0))); b.Size = new Size(20, 20);
        var p1 = a.AddPort(new PortModel(a, PortAlignment.Right)); p1.Initialized = true;
        var p2 = b.AddPort(new PortModel(b, PortAlignment.Left)); p2.Initialized = true;
        d.Links.Add(new LinkModel(p1, p2)); // added before history -> part of the baseline
        using var history = new DiagramHistory(d);

        history.Execute(new RemoveNodeCommand(d, a));
        d.Nodes.Count.ShouldBe(1);
        d.Links.Count.ShouldBe(0);

        history.Undo(); // re-adds node + link while IsApplying -> must not record new commands
        d.Nodes.Count.ShouldBe(2);
        d.Links.Count.ShouldBe(1);
        history.CanUndo.ShouldBeFalse(); // the delete was the only entry
        history.CanRedo.ShouldBeTrue();

        history.Redo();
        d.Nodes.Count.ShouldBe(1);
    }

    [Fact]
    public void Transaction_groups_multiple_moves_into_one_undo_step()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var n1 = d.Nodes.Add(new NodeModel(new Point(0, 0)) { Size = new Size(20, 20) });
        var n2 = d.Nodes.Add(new NodeModel(new Point(50, 0)) { Size = new Size(20, 20) });
        using var history = new DiagramHistory(d);

        using (history.Transaction())
        {
            n1.SetPosition(10, 10); n1.TriggerMoved();
            n2.SetPosition(60, 10); n2.TriggerMoved();
        }

        history.Undo(); // one undo reverts both moves
        n1.Position.ShouldBe(new Point(0, 0));
        n2.Position.ShouldBe(new Point(50, 0));
        history.CanUndo.ShouldBeFalse(); // it was a single grouped entry
    }

    [Fact]
    public void Transaction_cancels_a_link_added_then_removed_within_it()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var a = d.Nodes.Add(new NodeModel(new Point(0, 0)) { Size = new Size(20, 20) });
        var b = d.Nodes.Add(new NodeModel(new Point(100, 0)) { Size = new Size(20, 20) });
        using var history = new DiagramHistory(d);

        history.BeginTransaction();
        var link = d.Links.Add(new LinkModel(a, b)); // a transient drag-new-link...
        d.Links.Remove(link);                        // ...discarded before the gesture ends
        history.EndTransaction();

        history.CanUndo.ShouldBeFalse(); // nothing committed
    }
}
