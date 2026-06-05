using System;
using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Uml;

/// <summary>A UML-specific connection point, optionally tied to a member, operation, or literal row.</summary>
public sealed class UmlPortModel : PortModel
{
    /// <summary>The stable serialization kind for UML ports.</summary>
    public new const string ModelKindKey = "uml.port";

    /// <summary>Creates a UML port.</summary>
    public UmlPortModel(
        NodeModel parent,
        PortAlignment alignment = PortAlignment.Right,
        UmlPortKind kind = UmlPortKind.Association,
        string? name = null,
        Point? position = null,
        Size? size = null)
        : base(parent, alignment, position, size)
    {
        Kind = kind;
        Name = name;
    }

    /// <summary>Creates a UML port with the given id.</summary>
    public UmlPortModel(
        string id,
        NodeModel parent,
        PortAlignment alignment = PortAlignment.Right,
        UmlPortKind kind = UmlPortKind.Association,
        string? name = null,
        Point? position = null,
        Size? size = null)
        : base(id, parent, alignment, position, size)
    {
        Kind = kind;
        Name = name;
    }

    /// <summary>The port role.</summary>
    public UmlPortKind Kind { get; set; }

    /// <summary>An optional member, operation, or literal name the port should align with.</summary>
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

        var rowY = ResolveNamedRowCenter();
        if (rowY == null)
            return base.GetPortCenter();

        var x = Alignment == PortAlignment.Left
            ? Parent.Position.X
            : Parent.Position.X + parentSize.Width;

        return new Point(x, Parent.Position.Y + rowY.Value);
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
            Enum.TryParse<UmlPortKind>(kindText, out var parsedKind))
        {
            Kind = parsedKind;
        }

        if (data.TryGetValue("Name", out var name) && name is string nameText)
            Name = nameText;
    }

    private double? ResolveNamedRowCenter()
    {
        var name = Name ?? string.Empty;
        return Parent switch
        {
            UmlClassNode node => ResolveClassRow(node, name),
            UmlInterfaceNode node => ResolveOperationsRow(node.Operations, name, SectionStart() + UmlVisualMetrics.SectionHeaderHeight),
            UmlEnumNode node => ResolveLiteralRow(node, name),
            _ => null,
        };
    }

    private static double? ResolveClassRow(UmlClassNode node, string name)
    {
        for (var i = 0; i < node.Members.Count; i++)
            if (string.Equals(node.Members[i].Name, name, StringComparison.OrdinalIgnoreCase))
                return SectionStart() + UmlVisualMetrics.SectionHeaderHeight + i * UmlVisualMetrics.RowHeight + UmlVisualMetrics.RowHeight / 2;

        var operationStart = SectionStart() +
            UmlVisualMetrics.SectionHeaderHeight +
            node.Members.Count * UmlVisualMetrics.RowHeight +
            UmlVisualMetrics.SectionHeaderHeight;
        return ResolveOperationsRow(node.Operations, name, operationStart);
    }

    private static double? ResolveOperationsRow(IReadOnlyList<UmlOperation> operations, string name, double start)
    {
        for (var i = 0; i < operations.Count; i++)
            if (string.Equals(operations[i].Name, name, StringComparison.OrdinalIgnoreCase))
                return start + i * UmlVisualMetrics.RowHeight + UmlVisualMetrics.RowHeight / 2;

        return null;
    }

    private static double? ResolveLiteralRow(UmlEnumNode node, string name)
    {
        for (var i = 0; i < node.Literals.Count; i++)
            if (string.Equals(node.Literals[i], name, StringComparison.OrdinalIgnoreCase))
                return SectionStart() + UmlVisualMetrics.SectionHeaderHeight + i * UmlVisualMetrics.RowHeight + UmlVisualMetrics.RowHeight / 2;

        return null;
    }

    private static double SectionStart() => UmlVisualMetrics.HeaderHeight;
}
