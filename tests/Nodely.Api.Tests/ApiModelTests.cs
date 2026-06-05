using System.Collections.Generic;
using System.Linq;
using Nodely;
using Nodely.Avalonia.Api;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;
using Shouldly;
using Xunit;

namespace Nodely.Api.Tests;

public class ApiModelTests
{
    [Fact]
    public void Endpoint_node_defaults_to_route_method_status_and_accent()
    {
        var endpoint = new ApiEndpointNode(new Point(10, 20), "/orders/{id}", ApiEndpointMethod.Get)
        {
            Version = "v1",
            RequestType = "OrderQuery",
            ResponseType = "OrderDto",
            Summary = "Fetches one order",
        };

        endpoint.Name.ShouldBe("/orders/{id}");
        endpoint.Title.ShouldBe("/orders/{id}");
        endpoint.Route.ShouldBe("/orders/{id}");
        endpoint.Method.ShouldBe(ApiEndpointMethod.Get);
        endpoint.Status.ShouldBe(ApiEndpointStatus.Stable);
        endpoint.AccentColor.ShouldBe("#37A779");
        endpoint.IconKey.ShouldBe("HTTP");
        endpoint.Version.ShouldBe("v1");
        endpoint.RequestType.ShouldBe("OrderQuery");
        endpoint.ResponseType.ShouldBe("OrderDto");
    }

    [Fact]
    public void Clone_copies_api_node_data()
    {
        var contract = new ApiContractNode(new Point(30, 40), "OrderDto")
        {
            Version = "v1",
            Status = ApiEndpointStatus.Preview,
            Summary = "Public order shape",
            AccentColor = "#D18B30",
            IconKey = "DTO",
            Size = new Size(240, 140),
        };
        contract.Fields.Add(new ApiContractField("id", "string", required: true));
        contract.Fields.Add(new ApiContractField("total", "decimal"));

        var clone = contract.Clone().ShouldBeOfType<ApiContractNode>();

        clone.ShouldNotBeSameAs(contract);
        clone.Name.ShouldBe("OrderDto");
        clone.Version.ShouldBe("v1");
        clone.Status.ShouldBe(ApiEndpointStatus.Preview);
        clone.Summary.ShouldBe("Public order shape");
        clone.AccentColor.ShouldBe("#D18B30");
        clone.IconKey.ShouldBe("DTO");
        clone.Size.ShouldBe(new Size(240, 140));
        clone.Fields.Count.ShouldBe(2);
        clone.Fields[0].Name.ShouldBe("id");
        clone.Fields[0].Required.ShouldBeTrue();
    }

