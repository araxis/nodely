using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.StateMachine;

/// <summary>A choice pseudo-state.</summary>
public sealed class StateMachineChoiceNode : StateMachineNodeBase
{
    /// <summary>The stable serialization kind for choice states.</summary>
    public new const string ModelKindKey = "statemachine.choice";

    /// <summary>Creates a choice node.</summary>
    public StateMachineChoiceNode(Point position, string name = "Choice") : base(position, name) { }

    /// <summary>Creates a choice node with the given id.</summary>
    public StateMachineChoiceNode(string id, Point position, string name = "Choice") : base(id, position, name) { }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    protected override string DefaultName => "Choice";

    /// <inheritdoc />
    protected override string DefaultAccentColor => "#8B68B8";

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new StateMachineChoiceNode(Position, Name);
        CopyBaseTo(clone);
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
