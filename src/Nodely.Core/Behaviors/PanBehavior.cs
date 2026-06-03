using Nodely.Events;
using Nodely.Geometry;
using Nodely.Models.Base;

namespace Nodely.Behaviors;

/// <summary>Pans the canvas when the user drags empty space with the left button (no Shift).</summary>
public class PanBehavior : Behavior
{
    private Point? _initialPan;
    private double _lastClientX;
    private double _lastClientY;

    /// <summary>Creates and wires the behavior.</summary>
    public PanBehavior(Diagram diagram) : base(diagram)
    {
        Diagram.PointerDown += OnPointerDown;
        Diagram.PointerMove += OnPointerMove;
        Diagram.PointerUp += OnPointerUp;
    }

    private void OnPointerDown(Model? model, PointerEvent e)
    {
        if (e.Button != PointerButton.Left)
            return;

        Start(model, e.X, e.Y, e.ShiftKey);
    }

    private void OnPointerMove(Model? model, PointerEvent e) => Move(e.X, e.Y);

    private void OnPointerUp(Model? model, PointerEvent e) => End();

    private void Start(Model? model, double x, double y, bool shiftKey)
    {
        if (!Diagram.Options.AllowPanning || model != null || shiftKey)
            return;

        _initialPan = Diagram.Pan;
        _lastClientX = x;
        _lastClientY = y;
    }

    private void Move(double x, double y)
    {
        if (!Diagram.Options.AllowPanning || _initialPan == null)
            return;

        var deltaX = x - _lastClientX - (Diagram.Pan.X - _initialPan.X);
        var deltaY = y - _lastClientY - (Diagram.Pan.Y - _initialPan.Y);
        Diagram.UpdatePan(deltaX, deltaY);
    }

    private void End()
    {
        if (!Diagram.Options.AllowPanning)
            return;

        _initialPan = null;
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        Diagram.PointerDown -= OnPointerDown;
        Diagram.PointerMove -= OnPointerMove;
        Diagram.PointerUp -= OnPointerUp;
    }
}
