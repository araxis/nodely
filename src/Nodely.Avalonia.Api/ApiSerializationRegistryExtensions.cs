using System;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;

namespace Nodely.Avalonia.Api;

/// <summary>Serialization registration helpers for the API node pack.</summary>
public static class ApiSerializationRegistryExtensions
{
    /// <summary>Registers API node, port, and link factories.</summary>
    public static DiagramSerializationRegistry UseApiNodes(this DiagramSerializationRegistry registry)
    {
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        registry.RegisterNode(ApiServiceNode.ModelKindKey,
            snapshot => new ApiServiceNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Service"));
        registry.RegisterNode(ApiEndpointNode.ModelKindKey,
            snapshot => new ApiEndpointNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "/resource"));
        registry.RegisterNode(ApiContractNode.ModelKindKey,
            snapshot => new ApiContractNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Contract"));
        registry.RegisterNode(ApiOperationNode.ModelKindKey,
            snapshot => new ApiOperationNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Operation"));
        registry.RegisterNode(ApiClientNode.ModelKindKey,
            snapshot => new ApiClientNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Client"));
        registry.RegisterNode(ApiGatewayNode.ModelKindKey,
            snapshot => new ApiGatewayNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Gateway"));
        registry.RegisterNode(ApiAuthNode.ModelKindKey,
            snapshot => new ApiAuthNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Auth"));
        registry.RegisterNode(ApiGroupNode.ModelKindKey,
            snapshot => new ApiGroupNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "API group"));

        registry.RegisterPort(ApiPortModel.ModelKindKey, (snapshot, parent) =>
            new ApiPortModel(snapshot.Id, parent, ParseAlignment(snapshot.Alignment), position: parent.Position));

        registry.RegisterLink(ApiLink.ModelKindKey, CreateLink);

        return registry;
    }

    private static ApiLink CreateLink(LinkSnapshot snapshot, Anchor source, Anchor target)
        => new(snapshot.Id, source, target);

    private static PortAlignment ParseAlignment(string alignment)
        => Enum.TryParse<PortAlignment>(alignment, out var parsed) ? parsed : PortAlignment.Right;
}
