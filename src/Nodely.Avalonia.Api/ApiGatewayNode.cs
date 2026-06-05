using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Api;

/// <summary>An API gateway or edge route node.</summary>
public sealed class ApiGatewayNode : ApiNodeBase
{
    public new const string ModelKindKey = "api.gateway";

    private string? _host;

    public ApiGatewayNode(Point position, string name = "Gateway") : base(position, name) { }

    public ApiGatewayNode(string id, Point position, string name = "Gateway") : base(id, position, name) { }

    /// <summary>Optional gateway host.</summary>
    public string? Host
    {
        get => _host;
        set
        {
            _host = NormalizeOptional(value);
            Refresh();
        }
    }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "Gateway";

    protected override string DefaultAccentColor => "#4C8BDC";

    protected override string DefaultIconKey => "GW";

    public override NodeModel Clone()
    {
        var clone = new ApiGatewayNode(Position, Name) { Host = Host };
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["Host"] = Host;
        return extra;
    }

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        if (data.TryGetValue("Host", out var host) && host is string hostText)
            _host = NormalizeOptional(hostText);
    }
}
