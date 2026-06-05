using System;
using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Network;

/// <summary>Base class for network topology nodes.</summary>
public abstract class NetworkNodeBase : NodeModel
{
    private string _name;
    private string? _address;
    private NetworkStatus _status = NetworkStatus.Online;
    private string _role;
    private string? _notes;
    private string _accentColor;
    private string? _iconKey;
    private string? _zone;

    /// <summary>Creates a network node.</summary>
    protected NetworkNodeBase(Point position, string name)
        : base(position)
    {
        _name = Normalize(name, DefaultName);
        _role = DefaultRole;
        _accentColor = DefaultAccentColor;
        _iconKey = DefaultIconKey;
        UpdateTitle();
    }

    /// <summary>Creates a network node with the given id.</summary>
    protected NetworkNodeBase(string id, Point position, string name)
        : base(id, position)
    {
        _name = Normalize(name, DefaultName);
        _role = DefaultRole;
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

    /// <summary>Optional address, host name, or CIDR block.</summary>
    public string? Address
    {
        get => _address;
        set
        {
            _address = NormalizeOptional(value);
            Refresh();
        }
    }

    /// <summary>Operational status.</summary>
    public NetworkStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            Refresh();
        }
    }

    /// <summary>Role text used by host apps and renderers.</summary>
    public string Role
    {
        get => _role;
        set
        {
            _role = Normalize(value, DefaultRole);
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

    /// <summary>Optional environment or topology zone.</summary>
    public string? Zone
    {
        get => _zone;
        set
        {
            _zone = NormalizeOptional(value);
            Refresh();
        }
    }

    /// <summary>Default node name.</summary>
    protected abstract string DefaultName { get; }

    /// <summary>Default role text.</summary>
    protected abstract string DefaultRole { get; }

    /// <summary>Default accent color.</summary>
    protected abstract string DefaultAccentColor { get; }

    /// <summary>Default icon key.</summary>
    protected abstract string DefaultIconKey { get; }

    /// <summary>Copies shared network fields to a clone.</summary>
    protected void CopyBaseTo(NetworkNodeBase clone)
    {
        clone.Name = Name;
        clone.Address = Address;
        clone.Status = Status;
        clone.Role = Role;
        clone.Notes = Notes;
        clone.AccentColor = AccentColor;
        clone.IconKey = IconKey;
        clone.Zone = Zone;
        clone.Size = Size;
        clone.ControlledSize = ControlledSize;
    }

    /// <summary>Writes shared extra data.</summary>
    protected Dictionary<string, object?> BuildBaseExtra() => new()
    {
        ["Name"] = Name,
        ["Address"] = Address,
        ["Status"] = Status.ToString(),
        ["Role"] = Role,
        ["Notes"] = Notes,
        ["AccentColor"] = AccentColor,
        ["IconKey"] = IconKey,
        ["Zone"] = Zone,
    };

    /// <summary>Reads shared extra data.</summary>
    protected void ApplyBaseExtra(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Name", out var name) && name is string nameText)
            _name = Normalize(nameText, DefaultName);
        if (data.TryGetValue("Address", out var address) && address is string addressText)
            _address = NormalizeOptional(addressText);
        if (data.TryGetValue("Status", out var status) && status is string statusText && Enum.TryParse<NetworkStatus>(statusText, out var parsedStatus))
            _status = parsedStatus;
        if (data.TryGetValue("Role", out var role) && role is string roleText)
            _role = Normalize(roleText, DefaultRole);
        if (data.TryGetValue("Notes", out var notes) && notes is string notesText)
            _notes = NormalizeOptional(notesText);
        if (data.TryGetValue("AccentColor", out var accent) && accent is string accentText)
            _accentColor = Normalize(accentText, DefaultAccentColor);
        if (data.TryGetValue("IconKey", out var icon) && icon is string iconText)
            _iconKey = NormalizeOptional(iconText);
        if (data.TryGetValue("Zone", out var zone) && zone is string zoneText)
            _zone = NormalizeOptional(zoneText);

        UpdateTitle();
    }

    private void UpdateTitle() => Title = Name;

    protected static string Normalize(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    protected static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
