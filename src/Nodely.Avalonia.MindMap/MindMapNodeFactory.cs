using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;

namespace Nodely.Avalonia.MindMap;

/// <summary>Creates mind-map model types from serialization snapshots.</summary>
public static class MindMapNodeFactory
{
    /// <summary>Creates a registry that restores mind-map nodes, ports, and links.</summary>
    public static DiagramSerializationRegistry CreateRegistry() => new DiagramSerializationRegistry().UseMindMapNodes();

    /// <summary>Creates a node for the snapshot kind, falling back to <see cref="NodeModel"/>.</summary>
    public static NodeModel Create(NodeSnapshot snapshot)
        => TryCreate(snapshot, out var node)
            ? node
            : new NodeModel(snapshot.Id, new Point(snapshot.X, snapshot.Y)) { Title = snapshot.Title };

    /// <summary>Attempts to create a mind-map node for the snapshot kind.</summary>
    public static bool TryCreate(NodeSnapshot snapshot, out NodeModel node)
    {
        var position = new Point(snapshot.X, snapshot.Y);
        node = snapshot.Kind switch
        {
            MindMapRootNode.ModelKindKey or nameof(MindMapRootNode) => new MindMapRootNode(snapshot.Id, position, snapshot.Title ?? "Central topic"),
            MindMapBranchNode.ModelKindKey or nameof(MindMapBranchNode) => new MindMapBranchNode(snapshot.Id, position, snapshot.Title ?? "Branch"),
            MindMapLeafNode.ModelKindKey or nameof(MindMapLeafNode) => new MindMapLeafNode(snapshot.Id, position, snapshot.Title ?? "Leaf"),
            _ => null!,
        };

        return node != null;
    }
}
