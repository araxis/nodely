using System;
using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Avalonia.MindMap;

/// <summary>A mind-map topic port for branch and association links.</summary>
public sealed class MindMapPortModel : PortModel
{
    /// <summary>The stable serialization kind for mind-map ports.</summary>
    public new const string ModelKindKey = "mindmap.port";

    /// <summary>Creates a mind-map port.</summary>
    public MindMapPortModel(
        NodeModel parent,
        PortAlignment alignment = PortAlignment.Right,
        MindMapPortRole role = MindMapPortRole.Branch,
        string? name = null,
        Point? position = null,
        Size? size = null)
        : base(parent, alignment, position, size)
    {
        Role = role;
        Name = NormalizeOptional(name);
    }

    /// <summary>Creates a mind-map port with the given id.</summary>
    public MindMapPortModel(
        string id,
        NodeModel parent,
        PortAlignment alignment = PortAlignment.Right,
        MindMapPortRole role = MindMapPortRole.Branch,
        string? name = null,
        Point? position = null,
        Size? size = null)
        : base(id, parent, alignment, position, size)
    {
        Role = role;
        Name = NormalizeOptional(name);
    }

    /// <summary>The port role.</summary>
    public MindMapPortRole Role { get; set; }

    /// <summary>An optional semantic port name, e.g. <c>branch-out</c>.</summary>
    public string? Name { get; set; }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override bool CanAttachTo(ILinkable other)
        => other is MindMapPortModel port && port != this && port.Parent != Parent && !port.Locked;

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = new Dictionary<string, object?> { ["PortRole"] = Role.ToString() };
        if (!string.IsNullOrWhiteSpace(Name))
            extra["Name"] = Name;
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("PortRole", out var role) &&
            role is string roleText &&
            Enum.TryParse<MindMapPortRole>(roleText, out var parsedRole))
        {
            Role = parsedRole;
        }

        if (data.TryGetValue("Name", out var name) && name is string nameText)
            Name = NormalizeOptional(nameText);
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
