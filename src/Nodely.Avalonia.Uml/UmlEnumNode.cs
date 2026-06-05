using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Uml;

/// <summary>A UML enum node.</summary>
public sealed class UmlEnumNode : UmlNodeBase
{
    /// <summary>The stable serialization kind for UML enum nodes.</summary>
    public new const string ModelKindKey = "uml.enum";

    /// <summary>Creates an enum node.</summary>
    public UmlEnumNode(Point position, string name = "Enum") : base(position, name) { }

    /// <summary>Creates an enum node with the given id.</summary>
    public UmlEnumNode(string id, Point position, string name = "Enum") : base(id, position, name) { }

    /// <summary>The enum literals.</summary>
    public ObservableCollection<string> Literals { get; } = new();

    /// <inheritdoc />
    protected override string DefaultName => "Enum";

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new UmlEnumNode(Position, Name);
        CopyBaseTo(clone);
        foreach (var literal in Literals)
            clone.Literals.Add(literal);
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["LiteralsJson"] = JsonSerializer.Serialize(Literals);
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        Literals.Clear();
        foreach (var literal in DeserializeList<string>(data, "LiteralsJson"))
            Literals.Add(literal);
    }
}
