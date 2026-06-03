using System.Diagnostics;
using Nodely.Events;
using Nodely.Models.Base;

namespace Nodely.Behaviors;

/// <summary>Synthesizes click and double-click events from raw pointer down/move/up sequences.</summary>
public class EventsBehavior : Behavior
{
    private readonly Stopwatch _pointerClickSw = new();
    private Model? _model;
    private bool _capturePointerMove;
    private int _pointerMovedCount;

    /// <summary>Creates and wires the behavior.</summary>
    public EventsBehavior(Diagram diagram) : base(diagram)
    {
        Diagram.PointerDown += OnPointerDown;
        Diagram.PointerMove += OnPointerMove;
        Diagram.PointerUp += OnPointerUp;
        Diagram.PointerClick += OnPointerClick;
    }

    private void OnPointerClick(Model? model, PointerEvent e)
    {
        if (_pointerClickSw.IsRunning && _pointerClickSw.ElapsedMilliseconds <= 500)
            Diagram.TriggerPointerDoubleClick(model, e);

        _pointerClickSw.Restart();
    }

    private void OnPointerDown(Model? model, PointerEvent e)
    {
        _capturePointerMove = true;
        _pointerMovedCount = 0;
        _model = model;
    }

    private void OnPointerMove(Model? model, PointerEvent e)
    {
        if (!_capturePointerMove)
            return;

        _pointerMovedCount++;
    }

    private void OnPointerUp(Model? model, PointerEvent e)
    {
        if (!_capturePointerMove)
            return;

        _capturePointerMove = false;
        if (_pointerMovedCount > 0)
            return;

        if (_model == model)
        {
            Diagram.TriggerPointerClick(model, e);
            _model = null;
        }
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        Diagram.PointerDown -= OnPointerDown;
        Diagram.PointerMove -= OnPointerMove;
        Diagram.PointerUp -= OnPointerUp;
        Diagram.PointerClick -= OnPointerClick;
        _model = null;
    }
}
