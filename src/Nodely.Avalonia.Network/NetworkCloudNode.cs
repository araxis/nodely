using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Network;

/// <summary>An external network or cloud boundary node.</summary>
public sealed class NetworkCloudNode : NetworkNodeBase
{
    public new const string ModelKindKey = "network.cloud";

    public NetworkCloudNode(Point position, string name = "Cloud") : base(position, name) { }

    public NetworkCloudNode(string id, Point position, string name = "Cloud") : base(id, position, name) { }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "Cloud";

    protected override string DefaultRole => "External";

    protected override string DefaultAccentColor => "#4D9EFF";

    protected override string DefaultIconKey => "NET";

    public override NodeModel Clone()
    {
        var clone = new NetworkCloudNode(Position, Name);
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
