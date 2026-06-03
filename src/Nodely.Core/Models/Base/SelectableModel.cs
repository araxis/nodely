using System;

namespace Nodely.Models.Base;

/// <summary>A model that can be selected and ordered (z-order) within the diagram.</summary>
public abstract class SelectableModel : Model
{
    private int _order;

    /// <summary>Raised when <see cref="Order"/> changes.</summary>
    public event Action<SelectableModel>? OrderChanged;

    /// <summary>Creates a selectable model with a generated id.</summary>
    protected SelectableModel() { }

    /// <summary>Creates a selectable model with the given id.</summary>
    protected SelectableModel(string id) : base(id) { }

    /// <summary>Whether the model is currently selected. Set by the diagram's selection API.</summary>
    public bool Selected { get; internal set; }

    /// <summary>The z-order of the model. Higher values render in front.</summary>
    public int Order
    {
        get => _order;
        set
        {
            if (value == _order)
                return;

            _order = value;
            OrderChanged?.Invoke(this);
        }
    }
}
