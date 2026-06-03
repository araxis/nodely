namespace Nodely.Events;

/// <summary>
/// Identifies which pointer/mouse button an event concerns. Values match the common web/DOM convention
/// (Left=0, Middle=1, Right=2) so ported interaction logic reads the same; the Avalonia layer maps
/// Avalonia's pointer update kinds onto these.
/// </summary>
public enum PointerButton
{
    /// <summary>No button (e.g. a pure move).</summary>
    None = -1,

    /// <summary>The primary (usually left) button.</summary>
    Left = 0,

    /// <summary>The middle (wheel) button.</summary>
    Middle = 1,

    /// <summary>The secondary (usually right) button.</summary>
    Right = 2,

    /// <summary>The first extended button.</summary>
    XButton1 = 3,

    /// <summary>The second extended button.</summary>
    XButton2 = 4
}
