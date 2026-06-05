using System;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;

namespace Nodely.Avalonia.Network;

/// <summary>Serialization registration helpers for the network node pack.</summary>
public static class NetworkSerializationRegistryExtensions
{
    /// <summary>Registers network node, port, and link factories.</summary>
    public static DiagramSerializationRegistry UseNetworkNodes(this DiagramSerializationRegistry registry)
    {
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        registry.RegisterNode(NetworkRouterNode.ModelKindKey,
            snapshot => new NetworkRouterNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Router"));
        registry.RegisterNode(NetworkSwitchNode.ModelKindKey,
            snapshot => new NetworkSwitchNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Switch"));
        registry.RegisterNode(NetworkFirewallNode.ModelKindKey,
            snapshot => new NetworkFirewallNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Firewall"));
        registry.RegisterNode(NetworkLoadBalancerNode.ModelKindKey,
            snapshot => new NetworkLoadBalancerNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Load balancer"));
        registry.RegisterNode(NetworkServerNode.ModelKindKey,
            snapshot => new NetworkServerNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Server"));
        registry.RegisterNode(NetworkClientNode.ModelKindKey,
            snapshot => new NetworkClientNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Client"));
        registry.RegisterNode(NetworkCloudNode.ModelKindKey,
            snapshot => new NetworkCloudNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Cloud"));
        registry.RegisterNode(NetworkServiceNode.ModelKindKey,
            snapshot => new NetworkServiceNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Service"));
        registry.RegisterNode(NetworkZoneNode.ModelKindKey,
            snapshot => new NetworkZoneNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Zone"));

        registry.RegisterPort(NetworkPortModel.ModelKindKey, (snapshot, parent) =>
            new NetworkPortModel(snapshot.Id, parent, ParseAlignment(snapshot.Alignment), position: parent.Position));

        registry.RegisterLink(NetworkLink.ModelKindKey, CreateLink);

        return registry;
    }

    private static NetworkLink CreateLink(LinkSnapshot snapshot, Anchor source, Anchor target)
        => new(snapshot.Id, source, target);

    private static PortAlignment ParseAlignment(string alignment)
        => Enum.TryParse<PortAlignment>(alignment, out var parsed) ? parsed : PortAlignment.Right;
}
