using Nodely.Events;

namespace Nodely.Behaviors;

/// <summary>Zooms the canvas on wheel input, keeping the point under the pointer stationary.</summary>
public class ZoomBehavior : Behavior
{
    /// <summary>Creates and wires the behavior.</summary>
    public ZoomBehavior(Diagram diagram) : base(diagram)
    {
        Diagram.Wheel += OnWheel;
    }

    private void OnWheel(WheelEvent e)
    {
        if (Diagram.Container == null || e.DeltaY == 0)
            return;

        if (!Diagram.Options.Zoom.Enabled)
            return;

        var scale = Diagram.Options.Zoom.ScaleFactor;
        var oldZoom = Diagram.Zoom;
        var deltaY = Diagram.Options.Zoom.Inverse ? e.DeltaY * -1 : e.DeltaY;
        var newZoom = deltaY > 0 ? oldZoom * scale : oldZoom / scale;

        var min = Diagram.Options.Zoom.Minimum;
        var max = Diagram.Options.Zoom.Maximum;
        newZoom = newZoom < min ? min : (newZoom > max ? max : newZoom);

        if (newZoom < 0 || newZoom == Diagram.Zoom)
            return;

        // Pan adjustment so the point under the cursor stays fixed (from react-diagrams' ZoomCanvasAction).
        var clientWidth = Diagram.Container.Width;
        var clientHeight = Diagram.Container.Height;
        var widthDiff = clientWidth * newZoom - clientWidth * oldZoom;
        var heightDiff = clientHeight * newZoom - clientHeight * oldZoom;
        var clientX = e.X - Diagram.Container.Left;
        var clientY = e.Y - Diagram.Container.Top;
        var xFactor = (clientX - Diagram.Pan.X) / oldZoom / clientWidth;
        var yFactor = (clientY - Diagram.Pan.Y) / oldZoom / clientHeight;
        var newPanX = Diagram.Pan.X - widthDiff * xFactor;
        var newPanY = Diagram.Pan.Y - heightDiff * yFactor;

        Diagram.Batch(() =>
        {
            Diagram.SetPan(newPanX, newPanY);
            Diagram.SetZoom(newZoom);
        });
    }

    /// <inheritdoc />
    public override void Dispose() => Diagram.Wheel -= OnWheel;
}
