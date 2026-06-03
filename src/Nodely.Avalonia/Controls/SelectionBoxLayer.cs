using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Nodely.Avalonia.Controls;

/// <summary>Draws the marquee (rubber-band) rectangle while the user Shift-drags to select. Screen-space.</summary>
internal sealed class SelectionBoxLayer : Control
{
    private static readonly IBrush Fill = new SolidColorBrush(Color.FromArgb(0x22, 0x4D, 0x9E, 0xFF));
    private static readonly IPen Stroke =
        new Pen(new SolidColorBrush(Color.FromRgb(0x4D, 0x9E, 0xFF)), 1) { DashStyle = DashStyle.Dash };

    private Rect? _rect;

    public SelectionBoxLayer()
    {
        IsHitTestVisible = false;
    }

    public void SetRect(Rect rect)
    {
        _rect = rect;
        InvalidateVisual();
    }

    public void Clear()
    {
        if (_rect == null)
            return;

        _rect = null;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        if (_rect is { } r)
            context.DrawRectangle(Fill, Stroke, r);
    }
}
