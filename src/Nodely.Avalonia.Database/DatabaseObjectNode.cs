using System;
using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Database;

/// <summary>Base class for database object nodes.</summary>
public abstract class DatabaseObjectNode : NodeModel
{
    private string _schema = "dbo";
    private string _objectName;

    /// <summary>Creates a database object node.</summary>
    protected DatabaseObjectNode(Point position, string objectName, string schema = "dbo")
        : base(position)
    {
        _schema = Normalize(schema, "dbo");
        _objectName = Normalize(objectName, DefaultObjectName);
        UpdateTitle();
    }

    /// <summary>Creates a database object node with the given id.</summary>
    protected DatabaseObjectNode(string id, Point position, string objectName, string schema = "dbo")
        : base(id, position)
    {
        _schema = Normalize(schema, "dbo");
        _objectName = Normalize(objectName, DefaultObjectName);
        UpdateTitle();
    }

    /// <summary>The database schema.</summary>
    public string Schema
    {
        get => _schema;
        set
        {
            _schema = Normalize(value, "dbo");
            UpdateTitle();
            Refresh();
        }
    }

    /// <summary>The object name without schema.</summary>
    public string ObjectName
    {
        get => _objectName;
        set
        {
            _objectName = Normalize(value, DefaultObjectName);
            UpdateTitle();
            Refresh();
        }
    }

    /// <summary>The default name used when a snapshot omits the object name.</summary>
    protected abstract string DefaultObjectName { get; }

    /// <summary>Copies shared database-object fields to a clone.</summary>
    protected void CopyBaseTo(DatabaseObjectNode clone)
    {
        clone.Schema = Schema;
        clone.ObjectName = ObjectName;
        clone.Size = Size;
        clone.ControlledSize = ControlledSize;
    }

    /// <summary>Writes shared extra data.</summary>
    protected Dictionary<string, object?> BuildBaseExtra() => new()
    {
        ["Schema"] = Schema,
        ["ObjectName"] = ObjectName,
    };

    /// <summary>Reads shared extra data.</summary>
    protected void ApplyBaseExtra(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Schema", out var schema) && schema is string schemaText)
            _schema = Normalize(schemaText, "dbo");
        if (data.TryGetValue("ObjectName", out var name) && name is string nameText)
            _objectName = Normalize(nameText, DefaultObjectName);

        UpdateTitle();
    }

    private void UpdateTitle() => Title = string.IsNullOrWhiteSpace(Schema)
        ? ObjectName
        : Schema + "." + ObjectName;

    private static string Normalize(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
