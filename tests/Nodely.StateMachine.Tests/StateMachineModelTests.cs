using System.Collections.Generic;
using System.Linq;
using Nodely;
using Nodely.Avalonia.StateMachine;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;
using Shouldly;
using Xunit;

namespace Nodely.StateMachine.Tests;

public class StateMachineModelTests
{
    [Fact]
    public void State_node_defaults_to_name_actions_and_accent()
    {
        var state = new StateMachineStateNode(new Point(10, 20), "Waiting")
        {
            Description = "Accepts incoming work",
            EntryAction = "start timer",
            ExitAction = "stop timer",
            AccentColor = "#37A779",
        };

        state.Name.ShouldBe("Waiting");
        state.Title.ShouldBe("Waiting");
        state.Description.ShouldBe("Accepts incoming work");
        state.EntryAction.ShouldBe("start timer");
        state.ExitAction.ShouldBe("stop timer");
        state.AccentColor.ShouldBe("#37A779");
    }

    [Fact]
    public void Clone_copies_state_machine_node_data()
    {
        var node = new StateMachineStateNode(new Point(30, 40), "Active")
        {
            Description = "Processing requests",
            EntryAction = "allocate",
            ExitAction = "release",
            AccentColor = "#4D9EFF",
            Size = new Size(240, 130),
        };

        var clone = node.Clone().ShouldBeOfType<StateMachineStateNode>();

        clone.ShouldNotBeSameAs(node);
        clone.Name.ShouldBe("Active");
        clone.Description.ShouldBe("Processing requests");
        clone.EntryAction.ShouldBe("allocate");
        clone.ExitAction.ShouldBe("release");
        clone.AccentColor.ShouldBe("#4D9EFF");
        clone.Size.ShouldBe(new Size(240, 130));
    }

    [Fact]
    public void Extra_data_round_trips_state_machine_fields_through_serializer()
    {
        var diagram = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var initial = diagram.Nodes.Add(new StateMachineInitialNode("initial", new Point(0, 0), "Boot"));
        var waiting = diagram.Nodes.Add(new StateMachineStateNode("waiting", new Point(260, 0), "Waiting")
        {
            Description = "Idle state",
            EntryAction = "show ready",
            ExitAction = "clear ready",
            AccentColor = "#37A779",
        });
        var choice = diagram.Nodes.Add(new StateMachineChoiceNode("choice", new Point(520, 0), "Can run?"));
        var final = diagram.Nodes.Add(new StateMachineFinalNode("final", new Point(780, 0), "Done"));
        var note = diagram.Nodes.Add(new StateMachineNoteNode("note", new Point(260, 220), "Transitions store trigger, guard, action, and priority.")
        {
            AccentColor = "#D89C35",
        });

        var initialOut = initial.AddPort(new StateMachinePortModel(initial, PortAlignment.Right, StateMachinePortRole.Exit, "out"));
        var waitingIn = waiting.AddPort(new StateMachinePortModel(waiting, PortAlignment.Left, StateMachinePortRole.Entry, "in"));
        var waitingOut = waiting.AddPort(new StateMachinePortModel(waiting, PortAlignment.Right, StateMachinePortRole.Exit, "done"));
        var choiceIn = choice.AddPort(new StateMachinePortModel(choice, PortAlignment.Left, StateMachinePortRole.Entry, "in"));

        diagram.Links.Add(new StateMachineTransitionLink(initialOut, waitingIn)
        {
            Trigger = "created",
            Action = "load",
            Priority = 1,
        });
        diagram.Links.Add(new StateMachineTransitionLink(waitingOut, choiceIn, StateMachineTransitionKind.Choice)
        {
            Trigger = "submit",
            Guard = "has data",
            Action = "validate",
            AccentColor = "#8B68B8",
            Priority = 2,
        });
        diagram.Links.Add(new StateMachineTransitionLink(choice, final)
        {
            Trigger = "accepted",
        });
        diagram.Links.Add(new StateMachineTransitionLink(waiting, waiting, StateMachineTransitionKind.Self)
        {
            Trigger = "retry",
            Guard = "attempts < 3",
        });

        var json = DiagramSerializer.Serialize(diagram);

        var loaded = new NodelyDiagram(null, registerDefaultBehaviors: false);
        DiagramSerializer.Deserialize(loaded, json, StateMachineNodeFactory.CreateRegistry());

        var restoredWaiting = loaded.Nodes.Single(n => n.Id == "waiting").ShouldBeOfType<StateMachineStateNode>();
        restoredWaiting.Description.ShouldBe("Idle state");
        restoredWaiting.EntryAction.ShouldBe("show ready");
        restoredWaiting.ExitAction.ShouldBe("clear ready");
        restoredWaiting.Ports.OfType<StateMachinePortModel>().ShouldContain(port => port.Role == StateMachinePortRole.Entry && port.Name == "in");

        var restoredNote = loaded.Nodes.Single(n => n.Id == "note").ShouldBeOfType<StateMachineNoteNode>();
        restoredNote.Text.ShouldBe("Transitions store trigger, guard, action, and priority.");
        restoredNote.AccentColor.ShouldBe("#D89C35");

        var restoredChoice = loaded.Links.OfType<StateMachineTransitionLink>().Single(link => link.Kind == StateMachineTransitionKind.Choice);
        restoredChoice.Trigger.ShouldBe("submit");
        restoredChoice.Guard.ShouldBe("has data");
        restoredChoice.Action.ShouldBe("validate");
        restoredChoice.Priority.ShouldBe(2);
        restoredChoice.AccentColor.ShouldBe("#8B68B8");
        restoredChoice.Labels.Single().Content.ShouldBe("submit [has data] / validate");
    }

