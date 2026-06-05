using System;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;

namespace Nodely.Avalonia.MindMap;

/// <summary>Serialization registration helpers for the mind-map node pack.</summary>
public static class MindMapSerializationRegistryExtensions
{
    /// <summary>Registers mind-map node, port, and link factories.</summary>
    public static DiagramSerializationRegistry UseMindMapNodes(this DiagramSerializationRegistry registry)
    {
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        registry.RegisterNode(MindMapRootNode.ModelKindKey,
            snapshot => new MindMapRootNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Central topic"));
        registry.RegisterNode(MindMapBranchNode.ModelKindKey,
            snapshot => new MindMapBranchNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Branch"));
        registry.RegisterNode(MindMapLeafNode.ModelKindKey,
            snapshot => new MindMapLeafNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Leaf"));

        registry.RegisterPort(MindMapPortModel.ModelKindKey, (snapshot, parent) =>
            new MindMapPortModel(snapshot.Id, parent, ParseAlignment(snapshot.Alignment), position: parent.Position));

        registry.RegisterLink(MindMapLink.ModelKindKey, CreateLink);

        return registry;
    }

    private static MindMapLink CreateLink(LinkSnapshot snapshot, Anchor source, Anchor target)
        => new(snapshot.Id, source, target);

    private static PortAlignment ParseAlignment(string alignment)
        => Enum.TryParse<PortAlignment>(alignment, out var parsed) ? parsed : PortAlignment.Right;
}
