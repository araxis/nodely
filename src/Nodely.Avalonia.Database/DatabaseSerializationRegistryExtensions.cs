using System;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;

namespace Nodely.Avalonia.Database;

/// <summary>Serialization registration helpers for the database node pack.</summary>
public static class DatabaseSerializationRegistryExtensions
{
    /// <summary>Registers database node, port, and relationship link factories.</summary>
    public static DiagramSerializationRegistry UseDatabaseNodes(this DiagramSerializationRegistry registry)
    {
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        registry.RegisterNode(DatabaseTableNode.ModelKindKey,
            snapshot => new DatabaseTableNode(snapshot.Id, new Point(snapshot.X, snapshot.Y)));
        registry.RegisterNode(DatabaseViewNode.ModelKindKey,
            snapshot => new DatabaseViewNode(snapshot.Id, new Point(snapshot.X, snapshot.Y)));
        registry.RegisterNode(DatabaseProcedureNode.ModelKindKey,
            snapshot => new DatabaseProcedureNode(snapshot.Id, new Point(snapshot.X, snapshot.Y)));

        registry.RegisterPort(DatabasePortModel.ModelKindKey, (snapshot, parent) =>
            new DatabasePortModel(snapshot.Id, parent, ParseAlignment(snapshot.Alignment), position: parent.Position));

        registry.RegisterLink(DatabaseRelationshipLink.ModelKindKey, (snapshot, source, target) =>
            new DatabaseRelationshipLink(snapshot.Id, source, target));

        return registry;
    }

    private static PortAlignment ParseAlignment(string alignment)
        => Enum.TryParse<PortAlignment>(alignment, out var parsed) ? parsed : PortAlignment.Bottom;
}
