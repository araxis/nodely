using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.StateMachine;

/// <summary>The initial pseudo-state.</summary>
public sealed class StateMachineInitialNode : StateMachineNodeBase
{
    /// <summary>The stable serialization kind for initial states.</summary>
    public new const string ModelKindKey = "statemachine.initial";

    /// <summary>Creates an initial node.</summary>
    public StateMachineInitialNode(Point position, string name = "Initial") : base(position, name) { }

    /// <summary>Creates an initial node with the given id.</summary>
    public StateMachineInitialNode(string id, Point position, string name = "Initial") : base(id, position, name) { }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    protected override string DefaultName => "Initial";

    /// <inheritdoc />
    protected override string DefaultAccentColor => "#4D9EFF";

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new StateMachineInitialNode(Position, Name);
        CopyBaseTo(clone);
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
