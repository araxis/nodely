using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.StateMachine;

/// <summary>A regular state with optional entry and exit actions.</summary>
public sealed class StateMachineStateNode : StateMachineNodeBase
{
    /// <summary>The stable serialization kind for regular states.</summary>
    public new const string ModelKindKey = "statemachine.state";

    private string? _entryAction;
    private string? _exitAction;

    /// <summary>Creates a state node.</summary>
    public StateMachineStateNode(Point position, string name = "State") : base(position, name) { }

    /// <summary>Creates a state node with the given id.</summary>
    public StateMachineStateNode(string id, Point position, string name = "State") : base(id, position, name) { }

    /// <summary>Optional entry action.</summary>
    public string? EntryAction
    {
        get => _entryAction;
        set
        {
            _entryAction = NormalizeOptional(value);
            Refresh();
        }
    }

    /// <summary>Optional exit action.</summary>
    public string? ExitAction
    {
        get => _exitAction;
        set
        {
            _exitAction = NormalizeOptional(value);
            Refresh();
        }
    }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    protected override string DefaultName => "State";

    /// <inheritdoc />
    protected override string DefaultAccentColor => "#37A779";

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new StateMachineStateNode(Position, Name)
        {
            EntryAction = EntryAction,
            ExitAction = ExitAction,
        };
        CopyBaseTo(clone);
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        if (!string.IsNullOrWhiteSpace(EntryAction))
            extra["EntryAction"] = EntryAction;
        if (!string.IsNullOrWhiteSpace(ExitAction))
            extra["ExitAction"] = ExitAction;
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        if (data.TryGetValue("EntryAction", out var entry) && entry is string entryText)
            _entryAction = NormalizeOptional(entryText);
        if (data.TryGetValue("ExitAction", out var exit) && exit is string exitText)
            _exitAction = NormalizeOptional(exitText);
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
