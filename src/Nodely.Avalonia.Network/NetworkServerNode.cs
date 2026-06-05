using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Network;

/// <summary>A server, VM, or host node.</summary>
public sealed class NetworkServerNode : NetworkNodeBase
{
    public new const string ModelKindKey = "network.server";

    public NetworkServerNode(Point position, string name = "Server") : base(position, name) { }

    public NetworkServerNode(string id, Point position, string name = "Server") : base(id, position, name) { }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "Server";

    protected override string DefaultRole => "Server";

    protected override string DefaultAccentColor => "#D18B30";

    protected override string DefaultIconKey => "SRV";

    public override NodeModel Clone()
    {
        var clone = new NetworkServerNode(Position, Name);
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
