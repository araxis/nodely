using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Uml;

/// <summary>A UML class node.</summary>
public sealed class UmlClassNode : UmlNodeBase
{
    /// <summary>The stable serialization kind for UML class nodes.</summary>
    public new const string ModelKindKey = "uml.class";

    /// <summary>Creates a class node.</summary>
    public UmlClassNode(Point position, string name = "Class") : base(position, name) { }

    /// <summary>Creates a class node with the given id.</summary>
    public UmlClassNode(string id, Point position, string name = "Class") : base(id, position, name) { }

    /// <summary>The class members.</summary>
    public ObservableCollection<UmlMember> Members { get; } = new();

    /// <summary>The class operations.</summary>
    public ObservableCollection<UmlOperation> Operations { get; } = new();

    /// <summary>Whether the class is abstract.</summary>
    public bool IsAbstract { get; set; }

    /// <summary>Whether the class is static.</summary>
    public bool IsStatic { get; set; }

    /// <inheritdoc />
    protected override string DefaultName => "Class";

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new UmlClassNode(Position, Name)
        {
            IsAbstract = IsAbstract,
            IsStatic = IsStatic,
        };
        CopyBaseTo(clone);
        foreach (var member in Members)
            clone.Members.Add(member.Clone());
        foreach (var operation in Operations)
            clone.Operations.Add(operation.Clone());
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["IsAbstract"] = IsAbstract;
        extra["IsStatic"] = IsStatic;
        extra["MembersJson"] = JsonSerializer.Serialize(Members);
        extra["OperationsJson"] = JsonSerializer.Serialize(Operations);
        return extra;
    }

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        IsAbstract = data.TryGetValue("IsAbstract", out var isAbstract) && isAbstract is bool abstractValue && abstractValue;
        IsStatic = data.TryGetValue("IsStatic", out var isStatic) && isStatic is bool staticValue && staticValue;

        Members.Clear();
        foreach (var member in DeserializeList<UmlMember>(data, "MembersJson"))
            Members.Add(member);

        Operations.Clear();
        foreach (var operation in DeserializeList<UmlOperation>(data, "OperationsJson"))
            Operations.Add(operation);
    }
}
