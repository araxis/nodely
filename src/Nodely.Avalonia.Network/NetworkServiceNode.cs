using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Network;

/// <summary>An application service or network-facing workload node.</summary>
public sealed class NetworkServiceNode : NetworkNodeBase
{
    public new const string ModelKindKey = "network.service";

    public NetworkServiceNode(Point position, string name = "Service") : base(position, name) { }

    public NetworkServiceNode(string id, Point position, string name = "Service") : base(id, position, name) { }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "Service";

    protected override string DefaultRole => "Service";

    protected override string DefaultAccentColor => "#9670C7";

    protected override string DefaultIconKey => "SVC";

    public override NodeModel Clone()
    {
        var clone = new NetworkServiceNode(Position, Name);
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
