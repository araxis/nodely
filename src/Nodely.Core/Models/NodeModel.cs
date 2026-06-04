using System;
using System.Collections.Generic;
using System.Linq;
using Nodely.Geometry;
using Nodely.Models.Base;

namespace Nodely.Models;

/// <summary>A positioned, sized node on the canvas. Holds ports and tracks the links attached to it.</summary>
public class NodeModel : MovableModel, IHasBounds, IHasShape, ILinkable
{
    /// <summary>The stable serialization kind for a default node.</summary>
    public const string ModelKindKey = "node";

    private readonly List<PortModel> _ports = new();
    private readonly List<BaseLinkModel> _links = new();
    private Size? _size;

    /// <summary>Raised when <see cref="Size"/> changes.</summary>
    public event Action<NodeModel>? SizeChanged;

    /// <summary>Raised while the node is moving.</summary>
    public event Action<NodeModel>? Moving;

    /// <summary>Creates a node at the given position.</summary>
    public NodeModel(Point? position = null) : base(position) { }

    /// <summary>Creates a node with the given id at the given position.</summary>
    public NodeModel(string id, Point? position = null) : base(id, position) { }

    /// <summary>The measured size of the node (null until the UI measures it).</summary>
    public Size? Size
    {
        get => _size;
        set
        {
            if (value?.Equals(_size) == true)
                return;

            _size = value;
            SizeChanged?.Invoke(this);
        }
    }

    /// <summary>
    /// When true, the node's size is controlled by code (e.g. interactive resize) rather than measured
    /// from its content. The UI then arranges the node at its <see cref="Size"/> instead of overwriting it.
    /// </summary>
    public bool ControlledSize { get; set; }

    /// <summary>The group this node belongs to, if any.</summary>
    public GroupModel? Group { get; internal set; }

    /// <summary>An optional title.</summary>
    public string? Title { get; set; }

    /// <summary>The node's ports.</summary>
    public IReadOnlyList<PortModel> Ports => _ports;

    /// <summary>Links attached directly to the node (port-less links).</summary>
    public IReadOnlyList<BaseLinkModel> Links => _links;

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;

    /// <summary>All links attached through the node's ports.</summary>
    public IEnumerable<BaseLinkModel> PortLinks => Ports.SelectMany(p => p.Links);

    /// <summary>Adds an existing port.</summary>
    public PortModel AddPort(PortModel port)
    {
        _ports.Add(port);
        return port;
    }

    /// <summary>Adds a new port with the given alignment.</summary>
    public PortModel AddPort(PortAlignment alignment = PortAlignment.Bottom)
        => AddPort(new PortModel(this, alignment, Position));

    /// <summary>Gets the first port with the given alignment.</summary>
    public PortModel? GetPort(PortAlignment alignment) => Ports.FirstOrDefault(p => p.Alignment == alignment);

    /// <summary>Gets the first port of type <typeparamref name="T"/> with the given alignment.</summary>
    public T? GetPort<T>(PortAlignment alignment) where T : PortModel => (T?)GetPort(alignment);

    /// <summary>Removes a port.</summary>
    public bool RemovePort(PortModel port) => _ports.Remove(port);

    /// <summary>Refreshes the node and all its ports.</summary>
    public void RefreshAll()
    {
        Refresh();
        _ports.ForEach(p => p.RefreshAll());
    }

    /// <summary>Refreshes every link attached to this node.</summary>
    public void RefreshLinks()
    {
        foreach (var link in Links)
            link.Refresh();
    }

    /// <summary>Marks all ports as uninitialized and refreshes them (forces re-measure).</summary>
    public void ReinitializePorts()
    {
        foreach (var port in Ports)
        {
            port.Initialized = false;
            port.Refresh();
        }
    }

    /// <inheritdoc />
    public override void SetPosition(double x, double y)
    {
        var deltaX = x - Position.X;
        var deltaY = y - Position.Y;
        base.SetPosition(x, y);

        UpdatePortPositions(deltaX, deltaY);
        Refresh();
        RefreshLinks();
        Moving?.Invoke(this);
    }

    /// <summary>Moves the node by a delta without raising <see cref="MovableModel.Moved"/>.</summary>
    public virtual void UpdatePositionSilently(double deltaX, double deltaY)
    {
        base.SetPosition(Position.X + deltaX, Position.Y + deltaY);
        UpdatePortPositions(deltaX, deltaY);
        Refresh();
    }

    /// <summary>The node's bounds (excluding ports).</summary>
    public Rectangle? GetBounds() => GetBounds(false);

    /// <summary>The node's bounds, optionally expanded to include ports.</summary>
    public Rectangle? GetBounds(bool includePorts)
    {
        if (Size == null)
            return null;

        if (!includePorts)
            return new Rectangle(Position, Size);

        var leftPort = GetPort(PortAlignment.Left);
        var topPort = GetPort(PortAlignment.Top);
        var rightPort = GetPort(PortAlignment.Right);
        var bottomPort = GetPort(PortAlignment.Bottom);

        var left = leftPort == null ? Position.X : Math.Min(Position.X, leftPort.Position.X);
        var top = topPort == null ? Position.Y : Math.Min(Position.Y, topPort.Position.Y);
        var right = rightPort == null
            ? Position.X + Size!.Width
            : Math.Max(rightPort.Position.X + rightPort.Size.Width, Position.X + Size!.Width);
        var bottom = bottomPort == null
            ? Position.Y + Size!.Height
            : Math.Max(bottomPort.Position.Y + bottomPort.Size.Height, Position.Y + Size!.Height);

        return new Rectangle(left, top, right, bottom);
    }

    /// <summary>The shape links use to attach to this node (a rectangle by default).</summary>
    public virtual IShape GetShape() => Shapes.Rectangle(this);

    /// <summary>
    /// Creates a copy of this node (its own data only — no ports or links). Override in a subclass to copy its
    /// extra data so duplicate/paste preserves it (see the demo's <c>TaskNode</c>).
    /// </summary>
    public virtual NodeModel Clone() => new(Position)
    {
        Title = Title,
        Size = Size,
    };

    /// <inheritdoc />
    public virtual bool CanAttachTo(ILinkable other) => other is not PortModel && other is not BaseLinkModel;

    private void UpdatePortPositions(double deltaX, double deltaY)
    {
        foreach (var port in _ports)
        {
            port.Position = new Point(port.Position.X + deltaX, port.Position.Y + deltaY);
            port.RefreshLinks();
        }
    }

    /// <summary>Raises <see cref="Moving"/>.</summary>
    protected void TriggerMoving() => Moving?.Invoke(this);

    void ILinkable.AddLink(BaseLinkModel link) => _links.Add(link);

    void ILinkable.RemoveLink(BaseLinkModel link) => _links.Remove(link);
}
