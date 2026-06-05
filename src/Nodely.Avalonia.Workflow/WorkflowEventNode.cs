using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Workflow;

/// <summary>A workflow event node.</summary>
public sealed class WorkflowEventNode : WorkflowNodeBase
{
    /// <summary>The stable serialization kind for workflow event nodes.</summary>
    public new const string ModelKindKey = "workflow.event";

    /// <summary>Creates an event node.</summary>
    public WorkflowEventNode(Point position, string label = "Event") : base(position, label) { }

    /// <summary>Creates an event node with the given id.</summary>
    public WorkflowEventNode(string id, Point position, string label = "Event") : base(id, position, label) { }

    /// <summary>The event category.</summary>
    public WorkflowEventKind EventKind { get; set; } = WorkflowEventKind.Event;

    /// <inheritdoc />
    protected override string DefaultLabel => "Event";

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new WorkflowEventNode(Position, Label)
        {
            EventKind = EventKind,
        };
        CopyBaseTo(clone);
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["EventKind"] = EventKind.ToString();
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        EventKind = ParseEnum(data, "EventKind", WorkflowEventKind.Event);
    }
}
