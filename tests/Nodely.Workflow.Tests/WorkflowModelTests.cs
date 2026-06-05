using System.Linq;
using Nodely;
using Nodely.Avalonia.Workflow;
using Nodely.Geometry;
using Nodely.Serialization;
using Shouldly;
using Xunit;

namespace Nodely.Workflow.Tests;

public class WorkflowModelTests
{
    [Fact]
    public void Task_node_defaults_to_label_and_mutable_status()
    {
        var task = new WorkflowTaskNode(new Point(10, 20), "Approve request");

        task.Label.ShouldBe("Approve request");
        task.Title.ShouldBe("Approve request");
        task.TaskType.ShouldBe(WorkflowTaskType.Task);
        task.Status.ShouldBe(WorkflowTaskStatus.Draft);

        task.TaskType = WorkflowTaskType.User;
        task.Status = WorkflowTaskStatus.Ready;
        task.Notes = "Needs manager approval";

        task.TaskType.ShouldBe(WorkflowTaskType.User);
        task.Status.ShouldBe(WorkflowTaskStatus.Ready);
        task.Notes.ShouldBe("Needs manager approval");
    }

    [Fact]
    public void Clone_copies_workflow_node_data()
    {
        var node = new WorkflowTaskNode(new Point(30, 40), "Send invoice")
        {
            TaskType = WorkflowTaskType.Service,
            Status = WorkflowTaskStatus.Running,
            Notes = "Retryable",
            Size = new Size(220, 120),
        };

        var clone = node.Clone().ShouldBeOfType<WorkflowTaskNode>();

        clone.ShouldNotBeSameAs(node);
        clone.Label.ShouldBe("Send invoice");
        clone.TaskType.ShouldBe(WorkflowTaskType.Service);
        clone.Status.ShouldBe(WorkflowTaskStatus.Running);
        clone.Notes.ShouldBe("Retryable");
        clone.Size.ShouldBe(new Size(220, 120));
    }

    [Fact]
    public void Extra_data_round_trips_workflow_fields_through_serializer()
    {
        var diagram = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var start = diagram.Nodes.Add(new WorkflowStartNode("start", new Point(10, 20), "Submitted"));
        var review = diagram.Nodes.Add(new WorkflowTaskNode("review", new Point(260, 20), "Review")
        {
            TaskType = WorkflowTaskType.User,
            Status = WorkflowTaskStatus.Ready,
            Notes = "Two approvers",
        });
        var decision = diagram.Nodes.Add(new WorkflowDecisionNode("decision", new Point(520, 20), "Approved?")
        {
            Condition = "amount <= limit",
        });
        var eventNode = diagram.Nodes.Add(new WorkflowEventNode("timeout", new Point(520, 220), "Timeout")
        {
            EventKind = WorkflowEventKind.Timer,
        });

        diagram.Links.Add(new WorkflowLink(start, review, WorkflowLinkKind.Sequence) { Label = "submit" });
        diagram.Links.Add(new WorkflowLink(review, decision, WorkflowLinkKind.Conditional)
        {
            Label = "route",
            Condition = "approved",
        });
        diagram.Links.Add(new WorkflowLink(decision, eventNode, WorkflowLinkKind.Error)
        {
            Label = "timeout",
        });

        var json = DiagramSerializer.Serialize(diagram);

        var loaded = new NodelyDiagram(null, registerDefaultBehaviors: false);
        DiagramSerializer.Deserialize(loaded, json, WorkflowNodeFactory.CreateRegistry());

        var restoredReview = loaded.Nodes.Single(n => n.Id == "review").ShouldBeOfType<WorkflowTaskNode>();
        restoredReview.TaskType.ShouldBe(WorkflowTaskType.User);
        restoredReview.Status.ShouldBe(WorkflowTaskStatus.Ready);
        restoredReview.Notes.ShouldBe("Two approvers");

        var restoredDecision = loaded.Nodes.Single(n => n.Id == "decision").ShouldBeOfType<WorkflowDecisionNode>();
        restoredDecision.Condition.ShouldBe("amount <= limit");

        var restoredEvent = loaded.Nodes.Single(n => n.Id == "timeout").ShouldBeOfType<WorkflowEventNode>();
        restoredEvent.EventKind.ShouldBe(WorkflowEventKind.Timer);

        var restoredConditional = loaded.Links.OfType<WorkflowLink>().Single(link => link.Kind == WorkflowLinkKind.Conditional);
        restoredConditional.Label.ShouldBe("route");
        restoredConditional.Condition.ShouldBe("approved");
        restoredConditional.Labels.Select(label => label.Content).ShouldBe(new[] { "route", "approved" });
    }

    [Fact]
    public void Factory_restores_workflow_nodes()
    {
        WorkflowNodeFactory.Create(new NodeSnapshot { Kind = WorkflowStartNode.ModelKindKey, Title = "Start", X = 1, Y = 2 })
            .ShouldBeOfType<WorkflowStartNode>();
        WorkflowNodeFactory.Create(new NodeSnapshot { Kind = WorkflowEndNode.ModelKindKey, Title = "End", X = 1, Y = 2 })
            .ShouldBeOfType<WorkflowEndNode>();
        WorkflowNodeFactory.Create(new NodeSnapshot { Kind = WorkflowTaskNode.ModelKindKey, Title = "Task", X = 1, Y = 2 })
            .ShouldBeOfType<WorkflowTaskNode>();
        WorkflowNodeFactory.Create(new NodeSnapshot { Kind = WorkflowDecisionNode.ModelKindKey, Title = "Decision", X = 1, Y = 2 })
            .ShouldBeOfType<WorkflowDecisionNode>();
        WorkflowNodeFactory.Create(new NodeSnapshot { Kind = WorkflowGatewayNode.ModelKindKey, Title = "Gateway", X = 1, Y = 2 })
            .ShouldBeOfType<WorkflowGatewayNode>();
        WorkflowNodeFactory.Create(new NodeSnapshot { Kind = WorkflowEventNode.ModelKindKey, Title = "Event", X = 1, Y = 2 })
            .ShouldBeOfType<WorkflowEventNode>();
        WorkflowNodeFactory.Create(new NodeSnapshot { Kind = WorkflowNoteNode.ModelKindKey, Title = "Note", X = 1, Y = 2 })
            .ShouldBeOfType<WorkflowNoteNode>();
    }

    [Fact]
    public void Workflow_link_sets_metadata_defaults_and_labels()
    {
        var source = new WorkflowTaskNode(new Point(0, 0), "Review");
        var target = new WorkflowEndNode(new Point(200, 0), "Rejected");
        var link = new WorkflowLink(source, target, WorkflowLinkKind.Conditional)
        {
            Label = "reject",
            Condition = "has errors",
        };

        link.Kind.ShouldBe(WorkflowLinkKind.Conditional);
        link.Segmentable.ShouldBeTrue();
        link.TargetMarker.ShouldNotBeNull();
        link.Width.ShouldBe(2.2);
        link.Label.ShouldBe("reject");
        link.Condition.ShouldBe("has errors");
        link.Labels.Select(label => label.Content).ShouldBe(new[] { "reject", "has errors" });
    }
}
