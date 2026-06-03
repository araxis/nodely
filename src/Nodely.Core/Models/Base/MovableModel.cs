using System;
using Nodely.Geometry;

namespace Nodely.Models.Base;

/// <summary>
/// A selectable model that has a position and can be moved (nodes and groups).
/// Movable models are selectable: interacting to move one also selects it.
/// </summary>
public abstract class MovableModel : SelectableModel
{
    /// <summary>Raised when the model has been moved (after its position settles).</summary>
    public event Action<MovableModel>? Moved;

    /// <summary>Creates a movable model at <paramref name="position"/> (defaults to the origin).</summary>
    protected MovableModel(Point? position = null) => Position = position ?? Point.Zero;

    /// <summary>Creates a movable model with the given id at <paramref name="position"/>.</summary>
    protected MovableModel(string id, Point? position = null) : base(id) => Position = position ?? Point.Zero;

    /// <summary>The top-left position of the model in diagram space.</summary>
    public Point Position { get; set; }

    /// <summary>Sets the position to the given coordinates.</summary>
    public virtual void SetPosition(double x, double y) => Position = new Point(x, y);

    /// <summary>Raises <see cref="Moved"/>. Only call this if you know what you're doing.</summary>
    public void TriggerMoved() => Moved?.Invoke(this);
}
