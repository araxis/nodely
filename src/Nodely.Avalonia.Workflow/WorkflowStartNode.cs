using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Workflow;

/// <summary>A workflow start node.</summary>
public sealed class WorkflowStartNode : WorkflowNodeBase
{
    /// <summary>The stable serialization kind for workflow start nodes.</summary>
    public new const string ModelKindKey = "workflow.start";

    /// <summary>Creates a start node.</summary>
    public WorkflowStartNode(Point position, string label = "Start") : base(position, label) { }

    /// <summary>Creates a start node with the given id.</summary>
    public WorkflowStartNode(string id, Point position, string label = "Start") : base(id, position, label) { }

    /// <inheritdoc />
    protected override string DefaultLabel => "Start";

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new WorkflowStartNode(Position, Label);
        CopyBaseTo(clone);
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
