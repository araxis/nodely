using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.MindMap;

/// <summary>A compact leaf topic.</summary>
public sealed class MindMapLeafNode : MindMapTopicNode
{
    /// <summary>The stable serialization kind for mind-map leaf topics.</summary>
    public new const string ModelKindKey = "mindmap.leaf";

    /// <summary>Creates a leaf topic.</summary>
    public MindMapLeafNode(Point position, string topic = "Leaf") : base(position, topic) { }

    /// <summary>Creates a leaf topic with the given id.</summary>
    public MindMapLeafNode(string id, Point position, string topic = "Leaf") : base(id, position, topic) { }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    protected override string DefaultTopic => "Leaf";

    /// <inheritdoc />
    protected override string DefaultAccentColor => "#D89C35";

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new MindMapLeafNode(Position, Topic);
        CopyBaseTo(clone);
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
