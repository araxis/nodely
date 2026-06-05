using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Database;
using Nodely.Avalonia.Uml;
using Nodely.Avalonia.Workflow;
using Nodely.Serialization;
using Shouldly;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Tests;

public class WorkflowPackRenderingTests
{
    [AvaloniaFact]
    public void UseWorkflowNodes_registers_workflow_renderers_and_link_drawing()
    {
        var diagram = new NodelyDiagram();
        var start = diagram.Nodes.Add(new WorkflowStartNode(new NodelyPoint(40, 40), "Start"));
        var end = diagram.Nodes.Add(new WorkflowEndNode(new NodelyPoint(820, 40), "End"));
        var task = diagram.Nodes.Add(new WorkflowTaskNode(new NodelyPoint(220, 40), "Review") { TaskType = WorkflowTaskType.User });
        var decision = diagram.Nodes.Add(new WorkflowDecisionNode(new NodelyPoint(420, 40), "Approved?") { Condition = "amount <= limit" });
        var gateway = diagram.Nodes.Add(new WorkflowGatewayNode(new NodelyPoint(620, 40), "Route") { GatewayKind = WorkflowGatewayKind.Parallel });
        var eventNode = diagram.Nodes.Add(new WorkflowEventNode(new NodelyPoint(420, 260), "Timeout") { EventKind = WorkflowEventKind.Timer });
        var note = diagram.Nodes.Add(new WorkflowNoteNode(new NodelyPoint(620, 260), "First release has no execution engine."));
        var link = diagram.Links.Add(new WorkflowLink(task, decision, WorkflowLinkKind.Conditional)
        {
            Label = "route",
            Condition = "approved",
        });

        var canvas = new DiagramCanvas { Diagram = diagram }.UseWorkflowNodes();

        canvas.BuildNodeContent(start).ShouldBeOfType<Border>().Tag.ShouldBe("workflow-start-node");
        canvas.BuildNodeContent(end).ShouldBeOfType<Border>().Tag.ShouldBe("workflow-end-node");
        canvas.BuildNodeContent(task).ShouldBeOfType<Border>().Tag.ShouldBe("workflow-task-node");
        canvas.BuildNodeContent(decision).ShouldBeOfType<Border>().Tag.ShouldBe("workflow-decision-node");
        canvas.BuildNodeContent(gateway).ShouldBeOfType<Border>().Tag.ShouldBe("workflow-gateway-node");
        canvas.BuildNodeContent(eventNode).ShouldBeOfType<Border>().Tag.ShouldBe("workflow-event-node");
        canvas.BuildNodeContent(note).ShouldBeOfType<Border>().Tag.ShouldBe("workflow-note-node");
        canvas.ResolveLinkDrawer(link).ShouldNotBeNull();
        canvas.ResolveLinkStyle(link).DashStyle.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public void Side_pack_registrations_compose_on_canvas_and_serializer_registry()
    {
        var diagram = new NodelyDiagram();
        var table = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(0, 0), "Customers"));
        var entity = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(260, 0), "Customer"));
        var task = diagram.Nodes.Add(new WorkflowTaskNode(new NodelyPoint(520, 0), "Sync"));
        var link = diagram.Links.Add(new WorkflowLink(entity, task, WorkflowLinkKind.Message)
        {
            Label = "sync",
        });

        var canvas = new DiagramCanvas { Diagram = diagram }.UseDatabaseNodes().UseUmlNodes().UseWorkflowNodes();
        canvas.BuildNodeContent(table).ShouldBeOfType<Border>().Tag.ShouldBe("database-table-node");
        canvas.BuildNodeContent(entity).ShouldBeOfType<Border>().Tag.ShouldBe("uml-class-node");
        canvas.BuildNodeContent(task).ShouldBeOfType<Border>().Tag.ShouldBe("workflow-task-node");
        canvas.ResolveLinkDrawer(link).ShouldNotBeNull();

        var registry = DatabaseNodeFactory.CreateRegistry()
            .UseUmlNodes()
            .UseWorkflowNodes();
        var json = DiagramSerializer.Serialize(diagram);
        var loaded = new NodelyDiagram();
        DiagramSerializer.Deserialize(loaded, json, registry);

        loaded.Nodes.ShouldContain(n => n is DatabaseTableNode);
        loaded.Nodes.ShouldContain(n => n is UmlClassNode);
        loaded.Nodes.ShouldContain(n => n is WorkflowTaskNode);
        loaded.Links.Single().ShouldBeOfType<WorkflowLink>();
    }
}
