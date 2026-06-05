using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.MindMap;

/// <summary>A first-class branch topic.</summary>
public sealed class MindMapBranchNode : MindMapTopicNode
{
    /// <summary>The stable serialization kind for mind-map branch topics.</summary>
    public new const string ModelKindKey = "mindmap.branch";

    /// <summary>Creates a branch topic.</summary>
    public MindMapBranchNode(Point position, string topic = "Branch") : base(position, topic) { }

    /// <summary>Creates a branch topic with the given id.</summary>
    public MindMapBranchNode(string id, Point position, string topic = "Branch") : base(id, position, topic) { }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    protected override string DefaultTopic => "Branch";

    /// <inheritdoc />
    protected override string DefaultAccentColor => "#37A779";

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new MindMapBranchNode(Position, Topic);
        CopyBaseTo(clone);
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
