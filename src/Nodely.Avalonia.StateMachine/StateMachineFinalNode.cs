using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.StateMachine;

/// <summary>The final pseudo-state.</summary>
public sealed class StateMachineFinalNode : StateMachineNodeBase
{
    /// <summary>The stable serialization kind for final states.</summary>
    public new const string ModelKindKey = "statemachine.final";

    /// <summary>Creates a final node.</summary>
    public StateMachineFinalNode(Point position, string name = "Final") : base(position, name) { }

    /// <summary>Creates a final node with the given id.</summary>
    public StateMachineFinalNode(string id, Point position, string name = "Final") : base(id, position, name) { }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    protected override string DefaultName => "Final";

    /// <inheritdoc />
    protected override string DefaultAccentColor => "#C45552";

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new StateMachineFinalNode(Position, Name);
        CopyBaseTo(clone);
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
