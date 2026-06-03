using System;
using System.Collections.Generic;

namespace Nodely.Models.Base;

/// <summary>The root of every diagram model. Carries an identity, lock/visibility state, and change notification.</summary>
public abstract class Model
{
    private bool _visible = true;
    private Dictionary<string, object?>? _data;

    /// <summary>Creates a model with a generated id.</summary>
    protected Model() : this(Guid.NewGuid().ToString()) { }

    /// <summary>Creates a model with the given id.</summary>
    protected Model(string id) => Id = id;

    /// <summary>Raised when <see cref="Refresh"/> is called (the model wants to be re-rendered).</summary>
    public event Action<Model>? Changed;

    /// <summary>Raised when <see cref="Visible"/> changes.</summary>
    public event Action<Model>? VisibilityChanged;

    /// <summary>The stable identity of this model.</summary>
    public string Id { get; }

    /// <summary>When true, the model should not respond to user interaction.</summary>
    public bool Locked { get; set; }

    /// <summary>An arbitrary user value attached to this model. Nodely never reads it — it's yours.</summary>
    public object? Tag { get; set; }

    /// <summary>
    /// A free-form bag for attaching named data to a model without subclassing. Allocated lazily, so models
    /// that don't use it cost nothing.
    /// </summary>
    public IDictionary<string, object?> Data => _data ??= new Dictionary<string, object?>();

    /// <summary>Whether the model is rendered.</summary>
    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible == value)
                return;

            _visible = value;
            VisibilityChanged?.Invoke(this);
        }
    }

    /// <summary>Requests a re-render of this model by raising <see cref="Changed"/>.</summary>
    public virtual void Refresh() => Changed?.Invoke(this);
}
