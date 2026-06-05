using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Network;

/// <summary>A load balancer or traffic distribution node.</summary>
public sealed class NetworkLoadBalancerNode : NetworkNodeBase
{
    public new const string ModelKindKey = "network.loadbalancer";

    public NetworkLoadBalancerNode(Point position, string name = "Load balancer") : base(position, name) { }

    public NetworkLoadBalancerNode(string id, Point position, string name = "Load balancer") : base(id, position, name) { }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "Load balancer";

    protected override string DefaultRole => "Balancer";

    protected override string DefaultAccentColor => "#8B68B8";

    protected override string DefaultIconKey => "LB";

    public override NodeModel Clone()
    {
        var clone = new NetworkLoadBalancerNode(Position, Name);
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
