using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Api;

/// <summary>An API client or consumer node.</summary>
public sealed class ApiClientNode : ApiNodeBase
{
    public new const string ModelKindKey = "api.client";

    private string? _platform;

    public ApiClientNode(Point position, string name = "Client") : base(position, name) { }

    public ApiClientNode(string id, Point position, string name = "Client") : base(id, position, name) { }

    /// <summary>Optional platform or app type.</summary>
    public string? Platform
    {
        get => _platform;
        set
        {
            _platform = NormalizeOptional(value);
            Refresh();
        }
    }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "Client";

    protected override string DefaultAccentColor => "#33A6B8";

    protected override string DefaultIconKey => "APP";

    public override NodeModel Clone()
    {
        var clone = new ApiClientNode(Position, Name) { Platform = Platform };
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["Platform"] = Platform;
        return extra;
    }

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        if (data.TryGetValue("Platform", out var platform) && platform is string platformText)
            _platform = NormalizeOptional(platformText);
    }
}
