---
id: network
title: Network pack
sidebar_position: 14
---

# Network pack

`Nodely.Avalonia.Network` is an optional side package for network topology editors. It provides device
models, typed ports, topology links, pack-owned renderers, status badges, serialization helpers, and a small
arrange helper without changing the main packages.

## Install

```powershell
dotnet add package Nodely.Avalonia.Network
dotnet add package Nodely.Serialization
```

The package targets `net8.0` and `net10.0`, and depends on `Nodely.Avalonia` plus `Nodely.Serialization`.

## Register renderers

Call `UseNetworkNodes()` after creating the canvas:

```csharp
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Network;

var canvas = new DiagramCanvas
{
    Diagram = diagram,
};

canvas.UseNetworkNodes();
```

This registers:

- router, switch, firewall, load balancer, server, client, cloud, service, and zone renderers
- visible network port renderers for LAN, WAN, uplink, downlink, management, service, and client roles
- typed link styles for ethernet, fiber, wireless, VPN tunnel, dependency, and blocked links
- link glyph drawing for wireless, tunnel, fiber, dependency, blocked, and degraded links

## Create a topology

```csharp
using Nodely;
using Nodely.Avalonia.Network;
using Nodely.Models;
using Point = Nodely.Geometry.Point;

var diagram = new NodelyDiagram();

var internet = diagram.Nodes.Add(new NetworkCloudNode(new Point(0, 0), "Internet")
{
    Address = "0.0.0.0/0",
    Zone = "external",
});

var router = diagram.Nodes.Add(new NetworkRouterNode(new Point(250, 0), "Edge router")
{
    Address = "203.0.113.10",
    Notes = "Dual WAN edge",
});

var firewall = diagram.Nodes.Add(new NetworkFirewallNode(new Point(500, 0), "Policy gateway")
{
    Address = "10.0.0.1",
    Status = NetworkStatus.Maintenance,
});

var api = diagram.Nodes.Add(new NetworkServiceNode(new Point(760, 0), "Orders API")
{
    Address = "orders.internal",
    Zone = "prod",
});
```

Nodes store:

- `Name`
- `Address`
- `Status`
- `Role`
- `Notes`
- `AccentColor`
- `IconKey`
- `Zone`

`NetworkSwitchNode` also stores `PortCount` and `ActivePorts` for the rendered switch face.

## Add ports and links

Use `NetworkPortModel` for visible, semantic attachment points:

```csharp
var internetWan = internet.AddPort(new NetworkPortModel(internet, PortAlignment.Right, NetworkPortRole.Wan, "internet"));
var routerWan = router.AddPort(new NetworkPortModel(router, PortAlignment.Left, NetworkPortRole.Wan, "wan0"));
var routerLan = router.AddPort(new NetworkPortModel(router, PortAlignment.Right, NetworkPortRole.Lan, "lan0"));
var firewallWan = firewall.AddPort(new NetworkPortModel(firewall, PortAlignment.Left, NetworkPortRole.Wan, "outside"));

diagram.Links.Add(new NetworkLink(internetWan, routerWan, NetworkLinkKind.Fiber)
{
    Label = "primary",
    Protocol = "BGP",
    Bandwidth = "10Gbps",
    Latency = "3ms",
    Direction = NetworkLinkDirection.Bidirectional,
});

diagram.Links.Add(new NetworkLink(routerLan, firewallWan, NetworkLinkKind.VpnTunnel)
{
    Label = "edge tunnel",
    Protocol = "IPsec",
    Status = NetworkStatus.Warning,
});
```

Links store:

- `Kind`
- `Label`
- `Protocol`
- `Bandwidth`
- `Latency`
- `Status`
- `Direction`
- `AccentColor`

## Arrange

`NetworkLayout.Arrange()` is intentionally small. It places common topology roles into readable columns:
external/client, routing edge, security/traffic distribution, switching/zone, service, and server.

```csharp
NetworkLayout.Arrange(diagram);
canvas.ZoomToFit();
```

For an undoable toolbar action:

```csharp
canvas.RunAsUndoableMove(() => NetworkLayout.Arrange(diagram));
canvas.RefreshVisuals();
canvas.ZoomToFit();
```

## Save and load

Use the Network serializer registration when restoring saved diagrams:

```csharp
using Nodely.Serialization;

var registry = NetworkNodeFactory.CreateRegistry();

string json = DiagramSerializer.Serialize(diagram);

var loaded = new NodelyDiagram();
DiagramSerializer.Deserialize(loaded, json, registry);
```

If your app uses several side packages, compose the same registry:

```csharp
var registry = NetworkNodeFactory.CreateRegistry()
    .UseDatabaseNodes()
    .UseWorkflowNodes()
    .UseStateMachineNodes();
```

The Network pack uses stable model-kind keys and existing extra-data hooks, so device, port, and link
metadata round-trips without a snapshot schema change.
