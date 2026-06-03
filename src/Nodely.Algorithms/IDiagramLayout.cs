namespace Nodely.Algorithms;

/// <summary>
/// A pluggable diagram layout: repositions a diagram's nodes. Implement this to drop in any layout
/// (force-directed, tree, your own) uniformly — e.g. <c>canvas.RunAsUndoableMove(() =&gt; layout.Arrange(diagram))</c>.
/// </summary>
public interface IDiagramLayout
{
    /// <summary>Repositions the diagram's nodes in place.</summary>
    void Arrange(Diagram diagram);
}

/// <summary>An <see cref="IDiagramLayout"/> over the built-in <see cref="LayeredLayout"/>.</summary>
public sealed class LayeredDiagramLayout : IDiagramLayout
{
    private readonly LayeredLayoutOptions? _options;

    /// <summary>Creates the layout with optional tuning.</summary>
    public LayeredDiagramLayout(LayeredLayoutOptions? options = null) => _options = options;

    /// <inheritdoc />
    public void Arrange(Diagram diagram) => LayeredLayout.Arrange(diagram, _options);
}
