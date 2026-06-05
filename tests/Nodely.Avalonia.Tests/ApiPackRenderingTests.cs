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

public class ApiPackRenderingTests
{
    [AvaloniaFact]
    public void UseApiNodes_registers_api_node_port_and_link_renderers()
    {
        var diagram = new NodelyDiagram();
        var service = diagram.Nodes.Add(new ApiServiceNode(new NodelyPoint(0, 0), "Orders service"));
        var endpoint = diagram.Nodes.Add(new ApiEndpointNode(new NodelyPoint(260, 0), "/orders", ApiEndpointMethod.Post));
        var contract = diagram.Nodes.Add(new ApiContractNode(new NodelyPoint(520, 0), "OrderDto"));
        var operation = diagram.Nodes.Add(new ApiOperationNode(new NodelyPoint(780, 0), "Create order"));
        var client = diagram.Nodes.Add(new ApiClientNode(new NodelyPoint(0, 180), "Partner portal"));
        var gateway = diagram.Nodes.Add(new ApiGatewayNode(new NodelyPoint(260, 180), "Public gateway"));
        var auth = diagram.Nodes.Add(new ApiAuthNode(new NodelyPoint(520, 180), "Orders policy"));
        var group = diagram.Nodes.Add(new ApiGroupNode(new NodelyPoint(780, 180), "Orders API"));

        contract.Fields.Add(new ApiContractField("id", "string", required: true));
        endpoint.RequestType = "CreateOrderRequest";
        endpoint.ResponseType = "OrderDto";
        auth.Scheme = "OAuth2";

        var clientPort = client.AddPort(new ApiPortModel(client, PortAlignment.Right, ApiPortRole.Request, "request"));
        var gatewayPort = gateway.AddPort(new ApiPortModel(gateway, PortAlignment.Left, ApiPortRole.Request, "public"));
        var request = diagram.Links.Add(new ApiLink(clientPort, gatewayPort, ApiLinkKind.Request)
        {
            Label = "public",
            Protocol = "HTTPS",
        });
        var secured = diagram.Links.Add(new ApiLink(auth, endpoint, ApiLinkKind.Secures)
        {
            Label = "scope",
            Status = ApiEndpointStatus.Internal,
        });

        var canvas = new DiagramCanvas { Diagram = diagram, Palette = NodelyPalettes.Light }.UseApiNodes();

        canvas.BuildNodeContent(service).ShouldBeOfType<Border>().Tag.ShouldBe("api-service-node");
        canvas.BuildNodeContent(endpoint).ShouldBeOfType<Border>().Tag.ShouldBe("api-endpoint-node");
        canvas.BuildNodeContent(contract).ShouldBeOfType<Border>().Tag.ShouldBe("api-contract-node");
        canvas.BuildNodeContent(operation).ShouldBeOfType<Border>().Tag.ShouldBe("api-operation-node");
        canvas.BuildNodeContent(client).ShouldBeOfType<Border>().Tag.ShouldBe("api-client-node");
        canvas.BuildNodeContent(gateway).ShouldBeOfType<Border>().Tag.ShouldBe("api-gateway-node");
        canvas.BuildNodeContent(auth).ShouldBeOfType<Border>().Tag.ShouldBe("api-auth-node");
        canvas.BuildNodeContent(group).ShouldBeOfType<Border>().Tag.ShouldBe("api-group-node");
        canvas.BuildPortContent((ApiPortModel)clientPort).ShouldBeOfType<Grid>().Tag.ShouldBe("api-port");
        canvas.ResolveLinkDrawer(request).ShouldNotBeNull();
        canvas.ResolveLinkDrawer(secured).ShouldNotBeNull();
        canvas.ResolveLinkStyle(request).DashStyle.ShouldBeNull();
        canvas.ResolveLinkStyle(secured).DashStyle.ShouldNotBeNull();

        canvas.Palette = NodelyPalettes.Dark;
        canvas.BuildNodeContent(endpoint).ShouldBeOfType<Border>().Tag.ShouldBe("api-endpoint-node");
    }

    [AvaloniaFact]
    public void Api_pack_registrations_compose_on_canvas_and_serializer_registry()
    {
        var diagram = new NodelyDiagram();
        var api = diagram.Nodes.Add(new ApiEndpointNode(new NodelyPoint(0, 0), "/orders"));
        var table = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(260, 0), "Orders"));
        var root = diagram.Nodes.Add(new MindMapRootNode(new NodelyPoint(520, 0), "Plan"));
        var network = diagram.Nodes.Add(new NetworkRouterNode(new NodelyPoint(780, 0), "Edge"));
        var entity = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(1040, 0), "Order"));
        var state = diagram.Nodes.Add(new StateMachineStateNode(new NodelyPoint(1300, 0), "Waiting"));
        var task = diagram.Nodes.Add(new WorkflowTaskNode(new NodelyPoint(1560, 0), "Sync"));
        var link = diagram.Links.Add(new ApiLink(api, entity, ApiLinkKind.DependsOn)
        {
            Label = "contract",
        });

        var canvas = new DiagramCanvas { Diagram = diagram }
            .UseApiNodes()
            .UseDatabaseNodes()
            .UseMindMapNodes()
            .UseNetworkNodes()
            .UseStateMachineNodes()
            .UseUmlNodes()
            .UseWorkflowNodes();
        canvas.BuildNodeContent(api).ShouldBeOfType<Border>().Tag.ShouldBe("api-endpoint-node");
        canvas.BuildNodeContent(table).ShouldBeOfType<Border>().Tag.ShouldBe("database-table-node");
        canvas.BuildNodeContent(root).ShouldBeOfType<Border>().Tag.ShouldBe("mindmap-root-node");
        canvas.BuildNodeContent(network).ShouldBeOfType<Border>().Tag.ShouldBe("network-router-node");
        canvas.BuildNodeContent(entity).ShouldBeOfType<Border>().Tag.ShouldBe("uml-class-node");
        canvas.BuildNodeContent(state).ShouldBeOfType<Border>().Tag.ShouldBe("statemachine-state-node");
        canvas.BuildNodeContent(task).ShouldBeOfType<Border>().Tag.ShouldBe("workflow-task-node");
        canvas.ResolveLinkDrawer(link).ShouldNotBeNull();

        var registry = ApiNodeFactory.CreateRegistry()
            .UseDatabaseNodes()
            .UseMindMapNodes()
            .UseNetworkNodes()
            .UseStateMachineNodes()
            .UseUmlNodes()
            .UseWorkflowNodes();
        var json = DiagramSerializer.Serialize(diagram);
        var loaded = new NodelyDiagram();
        DiagramSerializer.Deserialize(loaded, json, registry);

        loaded.Nodes.ShouldContain(n => n is ApiEndpointNode);
        loaded.Nodes.ShouldContain(n => n is DatabaseTableNode);
        loaded.Nodes.ShouldContain(n => n is MindMapRootNode);
        loaded.Nodes.ShouldContain(n => n is NetworkRouterNode);
        loaded.Nodes.ShouldContain(n => n is UmlClassNode);
        loaded.Nodes.ShouldContain(n => n is StateMachineStateNode);
        loaded.Nodes.ShouldContain(n => n is WorkflowTaskNode);
        loaded.Links.Single().ShouldBeOfType<ApiLink>();
    }
}