    [Fact]
    public void Extra_data_round_trips_api_fields_through_serializer()
    {
        var diagram = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var client = diagram.Nodes.Add(new ApiClientNode("client", new Point(0, 0), "Partner portal")
        {
            Platform = "web app",
        });
        var gateway = diagram.Nodes.Add(new ApiGatewayNode("gateway", new Point(250, 0), "Public gateway")
        {
            Host = "api.example.test",
        });
        var auth = diagram.Nodes.Add(new ApiAuthNode("auth", new Point(500, 0), "Orders policy")
        {
            Scheme = "OAuth2",
            Scopes = "orders:read",
            Status = ApiEndpointStatus.Internal,
        });
        var endpoint = diagram.Nodes.Add(new ApiEndpointNode("endpoint", new Point(750, 0), "/orders", ApiEndpointMethod.Post)
        {
            RequestType = "CreateOrderRequest",
            ResponseType = "OrderDto",
            Status = ApiEndpointStatus.Preview,
            Version = "v1",
        });
        var operation = diagram.Nodes.Add(new ApiOperationNode("operation", new Point(1000, 0), "Create order")
        {
            Input = "CreateOrderRequest",
            Output = "OrderDto",
        });
        var contract = diagram.Nodes.Add(new ApiContractNode("contract", new Point(1250, 0), "OrderDto")
        {
            Version = "v1",
        });
        contract.Fields.Add(new ApiContractField("id", "string", required: true));
        contract.Fields.Add(new ApiContractField("total", "decimal", required: true));

        var clientPort = client.AddPort(new ApiPortModel(client, PortAlignment.Right, ApiPortRole.Request, "request"));
        var gatewayPort = gateway.AddPort(new ApiPortModel(gateway, PortAlignment.Left, ApiPortRole.Request, "public"));
        var authPort = auth.AddPort(new ApiPortModel(auth, PortAlignment.Right, ApiPortRole.Auth, "policy"));
        var endpointAuth = endpoint.AddPort(new ApiPortModel(endpoint, PortAlignment.Bottom, ApiPortRole.Auth, "scope"));
        var endpointResponse = endpoint.AddPort(new ApiPortModel(endpoint, PortAlignment.Right, ApiPortRole.Response, "201"));
        var contractPort = contract.AddPort(new ApiPortModel(contract, PortAlignment.Left, ApiPortRole.Response, "dto"));

        diagram.Links.Add(new ApiLink(clientPort, gatewayPort, ApiLinkKind.Request)
        {
            Label = "public",
            Protocol = "HTTPS",
            Payload = "JSON",
        });
        diagram.Links.Add(new ApiLink(authPort, endpointAuth, ApiLinkKind.Secures)
        {
            Label = "scope",
            Status = ApiEndpointStatus.Internal,
        });
        diagram.Links.Add(new ApiLink(endpointResponse, contractPort, ApiLinkKind.Response)
        {
            Label = "201",
            Payload = "OrderDto",
        });
        diagram.Links.Add(new ApiLink(endpoint, operation, ApiLinkKind.DependsOn)
        {
            Label = "handler",
        });

        var json = DiagramSerializer.Serialize(diagram);

        var loaded = new NodelyDiagram(null, registerDefaultBehaviors: false);
        DiagramSerializer.Deserialize(loaded, json, ApiNodeFactory.CreateRegistry());

        var restoredEndpoint = loaded.Nodes.Single(n => n.Id == "endpoint").ShouldBeOfType<ApiEndpointNode>();
        restoredEndpoint.Route.ShouldBe("/orders");
        restoredEndpoint.Method.ShouldBe(ApiEndpointMethod.Post);
        restoredEndpoint.Status.ShouldBe(ApiEndpointStatus.Preview);
        restoredEndpoint.Ports.OfType<ApiPortModel>().Single(port => port.Role == ApiPortRole.Response).Name.ShouldBe("201");

        var restoredContract = loaded.Nodes.Single(n => n.Id == "contract").ShouldBeOfType<ApiContractNode>();
        restoredContract.Fields.Count.ShouldBe(2);
        restoredContract.Fields[0].Required.ShouldBeTrue();

        var restoredRequest = loaded.Links.OfType<ApiLink>().Single(link => link.Kind == ApiLinkKind.Request);
        restoredRequest.Label.ShouldBe("public");
        restoredRequest.Protocol.ShouldBe("HTTPS");
        restoredRequest.Payload.ShouldBe("JSON");
        restoredRequest.Labels.Single().Content.ShouldBe("public | HTTPS | JSON");
    }

