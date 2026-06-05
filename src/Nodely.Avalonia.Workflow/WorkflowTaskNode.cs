using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Workflow;

/// <summary>A workflow task node.</summary>
public sealed class WorkflowTaskNode : WorkflowNodeBase
{
    /// <summary>The stable serialization kind for workflow task nodes.</summary>
    public new const string ModelKindKey = "workflow.task";

    /// <summary>Creates a task node.</summary>
    public WorkflowTaskNode(Point position, string label = "Task") : base(position, label) { }

    /// <summary>Creates a task node with the given id.</summary>
    public WorkflowTaskNode(string id, Point position, string label = "Task") : base(id, position, label) { }

    /// <summary>The task category.</summary>
    public WorkflowTaskType TaskType { get; set; } = WorkflowTaskType.Task;

    /// <summary>The task status.</summary>
    public WorkflowTaskStatus Status { get; set; } = WorkflowTaskStatus.Draft;

    /// <inheritdoc />
    protected override string DefaultLabel => "Task";

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new WorkflowTaskNode(Position, Label)
        {
            TaskType = TaskType,
            Status = Status,
        };
        CopyBaseTo(clone);
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["TaskType"] = TaskType.ToString();
        extra["Status"] = Status.ToString();
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        TaskType = ParseEnum(data, "TaskType", WorkflowTaskType.Task);
        Status = ParseEnum(data, "Status", WorkflowTaskStatus.Draft);
    }
}
