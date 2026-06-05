using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Api;

/// <summary>An API service boundary node.</summary>
public sealed class ApiServiceNode : ApiNodeBase
{
    public new const string ModelKindKey = "api.service";

    private string? _baseUrl;
    private string? _owner;

    public ApiServiceNode(Point position, string name = "Service") : base(position, name) { }

    public ApiServiceNode(string id, Point position, string name = "Service") : base(id, position, name) { }

    /// <summary>Optional base URL or host name.</summary>
    public string? BaseUrl
    {
        get => _baseUrl;
        set
        {
            _baseUrl = NormalizeOptional(value);
            Refresh();
        }
    }

    /// <summary>Optional owner or team text.</summary>
    public string? Owner
    {
        get => _owner;
        set
        {
            _owner = NormalizeOptional(value);
            Refresh();
        }
    }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "Service";

    protected override string DefaultAccentColor => "#4D9EFF";

    protected override string DefaultIconKey => "SVC";

    public override NodeModel Clone()
    {
        var clone = new ApiServiceNode(Position, Name)
        {
            BaseUrl = BaseUrl,
            Owner = Owner,
        };
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["BaseUrl"] = BaseUrl;
        extra["Owner"] = Owner;
        return extra;
    }

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        if (data.TryGetValue("BaseUrl", out var baseUrl) && baseUrl is string baseUrlText)
            _baseUrl = NormalizeOptional(baseUrlText);
        if (data.TryGetValue("Owner", out var owner) && owner is string ownerText)
            _owner = NormalizeOptional(ownerText);
    }
}
