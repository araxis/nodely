using System;
using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Database;

/// <summary>A port with a database-specific role.</summary>
public sealed class DatabasePortModel : PortModel
{
    /// <summary>The stable serialization kind for database ports.</summary>
    public new const string ModelKindKey = "database.port";

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

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override Point GetPortCenter()
    {
        if (Parent.Size is not { } parentSize ||
            string.IsNullOrWhiteSpace(Name) ||
            Alignment is not (PortAlignment.Left or PortAlignment.Right))
        {
            return base.GetPortCenter();
        }

        var rowIndex = FindRowIndex();
        if (rowIndex < 0)
            return base.GetPortCenter();

        var centerY = Parent.Position.Y +
            DatabaseVisualMetrics.HeaderHeight +
            DatabaseVisualMetrics.BodyTopPadding +
            rowIndex * DatabaseVisualMetrics.RowHeight +
            DatabaseVisualMetrics.RowHeight / 2;

        var x = Alignment == PortAlignment.Left
            ? Parent.Position.X
            : Parent.Position.X + parentSize.Width;

        return new Point(x, centerY);
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = new Dictionary<string, object?> { ["PortKind"] = Kind.ToString() };
        if (!string.IsNullOrWhiteSpace(Name))
            extra["Name"] = Name;
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("PortKind", out var kind) &&
            kind is string kindText &&
            Enum.TryParse<DatabasePortKind>(kindText, out var parsedKind))
        {
            Kind = parsedKind;
        }

        if (data.TryGetValue("Name", out var name) && name is string nameText)
            Name = nameText;
    }

    private int FindRowIndex()
    {
        var name = Name ?? string.Empty;
        return Parent switch
        {
            DatabaseTableNode table => FindColumn(table, name),
            DatabaseViewNode view => FindColumn(view, name),
            DatabaseProcedureNode procedure => FindParameter(procedure, name),
            _ => -1,
        };
    }

    private static int FindColumn(DatabaseTableNode table, string name)
    {
        for (var i = 0; i < table.Columns.Count; i++)
            if (string.Equals(table.Columns[i].Name, name, StringComparison.OrdinalIgnoreCase))
                return i;

        return -1;
    }

    private static int FindColumn(DatabaseViewNode view, string name)
    {
        for (var i = 0; i < view.Columns.Count; i++)
            if (string.Equals(view.Columns[i].Name, name, StringComparison.OrdinalIgnoreCase))
                return i;

        return -1;
    }

    private static int FindParameter(DatabaseProcedureNode procedure, string name)
    {
        for (var i = 0; i < procedure.Parameters.Count; i++)
            if (string.Equals(procedure.Parameters[i].Name, name, StringComparison.OrdinalIgnoreCase))
                return i;

        return -1;
    }
}
