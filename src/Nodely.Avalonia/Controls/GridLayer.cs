using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Nodely.Avalonia.Controls;

/// <summary>
/// Immediate-mode background grid for a <see cref="DiagramCanvas"/>. Reads pan/zoom from the owning canvas'
/// diagram and draws grid lines that move and scale with the viewport. Not hit-test visible, so pointer
/// input passes through to the canvas.
/// </summary>
internal sealed class GridLayer : Control
{
    private readonly DiagramCanvas _owner;

    public GridLayer(DiagramCanvas owner)
    {
        _owner = owner;
        IsHitTestVisible = false;
    }

    public override void Render(DrawingContext context)
    {
        var diagram = _owner.Diagram;
        if (diagram == null || _owner.GridBrush is not { } brush || _owner.GridSize <= 0)
            return;

        var step = _owner.GridSize * diagram.Zoom;
        if (step < 4) // avoid drawing an excessive number of lines when zoomed far out
            return;

        var rect = new Rect(Bounds.Size);
        var pen = new Pen(brush, 1);

        var startX = diagram.Pan.X % step;
        if (startX < 0) startX += step;
        for (var x = startX; x <= rect.Width; x += step)
            context.DrawLine(pen, new Point(x, 0), new Point(x, rect.Height));

        var startY = diagram.Pan.Y % step;
        if (startY < 0) startY += step;
        for (var y = startY; y <= rect.Height; y += step)
            context.DrawLine(pen, new Point(0, y), new Point(rect.Width, y));
    }
}
