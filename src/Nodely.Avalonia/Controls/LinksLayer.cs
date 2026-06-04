using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Nodely.Models;
using Nodely.Models.Base;
using Nodely.PathGenerators;
using AvGeometry = Avalonia.Media.Geometry;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Controls;

/// <summary>
/// Immediate-mode layer that draws every link's generated path under the shared pan/zoom transform. The
/// built <see cref="AvGeometry"/> is cached per link and only rebuilt when that link changes. The layer stays
/// transparent to Avalonia hit-testing; the canvas resolves link hits with backend-independent path math.
/// </summary>
internal sealed class LinksLayer : Control
{
    private readonly DiagramCanvas _owner;
    private const double HitTolerance = 6; // screen-space half-width of the clickable band around a link

    private readonly Dictionary<BaseLinkModel, AvGeometry> _geometryCache = new();
    private readonly Dictionary<LinkMarker, AvGeometry> _markerGeometryCache = new();
    private Diagram? _diagram;

    public LinksLayer(DiagramCanvas owner)
    {
        _owner = owner;
        IsHitTestVisible = false;
        ClipToBounds = false;
        RenderTransformOrigin = RelativePoint.TopLeft;
    }

    public void SetDiagram(Diagram? diagram)
    {
        if (ReferenceEquals(_diagram, diagram))
            return;

        if (_diagram != null)
        {
            _diagram.Links.Added -= OnLinkAdded;
            _diagram.Links.Removed -= OnLinkRemoved;
            foreach (var link in _diagram.Links)
                link.Changed -= OnLinkChanged;
        }

        _geometryCache.Clear();
        _markerGeometryCache.Clear();
        _diagram = diagram;

        if (diagram != null)
        {
            diagram.Links.Added += OnLinkAdded;
            diagram.Links.Removed += OnLinkRemoved;
            foreach (var link in diagram.Links)
                link.Changed += OnLinkChanged;
        }

        UpdateTransform();
        InvalidateVisual();
    }

    public void UpdateTransform()
    {
        var d = _diagram;
        RenderTransform = d == null
            ? null
            : new TransformGroup
            {
                Children = { new ScaleTransform(d.Zoom, d.Zoom), new TranslateTransform(d.Pan.X, d.Pan.Y) },
            };
    }

    private void OnLinkAdded(BaseLinkModel link)
    {
        link.Changed += OnLinkChanged;
        InvalidateVisual();
    }

    private void OnLinkRemoved(BaseLinkModel link)
    {
        link.Changed -= OnLinkChanged;
        _geometryCache.Remove(link);
        InvalidateVisual();
    }

    private void OnLinkChanged(Model m)
    {
        if (m is BaseLinkModel link)
            _geometryCache.Remove(link); // the route changed; rebuild its geometry lazily
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        var d = _diagram;
        if (d == null)
            return;

        foreach (var link in d.Links)
        {
            var result = link.PathGeneratorResult;
            if (result == null)
                continue;

            if (!_geometryCache.TryGetValue(link, out var geometry))
                _geometryCache[link] = geometry = PathDataGeometry.ToGeometry(result.FullPath);

            // A registered custom drawer (RegisterLink) takes over; otherwise draw the standard link.
            var drawer = _owner.ResolveLinkDrawer(link);
            if (drawer != null)
            {
                var ctx = new LinkRenderContext(
                    link, _owner.Palette, link.Selected, geometry, result,
                    () => DrawDefaultLink(context, link, geometry, result));
                drawer(context, ctx);
            }
            else
            {
                DrawDefaultLink(context, link, geometry, result);
            }
        }
    }

