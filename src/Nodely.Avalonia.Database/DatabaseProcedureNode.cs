using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Database;

/// <summary>A stored procedure node with parameters.</summary>
public sealed class DatabaseProcedureNode : DatabaseObjectNode
{
    /// <summary>Creates a procedure node.</summary>
    public DatabaseProcedureNode(Point position, string objectName = "Procedure", string schema = "dbo")
        : base(position, objectName, schema) { }

    /// <summary>Creates a procedure node with the given id.</summary>
    public DatabaseProcedureNode(string id, Point position, string objectName = "Procedure", string schema = "dbo")
        : base(id, position, objectName, schema) { }

    /// <summary>The procedure parameters.</summary>
    public ObservableCollection<DatabaseParameter> Parameters { get; } = new();

    /// <inheritdoc />
    protected override string DefaultObjectName => "Procedure";

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new DatabaseProcedureNode(Position, ObjectName, Schema);
        CopyBaseTo(clone);
        foreach (var parameter in Parameters)
            clone.Parameters.Add(parameter.Clone());
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["ParametersJson"] = JsonSerializer.Serialize(Parameters);
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        Parameters.Clear();
        foreach (var parameter in DatabaseTableNode.DeserializeList<DatabaseParameter>(data, "ParametersJson"))
            Parameters.Add(parameter);
    }
}
