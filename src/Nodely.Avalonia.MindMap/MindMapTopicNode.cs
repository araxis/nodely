using System;
using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.MindMap;

/// <summary>Base class for mind-map topic nodes.</summary>
public abstract class MindMapTopicNode : NodeModel
{
    private string _topic;
    private string? _notes;
    private string _accentColor;
    private bool _collapsed;
    private string? _iconKey;
    private MindMapTopicSide _side;

    /// <summary>Creates a mind-map topic node.</summary>
    protected MindMapTopicNode(Point position, string topic)
        : base(position)
    {
        _topic = Normalize(topic, DefaultTopic);
        _accentColor = DefaultAccentColor;
        UpdateTitle();
    }

    /// <summary>Creates a mind-map topic node with the given id.</summary>
    protected MindMapTopicNode(string id, Point position, string topic)
        : base(id, position)
    {
        _topic = Normalize(topic, DefaultTopic);
        _accentColor = DefaultAccentColor;
        UpdateTitle();
    }

    /// <summary>The topic text.</summary>
    public string Topic
    {
        get => _topic;
        set
        {
            _topic = Normalize(value, DefaultTopic);
            UpdateTitle();
            Refresh();
        }
    }

    /// <summary>Optional supporting notes.</summary>
    public string? Notes
    {
        get => _notes;
        set
        {
            _notes = NormalizeOptional(value);
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

    /// <summary>Whether this topic hides its branch descendants.</summary>
    public bool Collapsed
    {
        get => _collapsed;
        set
        {
            if (_collapsed == value)
                return;

            _collapsed = value;
            Refresh();
        }
    }

    /// <summary>Optional icon key for app-owned icon mapping.</summary>
    public string? IconKey
    {
        get => _iconKey;
        set
        {
            _iconKey = NormalizeOptional(value);
            Refresh();
        }
    }

    /// <summary>Preferred layout side.</summary>
    public MindMapTopicSide Side
    {
        get => _side;
        set
        {
            _side = value;
            Refresh();
        }
    }

    /// <summary>Default topic text.</summary>
    protected abstract string DefaultTopic { get; }

    /// <summary>Default accent color.</summary>
    protected abstract string DefaultAccentColor { get; }

    /// <summary>Copies shared mind-map fields to a clone.</summary>
    protected void CopyBaseTo(MindMapTopicNode clone)
    {
        clone.Topic = Topic;
        clone.Notes = Notes;
        clone.AccentColor = AccentColor;
        clone.Collapsed = Collapsed;
        clone.IconKey = IconKey;
        clone.Side = Side;
        clone.Size = Size;
        clone.ControlledSize = ControlledSize;
    }

    /// <summary>Writes shared extra data.</summary>
    protected Dictionary<string, object?> BuildBaseExtra() => new()
    {
        ["Topic"] = Topic,
        ["Notes"] = Notes,
        ["AccentColor"] = AccentColor,
        ["Collapsed"] = Collapsed,
        ["IconKey"] = IconKey,
        ["Side"] = Side.ToString(),
    };

    /// <summary>Reads shared extra data.</summary>
    protected void ApplyBaseExtra(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Topic", out var topic) && topic is string topicText)
            _topic = Normalize(topicText, DefaultTopic);
        if (data.TryGetValue("Notes", out var notes) && notes is string notesText)
            _notes = NormalizeOptional(notesText);
        if (data.TryGetValue("AccentColor", out var accent) && accent is string accentText)
            _accentColor = Normalize(accentText, DefaultAccentColor);
        if (data.TryGetValue("Collapsed", out var collapsed) && collapsed is bool collapsedValue)
            _collapsed = collapsedValue;
        if (data.TryGetValue("IconKey", out var icon) && icon is string iconText)
            _iconKey = NormalizeOptional(iconText);
        if (data.TryGetValue("Side", out var side) && side is string sideText && Enum.TryParse<MindMapTopicSide>(sideText, out var parsedSide))
            _side = parsedSide;

        UpdateTitle();
    }

    private void UpdateTitle() => Title = Topic;

    private static string Normalize(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
