using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Uml;

/// <summary>A UML interface node.</summary>
public sealed class UmlInterfaceNode : UmlNodeBase
{
    /// <summary>The stable serialization kind for UML interface nodes.</summary>
    public new const string ModelKindKey = "uml.interface";

    /// <summary>Creates an interface node.</summary>
    public UmlInterfaceNode(Point position, string name = "Interface") : base(position, name) { }

    /// <summary>Creates an interface node with the given id.</summary>
    public UmlInterfaceNode(string id, Point position, string name = "Interface") : base(id, position, name) { }

    /// <summary>The interface operations.</summary>
    public ObservableCollection<UmlOperation> Operations { get; } = new();

    /// <inheritdoc />
    protected override string DefaultName => "Interface";

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new UmlInterfaceNode(Position, Name);
        CopyBaseTo(clone);
        foreach (var operation in Operations)
            clone.Operations.Add(operation.Clone());
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["OperationsJson"] = JsonSerializer.Serialize(Operations);
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        Operations.Clear();
        foreach (var operation in DeserializeList<UmlOperation>(data, "OperationsJson"))
            Operations.Add(operation);
    }
}
