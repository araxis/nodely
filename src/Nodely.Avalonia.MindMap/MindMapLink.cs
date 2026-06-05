using System;
using System.Collections.Generic;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.PathGenerators;

namespace Nodely.Avalonia.MindMap;

/// <summary>A curved mind-map branch or association link.</summary>
public sealed class MindMapLink : LinkModel
{
    /// <summary>The stable serialization kind for mind-map links.</summary>
    public new const string ModelKindKey = "mindmap.link";

    private static readonly LinkMarker HiddenMarker = LinkMarker.NewRectangle(0, 0);
    private MindMapLinkKind _kind;
    private string? _label;
    private string? _accentColor;

    /// <summary>Creates a mind-map link between two ports.</summary>
    public MindMapLink(PortModel sourcePort, PortModel targetPort, MindMapLinkKind kind = MindMapLinkKind.Branch)
        : base(sourcePort, targetPort)
    {
        Kind = kind;
    }

    /// <summary>Creates a mind-map link with the given id between two ports.</summary>
    public MindMapLink(string id, PortModel sourcePort, PortModel targetPort, MindMapLinkKind kind = MindMapLinkKind.Branch)
        : base(id, sourcePort, targetPort)
    {
        Kind = kind;
    }

    /// <summary>Creates a mind-map link between two nodes.</summary>
    public MindMapLink(NodeModel sourceNode, NodeModel targetNode, MindMapLinkKind kind = MindMapLinkKind.Branch)
        : base(sourceNode, targetNode)
    {
        Kind = kind;
    }

    /// <summary>Creates a mind-map link with the given id between two anchors.</summary>
    public MindMapLink(string id, Anchor source, Anchor target, MindMapLinkKind kind = MindMapLinkKind.Branch)
        : base(id, source, target)
    {
        Kind = kind;
    }

    /// <summary>The link kind.</summary>
    public MindMapLinkKind Kind
    {
        get => _kind;
        set
        {
            _kind = value;
            ApplyKindDefaults();
            Refresh();
        }
    }

    /// <summary>Optional label placed near the middle of the link.</summary>
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

    /// <summary>Optional accent color for this link, stored as a hex string such as <c>#37A779</c>.</summary>
    public string? AccentColor
    {
        get => _accentColor;
        set
        {
            _accentColor = NormalizeOptional(value);
            Refresh();
        }
    }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = new Dictionary<string, object?> { ["MindMapLinkKind"] = Kind.ToString() };
        if (!string.IsNullOrWhiteSpace(Label))
            extra["Label"] = Label;
        if (!string.IsNullOrWhiteSpace(AccentColor))
            extra["AccentColor"] = AccentColor;
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("MindMapLinkKind", out var kind) &&
            kind is string kindText &&
            Enum.TryParse<MindMapLinkKind>(kindText, out var parsedKind))
        {
            Kind = parsedKind;
        }

        if (data.TryGetValue("Label", out var label) && label is string labelText)
            Label = labelText;
        if (data.TryGetValue("AccentColor", out var accent) && accent is string accentText)
            AccentColor = accentText;
    }

    private void ApplyKindDefaults()
    {
        Segmentable = true;
        SourceMarker = HiddenMarker;
        TargetMarker = HiddenMarker;
        PathGenerator = new SmoothPathGenerator(90);
        Width = Kind == MindMapLinkKind.Branch ? 3.4 : 2.1;
    }

    private void SyncLabels()
    {
        Labels.Clear();
        if (!string.IsNullOrWhiteSpace(_label))
            AddLabel(_label, 0.5, new Point(0, Kind == MindMapLinkKind.Branch ? -18 : 16));
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
