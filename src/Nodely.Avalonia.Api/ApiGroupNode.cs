using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Api;

/// <summary>A visual API group or bounded context node.</summary>
public sealed class ApiGroupNode : ApiNodeBase
{
    public new const string ModelKindKey = "api.group";

    public ApiGroupNode(Point position, string name = "API group") : base(position, name) { }

    public ApiGroupNode(string id, Point position, string name = "API group") : base(id, position, name) { }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "API group";

    protected override string DefaultAccentColor => "#78909C";

    protected override string DefaultIconKey => "GRP";

    public override NodeModel Clone()
    {
        var clone = new ApiGroupNode(Position, Name);
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
