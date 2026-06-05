using System;
using System.Collections.Generic;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.PathGenerators;

namespace Nodely.Avalonia.StateMachine;

/// <summary>A state-machine transition between states, pseudo-states, ports, or anchors.</summary>
public sealed class StateMachineTransitionLink : LinkModel
{
    /// <summary>The stable serialization kind for state-machine transitions.</summary>
    public new const string ModelKindKey = "statemachine.transition";

    private StateMachineTransitionKind _kind;
    private string? _trigger;
    private string? _guard;
    private string? _action;
    private int _priority;
    private string? _accentColor;

    /// <summary>Creates a transition between two nodes.</summary>
    public StateMachineTransitionLink(NodeModel sourceNode, NodeModel targetNode, StateMachineTransitionKind kind = StateMachineTransitionKind.Normal)
        : base(sourceNode, targetNode)
    {
        Kind = kind;
    }

    /// <summary>Creates a transition between two ports.</summary>
    public StateMachineTransitionLink(PortModel sourcePort, PortModel targetPort, StateMachineTransitionKind kind = StateMachineTransitionKind.Normal)
        : base(sourcePort, targetPort)
    {
        Kind = kind;
    }

    /// <summary>Creates a transition with the given id between two anchors.</summary>
    public StateMachineTransitionLink(string id, Anchor source, Anchor target, StateMachineTransitionKind kind = StateMachineTransitionKind.Normal)
        : base(id, source, target)
    {
        Kind = kind;
    }

    /// <summary>The transition kind.</summary>
    public StateMachineTransitionKind Kind
    {
        get => _kind;
        set
        {
            _kind = value;
            ApplyKindDefaults();
            SyncLabels();
            Refresh();
        }
    }

    /// <summary>The event or signal that triggers this transition.</summary>
    public string? Trigger
    {
        get => _trigger;
        set
        {
            _trigger = NormalizeOptional(value);
            SyncLabels();
            Refresh();
        }
    }

    /// <summary>Optional guard expression.</summary>
    public string? Guard
    {
        get => _guard;
        set
        {
            _guard = NormalizeOptional(value);
            SyncLabels();
            Refresh();
        }
    }

    /// <summary>Optional action run by this transition.</summary>
    public string? Action
    {
        get => _action;
        set
        {
            _action = NormalizeOptional(value);
            SyncLabels();
            Refresh();
        }
    }

    /// <summary>Optional priority used by host apps when several transitions can fire.</summary>
    public int Priority
    {
        get => _priority;
        set
        {
            _priority = Math.Max(0, value);
            Refresh();
        }
    }

    /// <summary>Optional accent color for this transition, stored as a hex string such as <c>#C45552</c>.</summary>
    public string? AccentColor
    {
        get => _accentColor;
        set
        {
            _accentColor = NormalizeOptional(value);
            Refresh();
        }
    }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = new Dictionary<string, object?> { ["TransitionKind"] = Kind.ToString() };
        if (!string.IsNullOrWhiteSpace(Trigger))
            extra["Trigger"] = Trigger;
        if (!string.IsNullOrWhiteSpace(Guard))
            extra["Guard"] = Guard;
        if (!string.IsNullOrWhiteSpace(Action))
            extra["Action"] = Action;
        if (Priority > 0)
            extra["Priority"] = Priority;
        if (!string.IsNullOrWhiteSpace(AccentColor))
            extra["AccentColor"] = AccentColor;
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("TransitionKind", out var kind) &&
            kind is string kindText &&
            Enum.TryParse<StateMachineTransitionKind>(kindText, out var parsedKind))
        {
            Kind = parsedKind;
        }

        if (data.TryGetValue("Trigger", out var trigger) && trigger is string triggerText)
            Trigger = triggerText;
        if (data.TryGetValue("Guard", out var guard) && guard is string guardText)
            Guard = guardText;
        if (data.TryGetValue("Action", out var action) && action is string actionText)
            Action = actionText;
        if (data.TryGetValue("Priority", out var priority) && priority is not null)
            Priority = ParseInt(priority, fallback: 0);
        if (data.TryGetValue("AccentColor", out var accent) && accent is string accentText)
            AccentColor = accentText;

        SyncLabels();
    }

    private void ApplyKindDefaults()
    {
        Segmentable = Kind != StateMachineTransitionKind.Self;
        TargetMarker = LinkMarker.Arrow;
        PathGenerator = new SmoothPathGenerator(Kind == StateMachineTransitionKind.Self ? 120 : 80);
        Width = Kind switch
        {
            StateMachineTransitionKind.Error => 2.8,
            StateMachineTransitionKind.Self => 2.5,
            StateMachineTransitionKind.Timeout => 2.4,
            _ => 2.2,
        };
    }

    private void SyncLabels()
    {
        Labels.Clear();
        var text = FormatLabel();
        if (!string.IsNullOrWhiteSpace(text) && Kind != StateMachineTransitionKind.Self)
            AddLabel(text, 0.5, new Point(0, Kind == StateMachineTransitionKind.Choice ? 18 : -18));
    }

    internal string FormatLabel()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(Trigger))
            parts.Add(Trigger!);
        if (!string.IsNullOrWhiteSpace(Guard))
            parts.Add("[" + Guard + "]");
        if (!string.IsNullOrWhiteSpace(Action))
            parts.Add("/ " + Action);
        return string.Join(" ", parts);
    }

    private static int ParseInt(object value, int fallback)
    {
        return value switch
        {
            int intValue => intValue,
            long longValue => longValue > int.MaxValue ? int.MaxValue : (int)Math.Max(0, longValue),
            double doubleValue => (int)Math.Max(0, doubleValue),
            string text when int.TryParse(text, out var parsed) => Math.Max(0, parsed),
            _ => fallback,
        };
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
