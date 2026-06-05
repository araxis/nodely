using System;
using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Avalonia.Api;

/// <summary>An API port with a typed semantic role.</summary>
public sealed class ApiPortModel : PortModel
{
    public new const string ModelKindKey = "api.port";

    public ApiPortModel(
        NodeModel parent,
        PortAlignment alignment = PortAlignment.Right,
        ApiPortRole role = ApiPortRole.Request,
        string? name = null,
        Point? position = null,
        Size? size = null)
        : base(parent, alignment, position, size)
    {
        Role = role;
        Name = NormalizeOptional(name);
    }

    public ApiPortModel(
        string id,
        NodeModel parent,
        PortAlignment alignment = PortAlignment.Right,
        ApiPortRole role = ApiPortRole.Request,
        string? name = null,
        Point? position = null,
        Size? size = null)
        : base(id, parent, alignment, position, size)
    {
        Role = role;
        Name = NormalizeOptional(name);
    }

    /// <summary>The port role.</summary>
    public ApiPortRole Role { get; set; }

    /// <summary>Optional semantic name.</summary>
    public string? Name { get; set; }

    public override string ModelKind => ModelKindKey;

    public override bool CanAttachTo(ILinkable other)
        => other is ApiPortModel port && port != this && port.Parent != Parent && !port.Locked;

    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = new Dictionary<string, object?> { ["PortRole"] = Role.ToString() };
        if (!string.IsNullOrWhiteSpace(Name))
            extra["Name"] = Name;
        return extra;
    }

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("PortRole", out var role) &&
            role is string roleText &&
            Enum.TryParse<ApiPortRole>(roleText, out var parsedRole))
        {
            Role = parsedRole;
        }

        if (data.TryGetValue("Name", out var name) && name is string nameText)
            Name = NormalizeOptional(nameText);
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
