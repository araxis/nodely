using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;

namespace Nodely.Avalonia.Network;

/// <summary>Creates network node types from serialization snapshots.</summary>
public static class NetworkNodeFactory
{
    /// <summary>Creates a registry that restores network nodes, ports, and links.</summary>
    public static DiagramSerializationRegistry CreateRegistry() => new DiagramSerializationRegistry().UseNetworkNodes();

    /// <summary>Creates a node for the snapshot kind, falling back to <see cref="NodeModel"/>.</summary>
    public static NodeModel Create(NodeSnapshot snapshot)
        => TryCreate(snapshot, out var node)
            ? node
            : new NodeModel(snapshot.Id, new Point(snapshot.X, snapshot.Y)) { Title = snapshot.Title };

    /// <summary>Attempts to create a network node for the snapshot kind.</summary>
    public static bool TryCreate(NodeSnapshot snapshot, out NodeModel node)
    {
        var position = new Point(snapshot.X, snapshot.Y);
        node = snapshot.Kind switch
        {
            NetworkRouterNode.ModelKindKey or nameof(NetworkRouterNode) => new NetworkRouterNode(snapshot.Id, position, snapshot.Title ?? "Router"),
            NetworkSwitchNode.ModelKindKey or nameof(NetworkSwitchNode) => new NetworkSwitchNode(snapshot.Id, position, snapshot.Title ?? "Switch"),
            NetworkFirewallNode.ModelKindKey or nameof(NetworkFirewallNode) => new NetworkFirewallNode(snapshot.Id, position, snapshot.Title ?? "Firewall"),
            NetworkLoadBalancerNode.ModelKindKey or nameof(NetworkLoadBalancerNode) => new NetworkLoadBalancerNode(snapshot.Id, position, snapshot.Title ?? "Load balancer"),
            NetworkServerNode.ModelKindKey or nameof(NetworkServerNode) => new NetworkServerNode(snapshot.Id, position, snapshot.Title ?? "Server"),
            NetworkClientNode.ModelKindKey or nameof(NetworkClientNode) => new NetworkClientNode(snapshot.Id, position, snapshot.Title ?? "Client"),
            NetworkCloudNode.ModelKindKey or nameof(NetworkCloudNode) => new NetworkCloudNode(snapshot.Id, position, snapshot.Title ?? "Cloud"),
            NetworkServiceNode.ModelKindKey or nameof(NetworkServiceNode) => new NetworkServiceNode(snapshot.Id, position, snapshot.Title ?? "Service"),
            NetworkZoneNode.ModelKindKey or nameof(NetworkZoneNode) => new NetworkZoneNode(snapshot.Id, position, snapshot.Title ?? "Zone"),
            _ => null!,
        };

        return node != null;
    }
}
