using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Network;

/// <summary>A client device node.</summary>
public sealed class NetworkClientNode : NetworkNodeBase
{
    public new const string ModelKindKey = "network.client";

    public NetworkClientNode(Point position, string name = "Client") : base(position, name) { }

    public NetworkClientNode(string id, Point position, string name = "Client") : base(id, position, name) { }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "Client";

    protected override string DefaultRole => "Client";

    protected override string DefaultAccentColor => "#33A6B8";

    protected override string DefaultIconKey => "CLI";

    public override NodeModel Clone()
    {
        var clone = new NetworkClientNode(Position, Name);
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