    [Fact]
    public void Factory_restores_state_machine_nodes()
    {
        StateMachineNodeFactory.Create(new NodeSnapshot { Kind = StateMachineInitialNode.ModelKindKey, Title = "Initial", X = 1, Y = 2 })
            .ShouldBeOfType<StateMachineInitialNode>();
        StateMachineNodeFactory.Create(new NodeSnapshot { Kind = StateMachineStateNode.ModelKindKey, Title = "State", X = 1, Y = 2 })
            .ShouldBeOfType<StateMachineStateNode>();
        StateMachineNodeFactory.Create(new NodeSnapshot { Kind = StateMachineFinalNode.ModelKindKey, Title = "Final", X = 1, Y = 2 })
            .ShouldBeOfType<StateMachineFinalNode>();
        StateMachineNodeFactory.Create(new NodeSnapshot { Kind = StateMachineChoiceNode.ModelKindKey, Title = "Choice", X = 1, Y = 2 })
            .ShouldBeOfType<StateMachineChoiceNode>();
        StateMachineNodeFactory.Create(new NodeSnapshot { Kind = StateMachineNoteNode.ModelKindKey, Title = "Note", X = 1, Y = 2 })
            .ShouldBeOfType<StateMachineNoteNode>();
    }

    [Fact]
    public void Transition_link_sets_metadata_defaults_and_labels()
    {
        var source = new StateMachineStateNode(new Point(0, 0), "Waiting");
        var target = new StateMachineStateNode(new Point(200, 0), "Active");
        var link = new StateMachineTransitionLink(source, target, StateMachineTransitionKind.Timeout)
        {
            Trigger = "elapsed",
            Guard = "no response",
            Action = "notify",
            Priority = 4,
            AccentColor = "#D18B30",
        };

        link.Kind.ShouldBe(StateMachineTransitionKind.Timeout);
        link.Segmentable.ShouldBeTrue();
        link.TargetMarker.ShouldNotBeNull();
        link.Width.ShouldBe(2.4);
        link.Trigger.ShouldBe("elapsed");
        link.Guard.ShouldBe("no response");
        link.Action.ShouldBe("notify");
        link.Priority.ShouldBe(4);
        link.AccentColor.ShouldBe("#D18B30");
        link.Labels.Single().Content.ShouldBe("elapsed [no response] / notify");
    }

