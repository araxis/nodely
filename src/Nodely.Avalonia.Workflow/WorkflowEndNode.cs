using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Workflow;

/// <summary>A workflow end node.</summary>
public sealed class WorkflowEndNode : WorkflowNodeBase
{
    /// <summary>The stable serialization kind for workflow end nodes.</summary>
    public new const string ModelKindKey = "workflow.end";

    /// <summary>Creates an end node.</summary>
    public WorkflowEndNode(Point position, string label = "End") : base(position, label) { }

    /// <summary>Creates an end node with the given id.</summary>
    public WorkflowEndNode(string id, Point position, string label = "End") : base(id, position, label) { }

    /// <inheritdoc />
    protected override string DefaultLabel => "End";

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new WorkflowEndNode(Position, Label);
        CopyBaseTo(clone);
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
