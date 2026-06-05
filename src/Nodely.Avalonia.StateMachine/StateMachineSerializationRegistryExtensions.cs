using System;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;

namespace Nodely.Avalonia.StateMachine;

/// <summary>Serialization registration helpers for the state-machine node pack.</summary>
public static class StateMachineSerializationRegistryExtensions
{
    /// <summary>Registers state-machine node, port, and link factories.</summary>
    public static DiagramSerializationRegistry UseStateMachineNodes(this DiagramSerializationRegistry registry)
    {
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        registry.RegisterNode(StateMachineInitialNode.ModelKindKey,
            snapshot => new StateMachineInitialNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Initial"));
        registry.RegisterNode(StateMachineStateNode.ModelKindKey,
            snapshot => new StateMachineStateNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "State"));
        registry.RegisterNode(StateMachineFinalNode.ModelKindKey,
            snapshot => new StateMachineFinalNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Final"));
        registry.RegisterNode(StateMachineChoiceNode.ModelKindKey,
            snapshot => new StateMachineChoiceNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Choice"));
        registry.RegisterNode(StateMachineNoteNode.ModelKindKey,
            snapshot => new StateMachineNoteNode(snapshot.Id, new Point(snapshot.X, snapshot.Y), snapshot.Title ?? "Note"));

        registry.RegisterPort(StateMachinePortModel.ModelKindKey, (snapshot, parent) =>
            new StateMachinePortModel(snapshot.Id, parent, ParseAlignment(snapshot.Alignment), position: parent.Position));

        registry.RegisterLink(StateMachineTransitionLink.ModelKindKey, CreateTransition);

        return registry;
    }

    private static StateMachineTransitionLink CreateTransition(LinkSnapshot snapshot, Anchor source, Anchor target)
        => new(snapshot.Id, source, target);

    private static PortAlignment ParseAlignment(string alignment)
        => Enum.TryParse<PortAlignment>(alignment, out var parsed) ? parsed : PortAlignment.Right;
}
