using System;
using System.Collections;
using System.Collections.Generic;
using Nodely.Models.Base;

namespace Nodely;

/// <summary>
/// An ordered, observable collection of models of a single kind (the base for the diagram's node, link,
/// and group layers). Mutations are wrapped in the optional <see cref="IModelBatcher"/> so multiple
/// changes refresh once; with no batcher they apply immediately.
/// </summary>
/// <typeparam name="T">The model type held by this layer.</typeparam>
public class Layer<T> : IReadOnlyList<T> where T : Model
{
    private readonly List<T> _items = new();
    private readonly IModelBatcher? _batcher;

    /// <summary>Creates a layer, optionally driven by a batcher (supplied by the diagram in Phase 2).</summary>
    public Layer(IModelBatcher? batcher = null) => _batcher = batcher;

    /// <summary>Raised after a model is added.</summary>
    public event Action<T>? Added;

    /// <summary>Raised after a model is removed.</summary>
    public event Action<T>? Removed;

    /// <summary>Adds a model and returns it.</summary>
    public TSpecific Add<TSpecific>(TSpecific item) where TSpecific : T
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        Batch(() =>
        {
            _items.Add(item);
            OnItemAdded(item);
            Added?.Invoke(item);
        });
        return item;
    }

    /// <summary>Adds a batch of models.</summary>
    public void Add(IEnumerable<T> items)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        Batch(() =>
        {
            foreach (var item in items)
            {
                _items.Add(item);
                OnItemAdded(item);
                Added?.Invoke(item);
            }
        });
    }

    /// <summary>Removes a model if present.</summary>
    public void Remove(T item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (_items.Remove(item))
            Batch(() =>
            {
                OnItemRemoved(item);
                Removed?.Invoke(item);
            });
    }

    /// <summary>Removes a batch of models.</summary>
    public void Remove(IEnumerable<T> items)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        Batch(() =>
        {
            foreach (var item in items)
            {
                if (_items.Remove(item))
                {
                    OnItemRemoved(item);
                    Removed?.Invoke(item);
                }
            }
        });
    }

    /// <summary>True if the layer contains <paramref name="item"/>.</summary>
    public bool Contains(T item) => _items.Contains(item);

    /// <summary>Removes every model, raising <see cref="Removed"/> for each.</summary>
    public void Clear()
    {
        if (_items.Count == 0)
            return;

        Batch(() =>
        {
            for (var i = _items.Count - 1; i >= 0; i--)
            {
                var item = _items[i];
                _items.RemoveAt(i);
                OnItemRemoved(item);
                Removed?.Invoke(item);
            }
        });
    }

    /// <summary>Hook called when a model is added, before <see cref="Added"/>.</summary>
    protected virtual void OnItemAdded(T item) { }

    /// <summary>Hook called when a model is removed, before <see cref="Removed"/>.</summary>
    protected virtual void OnItemRemoved(T item) { }

    private void Batch(Action action)
    {
        if (_batcher is null)
            action();
        else
            _batcher.Batch(action);
    }

    /// <summary>The number of models in the layer.</summary>
    public int Count => _items.Count;

    /// <summary>Gets the model at <paramref name="index"/>.</summary>
    public T this[int index] => _items[index];

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
