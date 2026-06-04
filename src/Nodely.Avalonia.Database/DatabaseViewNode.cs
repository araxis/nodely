using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Database;

/// <summary>A database view node with projected columns.</summary>
public sealed class DatabaseViewNode : DatabaseObjectNode
{
    /// <summary>Creates a view node.</summary>
    public DatabaseViewNode(Point position, string objectName = "View", string schema = "dbo")
        : base(position, objectName, schema) { }

    /// <summary>Creates a view node with the given id.</summary>
    public DatabaseViewNode(string id, Point position, string objectName = "View", string schema = "dbo")
        : base(id, position, objectName, schema) { }

    /// <summary>The projected columns.</summary>
    public ObservableCollection<DatabaseColumn> Columns { get; } = new();

    /// <inheritdoc />
    protected override string DefaultObjectName => "View";

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new DatabaseViewNode(Position, ObjectName, Schema);
        CopyBaseTo(clone);
        foreach (var column in Columns)
            clone.Columns.Add(column.Clone());
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["ColumnsJson"] = JsonSerializer.Serialize(Columns);
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        Columns.Clear();
        foreach (var column in DatabaseTableNode.DeserializeList<DatabaseColumn>(data, "ColumnsJson"))
            Columns.Add(column);
    }
}
