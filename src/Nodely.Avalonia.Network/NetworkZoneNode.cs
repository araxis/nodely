using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Network;

/// <summary>A subnet, environment, or topology zone node.</summary>
public sealed class NetworkZoneNode : NetworkNodeBase
{
    public new const string ModelKindKey = "network.zone";

    public NetworkZoneNode(Point position, string name = "Zone") : base(position, name) { }

    public NetworkZoneNode(string id, Point position, string name = "Zone") : base(id, position, name) { }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "Zone";

    protected override string DefaultRole => "Subnet";

    protected override string DefaultAccentColor => "#78909C";

    protected override string DefaultIconKey => "ZONE";

    public override NodeModel Clone()
    {
        var clone = new NetworkZoneNode(Position, Name);
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData() => BuildBaseExtra();

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data) => ApplyBaseExtra(data);
}
