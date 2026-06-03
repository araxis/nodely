using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Nodely.Extensions;
using NodelyRect = Nodely.Geometry.Rectangle;
using AvPoint = Avalonia.Point;

namespace Nodely.Avalonia.Controls;

/// <summary>
/// An overview minimap for a <see cref="Diagram"/>: draws every node at scale plus the current viewport
/// rectangle, and pans the diagram when clicked or dragged. Place it anywhere and bind it to the same
/// diagram as the <see cref="DiagramCanvas"/>.
/// </summary>
public class DiagramNavigator : Control
{
    /// <summary>Defines the <see cref="Diagram"/> property.</summary>
    public static readonly StyledProperty<Diagram?> DiagramProperty =
        AvaloniaProperty.Register<DiagramNavigator, Diagram?>(nameof(Diagram));

    private static readonly IBrush BackBrush = new SolidColorBrush(Color.FromRgb(0x15, 0x15, 0x1A));
    private static readonly IBrush NodeBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x60, 0x70));
    private static readonly IBrush ViewportFill = new SolidColorBrush(Color.FromArgb(0x22, 0x4D, 0x9E, 0xFF));
    private static readonly IPen ViewportPen = new Pen(new SolidColorBrush(Color.FromRgb(0x4D, 0x9E, 0xFF)), 1);
    private const double ContentPadding = 20;

    private Diagram? _subscribed;

    /// <summary>Creates the navigator.</summary>
    public DiagramNavigator()
    {
        ClipToBounds = true;
    }

    /// <summary>The diagram this minimap reflects and pans.</summary>
    public Diagram? Diagram
    {
        get => GetValue(DiagramProperty);
        set => SetValue(DiagramProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DiagramProperty)
        {
            Subscribe(change.GetNewValue<Diagram?>());
            InvalidateVisual();
        }
    }

    private void Subscribe(Diagram? diagram)
    {
        if (ReferenceEquals(_subscribed, diagram))
            return;

        if (_subscribed != null)
        {
            _subscribed.Changed -= OnChanged;
            _subscribed.PanChanged -= OnChanged;
            _subscribed.ZoomChanged -= OnChanged;
            _subscribed.ContainerChanged -= OnChanged;
        }

        _subscribed = diagram;

        if (diagram != null)
        {
            diagram.Changed += OnChanged;
            diagram.PanChanged += OnChanged;
            diagram.ZoomChanged += OnChanged;
            diagram.ContainerChanged += OnChanged;
        }
    }

    private void OnChanged() => InvalidateVisual();

    private bool TryGetMapping(out NodelyRect content, out double scale, out double offsetX, out double offsetY)
    {
        content = NodelyRect.Zero;
        scale = 0;
        offsetX = 0;
        offsetY = 0;

        var d = Diagram;
        if (d == null || d.Nodes.Count == 0)
            return false;

        var bounds = d.Nodes.GetBounds();
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return false;

        content = bounds.Inflate(ContentPadding, ContentPadding);
        var navW = Bounds.Width;
        var navH = Bounds.Height;
        if (navW <= 0 || navH <= 0)
            return false;

        scale = Math.Min(navW / content.Width, navH / content.Height);
        offsetX = (navW - content.Width * scale) / 2;
        offsetY = (navH - content.Height * scale) / 2;
        return true;
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        context.FillRectangle(BackBrush, new Rect(Bounds.Size));

        var d = Diagram;
        if (d == null || !TryGetMapping(out var content, out var scale, out var offsetX, out var offsetY))
            return;

        Rect ToNav(double gx, double gy, double gw, double gh)
            => new((gx - content.Left) * scale + offsetX, (gy - content.Top) * scale + offsetY, gw * scale, gh * scale);

        foreach (var node in d.Nodes)
        {
            if (node.Size == null)
                continue;
            context.FillRectangle(NodeBrush, ToNav(node.Position.X, node.Position.Y, node.Size.Width, node.Size.Height));
        }

        if (d.Container != null && d.Zoom > 0)
        {
            var vpLeft = -d.Pan.X / d.Zoom;
            var vpTop = -d.Pan.Y / d.Zoom;
            var vpWidth = d.Container.Width / d.Zoom;
            var vpHeight = d.Container.Height / d.Zoom;
            context.DrawRectangle(ViewportFill, ViewportPen, ToNav(vpLeft, vpTop, vpWidth, vpHeight));
        }
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Pointer.Capture(this);
        PanTo(e.GetPosition(this));
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            PanTo(e.GetPosition(this));
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        e.Pointer.Capture(null);
    }

    private void PanTo(AvPoint navPoint)
    {
        var d = Diagram;
        if (d?.Container == null || !TryGetMapping(out var content, out var scale, out var offsetX, out var offsetY))
            return;

        var gx = (navPoint.X - offsetX) / scale + content.Left;
        var gy = (navPoint.Y - offsetY) / scale + content.Top;

        // Center the clicked diagram point in the main viewport.
        d.SetPan(d.Container.Width / 2 - gx * d.Zoom, d.Container.Height / 2 - gy * d.Zoom);
    }
}