    [Fact]
    public void Port_model_persists_role_and_name()
    {
        var node = new StateMachineStateNode(new Point(0, 0), "Waiting");
        var port = new StateMachinePortModel(node, PortAlignment.Right, StateMachinePortRole.Exit, "timeout");

        var data = port.GetExtraData();
        var restored = new StateMachinePortModel(node);
        restored.SetExtraData(new Dictionary<string, object?>(data));

        restored.Role.ShouldBe(StateMachinePortRole.Exit);
        restored.Name.ShouldBe("timeout");
    }

    [Fact]
    public void Arrange_places_reachable_states_in_columns_and_ignores_self_transitions()
    {
        var diagram = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var initial = diagram.Nodes.Add(new StateMachineInitialNode(new Point(0, 0), "Start") { Size = new Size(64, 64) });
        var waiting = diagram.Nodes.Add(new StateMachineStateNode(new Point(0, 0), "Waiting") { Size = new Size(220, 110) });
        var active = diagram.Nodes.Add(new StateMachineStateNode(new Point(0, 0), "Active") { Size = new Size(220, 110) });
        var final = diagram.Nodes.Add(new StateMachineFinalNode(new Point(0, 0), "Done") { Size = new Size(64, 64) });

        diagram.Links.Add(new StateMachineTransitionLink(initial, waiting));
        diagram.Links.Add(new StateMachineTransitionLink(waiting, active));
        diagram.Links.Add(new StateMachineTransitionLink(active, final));
        diagram.Links.Add(new StateMachineTransitionLink(active, active, StateMachineTransitionKind.Self));

        StateMachineLayout.Arrange(diagram, new StateMachineLayoutOptions { OriginX = 0, OriginY = 0, ColumnSpacing = 240 });

        waiting.Position.X.ShouldBeGreaterThan(initial.Position.X);
        active.Position.X.ShouldBeGreaterThan(waiting.Position.X);
        final.Position.X.ShouldBeGreaterThan(active.Position.X);
    }

    [Fact]
    public void Arrange_handles_retry_cycles_without_requeueing_forever()
    {
        var diagram = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var initial = diagram.Nodes.Add(new StateMachineInitialNode(new Point(0, 0), "Start") { Size = new Size(64, 64) });
        var idle = diagram.Nodes.Add(new StateMachineStateNode(new Point(0, 0), "Idle") { Size = new Size(220, 110) });
        var running = diagram.Nodes.Add(new StateMachineStateNode(new Point(0, 0), "Running") { Size = new Size(220, 110) });
        var delayed = diagram.Nodes.Add(new StateMachineStateNode(new Point(0, 0), "Delayed") { Size = new Size(220, 110) });
        var final = diagram.Nodes.Add(new StateMachineFinalNode(new Point(0, 0), "Done") { Size = new Size(64, 64) });

        diagram.Links.Add(new StateMachineTransitionLink(initial, idle));
        diagram.Links.Add(new StateMachineTransitionLink(idle, running));
        diagram.Links.Add(new StateMachineTransitionLink(running, delayed, StateMachineTransitionKind.Timeout));
        diagram.Links.Add(new StateMachineTransitionLink(delayed, idle, StateMachineTransitionKind.Timeout));
        diagram.Links.Add(new StateMachineTransitionLink(running, final));

        StateMachineLayout.Arrange(diagram, new StateMachineLayoutOptions { OriginX = 0, OriginY = 0, ColumnSpacing = 240 });

        idle.Position.X.ShouldBeGreaterThan(initial.Position.X);
        running.Position.X.ShouldBeGreaterThan(idle.Position.X);
        delayed.Position.X.ShouldBeGreaterThan(running.Position.X);
        final.Position.X.ShouldBeGreaterThan(running.Position.X);
    }
}
