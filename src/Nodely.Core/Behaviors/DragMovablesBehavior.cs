using System;
using System.Collections.Generic;
using Nodely.Events;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Behaviors;

/// <summary>Moves the selected movable models as the pointer drags, with optional grid snapping.</summary>
public class DragMovablesBehavior : Behavior
{
    private readonly Dictionary<MovableModel, Point> _initialPositions = new();
    private double? _lastClientX;
    private double? _lastClientY;
    private bool _moved;

    /// <summary>Creates and wires the behavior.</summary>
    public DragMovablesBehavior(Diagram diagram) : base(diagram)
    {
        Diagram.PointerDown += OnPointerDown;
        Diagram.PointerMove += OnPointerMove;
        Diagram.PointerUp += OnPointerUp;
    }

    private void OnPointerDown(Model? model, PointerEvent e)
    {
        if (model is not MovableModel)
            return;

        _initialPositions.Clear();
        foreach (var sm in Diagram.GetSelectedModels())
        {
            if (sm is not MovableModel movable || movable.Locked)
                continue;

            if (Diagram.Options.CanDrag?.Invoke(movable) == false)
                continue;

            // Special case: a node in a non-auto-size group is moved with the group, not on its own.
            if (sm is NodeModel node && node.Group != null && !node.Group.AutoSize)
                continue;

            var position = movable.Position;
            if (Diagram.Options.GridSnapToCenter && movable is NodeModel n)
            {
                position = new Point(movable.Position.X + (n.Size?.Width ?? 0) / 2,
                    movable.Position.Y + (n.Size?.Height ?? 0) / 2);
            }

            _initialPositions.Add(movable, position);
        }

        _lastClientX = e.X;
        _lastClientY = e.Y;
        _moved = false;
    }

    private void OnPointerMove(Model? model, PointerEvent e)
    {
        if (_initialPositions.Count == 0 || _lastClientX == null || _lastClientY == null)
            return;

        _moved = true;
        var deltaX = (e.X - _lastClientX.Value) / Diagram.Zoom;
        var deltaY = (e.Y - _lastClientY.Value) / Diagram.Zoom;

        foreach (var kvp in _initialPositions)
        {
            var movable = kvp.Key;
            var initialPosition = kvp.Value;
            var ndx = ApplyGridSize(deltaX + initialPosition.X);
            var ndy = ApplyGridSize(deltaY + initialPosition.Y);

            double targetX, targetY;
            if (Diagram.Options.GridSnapToCenter && movable is NodeModel node)
            {
                targetX = ndx - (node.Size?.Width ?? 0) / 2;
                targetY = ndy - (node.Size?.Height ?? 0) / 2;
            }
            else
            {
                targetX = ndx;
                targetY = ndy;
            }

            if (Diagram.Options.SnapPosition is { } snap)
            {
                var snapped = snap(new Point(targetX, targetY));
                targetX = snapped.X;
                targetY = snapped.Y;
            }

            movable.SetPosition(targetX, targetY);
        }
    }

    private void OnPointerUp(Model? model, PointerEvent e)
    {
        if (_initialPositions.Count == 0)
            return;

        if (_moved)
        {
            foreach (var kvp in _initialPositions)
                kvp.Key.TriggerMoved();
        }

        _initialPositions.Clear();
        _lastClientX = null;
        _lastClientY = null;
    }

    private double ApplyGridSize(double n)
    {
        if (Diagram.Options.GridSize == null)
            return n;

        var gridSize = Diagram.Options.GridSize.Value;
        return gridSize * Math.Floor((n + gridSize / 2.0) / gridSize);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _initialPositions.Clear();
        Diagram.PointerDown -= OnPointerDown;
        Diagram.PointerMove -= OnPointerMove;
        Diagram.PointerUp -= OnPointerUp;
    }
}
