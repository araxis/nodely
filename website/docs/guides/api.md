---
title: API pack
---

# API pack

`Nodely.Avalonia.Api` is an optional side package for API and service-design diagrams. It provides service
nodes, endpoint cards, contract nodes, operation nodes, client/gateway/auth nodes, typed ports, API links,
pack-owned renderers, serialization registration, and a small arrange helper.

## Install

```bash
dotnet add package Nodely.Avalonia.Api
dotnet add package Nodely.Serialization
```

## Register renderers

Call `UseApiNodes()` after creating the canvas:

```csharp
using Nodely;
using Nodely.Avalonia.Api;
using Nodely.Avalonia.Controls;

var diagram = new NodelyDiagram();
var canvas = new DiagramCanvas { Diagram = diagram };

canvas.UseApiNodes();
```

The call registers API service, endpoint, contract, operation, client, gateway, auth, group, port, and link
renderers. Multiple packs can be registered on the same canvas:

```csharp
canvas
    .UseApiNodes()
    .UseDatabaseNodes()
    .UseNetworkNodes();
```

## Build a diagram

```csharp
using Nodely;
using Nodely.Avalonia.Api;
using Nodely.Models;
using Point = Nodely.Geometry.Point;

var diagram = new NodelyDiagram();

var client = diagram.Nodes.Add(new ApiClientNode(new Point(0, 0), "Partner portal")
{
    Platform = "web app",
    Version = "v2",
});

var gateway = diagram.Nodes.Add(new ApiGatewayNode(new Point(260, 0), "Public gateway")
{
    Host = "api.example.test",
});

var auth = diagram.Nodes.Add(new ApiAuthNode(new Point(520, 0), "Orders policy")
{
    Scheme = "OAuth2",
    Scopes = "orders:read orders:write",
    Status = ApiEndpointStatus.Internal,
});

var endpoint = diagram.Nodes.Add(new ApiEndpointNode(new Point(780, 0), "/orders", ApiEndpointMethod.Post)
{
    RequestType = "CreateOrderRequest",
    ResponseType = "OrderDto",
    Status = ApiEndpointStatus.Preview,
    Version = "v1",
});

var contract = diagram.Nodes.Add(new ApiContractNode(new Point(1040, 0), "OrderDto")
{
    Version = "v1",
});
contract.Fields.Add(new ApiContractField("id", "string", required: true));
contract.Fields.Add(new ApiContractField("total", "decimal", required: true));
```

## Ports and links

Use `ApiPortModel` for visible, semantic attachment points:

```csharp
var clientOut = client.AddPort(new ApiPortModel(client, PortAlignment.Right, ApiPortRole.Request, "request"));
var gatewayIn = gateway.AddPort(new ApiPortModel(gateway, PortAlignment.Left, ApiPortRole.Request, "public"));
var gatewayAuth = gateway.AddPort(new ApiPortModel(gateway, PortAlignment.Bottom, ApiPortRole.Auth, "auth"));
var authOut = auth.AddPort(new ApiPortModel(auth, PortAlignment.Right, ApiPortRole.Auth, "policy"));
var endpointIn = endpoint.AddPort(new ApiPortModel(endpoint, PortAlignment.Left, ApiPortRole.Request, "POST"));
var endpointOut = endpoint.AddPort(new ApiPortModel(endpoint, PortAlignment.Right, ApiPortRole.Response, "201"));
var contractIn = contract.AddPort(new ApiPortModel(contract, PortAlignment.Left, ApiPortRole.Response, "dto"));

diagram.Links.Add(new ApiLink(clientOut, gatewayIn, ApiLinkKind.Request)
{
    Label = "public",
    Protocol = "HTTPS",
    Payload = "JSON",
});

diagram.Links.Add(new ApiLink(authOut, gatewayAuth, ApiLinkKind.Secures)
{
    Label = "token check",
    Protocol = "OAuth2",
    Status = ApiEndpointStatus.Internal,
});

diagram.Links.Add(new ApiLink(endpointOut, contractIn, ApiLinkKind.Response)
{
    Label = "201",
    Payload = "OrderDto",
});
```

API links support:

- `Request`
- `Response`
- `Publishes`
- `Consumes`
- `DependsOn`
- `Secures`

Links can carry labels, protocol text, payload text, lifecycle status, and an optional accent color.

## Arrange

`ApiLayout.Arrange()` keeps the first release intentionally small. It places common API roles into readable
columns:

```csharp
ApiLayout.Arrange(diagram);
canvas.ZoomToFit();
```

Use it from a toolbar as one undoable move:

```csharp
canvas.RunAsUndoableMove(() => ApiLayout.Arrange(diagram));
canvas.RefreshVisuals();
canvas.ZoomToFit();
```

The default columns are clients, gateways, services/groups, endpoints, operations/auth, and contracts.

## Save and load

Use the API serializer registration when restoring saved diagrams:

```csharp
using Nodely.Avalonia.Api;
using Nodely.Serialization;

var json = DiagramSerializer.Serialize(diagram);

var loaded = new NodelyDiagram();
var registry = ApiNodeFactory.CreateRegistry();
DiagramSerializer.Deserialize(loaded, json, registry);
```

If an app uses several packs, compose the registry:

```csharp
var registry = ApiNodeFactory.CreateRegistry()
    .UseDatabaseNodes()
    .UseNetworkNodes()
    .UseUmlNodes()
    .UseWorkflowNodes();
```

The API pack uses stable model-kind keys and existing extra-data hooks, so node, port, and link metadata round
trips through the normal serializer.

## Runtime editing

API nodes expose ordinary mutable properties such as `Name`, `Version`, `Status`, `Summary`, `Route`,
`Method`, `RequestType`, `ResponseType`, `Fields`, `Scheme`, and `Scopes`. API links expose `Kind`, `Label`,
`Protocol`, `Payload`, `Status`, and `AccentColor`.

For undoable runtime inspectors, wrap edits with the canvas helper:

```csharp
canvas.RunAsUndoableEdit(
    apply: () => { endpoint.Route = "/orders/{id}"; endpoint.RefreshAll(); },
    undo: () => { endpoint.Route = "/orders"; endpoint.RefreshAll(); });
```
