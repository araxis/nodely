using System;
using System.Collections.Generic;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Uml;

/// <summary>A UML relationship link.</summary>
public sealed class UmlRelationshipLink : LinkModel
{
    /// <summary>The stable serialization kind for UML relationship links.</summary>
    public new const string ModelKindKey = "uml.relationship";

    private static readonly LinkMarker HiddenMarker = LinkMarker.NewRectangle(0, 0);
    private UmlRelationshipKind _kind;
    private string? _label;
    private string? _sourceMultiplicity;
    private string? _targetMultiplicity;

    /// <summary>Creates a UML relationship between two nodes.</summary>
    public UmlRelationshipLink(NodeModel sourceNode, NodeModel targetNode, UmlRelationshipKind kind = UmlRelationshipKind.Association)
        : base(sourceNode, targetNode)
    {
        Kind = kind;
    }

    /// <summary>Creates a UML relationship between two ports.</summary>
    public UmlRelationshipLink(PortModel sourcePort, PortModel targetPort, UmlRelationshipKind kind = UmlRelationshipKind.Association)
        : base(sourcePort, targetPort)
    {
        Kind = kind;
    }

    /// <summary>Creates a UML relationship with the given id between two ports.</summary>
    public UmlRelationshipLink(string id, PortModel sourcePort, PortModel targetPort, UmlRelationshipKind kind = UmlRelationshipKind.Association)
        : base(id, sourcePort, targetPort)
    {
        Kind = kind;
    }

    /// <summary>Creates a UML relationship with the given id between two anchors.</summary>
    public UmlRelationshipLink(string id, Anchor source, Anchor target, UmlRelationshipKind kind = UmlRelationshipKind.Association)
        : base(id, source, target)
    {
        Kind = kind;
    }

    /// <summary>The relationship kind.</summary>
    public UmlRelationshipKind Kind
    {
        get => _kind;
        set
        {
            _kind = value;
            ApplyKindDefaults();
            Refresh();
        }
    }

    /// <summary>Optional label placed near the middle of the relationship.</summary>
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

    /// <summary>Optional source multiplicity, e.g. <c>1</c>.</summary>
    public string? SourceMultiplicity
    {
        get => _sourceMultiplicity;
        set
        {
            _sourceMultiplicity = NormalizeOptional(value);
            SyncLabels();
            Refresh();
        }
    }

    /// <summary>Optional target multiplicity, e.g. <c>0..*</c>.</summary>
    public string? TargetMultiplicity
    {
        get => _targetMultiplicity;
        set
        {
            _targetMultiplicity = NormalizeOptional(value);
            SyncLabels();
            Refresh();
        }
    }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = new Dictionary<string, object?> { ["RelationshipKind"] = Kind.ToString() };
        if (!string.IsNullOrWhiteSpace(Label))
            extra["Label"] = Label;
        if (!string.IsNullOrWhiteSpace(SourceMultiplicity))
            extra["SourceMultiplicity"] = SourceMultiplicity;
        if (!string.IsNullOrWhiteSpace(TargetMultiplicity))
            extra["TargetMultiplicity"] = TargetMultiplicity;
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("RelationshipKind", out var kind) &&
            kind is string kindText &&
            Enum.TryParse<UmlRelationshipKind>(kindText, out var parsedKind))
        {
            Kind = parsedKind;
        }

        if (data.TryGetValue("Label", out var label) && label is string labelText)
            Label = labelText;
        if (data.TryGetValue("SourceMultiplicity", out var source) && source is string sourceText)
            SourceMultiplicity = sourceText;
        if (data.TryGetValue("TargetMultiplicity", out var target) && target is string targetText)
            TargetMultiplicity = targetText;
    }

    private void ApplyKindDefaults()
    {
        Segmentable = true;
        SourceMarker = HiddenMarker;
        TargetMarker = HiddenMarker;
        Width = Kind is UmlRelationshipKind.Dependency or UmlRelationshipKind.Realization ? 1.8 : 2.2;
    }

    private void SyncLabels()
    {
        Labels.Clear();

        if (!string.IsNullOrWhiteSpace(_label))
            AddLabel(_label, 0.5, new Point(0, -16));
        if (!string.IsNullOrWhiteSpace(_sourceMultiplicity))
            AddLabel(_sourceMultiplicity, 0.12, new Point(0, 14));
        if (!string.IsNullOrWhiteSpace(_targetMultiplicity))
            AddLabel(_targetMultiplicity, 0.88, new Point(0, 14));
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
