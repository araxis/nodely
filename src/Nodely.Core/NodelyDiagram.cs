using Nodely.Behaviors;
using Nodely.Options;

namespace Nodely;

/// <summary>
/// The default concrete diagram. Supplies <see cref="DiagramOptions"/> and registers the default
/// interaction behaviors (selection, drag, link-drag, pan, zoom, click synthesis, keyboard shortcuts,
/// virtualization). The registration order matters — e.g. selection runs before drag so a pointer-down
/// selects a node before the drag captures the selection.
/// </summary>
public class NodelyDiagram : Diagram
{
    private readonly DiagramOptions _options;

    /// <summary>Creates a diagram with the given options, optionally without the default behaviors.</summary>
    public NodelyDiagram(DiagramOptions? options = null, bool registerDefaultBehaviors = true)
    {
        _options = options ?? new DiagramOptions();

        if (!registerDefaultBehaviors)
            return;

        RegisterBehavior(new SelectionBehavior(this));
        RegisterBehavior(new DragMovablesBehavior(this));
        RegisterBehavior(new DragNewLinkBehavior(this));
        RegisterBehavior(new PanBehavior(this));
        RegisterBehavior(new ZoomBehavior(this));
        RegisterBehavior(new EventsBehavior(this));
        RegisterBehavior(new KeyboardShortcutsBehavior(this));
        RegisterBehavior(new VirtualizationBehavior(this));
    }

    /// <inheritdoc />
    public override DiagramOptions Options => _options;
}
