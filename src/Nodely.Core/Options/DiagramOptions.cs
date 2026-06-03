using System;
using Nodely.Geometry;
using Nodely.Models.Base;

namespace Nodely.Options;

/// <summary>Top-level diagram configuration.</summary>
public class DiagramOptions
{
    /// <summary>The grid size in diagram units, or null to disable the grid/snap.</summary>
    public int? GridSize { get; set; }

    /// <summary>Whether snapping aligns a node's center (rather than its top-left) to the grid.</summary>
    public bool GridSnapToCenter { get; set; }

    /// <summary>An optional rule deciding whether a movable (node/vertex) may be dragged. Return false to lock it.</summary>
    public Func<MovableModel, bool>? CanDrag { get; set; }

    /// <summary>
    /// An optional function that snaps a dragged model's position (custom grids, guidelines, …). Applied after
    /// any grid snapping, just before the move is committed.
    /// </summary>
    public Func<Point, Point>? SnapPosition { get; set; }

    /// <summary>Whether multiple models can be selected at once.</summary>
    public bool AllowMultiSelection { get; set; } = true;

    /// <summary>Whether the canvas can be panned.</summary>
    public bool AllowPanning { get; set; } = true;

    /// <summary>Zoom configuration.</summary>
    public virtual DiagramZoomOptions Zoom { get; } = new();

    /// <summary>Link configuration (default router/path generator, snapping, factories).</summary>
    public virtual DiagramLinkOptions Links { get; } = new();

    /// <summary>Group configuration.</summary>
    public virtual DiagramGroupOptions Groups { get; } = new();

    /// <summary>Delete/edit constraints.</summary>
    public virtual DiagramConstraintsOptions Constraints { get; } = new();

    /// <summary>Virtualization configuration.</summary>
    public virtual DiagramVirtualizationOptions Virtualization { get; } = new();
}
