using System;

namespace Nodely;

/// <summary>
/// An interaction state machine driven by the diagram's input events. Concrete behaviors (selection,
/// drag, pan, zoom, …) are added in Phase 3.
/// </summary>
public abstract class Behavior : IDisposable
{
    /// <summary>Creates a behavior bound to <paramref name="diagram"/>.</summary>
    protected Behavior(Diagram diagram) => Diagram = diagram;

    /// <summary>The owning diagram.</summary>
    protected Diagram Diagram { get; }

    /// <inheritdoc />
    public abstract void Dispose();
}
