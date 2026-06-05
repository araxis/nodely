using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;

namespace Nodely.Avalonia.Workflow;

/// <summary>Creates workflow node types from serialization snapshots.</summary>
public static class WorkflowNodeFactory
{
    /// <summary>Creates a registry that restores workflow nodes and links.</summary>
    public static DiagramSerializationRegistry CreateRegistry() => new DiagramSerializationRegistry().UseWorkflowNodes();

    /// <summary>Creates a node for the snapshot kind, falling back to <see cref="NodeModel"/>.</summary>
    public static NodeModel Create(NodeSnapshot snapshot)
        => TryCreate(snapshot, out var node)
            ? node
            : new NodeModel(snapshot.Id, new Point(snapshot.X, snapshot.Y)) { Title = snapshot.Title };

    /// <summary>Attempts to create a workflow node for the snapshot kind.</summary>
    public static bool TryCreate(NodeSnapshot snapshot, out NodeModel node)
    {
        var position = new Point(snapshot.X, snapshot.Y);
        node = snapshot.Kind switch
        {
            WorkflowStartNode.ModelKindKey or nameof(WorkflowStartNode) => new WorkflowStartNode(snapshot.Id, position, snapshot.Title ?? "Start"),
            WorkflowEndNode.ModelKindKey or nameof(WorkflowEndNode) => new WorkflowEndNode(snapshot.Id, position, snapshot.Title ?? "End"),
            WorkflowTaskNode.ModelKindKey or nameof(WorkflowTaskNode) => new WorkflowTaskNode(snapshot.Id, position, snapshot.Title ?? "Task"),
            WorkflowDecisionNode.ModelKindKey or nameof(WorkflowDecisionNode) => new WorkflowDecisionNode(snapshot.Id, position, snapshot.Title ?? "Decision"),
            WorkflowGatewayNode.ModelKindKey or nameof(WorkflowGatewayNode) => new WorkflowGatewayNode(snapshot.Id, position, snapshot.Title ?? "Gateway"),
            WorkflowEventNode.ModelKindKey or nameof(WorkflowEventNode) => new WorkflowEventNode(snapshot.Id, position, snapshot.Title ?? "Event"),
            WorkflowNoteNode.ModelKindKey or nameof(WorkflowNoteNode) => new WorkflowNoteNode(snapshot.Id, position, snapshot.Title ?? "Note"),
            _ => null!,
        };

        return node != null;
    }
}
