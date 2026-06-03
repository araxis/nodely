using System;
using System.Collections;
using System.Collections.Generic;
using Nodely.Models.Base;

namespace Nodely.Controls;

/// <summary>The set of adornment <see cref="Control"/>s attached to a single model, plus their visibility.</summary>
public class ControlsContainer : IReadOnlyList<Control>
{
    private readonly List<Control> _controls = new(4);

    /// <summary>Raised when the container's visibility changes.</summary>
    public event Action<Model>? VisibilityChanged;

    /// <summary>Raised when the set of controls changes.</summary>
    public event Action<Model>? ControlsChanged;

    /// <summary>Creates a container for <paramref name="model"/>.</summary>
    public ControlsContainer(Model model, ControlsType type = ControlsType.OnSelection)
    {
        Model = model;
        Type = type;
    }

    /// <summary>The model these controls adorn.</summary>
    public Model Model { get; }

    /// <summary>When the controls are shown.</summary>
    public ControlsType Type { get; set; }

    /// <summary>Whether the controls are currently visible.</summary>
    public bool Visible { get; private set; }

    /// <summary>Shows the controls.</summary>
    public void Show()
    {
        if (Visible)
            return;

        Visible = true;
        VisibilityChanged?.Invoke(Model);
    }

    /// <summary>Hides the controls.</summary>
    public void Hide()
    {
        if (!Visible)
            return;

        Visible = false;
        VisibilityChanged?.Invoke(Model);
    }

    /// <summary>Adds a control.</summary>
    public ControlsContainer Add(Control control)
    {
        _controls.Add(control);
        ControlsChanged?.Invoke(Model);
        return this;
    }

    /// <summary>Removes a control.</summary>
    public ControlsContainer Remove(Control control)
    {
        if (_controls.Remove(control))
            ControlsChanged?.Invoke(Model);

        return this;
    }

    /// <summary>The number of controls.</summary>
    public int Count => _controls.Count;

    /// <summary>Gets the control at <paramref name="index"/>.</summary>
    public Control this[int index] => _controls[index];

    /// <inheritdoc />
    public IEnumerator<Control> GetEnumerator() => _controls.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _controls.GetEnumerator();
}
