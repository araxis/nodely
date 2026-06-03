namespace Nodely.Models;

/// <summary>Where on a node a port sits, which also drives the direction a link leaves the port.</summary>
public enum PortAlignment
{
    /// <summary>Top-center.</summary>
    Top,

    /// <summary>Top-right corner.</summary>
    TopRight,

    /// <summary>Right-center.</summary>
    Right,

    /// <summary>Bottom-right corner.</summary>
    BottomRight,

    /// <summary>Bottom-center.</summary>
    Bottom,

    /// <summary>Bottom-left corner.</summary>
    BottomLeft,

    /// <summary>Left-center.</summary>
    Left,

    /// <summary>Top-left corner.</summary>
    TopLeft
}