    [Fact]
    public void Factory_restores_api_nodes()
    {
        ApiNodeFactory.Create(new NodeSnapshot { Kind = ApiServiceNode.ModelKindKey, Title = "Service", X = 1, Y = 2 })
            .ShouldBeOfType<ApiServiceNode>();
        ApiNodeFactory.Create(new NodeSnapshot { Kind = ApiEndpointNode.ModelKindKey, Title = "/orders", X = 1, Y = 2 })
            .ShouldBeOfType<ApiEndpointNode>();
        ApiNodeFactory.Create(new NodeSnapshot { Kind = ApiContractNode.ModelKindKey, Title = "OrderDto", X = 1, Y = 2 })
            .ShouldBeOfType<ApiContractNode>();
        ApiNodeFactory.Create(new NodeSnapshot { Kind = ApiOperationNode.ModelKindKey, Title = "Create order", X = 1, Y = 2 })
            .ShouldBeOfType<ApiOperationNode>();
        ApiNodeFactory.Create(new NodeSnapshot { Kind = ApiClientNode.ModelKindKey, Title = "Client", X = 1, Y = 2 })
            .ShouldBeOfType<ApiClientNode>();
        ApiNodeFactory.Create(new NodeSnapshot { Kind = ApiGatewayNode.ModelKindKey, Title = "Gateway", X = 1, Y = 2 })
            .ShouldBeOfType<ApiGatewayNode>();
        ApiNodeFactory.Create(new NodeSnapshot { Kind = ApiAuthNode.ModelKindKey, Title = "Auth", X = 1, Y = 2 })
            .ShouldBeOfType<ApiAuthNode>();
        ApiNodeFactory.Create(new NodeSnapshot { Kind = ApiGroupNode.ModelKindKey, Title = "Group", X = 1, Y = 2 })
            .ShouldBeOfType<ApiGroupNode>();
    }

    [Fact]
    public void Api_link_sets_metadata_defaults_markers_and_label()
    {
        var source = new ApiClientNode(new Point(0, 0), "Client");
        var target = new ApiEndpointNode(new Point(200, 0), "/orders");
        var link = new ApiLink(source, target, ApiLinkKind.Request)
        {
            Label = "public",
            Protocol = "HTTPS",
            Payload = "JSON",
            Status = ApiEndpointStatus.Preview,
            AccentColor = "#37A779",
        };

        link.Kind.ShouldBe(ApiLinkKind.Request);
        link.Segmentable.ShouldBeTrue();
        link.SourceMarker.ShouldNotBeNull();
        link.TargetMarker.ShouldNotBeNull();
        link.Width.ShouldBe(2.5);
        link.Labels.Single().Content.ShouldBe("public | HTTPS | JSON");
        link.Status.ShouldBe(ApiEndpointStatus.Preview);
        link.AccentColor.ShouldBe("#37A779");
    }

    [Fact]
    public void Port_model_persists_role_and_name()
    {
        var node = new ApiEndpointNode(new Point(0, 0), "/orders");
        var port = new ApiPortModel(node, PortAlignment.Right, ApiPortRole.Response, "201");

        var data = port.GetExtraData();
        var restored = new ApiPortModel(node);
        restored.SetExtraData(new Dictionary<string, object?>(data));

        restored.Role.ShouldBe(ApiPortRole.Response);
        restored.Name.ShouldBe("201");
    }

    [Fact]
    public void Arrange_places_api_nodes_in_expected_columns()
    {
        var diagram = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var client = diagram.Nodes.Add(new ApiClientNode(new Point(0, 0), "Client") { Size = new Size(176, 104) });
        var gateway = diagram.Nodes.Add(new ApiGatewayNode(new Point(0, 0), "Gateway") { Size = new Size(206, 116) });
        var service = diagram.Nodes.Add(new ApiServiceNode(new Point(0, 0), "Service") { Size = new Size(238, 128) });
        var endpoint = diagram.Nodes.Add(new ApiEndpointNode(new Point(0, 0), "/orders") { Size = new Size(270, 134) });
        var operation = diagram.Nodes.Add(new ApiOperationNode(new Point(0, 0), "Operation") { Size = new Size(222, 112) });
        var contract = diagram.Nodes.Add(new ApiContractNode(new Point(0, 0), "Contract") { Size = new Size(246, 134) });

        ApiLayout.Arrange(diagram, new ApiLayoutOptions { OriginX = 0, OriginY = 0, ColumnSpacing = 250 });

        gateway.Position.X.ShouldBeGreaterThan(client.Position.X);
        service.Position.X.ShouldBeGreaterThan(gateway.Position.X);
        endpoint.Position.X.ShouldBeGreaterThan(service.Position.X);
        operation.Position.X.ShouldBeGreaterThan(endpoint.Position.X);
        contract.Position.X.ShouldBeGreaterThan(operation.Position.X);
    }
}
