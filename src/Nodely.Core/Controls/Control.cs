using Nodely.Geometry;
using Nodely.Models.Base;

namespace Nodely.Controls;

/// <summary>An adornment attached to a model (resize handle, delete button, …). Rendering lands in Phase 8.</summary>
public abstract class Control
{
    /// <summary>The position of the control for the given model, or null to hide it.</summary>
    public abstract Point? GetPosition(Model model);
}
