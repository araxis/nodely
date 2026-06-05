namespace Nodely.Avalonia.Network;

/// <summary>Options for the network topology arrange helper.</summary>
public sealed class NetworkLayoutOptions
{
    /// <summary>The horizontal center of the arranged topology.</summary>
    public double OriginX { get; set; }

    /// <summary>The vertical center of the arranged topology.</summary>
    public double OriginY { get; set; }

    /// <summary>Distance between topology columns.</summary>
    public double ColumnSpacing { get; set; } = 250;

    /// <summary>Distance between nodes in a column.</summary>
    public double RowSpacing { get; set; } = 56;

    /// <summary>Fallback width when a node has not been measured yet.</summary>
    public double DefaultNodeWidth { get; set; } = NetworkVisualMetrics.DeviceWidth;

    /// <summary>Fallback height when a node has not been measured yet.</summary>
    public double DefaultNodeHeight { get; set; } = NetworkVisualMetrics.DeviceHeight;
}
