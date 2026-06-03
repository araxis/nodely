using Avalonia.Media;

namespace Nodely.Avalonia;

/// <summary>The set of brushes a <see cref="Nodely.Avalonia.Controls.DiagramCanvas"/> uses to render itself.</summary>
public sealed record NodelyPalette
{
    /// <summary>The canvas background.</summary>
    public required IBrush CanvasBackground { get; init; }

    /// <summary>The grid line color.</summary>
    public required IBrush GridLine { get; init; }

    /// <summary>The default node background.</summary>
    public required IBrush NodeBackground { get; init; }

    /// <summary>The default node border.</summary>
    public required IBrush NodeBorder { get; init; }

    /// <summary>The default node text color.</summary>
    public required IBrush NodeText { get; init; }

    /// <summary>The link stroke color.</summary>
    public required IBrush LinkStroke { get; init; }

    /// <summary>The port fill color.</summary>
    public required IBrush PortFill { get; init; }

    /// <summary>The port stroke color.</summary>
    public required IBrush PortStroke { get; init; }

    /// <summary>The selection/accent color.</summary>
    public required IBrush Selection { get; init; }

    /// <summary>The group container background.</summary>
    public required IBrush GroupBackground { get; init; }

    /// <summary>The group container border.</summary>
    public required IBrush GroupBorder { get; init; }

    /// <summary>The background chip drawn behind a link label.</summary>
    public IBrush LabelBackground { get; init; } = new SolidColorBrush(Color.FromArgb(0xDD, 0x2A, 0x2A, 0x33));

    /// <summary>The link label text color.</summary>
    public IBrush LabelForeground { get; init; } = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF4));
}

/// <summary>Built-in light and dark palettes for Nodely.</summary>
public static class NodelyPalettes
{
    /// <summary>The default dark palette.</summary>
    public static NodelyPalette Dark { get; } = new()
    {
        CanvasBackground = Brush(0xFF, 0x1E, 0x1E, 0x24),
        GridLine = Brush(0x20, 0xFF, 0xFF, 0xFF),
        NodeBackground = Brush(0xFF, 0x33, 0x33, 0x3D),
        NodeBorder = Brush(0xFF, 0x55, 0x55, 0x63),
        NodeText = Brush(0xFF, 0xF0, 0xF0, 0xF4),
        LinkStroke = Brush(0xFF, 0x9A, 0x9A, 0xAD),
        PortFill = Brush(0xFF, 0x4D, 0x9E, 0xFF),
        PortStroke = Brush(0xFF, 0x1E, 0x1E, 0x24),
        Selection = Brush(0xFF, 0x4D, 0x9E, 0xFF),
        GroupBackground = Brush(0x22, 0x4D, 0x9E, 0xFF),
        GroupBorder = Brush(0x88, 0x4D, 0x9E, 0xFF),
        LabelBackground = Brush(0xE6, 0x2A, 0x2A, 0x33),
        LabelForeground = Brush(0xFF, 0xF0, 0xF0, 0xF4),
    };

    /// <summary>A light palette.</summary>
    public static NodelyPalette Light { get; } = new()
    {
        CanvasBackground = Brush(0xFF, 0xF5, 0xF5, 0xF7),
        GridLine = Brush(0x18, 0x00, 0x00, 0x00),
        NodeBackground = Brush(0xFF, 0xFF, 0xFF, 0xFF),
        NodeBorder = Brush(0xFF, 0xC8, 0xC8, 0xD0),
        NodeText = Brush(0xFF, 0x1E, 0x1E, 0x24),
        LinkStroke = Brush(0xFF, 0x70, 0x70, 0x80),
        PortFill = Brush(0xFF, 0x2D, 0x7D, 0xE0),
        PortStroke = Brush(0xFF, 0xFF, 0xFF, 0xFF),
        Selection = Brush(0xFF, 0x2D, 0x7D, 0xE0),
        GroupBackground = Brush(0x22, 0x2D, 0x7D, 0xE0),
        GroupBorder = Brush(0x88, 0x2D, 0x7D, 0xE0),
        LabelBackground = Brush(0xF0, 0xFF, 0xFF, 0xFF),
        LabelForeground = Brush(0xFF, 0x1E, 0x1E, 0x24),
    };

    private static IBrush Brush(byte a, byte r, byte g, byte b) => new SolidColorBrush(Color.FromArgb(a, r, g, b));
}
