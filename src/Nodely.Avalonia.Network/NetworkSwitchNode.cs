using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.Network;

/// <summary>A network switch node with rendered port rows.</summary>
public sealed class NetworkSwitchNode : NetworkNodeBase
{
    public new const string ModelKindKey = "network.switch";

    private int _portCount = 24;
    private int _activePorts = 18;

    public NetworkSwitchNode(Point position, string name = "Switch") : base(position, name) { }

    public NetworkSwitchNode(string id, Point position, string name = "Switch") : base(id, position, name) { }

    /// <summary>Total switch ports shown by the renderer.</summary>
    public int PortCount
    {
        get => _portCount;
        set
        {
            _portCount = Clamp(value, 4, 48);
            _activePorts = Clamp(_activePorts, 0, _portCount);
            Refresh();
        }
    }

    /// <summary>Number of active switch ports shown as lit.</summary>
    public int ActivePorts
    {
        get => _activePorts;
        set
        {
            _activePorts = Clamp(value, 0, PortCount);
            Refresh();
        }
    }

    public override string ModelKind => ModelKindKey;

    protected override string DefaultName => "Switch";

    protected override string DefaultRole => "Switch";

    protected override string DefaultAccentColor => "#37A779";

    protected override string DefaultIconKey => "SW";

    public override NodeModel Clone()
    {
        var clone = new NetworkSwitchNode(Position, Name)
        {
            PortCount = PortCount,
            ActivePorts = ActivePorts,
        };
        CopyBaseTo(clone);
        return clone;
    }

    public override IReadOnlyDictionary<string, object?> GetExtraData()
    {
        var extra = BuildBaseExtra();
        extra["PortCount"] = PortCount;
        extra["ActivePorts"] = ActivePorts;
        return extra;
    }

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        ApplyBaseExtra(data);
        if (data.TryGetValue("PortCount", out var portCount) && portCount is not null)
            _portCount = Clamp(ParseInt(portCount, 24), 4, 48);
        if (data.TryGetValue("ActivePorts", out var activePorts) && activePorts is not null)
            _activePorts = Clamp(ParseInt(activePorts, 18), 0, _portCount);
    }

    private static int ParseInt(object value, int fallback)
        => value switch
        {
            int intValue => intValue,
            long longValue => longValue > int.MaxValue ? int.MaxValue : (int)longValue,
            double doubleValue => (int)doubleValue,
            string text when int.TryParse(text, out var parsed) => parsed,
            _ => fallback,
        };

    private static int Clamp(int value, int min, int max)
        => value < min ? min : value > max ? max : value;
}
