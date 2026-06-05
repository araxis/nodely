using System;
using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Avalonia.StateMachine;

/// <summary>A state-machine port for transition entry and exit points.</summary>
public sealed class StateMachinePortModel : PortModel
{
    /// <summary>The stable serialization kind for state-machine ports.</summary>
    public new const string ModelKindKey = "statemachine.port";

    /// <summary>Creates a state-machine port.</summary>
    public StateMachinePortModel(
        NodeModel parent,
        PortAlignment alignment = PortAlignment.Right,
        StateMachinePortRole role = StateMachinePortRole.Transition,
        string? name = null,
        Point? position = null,
        Size? size = null)
        : base(parent, alignment, position, size)
    {
        Role = role;
        Name = NormalizeOptional(name);
    }

    /// <summary>Creates a state-machine port with the given id.</summary>
    public StateMachinePortModel(
        string id,
        NodeModel parent,
        PortAlignment alignment = PortAlignment.Right,
        StateMachinePortRole role = StateMachinePortRole.Transition,
        string? name = null,
        Point? position = null,
        Size? size = null)
        : base(id, parent, alignment, position, size)
    {
        Role = role;
        Name = NormalizeOptional(name);
    }

    /// <summary>The transition role represented by this port.</summary>
    public StateMachinePortRole Role { get; set; }

    /// <summary>An optional semantic port name, such as <c>start</c> or <c>timeout</c>.</summary>
    public string? Name { get; set; }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override bool CanAttachTo(ILinkable other)
        => other is StateMachinePortModel port && port != this && port.Parent != Parent && !port.Locked;

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = new Dictionary<string, object?> { ["PortRole"] = Role.ToString() };
        if (!string.IsNullOrWhiteSpace(Name))
            extra["Name"] = Name;
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("PortRole", out var role) &&
            role is string roleText &&
            Enum.TryParse<StateMachinePortRole>(roleText, out var parsedRole))
        {
            Role = parsedRole;
        }

        if (data.TryGetValue("Name", out var name) && name is string nameText)
            Name = NormalizeOptional(nameText);
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