    private void DrawDefaultLink(DrawingContext context, BaseLinkModel link, AvGeometry geometry, PathGeneratorResult result)
    {
        var style = _owner.ResolveLinkStyle(link);
        var baseWidth = style.Width ?? (link is LinkModel lm ? lm.Width : 2);
        var brush = link.Selected
            ? (style.SelectedStroke ?? _owner.Palette.Selection)
            : (style.Stroke ?? _owner.Palette.LinkStroke);
        var penWidth = link.Selected ? baseWidth + 1.5 : baseWidth;
        var pen = new Pen(brush, penWidth) { DashStyle = style.DashStyle };
        context.DrawGeometry(null, pen, geometry);

        DrawMarker(context, link.EffectiveSourceMarker, result.SourceMarkerPosition, result.SourceMarkerAngle, brush);
        DrawMarker(context, link.EffectiveTargetMarker, result.TargetMarkerPosition, result.TargetMarkerAngle, brush);

        foreach (var label in link.Labels)
            DrawLabel(context, result.FullPath, label);
    }

    // A label is centered on the point at its resolved arc-length along the link, drawn as a rounded chip in
    // diagram space (the layer's pan/zoom transform then maps it to screen, so labels scale with zoom).
    private void DrawLabel(DrawingContext context, global::Nodely.Geometry.PathData path, LinkLabelModel label)
    {
        if (string.IsNullOrEmpty(label.Content))
            return;

        var position = path.PointAtDistance(ResolveLabelDistance(label.Distance, path.Length()));
        if (position is not { } point)
            return;

        var text = new FormattedText(
            label.Content, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            Typeface.Default, 12, _owner.Palette.LabelForeground);

        const double padX = 6, padY = 3;
        var cx = point.X + (label.Offset?.X ?? 0);
        var cy = point.Y + (label.Offset?.Y ?? 0);
        var rect = new Rect(
            cx - text.Width / 2 - padX, cy - text.Height / 2 - padY,
            text.Width + padX * 2, text.Height + padY * 2);

        context.DrawRectangle(_owner.Palette.LabelBackground, null, rect, 4, 4);
        context.DrawText(text, new global::Avalonia.Point(rect.X + padX, rect.Y + padY));
    }

    // Distance semantics from LinkLabelModel: [0,1] = fraction of length, > 1 = absolute from start,
    // < 0 = absolute from end, null = midpoint.
    private static double ResolveLabelDistance(double? distance, double length)
    {
        if (distance is not { } d)
            return length / 2;
        if (d >= 0 && d <= 1)
            return d * length;
        if (d > 1)
            return Math.Min(d, length);
        return Math.Max(0, length + d);
    }

    /// <summary>
    /// Returns the topmost link whose stroked path contains <paramref name="point"/> (diagram space), or null.
    /// Links are immediate-mode (not in the visual tree), so the canvas calls this for link hit-testing. The
    /// tolerance is divided by the zoom so the clickable band stays a constant width on screen.
    /// </summary>
    public BaseLinkModel? HitTest(NodelyPoint point, double zoom)
    {
        var d = _diagram;
        if (d == null)
            return null;

        var tolerance = zoom <= 0 ? HitTolerance : HitTolerance / zoom;
        BaseLinkModel? hit = null;

        foreach (var link in d.Links)
        {
            var result = link.PathGeneratorResult;
            if (result == null)
                continue;

            var width = link is LinkModel lm ? lm.Width : 2;
            if (result.FullPath.DistanceTo(point) <= Math.Max(width / 2, tolerance))
                hit = link; // keep the last match — that's the topmost in draw order
        }

        return hit;
    }

    // Markers draw in diagram space (the layer's pan/zoom RenderTransform maps to screen). The shape is defined
    // with the link end at the origin pointing along +X, so we rotate by the resolved angle, then translate to
    // the resolved end position. Local-space geometry is cached per marker shape (Arrow/Square/custom instance).
    private void DrawMarker(DrawingContext context, LinkMarker? marker, NodelyPoint? position, double? angleDegrees, IBrush brush)
    {
        if (marker == null || position is not { } pos || angleDegrees is not { } angle)
            return;

        if (!_markerGeometryCache.TryGetValue(marker, out var geometry))
            _markerGeometryCache[marker] = geometry = PathDataGeometry.ToGeometry(marker.Path, filled: true);

        var matrix = Matrix.CreateRotation(angle * Math.PI / 180) * Matrix.CreateTranslation(pos.X, pos.Y);
        using (context.PushTransform(matrix))
            context.DrawGeometry(brush, null, geometry);
    }
}
