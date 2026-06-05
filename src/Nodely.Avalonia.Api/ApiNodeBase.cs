using System;
using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Api;

/// <summary>Base class for API design nodes.</summary>
public abstract class ApiNodeBase : NodeModel
{
    private string _name;
    private string? _version;
    private ApiEndpointStatus _status = ApiEndpointStatus.Stable;
    private string? _summary;
    private string _accentColor;
    private string? _iconKey;

    /// <summary>Creates an API node.</summary>
    protected ApiNodeBase(Point position, string name)
        : base(position)
    {
        _name = Normalize(name, DefaultName);
        _accentColor = DefaultAccentColor;
        _iconKey = DefaultIconKey;
        UpdateTitle();
    }

    /// <summary>Creates an API node with the given id.</summary>
    protected ApiNodeBase(string id, Point position, string name)
        : base(id, position)
    {
        _name = Normalize(name, DefaultName);
        _accentColor = DefaultAccentColor;
        _iconKey = DefaultIconKey;
        UpdateTitle();
    }

    /// <summary>The display name.</summary>
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

    /// <summary>Optional API version text.</summary>
    public string? Version
    {
        get => _version;
        set
        {
            _version = NormalizeOptional(value);
            Refresh();
        }
    }

    /// <summary>Lifecycle status.</summary>
    public ApiEndpointStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            Refresh();
        }
    }

    /// <summary>Short supporting text.</summary>
    public string? Summary
    {
        get => _summary;
        set
        {
            _summary = NormalizeOptional(value);
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

    /// <summary>Optional icon key for host-owned icon mapping.</summary>
    public string? IconKey
    {
        get => _iconKey;
        set
        {
            _iconKey = NormalizeOptional(value);
            Refresh();
        }
    }

    /// <summary>Default node name.</summary>
    protected abstract string DefaultName { get; }

    /// <summary>Default accent color.</summary>
    protected abstract string DefaultAccentColor { get; }

    /// <summary>Default icon key.</summary>
    protected abstract string DefaultIconKey { get; }

    /// <summary>Copies shared API fields to a clone.</summary>
    protected void CopyBaseTo(ApiNodeBase clone)
    {
        clone.Name = Name;
        clone.Version = Version;
        clone.Status = Status;
        clone.Summary = Summary;
        clone.AccentColor = AccentColor;
        clone.IconKey = IconKey;
        clone.Size = Size;
        clone.ControlledSize = ControlledSize;
    }

    /// <summary>Writes shared extra data.</summary>
    protected Dictionary<string, object?> BuildBaseExtra() => new()
    {
        ["Name"] = Name,
        ["Version"] = Version,
        ["Status"] = Status.ToString(),
        ["Summary"] = Summary,
        ["AccentColor"] = AccentColor,
        ["IconKey"] = IconKey,
    };

    /// <summary>Reads shared extra data.</summary>
    protected void ApplyBaseExtra(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Name", out var name) && name is string nameText)
            _name = Normalize(nameText, DefaultName);
        if (data.TryGetValue("Version", out var version) && version is string versionText)
            _version = NormalizeOptional(versionText);
        if (data.TryGetValue("Status", out var status) && status is string statusText && Enum.TryParse<ApiEndpointStatus>(statusText, out var parsedStatus))
            _status = parsedStatus;
        if (data.TryGetValue("Summary", out var summary) && summary is string summaryText)
            _summary = NormalizeOptional(summaryText);
        if (data.TryGetValue("AccentColor", out var accent) && accent is string accentText)
            _accentColor = Normalize(accentText, DefaultAccentColor);
        if (data.TryGetValue("IconKey", out var icon) && icon is string iconText)
            _iconKey = NormalizeOptional(iconText);

        UpdateTitle();
    }

    private void UpdateTitle() => Title = Name;

    protected static string Normalize(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    protected static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
