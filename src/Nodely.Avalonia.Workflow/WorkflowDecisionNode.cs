using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Workflow;

/// <summary>A workflow decision node.</summary>
public sealed class WorkflowDecisionNode : WorkflowNodeBase
{
    /// <summary>The stable serialization kind for workflow decision nodes.</summary>
    public new const string ModelKindKey = "workflow.decision";

    /// <summary>Creates a decision node.</summary>
    public WorkflowDecisionNode(Point position, string label = "Decision") : base(position, label) { }

    /// <summary>Creates a decision node with the given id.</summary>
    public WorkflowDecisionNode(string id, Point position, string label = "Decision") : base(id, position, label) { }

    /// <summary>The condition associated with the decision.</summary>
    public string Condition { get; set; } = "";

    /// <inheritdoc />
    protected override string DefaultLabel => "Decision";

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new WorkflowDecisionNode(Position, Label)
        {
            Condition = Condition,
        };
        CopyBaseTo(clone);
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["Condition"] = Condition;
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        if (data.TryGetValue("Condition", out var condition) && condition is string conditionText)
            Condition = conditionText;
    }
}
