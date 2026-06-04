using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Uml;

/// <summary>Base class for named UML nodes.</summary>
public abstract class UmlNodeBase : NodeModel
{
    private string _name;

    /// <summary>Creates a named UML node.</summary>
    protected UmlNodeBase(Point position, string name)
        : base(position)
    {
        _name = Normalize(name, DefaultName);
        UpdateTitle();
    }

    /// <summary>Creates a named UML node with the given id.</summary>
    protected UmlNodeBase(string id, Point position, string name)
        : base(id, position)
    {
        _name = Normalize(name, DefaultName);
        UpdateTitle();
    }

    /// <summary>The UML element name.</summary>
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

    /// <summary>The UML stereotypes shown above the element name.</summary>
    public ObservableCollection<string> Stereotypes { get; } = new();

    /// <summary>The default element name.</summary>
    protected abstract string DefaultName { get; }

    /// <summary>Copies shared UML node fields to a clone.</summary>
    protected void CopyBaseTo(UmlNodeBase clone)
    {
        clone.Name = Name;
        clone.Size = Size;
        clone.ControlledSize = ControlledSize;
        clone.Stereotypes.Clear();
        foreach (var stereotype in Stereotypes)
            clone.Stereotypes.Add(stereotype);
    }

    /// <summary>Writes shared extra data.</summary>
    protected Dictionary<string, object?> BuildBaseExtra() => new()
    {
        ["Name"] = Name,
        ["StereotypesJson"] = JsonSerializer.Serialize(Stereotypes),
    };

    /// <summary>Reads shared extra data.</summary>
    protected void ApplyBaseExtra(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Name", out var name) && name is string nameText)
            _name = Normalize(nameText, DefaultName);

        Stereotypes.Clear();
        foreach (var stereotype in DeserializeList<string>(data, "StereotypesJson"))
            if (!string.IsNullOrWhiteSpace(stereotype))
                Stereotypes.Add(stereotype);

        UpdateTitle();
    }

    internal static IReadOnlyList<T> DeserializeList<T>(IReadOnlyDictionary<string, object?> data, string key)
    {
        if (!data.TryGetValue(key, out var value) || value is not string json || string.IsNullOrWhiteSpace(json))
            return new List<T>();

        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }

    private void UpdateTitle() => Title = Name;

    private static string Normalize(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
