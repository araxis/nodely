using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Uml;

/// <summary>A UML package node.</summary>
public sealed class UmlPackageNode : UmlNodeBase
{
    /// <summary>The stable serialization kind for UML package nodes.</summary>
    public new const string ModelKindKey = "uml.package";

    /// <summary>Creates a package node.</summary>
    public UmlPackageNode(Point position, string name = "Package") : base(position, name) { }

    /// <summary>Creates a package node with the given id.</summary>
    public UmlPackageNode(string id, Point position, string name = "Package") : base(id, position, name) { }

    /// <inheritdoc />
    protected override string DefaultName => "Package";

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <inheritdoc />
    public override NodeModel Clone()
    {
        var clone = new UmlPackageNode(Position, Name);
        CopyBaseTo(clone);
        return clone;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    /// <inheritdoc />
    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
