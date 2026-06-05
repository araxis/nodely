using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.MindMap;

/// <summary>The central mind-map topic.</summary>
public sealed class MindMapRootNode : MindMapTopicNode
{
    /// <summary>The stable serialization kind for mind-map root topics.</summary>
    public new const string ModelKindKey = "mindmap.root";

    /// <summary>Creates a root topic.</summary>
    public MindMapRootNode(Point position, string topic = "Central topic") : base(position, topic) { }

    /// <summary>Creates a root topic with the given id.</summary>
    public MindMapRootNode(string id, Point position, string topic = "Central topic") : base(id, position, topic) { }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    protected override string DefaultTopic => "Central topic";

    /// <inheritdoc />
    protected override string DefaultAccentColor => "#4D9EFF";

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new MindMapRootNode(Position, Topic);
        CopyBaseTo(clone);
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
