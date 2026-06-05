using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nodely;
using Nodely.Avalonia;
using Nodely.Avalonia.Api;
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Database;
using Nodely.Avalonia.MindMap;
using Nodely.Avalonia.Network;
using Nodely.Avalonia.StateMachine;
using Nodely.Avalonia.Uml;
using Nodely.Avalonia.Workflow;
using Nodely.Models;
using Nodely.Serialization;
using Shouldly;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Tests;

public class PackageCompositionTests
{
    [AvaloniaFact]
    public void All_side_package_renderers_ports_and_link_styles_coexist()
    {
        var fixture = CreateCompositionFixture();
        var canvas = CreateComposedCanvas(fixture.Diagram, NodelyPalettes.Light);

        canvas.BuildNodeContent(fixture.Api).ShouldBeOfType<Border>().Tag.ShouldBe("api-endpoint-node");
        canvas.BuildNodeContent(fixture.Database).ShouldBeOfType<Border>().Tag.ShouldBe("database-table-node");
        canvas.BuildNodeContent(fixture.MindMap).ShouldBeOfType<Border>().Tag.ShouldBe("mindmap-root-node");
        canvas.BuildNodeContent(fixture.Network).ShouldBeOfType<Border>().Tag.ShouldBe("network-router-node");
        canvas.BuildNodeContent(fixture.State).ShouldBeOfType<Border>().Tag.ShouldBe("statemachine-state-node");
        canvas.BuildNodeContent(fixture.Uml).ShouldBeOfType<Border>().Tag.ShouldBe("uml-class-node");
        canvas.BuildNodeContent(fixture.Workflow).ShouldBeOfType<Border>().Tag.ShouldBe("workflow-task-node");

        canvas.BuildPortContent(fixture.ApiPort).ShouldBeOfType<Grid>().Tag.ShouldBe("api-port");
        canvas.BuildPortContent(fixture.DatabasePort).ShouldBeOfType<Grid>().Tag.ShouldBe("database-relationship-port");
        canvas.BuildPortContent(fixture.MindMapPort).ShouldBeOfType<Grid>().Tag.ShouldBe("mindmap-port");
        canvas.BuildPortContent(fixture.NetworkPort).ShouldBeOfType<Grid>().Tag.ShouldBe("network-port");
        canvas.BuildPortContent(fixture.StatePort).ShouldBeOfType<Grid>().Tag.ShouldBe("statemachine-port");
        canvas.BuildPortContent(fixture.UmlPort).ShouldBeOfType<Grid>().Tag.ShouldBe("uml-port");

        canvas.ResolveLinkDrawer(fixture.ApiLink).ShouldNotBeNull();
        canvas.ResolveLinkDrawer(fixture.DatabaseLink).ShouldNotBeNull();
        canvas.ResolveLinkDrawer(fixture.MindMapLink).ShouldNotBeNull();
        canvas.ResolveLinkDrawer(fixture.NetworkLink).ShouldNotBeNull();
        canvas.ResolveLinkDrawer(fixture.StateLink).ShouldNotBeNull();
        canvas.ResolveLinkDrawer(fixture.UmlLink).ShouldNotBeNull();
        canvas.ResolveLinkDrawer(fixture.WorkflowLink).ShouldNotBeNull();

        canvas.ResolveLinkStyle(fixture.ApiLink).Width.ShouldBe(fixture.ApiLink.Width);
        canvas.ResolveLinkStyle(fixture.DatabaseLink).Width.ShouldBe(fixture.DatabaseLink.Width);
        canvas.ResolveLinkStyle(fixture.MindMapLink).Width.ShouldBe(fixture.MindMapLink.Width);
        canvas.ResolveLinkStyle(fixture.NetworkLink).Width.ShouldBe(fixture.NetworkLink.Width);
        canvas.ResolveLinkStyle(fixture.StateLink).Width.ShouldBe(fixture.StateLink.Width);
        canvas.ResolveLinkStyle(fixture.UmlLink).Width.ShouldBe(fixture.UmlLink.Width);
        canvas.ResolveLinkStyle(fixture.WorkflowLink).Width.ShouldBe(fixture.WorkflowLink.Width);

        canvas.Palette = NodelyPalettes.Dark;
        canvas.BuildNodeContent(fixture.Api).ShouldBeOfType<Border>().Tag.ShouldBe("api-endpoint-node");
        canvas.BuildNodeContent(fixture.Network).ShouldBeOfType<Border>().Tag.ShouldBe("network-router-node");
    }

