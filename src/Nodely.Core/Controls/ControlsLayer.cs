using System;
using System.Collections.Generic;
using Nodely.Models.Base;

namespace Nodely.Controls;

/// <summary>Tracks the adornment controls attached to models. Control rendering arrives in Phase 8.</summary>
public class ControlsLayer
{
    private readonly Dictionary<Model, ControlsContainer> _containers = new();

    /// <summary>Raised when a change should cause a re-render, carrying the responsible model.</summary>
    public event Action<Model>? ChangeCaused;

    /// <summary>The models that currently have controls.</summary>
    public IReadOnlyCollection<Model> Models => _containers.Keys;

    /// <summary>Gets or creates the controls container for <paramref name="model"/>.</summary>
    public ControlsContainer AddFor(Model model, ControlsType type = ControlsType.OnSelection)
    {
        if (_containers.TryGetValue(model, out var existing))
            return existing;

        var container = new ControlsContainer(model, type);
        container.VisibilityChanged += OnVisibilityChanged;
        container.ControlsChanged += RefreshIfVisible;
        model.Changed += RefreshIfVisible;
        _containers.Add(model, container);
        return container;
    }

    /// <summary>Gets the controls container for <paramref name="model"/>, if any.</summary>
    public ControlsContainer? GetFor(Model model)
        => _containers.TryGetValue(model, out var container) ? container : null;

    /// <summary>Removes the controls for <paramref name="model"/>.</summary>
    public bool RemoveFor(Model model)
    {
        if (!_containers.TryGetValue(model, out var container))
            return false;

        container.VisibilityChanged -= OnVisibilityChanged;
        container.ControlsChanged -= RefreshIfVisible;
        model.Changed -= RefreshIfVisible;
        _containers.Remove(model);
        ChangeCaused?.Invoke(model);
        return true;
    }

    /// <summary>Whether controls are visible for <paramref name="model"/>.</summary>
    public bool AreVisibleFor(Model model) => GetFor(model)?.Visible ?? false;

    private void RefreshIfVisible(Model cause)
    {
        if (!AreVisibleFor(cause))
            return;

        ChangeCaused?.Invoke(cause);
    }

    private void OnVisibilityChanged(Model cause) => ChangeCaused?.Invoke(cause);
}
