using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nodely;
using Nodely.Avalonia;
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Database;
using Nodely.Avalonia.MindMap;
using Nodely.Avalonia.StateMachine;
using Nodely.Avalonia.Uml;
using Nodely.Avalonia.Workflow;
using Nodely.Models;
using Nodely.Serialization;
using Shouldly;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Tests;

public class StateMachinePackRenderingTests
{
    [AvaloniaFact]
    public void UseStateMachineNodes_registers_state_port_and_transition_renderers()
    {
        var diagram = new NodelyDiagram();
        var initial = diagram.Nodes.Add(new StateMachineInitialNode(new NodelyPoint(0, 0), "Start"));
        var state = diagram.Nodes.Add(new StateMachineStateNode(new NodelyPoint(180, 0), "Waiting")
        {
            EntryAction = "begin",
            ExitAction = "complete",
        });
        var choice = diagram.Nodes.Add(new StateMachineChoiceNode(new NodelyPoint(460, 0), "Valid?"));
        var final = diagram.Nodes.Add(new StateMachineFinalNode(new NodelyPoint(680, 0), "Done"));
        var note = diagram.Nodes.Add(new StateMachineNoteNode(new NodelyPoint(180, 210), "Self transitions use a pack drawer."));

        var initialOut = initial.AddPort(new StateMachinePortModel(initial, PortAlignment.Right, StateMachinePortRole.Exit, "out"));
        var stateIn = state.AddPort(new StateMachinePortModel(state, PortAlignment.Left, StateMachinePortRole.Entry, "in"));
        var transition = diagram.Links.Add(new StateMachineTransitionLink(initialOut, stateIn)
        {
            Trigger = "created",
        });
        var timeout = diagram.Links.Add(new StateMachineTransitionLink(state, choice, StateMachineTransitionKind.Timeout)
        {
            Trigger = "elapsed",
        });
        var self = diagram.Links.Add(new StateMachineTransitionLink(state, state, StateMachineTransitionKind.Self)
        {
            Trigger = "retry",
        });

        var canvas = new DiagramCanvas { Diagram = diagram, Palette = NodelyPalettes.Light }.UseStateMachineNodes();

        canvas.BuildNodeContent(initial).ShouldBeOfType<Grid>().Tag.ShouldBe("statemachine-initial-node");
        canvas.BuildNodeContent(state).ShouldBeOfType<Border>().Tag.ShouldBe("statemachine-state-node");
        canvas.BuildNodeContent(choice).ShouldBeOfType<Grid>().Tag.ShouldBe("statemachine-choice-node");
        canvas.BuildNodeContent(final).ShouldBeOfType<Grid>().Tag.ShouldBe("statemachine-final-node");
        canvas.BuildNodeContent(note).ShouldBeOfType<Border>().Tag.ShouldBe("statemachine-note-node");
        canvas.BuildPortContent((StateMachinePortModel)initialOut).ShouldBeOfType<Grid>().Tag.ShouldBe("statemachine-port");
        canvas.ResolveLinkDrawer(transition).ShouldNotBeNull();
        canvas.ResolveLinkDrawer(self).ShouldNotBeNull();
        canvas.ResolveLinkStyle(timeout).DashStyle.ShouldNotBeNull();

        canvas.Palette = NodelyPalettes.Dark;
        canvas.BuildNodeContent(state).ShouldBeOfType<Border>().Tag.ShouldBe("statemachine-state-node");
    }

    [AvaloniaFact]
    public void Side_pack_registrations_compose_on_canvas_and_serializer_registry()
    {
        var diagram = new NodelyDiagram();
        var table = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(0, 0), "Customers"));
        var root = diagram.Nodes.Add(new MindMapRootNode(new NodelyPoint(260, 0), "Plan"));
        var entity = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(520, 0), "Customer"));
        var task = diagram.Nodes.Add(new WorkflowTaskNode(new NodelyPoint(780, 0), "Sync"));
        var state = diagram.Nodes.Add(new StateMachineStateNode(new NodelyPoint(1040, 0), "Waiting"));
        var final = diagram.Nodes.Add(new StateMachineFinalNode(new NodelyPoint(1300, 0), "Done"));
        var transition = diagram.Links.Add(new StateMachineTransitionLink(state, final)
        {
            Trigger = "complete",
        });

        var canvas = new DiagramCanvas { Diagram = diagram }
            .UseDatabaseNodes()
            .UseMindMapNodes()
            .UseStateMachineNodes()
            .UseUmlNodes()
            .UseWorkflowNodes();
        canvas.BuildNodeContent(table).ShouldBeOfType<Border>().Tag.ShouldBe("database-table-node");
        canvas.BuildNodeContent(root).ShouldBeOfType<Border>().Tag.ShouldBe("mindmap-root-node");
        canvas.BuildNodeContent(entity).ShouldBeOfType<Border>().Tag.ShouldBe("uml-class-node");
        canvas.BuildNodeContent(task).ShouldBeOfType<Border>().Tag.ShouldBe("workflow-task-node");
        canvas.BuildNodeContent(state).ShouldBeOfType<Border>().Tag.ShouldBe("statemachine-state-node");
        canvas.ResolveLinkDrawer(transition).ShouldNotBeNull();

        var registry = DatabaseNodeFactory.CreateRegistry()
            .UseMindMapNodes()
            .UseStateMachineNodes()
            .UseUmlNodes()
            .UseWorkflowNodes();
        var json = DiagramSerializer.Serialize(diagram);
        var loaded = new NodelyDiagram();
        DiagramSerializer.Deserialize(loaded, json, registry);

        loaded.Nodes.ShouldContain(n => n is DatabaseTableNode);
        loaded.Nodes.ShouldContain(n => n is MindMapRootNode);
        loaded.Nodes.ShouldContain(n => n is UmlClassNode);
        loaded.Nodes.ShouldContain(n => n is WorkflowTaskNode);
        loaded.Nodes.ShouldContain(n => n is StateMachineStateNode);
        loaded.Links.Single().ShouldBeOfType<StateMachineTransitionLink>();
    }
}