    [AvaloniaFact]
    public void All_side_package_registries_round_trip_in_one_serializer()
    {
        var fixture = CreateCompositionFixture();
        var registry = new DiagramSerializationRegistry()
            .UseWorkflowNodes()
            .UseUmlNodes()
            .UseStateMachineNodes()
            .UseNetworkNodes()
            .UseMindMapNodes()
            .UseDatabaseNodes()
            .UseApiNodes();

        var json = DiagramSerializer.Serialize(fixture.Diagram);
        var loaded = new NodelyDiagram();
        DiagramSerializer.Deserialize(loaded, json, registry);

        loaded.Nodes.ShouldContain(n => n is ApiEndpointNode);
        loaded.Nodes.ShouldContain(n => n is DatabaseTableNode);
        loaded.Nodes.ShouldContain(n => n is MindMapRootNode);
        loaded.Nodes.ShouldContain(n => n is NetworkRouterNode);
        loaded.Nodes.ShouldContain(n => n is StateMachineStateNode);
        loaded.Nodes.ShouldContain(n => n is UmlClassNode);
        loaded.Nodes.ShouldContain(n => n is WorkflowTaskNode);

        loaded.Links.Count(link => link is ApiLink).ShouldBe(1);
        loaded.Links.Count(link => link is DatabaseRelationshipLink).ShouldBe(1);
        loaded.Links.Count(link => link is MindMapLink).ShouldBe(1);
        loaded.Links.Count(link => link is NetworkLink).ShouldBe(1);
        loaded.Links.Count(link => link is StateMachineTransitionLink).ShouldBe(1);
        loaded.Links.Count(link => link is UmlRelationshipLink).ShouldBe(1);
        loaded.Links.Count(link => link is WorkflowLink).ShouldBe(1);
    }

    [AvaloniaFact]
    public void Runtime_edits_refresh_composed_pack_visuals_and_remain_undoable()
    {
        var fixture = CreateCompositionFixture();
        var canvas = CreateComposedCanvas(fixture.Diagram, NodelyPalettes.Light);

        canvas.RunAsUndoableEdit(
            () =>
            {
                fixture.Api.Route = "/orders/{id}";
                fixture.Database.Schema = "reporting";
                fixture.Network.Status = NetworkStatus.Warning;
                fixture.Workflow.Status = WorkflowTaskStatus.Running;
            },
            () =>
            {
                fixture.Api.Route = "/orders";
                fixture.Database.Schema = "sales";
                fixture.Network.Status = NetworkStatus.Online;
                fixture.Workflow.Status = WorkflowTaskStatus.Ready;
            });

        fixture.Api.Route.ShouldBe("/orders/{id}");
        fixture.Database.Schema.ShouldBe("reporting");
        fixture.Network.Status.ShouldBe(NetworkStatus.Warning);
        fixture.Workflow.Status.ShouldBe(WorkflowTaskStatus.Running);
        canvas.CanUndo.ShouldBeTrue();
        canvas.BuildNodeContent(fixture.Api).ShouldBeOfType<Border>().Tag.ShouldBe("api-endpoint-node");
        canvas.BuildNodeContent(fixture.Database).ShouldBeOfType<Border>().Tag.ShouldBe("database-table-node");

        canvas.Undo();
        fixture.Api.Route.ShouldBe("/orders");
        fixture.Database.Schema.ShouldBe("sales");
        fixture.Network.Status.ShouldBe(NetworkStatus.Online);
        fixture.Workflow.Status.ShouldBe(WorkflowTaskStatus.Ready);

        canvas.Redo();
        fixture.Api.Route.ShouldBe("/orders/{id}");
        fixture.Database.Schema.ShouldBe("reporting");
        fixture.Network.Status.ShouldBe(NetworkStatus.Warning);
        fixture.Workflow.Status.ShouldBe(WorkflowTaskStatus.Running);
    }

    private static DiagramCanvas CreateComposedCanvas(NodelyDiagram diagram, NodelyPalette palette) => new DiagramCanvas
    {
        Diagram = diagram,
        Palette = palette,
    }
        .UseApiNodes()
        .UseDatabaseNodes()
        .UseMindMapNodes()
        .UseNetworkNodes()
        .UseStateMachineNodes()
        .UseUmlNodes()
        .UseWorkflowNodes();

