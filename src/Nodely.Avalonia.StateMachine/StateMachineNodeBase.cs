using System;
using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.StateMachine;

/// <summary>Base class for named state-machine nodes.</summary>
public abstract class StateMachineNodeBase : NodeModel
{
    private string _name;
    private string? _description;
    private string _accentColor;

    /// <summary>Creates a state-machine node.</summary>
    protected StateMachineNodeBase(Point position, string name)
        : base(position)
    {
        _name = Normalize(name, DefaultName);
        _accentColor = DefaultAccentColor;
        UpdateTitle();
    }

    /// <summary>Creates a state-machine node with the given id.</summary>
    protected StateMachineNodeBase(string id, Point position, string name)
        : base(id, position)
    {
        _name = Normalize(name, DefaultName);
        _accentColor = DefaultAccentColor;
        UpdateTitle();
    }

    /// <summary>The state-machine element name.</summary>
    public string Name
    {
        get => _name;
        set
        {
            _name = Normalize(value, DefaultName);
            UpdateTitle();
            Refresh();
        }
    }

    /// <summary>Optional supporting description.</summary>
    public string? Description
    {
        get => _description;
        set
        {
            _description = NormalizeOptional(value);
            Refresh();
        }
    }

    /// <summary>Accent color stored as a hex string such as <c>#4D9EFF</c>.</summary>
    public string AccentColor
    {
        get => _accentColor;
        set
        {
            _accentColor = Normalize(value, DefaultAccentColor);
            Refresh();
        }
    }

    /// <summary>The default element name.</summary>
    protected abstract string DefaultName { get; }

    /// <summary>The default accent color.</summary>
    protected abstract string DefaultAccentColor { get; }

    /// <summary>Copies shared state-machine fields to a clone.</summary>
    protected void CopyBaseTo(StateMachineNodeBase clone)
    {
        clone.Name = Name;
        clone.Description = Description;
        clone.AccentColor = AccentColor;
        clone.Size = Size;
        clone.ControlledSize = ControlledSize;
    }

    /// <summary>Writes shared extra data.</summary>
    protected Dictionary<string, object?> BuildBaseExtra() => new()
    {
        ["Name"] = Name,
        ["Description"] = Description,
        ["AccentColor"] = AccentColor,
    };

    /// <summary>Reads shared extra data.</summary>
    protected void ApplyBaseExtra(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Name", out var name) && name is string nameText)
            _name = Normalize(nameText, DefaultName);
        if (data.TryGetValue("Description", out var description) && description is string descriptionText)
            _description = NormalizeOptional(descriptionText);
        if (data.TryGetValue("AccentColor", out var accent) && accent is string accentText)
            _accentColor = Normalize(accentText, DefaultAccentColor);

        UpdateTitle();
    }

    /// <summary>Parses an enum value from extra data.</summary>
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

    private void UpdateTitle() => Title = Name;

    private static string Normalize(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
