using System.Collections.Generic;
using System.Linq;
using Nodely.Extensions;
using Nodely.Geometry;

namespace Nodely.Models;

/// <summary>A node that contains other nodes; its bounds auto-fit its children and moving it moves them.</summary>
public class GroupModel : NodeModel
{
    /// <summary>The stable serialization kind for a default group.</summary>
    public new const string ModelKindKey = "group";

    private readonly List<NodeModel> _children;

    /// <summary>Creates a group around the given children.</summary>
    public GroupModel(IEnumerable<NodeModel> children, byte padding = 30, bool autoSize = true)
    {
        _children = new List<NodeModel>();

        Size = Size.Zero;
        Padding = padding;
        AutoSize = autoSize;
        Initialize(children);
    }

    /// <summary>Creates a group with the given id around the given children (used when loading snapshots).</summary>
    public GroupModel(string id, IEnumerable<NodeModel> children, byte padding = 30, bool autoSize = true) : base(id)
    {
        _children = new List<NodeModel>();

        Size = Size.Zero;
        Padding = padding;
        AutoSize = autoSize;
        Initialize(children);
    }

    /// <summary>The group's children.</summary>
    public IReadOnlyList<NodeModel> Children => _children;

    /// <summary>The padding kept between the children's bounds and the group edge.</summary>
    public byte Padding { get; }

    /// <summary>Whether the group resizes itself to fit its children.</summary>
    public bool AutoSize { get; }

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <summary>Adds a child to the group.</summary>
    public void AddChild(NodeModel child)
    {
        _children.Add(child);
        child.Group = this;
        child.SizeChanged += OnNodeChanged;
        child.Moving += OnNodeChanged;

        if (UpdateDimensions())
            Refresh();
    }

    /// <summary>Removes a child from the group.</summary>
    public void RemoveChild(NodeModel child)
    {
        if (!_children.Remove(child))
            return;

        child.Group = null;
        child.SizeChanged -= OnNodeChanged;
        child.Moving -= OnNodeChanged;

        if (UpdateDimensions())
        {
            Refresh();
            RefreshLinks();
        }
    }

    /// <inheritdoc />
    public override void SetPosition(double x, double y)
    {
        var deltaX = x - Position.X;
        var deltaY = y - Position.Y;
        base.SetPosition(x, y);

        foreach (var node in Children)
        {
            node.UpdatePositionSilently(deltaX, deltaY);
            node.RefreshLinks();
        }

        Refresh();
        RefreshLinks();
    }

    /// <inheritdoc />
    public override void UpdatePositionSilently(double deltaX, double deltaY)
    {
        base.UpdatePositionSilently(deltaX, deltaY);

        foreach (var child in Children)
            child.UpdatePositionSilently(deltaX, deltaY);

        Refresh();
    }

    /// <summary>Detaches all children without deleting them.</summary>
    public void Ungroup()
    {
        foreach (var child in Children)
        {
            child.Group = null;
            child.SizeChanged -= OnNodeChanged;
            child.Moving -= OnNodeChanged;
        }

        _children.Clear();
    }

    private void Initialize(IEnumerable<NodeModel> children)
    {
        foreach (var child in children)
        {
            _children.Add(child);
            child.Group = this;
            child.SizeChanged += OnNodeChanged;
            child.Moving += OnNodeChanged;
        }

        UpdateDimensions();
    }

    private void OnNodeChanged(NodeModel node)
    {
        if (UpdateDimensions())
            Refresh();
    }

    /// <summary>Recomputes the group's position/size from its children. Returns false if not ready.</summary>
    protected virtual bool UpdateDimensions()
    {
        if (Children.Count == 0)
            return true;

        if (Children.Any(n => n.Size == null))
            return false;

        var bounds = Children.GetBounds();

        var newPosition = new Point(bounds.Left - Padding, bounds.Top - Padding);
        if (!Position.Equals(newPosition))
        {
            Position = newPosition;
            TriggerMoving();
        }

        Size = new Size(bounds.Width + Padding * 2, bounds.Height + Padding * 2);
        return true;
    }
}
