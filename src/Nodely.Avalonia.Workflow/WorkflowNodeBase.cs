using System;
using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Workflow;

/// <summary>Base class for named workflow nodes.</summary>
public abstract class WorkflowNodeBase : NodeModel
{
    private string _label;
    private string _notes = "";

    /// <summary>Creates a workflow node.</summary>
    protected WorkflowNodeBase(Point position, string label)
        : base(position)
    {
        _label = Normalize(label, DefaultLabel);
        UpdateTitle();
    }

    /// <summary>Creates a workflow node with the given id.</summary>
    protected WorkflowNodeBase(string id, Point position, string label)
        : base(id, position)
    {
        _label = Normalize(label, DefaultLabel);
        UpdateTitle();
    }

    /// <summary>The text shown as the main node label.</summary>
    public string Label
    {
        get => _label;
        set
        {
            _label = Normalize(value, DefaultLabel);
            UpdateTitle();
            Refresh();
        }
    }

    /// <summary>Optional notes for app-specific workflow metadata.</summary>
    public string Notes
    {
        get => _notes;
        set
        {
            _notes = value?.Trim() ?? "";
            Refresh();
        }
    }

    /// <summary>The default node label.</summary>
    protected abstract string DefaultLabel { get; }

    /// <summary>Copies shared workflow node fields to a clone.</summary>
    protected void CopyBaseTo(WorkflowNodeBase clone)
    {
        clone.Label = Label;
        clone.Notes = Notes;
        clone.Size = Size;
        clone.ControlledSize = ControlledSize;
    }

    /// <summary>Writes shared extra data.</summary>
    protected Dictionary<string, object?> BuildBaseExtra() => new()
    {
        ["Label"] = Label,
        ["Notes"] = Notes,
    };

    /// <summary>Reads shared extra data.</summary>
    protected void ApplyBaseExtra(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Label", out var label) && label is string labelText)
            _label = Normalize(labelText, DefaultLabel);

        if (data.TryGetValue("Notes", out var notes) && notes is string notesText)
            _notes = notesText.Trim();

        UpdateTitle();
    }

    /// <summary>Parses an enum value from extra data, accepting strings and numeric serializer output.</summary>
    protected static TEnum ParseEnum<TEnum>(IReadOnlyDictionary<string, object?> data, string key, TEnum fallback)
        where TEnum : struct, Enum
    {
        if (!data.TryGetValue(key, out var value) || value is null)
            return fallback;

        if (value is string text && Enum.TryParse<TEnum>(text, out var parsedText))
            return parsedText;

        if (value is long longValue && Enum.IsDefined(typeof(TEnum), (int)longValue))
            return (TEnum)Enum.ToObject(typeof(TEnum), (int)longValue);

        if (value is double doubleValue && Enum.IsDefined(typeof(TEnum), (int)doubleValue))
            return (TEnum)Enum.ToObject(typeof(TEnum), (int)doubleValue);

        return fallback;
    }

    private void UpdateTitle() => Title = Label;

    private static string Normalize(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
