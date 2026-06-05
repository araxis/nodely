using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Api;

/// <summary>An API endpoint card node.</summary>
public sealed class ApiEndpointNode : ApiNodeBase
{
    public new const string ModelKindKey = "api.endpoint";

    private ApiEndpointMethod _method = ApiEndpointMethod.Get;
    private string _route = "/resource";
    private string? _requestType;
    private string? _responseType;

    public ApiEndpointNode(Point position, string route = "/resource", ApiEndpointMethod method = ApiEndpointMethod.Get)
        : base(position, route)
    {
        _route = Normalize(route, "/resource");
        _method = method;
        Name = _route;
    }

    public ApiEndpointNode(string id, Point position, string route = "/resource", ApiEndpointMethod method = ApiEndpointMethod.Get)
        : base(id, position, route)
    {
        _route = Normalize(route, "/resource");
        _method = method;
        Name = _route;
    }

    /// <summary>The endpoint method.</summary>
    public ApiEndpointMethod Method
    {
        get => _method;
        set
        {
            _method = value;
            Refresh();
        }
    }

    /// <summary>The route path.</summary>
    public string Route
    {
        get => _route;
        set
        {
            _route = Normalize(value, "/resource");
            Name = _route;
            Refresh();
        }
    }

    /// <summary>Optional request contract name.</summary>
    public string? RequestType
    {
        get => _requestType;
        set
        {
            _requestType = NormalizeOptional(value);
            Refresh();
        }
    }

    /// <summary>Optional response contract name.</summary>
    public string? ResponseType
    {
        get => _responseType;
        set
        {
            _responseType = NormalizeOptional(value);
            Refresh();
        }
    }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "/resource";

    protected override string DefaultAccentColor => "#37A779";

    protected override string DefaultIconKey => "HTTP";

    public override NodeModel Clone()
    {
        var clone = new ApiEndpointNode(Position, Route, Method)
        {
            RequestType = RequestType,
            ResponseType = ResponseType,
        };
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["Method"] = Method.ToString();
        extra["Route"] = Route;
        extra["RequestType"] = RequestType;
        extra["ResponseType"] = ResponseType;
        return extra;
    }

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        if (data.TryGetValue("Method", out var method) && method is string methodText && System.Enum.TryParse<ApiEndpointMethod>(methodText, out var parsedMethod))
            _method = parsedMethod;
        if (data.TryGetValue("Route", out var route) && route is string routeText)
            _route = Normalize(routeText, "/resource");
        if (data.TryGetValue("RequestType", out var request) && request is string requestText)
            _requestType = NormalizeOptional(requestText);
        if (data.TryGetValue("ResponseType", out var response) && response is string responseText)
            _responseType = NormalizeOptional(responseText);
        Name = _route;
    }
}
