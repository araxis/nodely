using Nodely.Events;
using Nodely.Models.Base;

namespace Nodely.Behaviors;

/// <summary>Selects/unselects models on pointer down, honoring Ctrl for multi-selection.</summary>
public class SelectionBehavior : Behavior
{
    /// <summary>Creates and wires the behavior.</summary>
    public SelectionBehavior(Diagram diagram) : base(diagram)
    {
        Diagram.PointerDown += OnPointerDown;
    }

    private void OnPointerDown(Model? model, PointerEvent e)
    {
        var ctrlKey = e.CtrlKey;
        switch (model)
        {
            case null:
                Diagram.UnselectAll();
                break;
            case SelectableModel sm when ctrlKey && sm.Selected:
                Diagram.UnselectModel(sm);
                break;
            case SelectableModel sm:
                if (!sm.Selected)
                    Diagram.SelectModel(sm, !ctrlKey || !Diagram.Options.AllowMultiSelection);
                break;
        }
    }

    /// <inheritdoc />
    public override void Dispose() => Diagram.PointerDown -= OnPointerDown;
}
