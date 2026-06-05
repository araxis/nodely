using System;
using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Avalonia.Network;

/// <summary>A network port with a typed role and optional switch-port index.</summary>
public sealed class NetworkPortModel : PortModel
{
    public new const string ModelKindKey = "network.port";

    public NetworkPortModel(
        NodeModel parent,
        PortAlignment alignment = PortAlignment.Right,
        NetworkPortRole role = NetworkPortRole.Lan,
        string? name = null,
        int? index = null,
        Point? position = null,
        Size? size = null)
        : base(parent, alignment, position, size)
    {
        Role = role;
        Name = NormalizeOptional(name);
        Index = index;
    }

    public NetworkPortModel(
        string id,
        NodeModel parent,
        PortAlignment alignment = PortAlignment.Right,
        NetworkPortRole role = NetworkPortRole.Lan,
        string? name = null,
        int? index = null,
        Point? position = null,
        Size? size = null)
        : base(id, parent, alignment, position, size)
    {
        Role = role;
        Name = NormalizeOptional(name);
        Index = index;
    }

    /// <summary>The port role.</summary>
    public NetworkPortRole Role { get; set; }

    /// <summary>Optional semantic name, such as <c>wan0</c> or <c>mgmt</c>.</summary>
    public string? Name { get; set; }

    /// <summary>Optional zero-based index used by switch-front renderers.</summary>
    public int? Index { get; set; }

    public override string ModelKind => ModelKindKey;

    public override bool CanAttachTo(ILinkable other)
        => other is NetworkPortModel port && port != this && port.Parent != Parent && !port.Locked;

    public override Point GetPortCenter()
    {
        if (Parent is not NetworkSwitchNode || Index is not { } index || Parent.Size is not { } parentSize)
            return base.GetPortCenter();

        var safeIndex = Math.Max(0, index);
        var columns = Math.Max(4, Math.Min(12, ((NetworkSwitchNode)Parent).PortCount));
        var x = Parent.Position.X + 22 + (safeIndex % columns) * 15;
        var y = Parent.Position.Y + NetworkVisualMetrics.HeaderHeight + 18 + (safeIndex / columns) * 17;
        return new Point(Math.Min(x, Parent.Position.X + parentSize.Width - 18), Math.Min(y, Parent.Position.Y + parentSize.Height - 14));
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = new Dictionary<string, object?> { ["PortRole"] = Role.ToString() };
        if (!string.IsNullOrWhiteSpace(Name))
            extra["Name"] = Name;
        if (Index.HasValue)
            extra["Index"] = Index.Value;
        return extra;
    }

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("PortRole", out var role) &&
            role is string roleText &&
            Enum.TryParse<NetworkPortRole>(roleText, out var parsedRole))
        {
            Role = parsedRole;
        }

        if (data.TryGetValue("Name", out var name) && name is string nameText)
            Name = NormalizeOptional(nameText);
        if (data.TryGetValue("Index", out var index) && index is not null)
            Index = ParseInt(index, 0);
    }

    private static int ParseInt(object value, int fallback)
        => value switch
        {
            int intValue => intValue,
            long longValue => longValue > int.MaxValue ? int.MaxValue : (int)longValue,
            double doubleValue => (int)doubleValue,
            string text when int.TryParse(text, out var parsed) => parsed,
            _ => fallback,
        };

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
