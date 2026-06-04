using System;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;

namespace Nodely.Avalonia.Uml;

/// <summary>Serialization registration helpers for the UML node pack.</summary>
public static class UmlSerializationRegistryExtensions
{
    /// <summary>Registers UML node and relationship link factories.</summary>
    public static DiagramSerializationRegistry UseUmlNodes(this DiagramSerializationRegistry registry)
    {
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        registry.RegisterNode(UmlClassNode.ModelKindKey,
            snapshot => new UmlClassNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Class"));
        registry.RegisterNode(UmlInterfaceNode.ModelKindKey,
            snapshot => new UmlInterfaceNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Interface"));
        registry.RegisterNode(UmlEnumNode.ModelKindKey,
            snapshot => new UmlEnumNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Enum"));
        registry.RegisterNode(UmlPackageNode.ModelKindKey,
            snapshot => new UmlPackageNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Package"));
        registry.RegisterNode(UmlNoteNode.ModelKindKey,
            snapshot => new UmlNoteNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Note"));

        registry.RegisterLink(UmlRelationshipLink.ModelKindKey, CreateRelationship);

        return registry;
    }

    private static UmlRelationshipLink CreateRelationship(LinkSnapshot snapshot, Anchor source, Anchor target)
        => new(snapshot.Id, source, target);
}
