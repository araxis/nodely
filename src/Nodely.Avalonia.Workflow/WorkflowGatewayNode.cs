using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Workflow;

/// <summary>A workflow gateway node.</summary>
public sealed class WorkflowGatewayNode : WorkflowNodeBase
{
    /// <summary>The stable serialization kind for workflow gateway nodes.</summary>
    public new const string ModelKindKey = "workflow.gateway";

    /// <summary>Creates a gateway node.</summary>
    public WorkflowGatewayNode(Point position, string label = "Gateway") : base(position, label) { }

    /// <summary>Creates a gateway node with the given id.</summary>
    public WorkflowGatewayNode(string id, Point position, string label = "Gateway") : base(id, position, label) { }

    /// <summary>The gateway routing kind.</summary>
    public WorkflowGatewayKind GatewayKind { get; set; } = WorkflowGatewayKind.Exclusive;

    /// <inheritdoc />
    protected override string DefaultLabel => "Gateway";

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new WorkflowGatewayNode(Position, Label)
        {
            GatewayKind = GatewayKind,
        };
        CopyBaseTo(clone);
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["GatewayKind"] = GatewayKind.ToString();
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        GatewayKind = ParseEnum(data, "GatewayKind", WorkflowGatewayKind.Exclusive);
    }
}
