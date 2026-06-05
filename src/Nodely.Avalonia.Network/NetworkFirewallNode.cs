using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Network;

/// <summary>A firewall or policy enforcement node.</summary>
public sealed class NetworkFirewallNode : NetworkNodeBase
{
    public new const string ModelKindKey = "network.firewall";

    public NetworkFirewallNode(Point position, string name = "Firewall") : base(position, name) { }

    public NetworkFirewallNode(string id, Point position, string name = "Firewall") : base(id, position, name) { }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "Firewall";

    protected override string DefaultRole => "Firewall";

    protected override string DefaultAccentColor => "#C45552";

    protected override string DefaultIconKey => "FW";

    public override NodeModel Clone()
    {
        var clone = new NetworkFirewallNode(Position, Name);
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
