using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Database;

/// <summary>A database table node with columns.</summary>
public sealed class DatabaseTableNode : DatabaseObjectNode
{
    /// <summary>Creates a table node.</summary>
    public DatabaseTableNode(Point position, string objectName = "Table", string schema = "dbo")
        : base(position, objectName, schema) { }

    /// <summary>Creates a table node with the given id.</summary>
    public DatabaseTableNode(string id, Point position, string objectName = "Table", string schema = "dbo")
        : base(id, position, objectName, schema) { }

    /// <summary>The table columns.</summary>
    public ObservableCollection<DatabaseColumn> Columns { get; } = new();

    /// <inheritdoc />
    protected override string DefaultObjectName => "Table";

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new DatabaseTableNode(Position, ObjectName, Schema);
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
        foreach (var column in DeserializeList<DatabaseColumn>(data, "ColumnsJson"))
            Columns.Add(column);
    }

    internal static IReadOnlyList<T> DeserializeList<T>(IReadOnlyDictionary<string, object?> data, string key)
    {
        if (!data.TryGetValue(key, out var value) || value is not string json || string.IsNullOrWhiteSpace(json))
            return new List<T>();

        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }
}
