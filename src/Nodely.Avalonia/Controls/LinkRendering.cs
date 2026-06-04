using System;
using Avalonia.Media;
using Nodely.Models.Base;
using Nodely.PathGenerators;
using AvGeometry = Avalonia.Media.Geometry;
using NodelyPathData = Nodely.Geometry.PathData;

namespace Nodely.Avalonia.Controls;

/// <summary>
/// Draws one link. Registered per link type via <see cref="DiagramCanvas.RegisterLink{TLink}"/> to fully
/// control link appearance (gradients, dashes, custom markers, animated flow, …). Call
/// <see cref="LinkRenderContext.DrawDefault"/> to render the standard link and augment it, or draw entirely
/// your own. Drawing happens in diagram space — the layer's pan/zoom transform maps it to the screen.
/// </summary>
public delegate void LinkDrawer(DrawingContext context, LinkRenderContext ctx);

/// <summary>Per-link style overrides. Null fields fall back to the palette / link defaults.</summary>
public sealed class LinkStyle
{
    /// <summary>Stroke brush (default: <c>Palette.LinkStroke</c>).</summary>
    public IBrush? Stroke { get; init; }

    /// <summary>Stroke brush when selected (default: <c>Palette.Selection</c>).</summary>
    public IBrush? SelectedStroke { get; init; }

    /// <summary>Stroke width (default: the link's own width, or 2).</summary>
    public double? Width { get; init; }

    /// <summary>Dash pattern (default: solid).</summary>
    public IDashStyle? DashStyle { get; init; }

    /// <summary>The all-defaults style.</summary>
    public static LinkStyle Default { get; } = new();
}

/// <summary>Context passed to typed link style registrations.</summary>
public sealed class LinkStyleContext
{
    internal LinkStyleContext(DiagramCanvas canvas, BaseLinkModel link)
    {
        Canvas = canvas;
        Link = link;
    }

    /// <summary>The canvas resolving the style.</summary>
    public DiagramCanvas Canvas { get; }

    /// <summary>The link being styled.</summary>
    public BaseLinkModel Link { get; }

    /// <summary>The current canvas palette.</summary>
    public NodelyPalette Palette => Canvas.Palette;

    /// <summary>Whether the link is currently selected.</summary>
    public bool IsSelected => Link.Selected;
}

/// <summary>The information a <see cref="LinkDrawer"/> needs to render a link.</summary>
public sealed class LinkRenderContext
{
    private readonly Action _drawDefault;

    internal LinkRenderContext(
        BaseLinkModel link,
        NodelyPalette palette,
        bool isSelected,
        AvGeometry geometry,
        PathGeneratorResult result,
        Action drawDefault)
    {
        Link = link;
        Palette = palette;
        IsSelected = isSelected;
        Geometry = geometry;
        Result = result;
        _drawDefault = drawDefault;
    }

    /// <summary>The link being drawn.</summary>
    public BaseLinkModel Link { get; }

    /// <summary>The canvas palette (so custom drawing can stay on-theme).</summary>
    public NodelyPalette Palette { get; }

    /// <summary>Whether the link is currently selected.</summary>
    public bool IsSelected { get; }

    /// <summary>The link's path as a ready-to-draw Avalonia geometry (in diagram space).</summary>
    public AvGeometry Geometry { get; }

    /// <summary>The generated path result, including marker angles/positions and per-segment sub-paths.</summary>
    public PathGeneratorResult Result { get; }

    /// <summary>The neutral path data, for sampling points or building custom geometry.</summary>
    public NodelyPathData Path => Result.FullPath;

    /// <summary>Renders the standard link (stroke + markers + labels). Call it to augment rather than replace.</summary>
    public void DrawDefault() => _drawDefault();
}
