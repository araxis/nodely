using System;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Serialization;

namespace Nodely.Avalonia.Workflow;

/// <summary>Serialization registration helpers for the workflow node pack.</summary>
public static class WorkflowSerializationRegistryExtensions
{
    /// <summary>Registers workflow node and link factories.</summary>
    public static DiagramSerializationRegistry UseWorkflowNodes(this DiagramSerializationRegistry registry)
    {
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        registry.RegisterNode(WorkflowStartNode.ModelKindKey,
            snapshot => new WorkflowStartNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Start"));
        registry.RegisterNode(WorkflowEndNode.ModelKindKey,
            snapshot => new WorkflowEndNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "End"));
        registry.RegisterNode(WorkflowTaskNode.ModelKindKey,
            snapshot => new WorkflowTaskNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Task"));
        registry.RegisterNode(WorkflowDecisionNode.ModelKindKey,
            snapshot => new WorkflowDecisionNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Decision"));
        registry.RegisterNode(WorkflowGatewayNode.ModelKindKey,
            snapshot => new WorkflowGatewayNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Gateway"));
        registry.RegisterNode(WorkflowEventNode.ModelKindKey,
            snapshot => new WorkflowEventNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Event"));
        registry.RegisterNode(WorkflowNoteNode.ModelKindKey,
            snapshot => new WorkflowNoteNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Note"));

        registry.RegisterLink(WorkflowLink.ModelKindKey, CreateWorkflowLink);

        return registry;
    }

    private static WorkflowLink CreateWorkflowLink(LinkSnapshot snapshot, Anchor source, Anchor target)
        => new(snapshot.Id, source, target);
}
