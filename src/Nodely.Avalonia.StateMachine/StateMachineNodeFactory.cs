using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;

namespace Nodely.Avalonia.StateMachine;

/// <summary>Creates state-machine node types from serialization snapshots.</summary>
public static class StateMachineNodeFactory
{
    /// <summary>Creates a registry that restores state-machine nodes, ports, and transitions.</summary>
    public static DiagramSerializationRegistry CreateRegistry() => new DiagramSerializationRegistry().UseStateMachineNodes();

    /// <summary>Creates a node for the snapshot kind, falling back to <see cref="NodeModel"/>.</summary>
    public static NodeModel Create(NodeSnapshot snapshot)
        => TryCreate(snapshot, out var node)
            ? node
            : new NodeModel(snapshot.Id, new Point(snapshot.X, snapshot.Y)) { Title = snapshot.Title };

    /// <summary>Attempts to create a state-machine node for the snapshot kind.</summary>
    public static bool TryCreate(NodeSnapshot snapshot, out NodeModel node)
    {
        var position = new Point(snapshot.X, snapshot.Y);
        node = snapshot.Kind switch
        {
            StateMachineInitialNode.ModelKindKey or nameof(StateMachineInitialNode) => new StateMachineInitialNode(snapshot.Id, position, snapshot.Title ?? "Initial"),
            StateMachineStateNode.ModelKindKey or nameof(StateMachineStateNode) => new StateMachineStateNode(snapshot.Id, position, snapshot.Title ?? "State"),
            StateMachineFinalNode.ModelKindKey or nameof(StateMachineFinalNode) => new StateMachineFinalNode(snapshot.Id, position, snapshot.Title ?? "Final"),
            StateMachineChoiceNode.ModelKindKey or nameof(StateMachineChoiceNode) => new StateMachineChoiceNode(snapshot.Id, position, snapshot.Title ?? "Choice"),
            StateMachineNoteNode.ModelKindKey or nameof(StateMachineNoteNode) => new StateMachineNoteNode(snapshot.Id, position, snapshot.Title ?? "Note"),
            _ => null!,
        };

        return node != null;
    }
}
