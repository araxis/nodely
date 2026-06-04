using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Database;

/// <summary>A port with a database-specific role.</summary>
public sealed class DatabasePortModel : PortModel
{
    /// <summary>Creates a database port.</summary>
    public DatabasePortModel(
        NodeModel parent,
        PortAlignment alignment = PortAlignment.Right,
        DatabasePortKind kind = DatabasePortKind.Relationship,
        string? name = null,
        Point? position = null,
        Size? size = null)
        : base(parent, alignment, position, size)
    {
        Kind = kind;
        Name = name;
    }

    /// <summary>Creates a database port with the given id.</summary>
    public DatabasePortModel(
        string id,
        NodeModel parent,
        PortAlignment alignment = PortAlignment.Right,
        DatabasePortKind kind = DatabasePortKind.Relationship,
        string? name = null,
        Point? position = null,
        Size? size = null)
        : base(id, parent, alignment, position, size)
    {
        Kind = kind;
        Name = name;
    }

    /// <summary>The port role.</summary>
    public DatabasePortKind Kind { get; set; }

    /// <summary>An optional logical name, such as a column name.</summary>
    public string? Name { get; set; }
}
