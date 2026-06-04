using Nodely.Anchors;
using Nodely.Models;

namespace Nodely.Avalonia.Database;

/// <summary>A relationship or dependency link between database objects.</summary>
public sealed class DatabaseRelationshipLink : LinkModel
{
    private RelationshipKind _kind;

    /// <summary>Creates a relationship between two ports.</summary>
    public DatabaseRelationshipLink(PortModel sourcePort, PortModel targetPort, RelationshipKind kind = RelationshipKind.OneToMany)
        : base(sourcePort, targetPort)
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

    private void ApplyKindDefaults()
    {
        Segmentable = true;
        SourceMarker = Kind == RelationshipKind.ManyToMany ? LinkMarker.Arrow : null;
        TargetMarker = Kind == RelationshipKind.Association ? null : LinkMarker.Arrow;
        Width = Kind == RelationshipKind.Dependency ? 1.75 : 2.4;
    }
}
