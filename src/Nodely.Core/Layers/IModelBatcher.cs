using System;

namespace Nodely;

/// <summary>
/// Coalesces a burst of model mutations into a single refresh. Implemented by the diagram (Phase 2);
/// a <see cref="Layer{T}"/> with no batcher simply runs the action immediately, which keeps the layer
/// unit-testable in isolation.
/// </summary>
public interface IModelBatcher
{
    /// <summary>Runs <paramref name="action"/>, suspending refresh until it completes.</summary>
    void Batch(Action action);
}
