using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.StateMachine;

/// <summary>A state-machine annotation note.</summary>
public sealed class StateMachineNoteNode : NodeModel
{
    /// <summary>The stable serialization kind for state-machine notes.</summary>
    public new const string ModelKindKey = "statemachine.note";

    private string _text;
    private string _accentColor = "#D89C35";

    /// <summary>Creates a note node.</summary>
    public StateMachineNoteNode(Point position, string text = "Note") : base(position)
    {
        _text = Normalize(text, "Note");
        UpdateTitle();
    }

    /// <summary>Creates a note node with the given id.</summary>
    public StateMachineNoteNode(string id, Point position, string text = "Note") : base(id, position)
    {
        _text = Normalize(text, "Note");
        UpdateTitle();
    }

    /// <summary>The note text.</summary>
    public string Text
    {
        get => _text;
        set
        {
            _text = Normalize(value, "Note");
            UpdateTitle();
            Refresh();
        }
    }

    /// <summary>Accent color stored as a hex string such as <c>#D89C35</c>.</summary>
    public string AccentColor
    {
        get => _accentColor;
        set
        {
            _accentColor = Normalize(value, "#D89C35");
            Refresh();
        }
    }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override NodeModel Clone() => new StateMachineNoteNode(Position, Text)
    {
        AccentColor = AccentColor,
        Size = Size,
        ControlledSize = ControlledSize,
    };

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData() => new Dictionary<string, object?>
    {
        ["Text"] = Text,
        ["AccentColor"] = AccentColor,
    };

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Text", out var text) && text is string textValue)
            _text = Normalize(textValue, "Note");
        if (data.TryGetValue("AccentColor", out var accent) && accent is string accentText)
            _accentColor = Normalize(accentText, "#D89C35");

        UpdateTitle();
    }

    private void UpdateTitle() => Title = Text;

    private static string Normalize(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