    private static CompositionFixture CreateCompositionFixture()
    {
        var diagram = new NodelyDiagram();
        var api = diagram.Nodes.Add(new ApiEndpointNode(new NodelyPoint(0, 0), "/orders", ApiEndpointMethod.Post)
        {
            RequestType = "CreateOrderRequest",
            ResponseType = "OrderDto",
        });
        var database = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(240, 0), "Orders", "sales"));
        database.Columns.Add(new DatabaseColumn("OrderId", "uuid", isPrimaryKey: true, isNullable: false));
        var mindMap = diagram.Nodes.Add(new MindMapRootNode(new NodelyPoint(480, 0), "Release"));
        var branch = diagram.Nodes.Add(new MindMapBranchNode(new NodelyPoint(700, 0), "Composition"));
        var network = diagram.Nodes.Add(new NetworkRouterNode(new NodelyPoint(920, 0), "Edge")
        {
            Status = NetworkStatus.Online,
        });
        var state = diagram.Nodes.Add(new StateMachineStateNode(new NodelyPoint(1160, 0), "Ready"));
        var nextState = diagram.Nodes.Add(new StateMachineStateNode(new NodelyPoint(1400, 0), "Done"));
        var uml = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(1640, 0), "Order"));
        var entity = diagram.Nodes.Add(new UmlInterfaceNode(new NodelyPoint(1880, 0), "IOrderRepository"));
        var workflow = diagram.Nodes.Add(new WorkflowTaskNode(new NodelyPoint(2120, 0), "Create order")
        {
            Status = WorkflowTaskStatus.Ready,
        });
        var workflowEnd = diagram.Nodes.Add(new WorkflowEndNode(new NodelyPoint(2360, 0), "Done"));

        var apiPort = api.AddPort(new ApiPortModel(api, PortAlignment.Right, ApiPortRole.Request, "request"));
        var databasePort = database.AddPort(new DatabasePortModel(database, PortAlignment.Left, DatabasePortKind.Relationship, "OrderId"));
        var mindMapPort = mindMap.AddPort(new MindMapPortModel(mindMap, PortAlignment.Right, MindMapPortRole.Branch, "branch"));
        var networkPort = network.AddPort(new NetworkPortModel(network, PortAlignment.Right, NetworkPortRole.Wan, "wan"));
        var statePort = state.AddPort(new StateMachinePortModel(state, PortAlignment.Right, StateMachinePortRole.Exit, "done"));
        var umlPort = uml.AddPort(new UmlPortModel(uml, PortAlignment.Right, UmlPortKind.Association, "OrderId"));

        var apiLink = diagram.Links.Add(new ApiLink(api, uml, ApiLinkKind.DependsOn) { Label = "contract" });
        var databaseLink = diagram.Links.Add(new DatabaseRelationshipLink(api, database, RelationshipKind.Dependency));
        var mindMapLink = diagram.Links.Add(new MindMapLink(mindMap, branch, MindMapLinkKind.Branch) { Label = "packs" });
        var networkLink = diagram.Links.Add(new NetworkLink(network, api, NetworkLinkKind.Dependency) { Label = "routes" });
        var stateLink = diagram.Links.Add(new StateMachineTransitionLink(state, nextState) { Trigger = "complete" });
        var umlLink = diagram.Links.Add(new UmlRelationshipLink(uml, entity, UmlRelationshipKind.Realization) { Label = "stores" });
        var workflowLink = diagram.Links.Add(new WorkflowLink(workflow, workflowEnd, WorkflowLinkKind.Sequence) { Label = "finish" });

        return new CompositionFixture(
            diagram,
            api,
            database,
            mindMap,
            network,
            state,
            uml,
            workflow,
            (ApiPortModel)apiPort,
            (DatabasePortModel)databasePort,
            (MindMapPortModel)mindMapPort,
            (NetworkPortModel)networkPort,
            (StateMachinePortModel)statePort,
            (UmlPortModel)umlPort,
            apiLink,
            databaseLink,
            mindMapLink,
            networkLink,
            stateLink,
            umlLink,
            workflowLink);
    }

    private sealed record CompositionFixture(
        NodelyDiagram Diagram,
        ApiEndpointNode Api,
        DatabaseTableNode Database,
        MindMapRootNode MindMap,
        NetworkRouterNode Network,
        StateMachineStateNode State,
        UmlClassNode Uml,
        WorkflowTaskNode Workflow,
        ApiPortModel ApiPort,
        DatabasePortModel DatabasePort,
        MindMapPortModel MindMapPort,
        NetworkPortModel NetworkPort,
        StateMachinePortModel StatePort,
        UmlPortModel UmlPort,
        ApiLink ApiLink,
        DatabaseRelationshipLink DatabaseLink,
        MindMapLink MindMapLink,
        NetworkLink NetworkLink,
        StateMachineTransitionLink StateLink,
        UmlRelationshipLink UmlLink,
        WorkflowLink WorkflowLink);
}
