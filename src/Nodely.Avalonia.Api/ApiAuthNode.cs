using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Api;

/// <summary>An API authentication or authorization policy node.</summary>
public sealed class ApiAuthNode : ApiNodeBase
{
    public new const string ModelKindKey = "api.auth";

    private string? _scheme;
    private string? _scopes;

    public ApiAuthNode(Point position, string name = "Auth") : base(position, name) { }

    public ApiAuthNode(string id, Point position, string name = "Auth") : base(id, position, name) { }

    /// <summary>Authentication scheme text.</summary>
    public string? Scheme
    {
        get => _scheme;
        set
        {
            _scheme = NormalizeOptional(value);
            Refresh();
        }
    }

    /// <summary>Optional scope or policy text.</summary>
    public string? Scopes
    {
        get => _scopes;
        set
        {
            _scopes = NormalizeOptional(value);
            Refresh();
        }
    }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "Auth";

    protected override string DefaultAccentColor => "#C45552";

    protected override string DefaultIconKey => "AUTH";

    public override NodeModel Clone()
    {
        var clone = new ApiAuthNode(Position, Name)
        {
            Scheme = Scheme,
            Scopes = Scopes,
        };
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["Scheme"] = Scheme;
        extra["Scopes"] = Scopes;
        return extra;
    }

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        if (data.TryGetValue("Scheme", out var scheme) && scheme is string schemeText)
            _scheme = NormalizeOptional(schemeText);
        if (data.TryGetValue("Scopes", out var scopes) && scopes is string scopesText)
            _scopes = NormalizeOptional(scopesText);
    }
}
