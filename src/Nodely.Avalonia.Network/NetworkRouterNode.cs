using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Network;

/// <summary>A router or routing edge node.</summary>
public sealed class NetworkRouterNode : NetworkNodeBase
{
    public new const string ModelKindKey = "network.router";

    public NetworkRouterNode(Point position, string name = "Router") : base(position, name) { }

    public NetworkRouterNode(string id, Point position, string name = "Router") : base(id, position, name) { }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "Router";

    protected override string DefaultRole => "Router";

    protected override string DefaultAccentColor => "#4D9EFF";

    protected override string DefaultIconKey => "RTR";

    public override NodeModel Clone()
    {
        var clone = new NetworkRouterNode(Position, Name);
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
