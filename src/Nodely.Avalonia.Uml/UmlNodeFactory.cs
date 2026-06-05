using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;

namespace Nodely.Avalonia.Uml;

/// <summary>Creates UML node types from serialization snapshots.</summary>
public static class UmlNodeFactory
{
    /// <summary>Creates a registry that restores UML nodes and relationship links.</summary>
    public static DiagramSerializationRegistry CreateRegistry() => new DiagramSerializationRegistry().UseUmlNodes();

    /// <summary>Creates a node for the snapshot kind, falling back to <see cref="NodeModel"/>.</summary>
    public static NodeModel Create(NodeSnapshot snapshot)
        => TryCreate(snapshot, out var node)
            ? node
            : new NodeModel(snapshot.Id, new Point(snapshot.X, snapshot.Y)) { Title = snapshot.Title };

    /// <summary>Attempts to create a UML node for the snapshot kind.</summary>
    public static bool TryCreate(NodeSnapshot snapshot, out NodeModel node)
    {
        var position = new Point(snapshot.X, snapshot.Y);
        node = snapshot.Kind switch
        {
            UmlClassNode.ModelKindKey or nameof(UmlClassNode) => new UmlClassNode(snapshot.Id, position, snapshot.Title ?? "Class"),
            UmlInterfaceNode.ModelKindKey or nameof(UmlInterfaceNode) => new UmlInterfaceNode(snapshot.Id, position, snapshot.Title ?? "Interface"),
            UmlEnumNode.ModelKindKey or nameof(UmlEnumNode) => new UmlEnumNode(snapshot.Id, position, snapshot.Title ?? "Enum"),
            UmlPackageNode.ModelKindKey or nameof(UmlPackageNode) => new UmlPackageNode(snapshot.Id, position, snapshot.Title ?? "Package"),
            UmlNoteNode.ModelKindKey or nameof(UmlNoteNode) => new UmlNoteNode(snapshot.Id, position, snapshot.Title ?? "Note"),
            _ => null!,
        };

        return node != null;
    }
}
