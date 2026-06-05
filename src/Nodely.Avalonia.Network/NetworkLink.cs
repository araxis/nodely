using System;
using System.Collections.Generic;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.PathGenerators;

namespace Nodely.Avalonia.Network;

/// <summary>A network topology link with protocol, capacity, latency, status, and direction metadata.</summary>
public sealed class NetworkLink : LinkModel
{
    public new const string ModelKindKey = "network.link";

    private static readonly LinkMarker HiddenMarker = LinkMarker.NewRectangle(0, 0);
    private NetworkLinkKind _kind;
    private NetworkStatus _status = NetworkStatus.Online;
    private NetworkLinkDirection _direction = NetworkLinkDirection.SourceToTarget;
    private string? _label;
    private string? _protocol;
    private string? _bandwidth;
    private string? _latency;
    private string? _accentColor;

    public NetworkLink(PortModel sourcePort, PortModel targetPort, NetworkLinkKind kind = NetworkLinkKind.Ethernet)
        : base(sourcePort, targetPort)
    {
        Kind = kind;
    }

    public NetworkLink(NodeModel sourceNode, NodeModel targetNode, NetworkLinkKind kind = NetworkLinkKind.Ethernet)
        : base(sourceNode, targetNode)
    {
        Kind = kind;
    }

    public NetworkLink(string id, Anchor source, Anchor target, NetworkLinkKind kind = NetworkLinkKind.Ethernet)
        : base(id, source, target)
    {
        Kind = kind;
    }

    /// <summary>The topology link kind.</summary>
    public NetworkLinkKind Kind
    {
        get => _kind;
        set
        {
            _kind = value;
            ApplyDefaults();
            Refresh();
        }
    }

    /// <summary>Operational status for this link.</summary>
    public NetworkStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            Refresh();
        }
    }

    /// <summary>Traffic direction metadata.</summary>
    public NetworkLinkDirection Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            ApplyDirectionMarkers();
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

    /// <summary>Optional bandwidth text.</summary>
    public string? Bandwidth
    {
        get => _bandwidth;
        set
        {
            _bandwidth = NormalizeOptional(value);
            SyncLabels();
            Refresh();
        }
    }

    /// <summary>Optional latency text.</summary>
    public string? Latency
    {
        get => _latency;
        set
        {
            _latency = NormalizeOptional(value);
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
            ["NetworkLinkKind"] = Kind.ToString(),
            ["Status"] = Status.ToString(),
            ["Direction"] = Direction.ToString(),
        };
        if (!string.IsNullOrWhiteSpace(Label))
            extra["Label"] = Label;
        if (!string.IsNullOrWhiteSpace(Protocol))
            extra["Protocol"] = Protocol;
        if (!string.IsNullOrWhiteSpace(Bandwidth))
            extra["Bandwidth"] = Bandwidth;
        if (!string.IsNullOrWhiteSpace(Latency))
            extra["Latency"] = Latency;
        if (!string.IsNullOrWhiteSpace(AccentColor))
            extra["AccentColor"] = AccentColor;
        return extra;
    }

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("NetworkLinkKind", out var kind) &&
            kind is string kindText &&
            Enum.TryParse<NetworkLinkKind>(kindText, out var parsedKind))
        {
            Kind = parsedKind;
        }

        if (data.TryGetValue("Status", out var status) && status is string statusText && Enum.TryParse<NetworkStatus>(statusText, out var parsedStatus))
            Status = parsedStatus;
        if (data.TryGetValue("Direction", out var direction) && direction is string directionText && Enum.TryParse<NetworkLinkDirection>(directionText, out var parsedDirection))
            Direction = parsedDirection;
        if (data.TryGetValue("Label", out var label) && label is string labelText)
            Label = labelText;
        if (data.TryGetValue("Protocol", out var protocol) && protocol is string protocolText)
            Protocol = protocolText;
        if (data.TryGetValue("Bandwidth", out var bandwidth) && bandwidth is string bandwidthText)
            Bandwidth = bandwidthText;
        if (data.TryGetValue("Latency", out var latency) && latency is string latencyText)
            Latency = latencyText;
        if (data.TryGetValue("AccentColor", out var accent) && accent is string accentText)
            AccentColor = accentText;

        ApplyDirectionMarkers();
        SyncLabels();
    }

    private void ApplyDefaults()
    {
        Segmentable = true;
        PathGenerator = new SmoothPathGenerator(Kind is NetworkLinkKind.Wireless or NetworkLinkKind.VpnTunnel ? 105 : 72);
        Width = Kind switch
        {
            NetworkLinkKind.Fiber => 3.4,
            NetworkLinkKind.Blocked => 2.8,
            NetworkLinkKind.Dependency => 1.9,
            NetworkLinkKind.Wireless => 2.5,
            _ => 2.4,
        };
        ApplyDirectionMarkers();
    }

    private void ApplyDirectionMarkers()
    {
        SourceMarker = Direction is NetworkLinkDirection.TargetToSource or NetworkLinkDirection.Bidirectional
            ? LinkMarker.Arrow
            : HiddenMarker;
        TargetMarker = Direction is NetworkLinkDirection.SourceToTarget or NetworkLinkDirection.Bidirectional
            ? LinkMarker.Arrow
            : HiddenMarker;
    }

    private void SyncLabels()
    {
        Labels.Clear();
        var text = FormatLabel();
        if (!string.IsNullOrWhiteSpace(text))
            AddLabel(text, 0.5, new Point(0, Kind == NetworkLinkKind.Dependency ? 18 : -18));
    }

    internal string FormatLabel()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(Label))
            parts.Add(Label!);
        if (!string.IsNullOrWhiteSpace(Protocol))
            parts.Add(Protocol!);
        if (!string.IsNullOrWhiteSpace(Bandwidth))
            parts.Add(Bandwidth!);
        if (!string.IsNullOrWhiteSpace(Latency))
            parts.Add(Latency!);
        return string.Join(" · ", parts);
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
