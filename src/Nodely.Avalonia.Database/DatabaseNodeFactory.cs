using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;

namespace Nodely.Avalonia.Database;

/// <summary>Creates database node types from serialization snapshots.</summary>
public static class DatabaseNodeFactory
{
    /// <summary>Creates a node for the snapshot kind, falling back to <see cref="NodeModel"/>.</summary>
    public static NodeModel Create(NodeSnapshot snapshot)
        => TryCreate(snapshot, out var node)
            ? node
            : new NodeModel(snapshot.Id, new Point(snapshot.X, snapshot.Y)) { Title = snapshot.Title };

    /// <summary>Attempts to create a database node for the snapshot kind.</summary>
    public static bool TryCreate(NodeSnapshot snapshot, out NodeModel node)
    {
        var position = new Point(snapshot.X, snapshot.Y);
        node = snapshot.Kind switch
        {
            nameof(DatabaseTableNode) => new DatabaseTableNode(snapshot.Id, position),
            nameof(DatabaseViewNode) => new DatabaseViewNode(snapshot.Id, position),
            nameof(DatabaseProcedureNode) => new DatabaseProcedureNode(snapshot.Id, position),
            _ => null!,
        };

        return node != null;
    }
}
