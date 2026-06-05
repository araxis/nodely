using System;
using System.Collections.Generic;
using Nodely.Anchors;
using Nodely.Models;

namespace Nodely.Avalonia.Database;

/// <summary>A relationship or dependency link between database objects.</summary>
public sealed class DatabaseRelationshipLink : LinkModel
{
    /// <summary>The stable serialization kind for database relationship links.</summary>
    public new const string ModelKindKey = "database.relationship";

    private RelationshipKind _kind;

    /// <summary>Creates a relationship between two ports.</summary>
    public DatabaseRelationshipLink(PortModel sourcePort, PortModel targetPort, RelationshipKind kind = RelationshipKind.OneToMany)
        : base(sourcePort, targetPort)
    {
        Kind = kind;
    }

    /// <summary>Creates a relationship with the given id between two ports.</summary>
    public DatabaseRelationshipLink(string id, PortModel sourcePort, PortModel targetPort, RelationshipKind kind = RelationshipKind.OneToMany)
        : base(id, sourcePort, targetPort)
    {
        Kind = kind;
    }

    /// <summary>Creates a relationship between two nodes.</summary>
    public DatabaseRelationshipLink(NodeModel sourceNode, NodeModel targetNode, RelationshipKind kind = RelationshipKind.Dependency)
        : base(sourceNode, targetNode)
    {
        Kind = kind;
    }

    /// <summary>Creates a relationship with the given id between two anchors.</summary>
    public DatabaseRelationshipLink(string id, Anchor source, Anchor target, RelationshipKind kind = RelationshipKind.Association)
        : base(id, source, target)
    {
        Kind = kind;
    }

    /// <summary>The relationship kind.</summary>
    public RelationshipKind Kind
    {
        get => _kind;
        set
        {
            _kind = value;
            ApplyKindDefaults();
            Refresh();
        }
    }

    /// <summary>Optional source cardinality label, e.g. <c>1</c>.</summary>
    public string? SourceCardinality { get; set; }

    /// <summary>Optional target cardinality label, e.g. <c>many</c>.</summary>
    public string? TargetCardinality { get; set; }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = new Dictionary<string, object?> { ["RelationshipKind"] = Kind.ToString() };
        if (!string.IsNullOrWhiteSpace(SourceCardinality))
            extra["SourceCardinality"] = SourceCardinality;
        if (!string.IsNullOrWhiteSpace(TargetCardinality))
            extra["TargetCardinality"] = TargetCardinality;
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("RelationshipKind", out var kind) &&
            kind is string kindText &&
            Enum.TryParse<RelationshipKind>(kindText, out var parsedKind))
        {
            Kind = parsedKind;
        }

        if (data.TryGetValue("SourceCardinality", out var source) && source is string sourceText)
            SourceCardinality = sourceText;
        if (data.TryGetValue("TargetCardinality", out var target) && target is string targetText)
            TargetCardinality = targetText;
    }

    private void ApplyKindDefaults()
    {
        Segmentable = true;
        SourceMarker = null;
        TargetMarker = null;
        Width = Kind == RelationshipKind.Dependency ? 1.75 : 2.4;
    }
}
