using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;

namespace Nodely.Avalonia.Api;

/// <summary>Creates API node types from serialization snapshots.</summary>
public static class ApiNodeFactory
{
    /// <summary>Creates a registry that restores API nodes, ports, and links.</summary>
    public static DiagramSerializationRegistry CreateRegistry() => new DiagramSerializationRegistry().UseApiNodes();

    /// <summary>Creates a node for the snapshot kind, falling back to <see cref="NodeModel"/>.</summary>
    public static NodeModel Create(NodeSnapshot snapshot)
        => TryCreate(snapshot, out var node)
            ? node
            : new NodeModel(snapshot.Id, new Point(snapshot.X, snapshot.Y)) { Title = snapshot.Title };

    /// <summary>Attempts to create an API node for the snapshot kind.</summary>
    public static bool TryCreate(NodeSnapshot snapshot, out NodeModel node)
    {
        var position = new Point(snapshot.X, snapshot.Y);
        node = snapshot.Kind switch
        {
            ApiServiceNode.ModelKindKey or nameof(ApiServiceNode) => new ApiServiceNode(snapshot.Id, position, snapshot.Title ?? "Service"),
            ApiEndpointNode.ModelKindKey or nameof(ApiEndpointNode) => new ApiEndpointNode(snapshot.Id, position, snapshot.Title ?? "/resource"),
            ApiContractNode.ModelKindKey or nameof(ApiContractNode) => new ApiContractNode(snapshot.Id, position, snapshot.Title ?? "Contract"),
            ApiOperationNode.ModelKindKey or nameof(ApiOperationNode) => new ApiOperationNode(snapshot.Id, position, snapshot.Title ?? "Operation"),
            ApiClientNode.ModelKindKey or nameof(ApiClientNode) => new ApiClientNode(snapshot.Id, position, snapshot.Title ?? "Client"),
            ApiGatewayNode.ModelKindKey or nameof(ApiGatewayNode) => new ApiGatewayNode(snapshot.Id, position, snapshot.Title ?? "Gateway"),
            ApiAuthNode.ModelKindKey or nameof(ApiAuthNode) => new ApiAuthNode(snapshot.Id, position, snapshot.Title ?? "Auth"),
            ApiGroupNode.ModelKindKey or nameof(ApiGroupNode) => new ApiGroupNode(snapshot.Id, position, snapshot.Title ?? "API group"),
            _ => null!,
        };

        return node != null;
    }
}
