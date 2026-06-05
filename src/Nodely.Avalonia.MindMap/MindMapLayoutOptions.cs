namespace Nodely.Avalonia.MindMap;

/// <summary>Options for mind-map arrange.</summary>
public sealed class MindMapLayoutOptions
{
    /// <summary>Root center X position.</summary>
    public double OriginX { get; set; }

    /// <summary>Root center Y position.</summary>
    public double OriginY { get; set; }

    /// <summary>Horizontal spacing between topic levels.</summary>
    public double LevelSpacing { get; set; } = 260;

    /// <summary>Vertical spacing between sibling subtrees.</summary>
    public double TopicSpacing { get; set; } = 34;

    /// <summary>Fallback width before the UI has measured a node.</summary>
    public double DefaultNodeWidth { get; set; } = MindMapVisualMetrics.BranchWidth;

    /// <summary>Fallback height before the UI has measured a node.</summary>
    public double DefaultNodeHeight { get; set; } = MindMapVisualMetrics.BranchHeight;
}
