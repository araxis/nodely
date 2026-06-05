using System;
using System.Collections.Generic;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.PathGenerators;

namespace Nodely.Avalonia.Api;

/// <summary>An API relationship link with request, response, event, dependency, and auth metadata.</summary>
public sealed class ApiLink : LinkModel
{
    public new const string ModelKindKey = "api.link";

    private static readonly LinkMarker HiddenMarker = LinkMarker.NewRectangle(0, 0);
    private ApiLinkKind _kind;
    private ApiEndpointStatus _status = ApiEndpointStatus.Stable;
    private string? _label;
    private string? _protocol;
    private string? _payload;
    private string? _accentColor;

    public ApiLink(PortModel sourcePort, PortModel targetPort, ApiLinkKind kind = ApiLinkKind.Request)
        : base(sourcePort, targetPort)
    {
        Kind = kind;
    }

    public ApiLink(NodeModel sourceNode, NodeModel targetNode, ApiLinkKind kind = ApiLinkKind.Request)
        : base(sourceNode, targetNode)
    {
        Kind = kind;
    }

    public ApiLink(string id, Anchor source, Anchor target, ApiLinkKind kind = ApiLinkKind.Request)
        : base(id, source, target)
    {
        Kind = kind;
    }

    /// <summary>The API relationship kind.</summary>
    public ApiLinkKind Kind
    {
        get => _kind;
        set
        {
            _kind = value;
            ApplyDefaults();
            Refresh();
        }
    }

    /// <summary>Lifecycle status for this relationship.</summary>
    public ApiEndpointStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            Refresh();
        }
    }

    /// <summary>Optional link label.</summary>
    public string? Label
    {
        get => _label;
        set
        {
            _label = NormalizeOptional(value);
            SyncLabels();
            Refresh();
        }
    }

    /// <summary>Optional protocol text.</summary>
    public string? Protocol
    {
        get => _protocol;
        set
        {
            _protocol = NormalizeOptional(value);
            SyncLabels();
            Refresh();
        }
    }

    /// <summary>Optional payload or contract text.</summary>
    public string? Payload
    {
        get => _payload;
        set
        {
            _payload = NormalizeOptional(value);
            SyncLabels();
            Refresh();
        }
    }

    /// <summary>Optional accent color for this link.</summary>
    public string? AccentColor
    {
        get => _accentColor;
        set
        {
            _accentColor = NormalizeOptional(value);
            Refresh();
        }
    }

    public override string ModelKind => ModelKindKey;

    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = new Dictionary<string, object?>
        {
            ["ApiLinkKind"] = Kind.ToString(),
            ["Status"] = Status.ToString(),
        };
        if (!string.IsNullOrWhiteSpace(Label))
            extra["Label"] = Label;
        if (!string.IsNullOrWhiteSpace(Protocol))
            extra["Protocol"] = Protocol;
        if (!string.IsNullOrWhiteSpace(Payload))
            extra["Payload"] = Payload;
        if (!string.IsNullOrWhiteSpace(AccentColor))
            extra["AccentColor"] = AccentColor;
        return extra;
    }

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("ApiLinkKind", out var kind) &&
            kind is string kindText &&
            Enum.TryParse<ApiLinkKind>(kindText, out var parsedKind))
        {
            Kind = parsedKind;
        }

        if (data.TryGetValue("Status", out var status) && status is string statusText && Enum.TryParse<ApiEndpointStatus>(statusText, out var parsedStatus))
            Status = parsedStatus;
        if (data.TryGetValue("Label", out var label) && label is string labelText)
            Label = labelText;
        if (data.TryGetValue("Protocol", out var protocol) && protocol is string protocolText)
            Protocol = protocolText;
        if (data.TryGetValue("Payload", out var payload) && payload is string payloadText)
            Payload = payloadText;
        if (data.TryGetValue("AccentColor", out var accent) && accent is string accentText)
            AccentColor = accentText;

        ApplyMarkers();
        SyncLabels();
    }

    private void ApplyDefaults()
    {
        Segmentable = true;
        PathGenerator = new SmoothPathGenerator(Kind is ApiLinkKind.Publishes or ApiLinkKind.Consumes ? 105 : 76);
        Width = Kind switch
        {
            ApiLinkKind.Response => 2.6,
            ApiLinkKind.Secures => 2.4,
            ApiLinkKind.DependsOn => 1.9,
            ApiLinkKind.Publishes or ApiLinkKind.Consumes => 2.3,
            _ => 2.5,
        };
        ApplyMarkers();
    }

    private void ApplyMarkers()
    {
        SourceMarker = HiddenMarker;
        TargetMarker = LinkMarker.Arrow;
    }

    private void SyncLabels()
    {
        Labels.Clear();
        var text = FormatLabel();
        if (!string.IsNullOrWhiteSpace(text))
            AddLabel(text, 0.5, new Point(0, Kind == ApiLinkKind.DependsOn ? 18 : -18));
    }

    internal string FormatLabel()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(Label))
            parts.Add(Label!);
        if (!string.IsNullOrWhiteSpace(Protocol))
            parts.Add(Protocol!);
        if (!string.IsNullOrWhiteSpace(Payload))
            parts.Add(Payload!);
        return string.Join(" | ", parts);
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
