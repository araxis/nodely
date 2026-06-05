namespace Nodely.Avalonia.StateMachine;

/// <summary>Options for the simple state-machine arrange helper.</summary>
public sealed class StateMachineLayoutOptions
{
    /// <summary>The X coordinate for the first column center.</summary>
    public double OriginX { get; set; } = 120;

    /// <summary>The Y coordinate for the first column center.</summary>
    public double OriginY { get; set; } = 260;

    /// <summary>Distance between logical columns.</summary>
    public double ColumnSpacing { get; set; } = 285;

    /// <summary>Distance between nodes in the same column.</summary>
    public double RowSpacing { get; set; } = 185;

    /// <summary>Fallback width for nodes that have not been measured.</summary>
    public double DefaultNodeWidth { get; set; } = StateMachineVisualMetrics.StateWidth;

    /// <summary>Fallback height for nodes that have not been measured.</summary>
    public double DefaultNodeHeight { get; set; } = StateMachineVisualMetrics.StateHeight;
}
