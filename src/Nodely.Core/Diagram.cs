using System;
using System.Collections.Generic;
using System.Linq;
using Nodely.Controls;
using Nodely.Events;
using Nodely.Extensions;
using Nodely.Geometry;
using Nodely.Layers;
using Nodely.Models.Base;
using Nodely.Options;

namespace Nodely;

/// <summary>
/// The diagram engine: owns the node/link/group/controls layers, the view state (pan/zoom/container),
/// selection, z-ordering, the behavior registry, and the framework-neutral input seam
/// (<c>Trigger*</c>). It is UI-agnostic; the Avalonia layer drives it. Concrete diagrams supply
/// <see cref="Options"/> and register behaviors (Phase 3).
/// </summary>
public abstract class Diagram : IModelBatcher
{
    private readonly Dictionary<Type, Behavior> _behaviors = new();
    private readonly List<SelectableModel> _orderedSelectables = new();

    /// <summary>Raised on pointer down over a model (or null for the empty canvas).</summary>
    public event Action<Model?, PointerEvent>? PointerDown;

    /// <summary>Raised on pointer move.</summary>
    public event Action<Model?, PointerEvent>? PointerMove;

    /// <summary>Raised on pointer up.</summary>
    public event Action<Model?, PointerEvent>? PointerUp;

    /// <summary>Raised when the pointer enters a model.</summary>
    public event Action<Model?, PointerEvent>? PointerEnter;

    /// <summary>Raised when the pointer leaves a model.</summary>
    public event Action<Model?, PointerEvent>? PointerLeave;

    /// <summary>Raised on a key press.</summary>
    public event Action<KeyboardEvent>? KeyDown;

    /// <summary>Raised on a wheel/scroll event.</summary>
    public event Action<WheelEvent>? Wheel;

    /// <summary>Raised on a click.</summary>
    public event Action<Model?, PointerEvent>? PointerClick;

    /// <summary>Raised on a double-click.</summary>
    public event Action<Model?, PointerEvent>? PointerDoubleClick;

    /// <summary>Raised when the selection changes (carrying the affected model).</summary>
    public event Action<SelectableModel>? SelectionChanged;

    /// <summary>Raised when the pan changes.</summary>
    public event Action? PanChanged;

    /// <summary>Raised when the zoom changes.</summary>
    public event Action? ZoomChanged;

    /// <summary>Raised when the container rectangle changes.</summary>
    public event Action? ContainerChanged;

    /// <summary>Raised when the diagram wants to be re-rendered.</summary>
    public event Action? Changed;

    /// <summary>Creates the diagram and its layers.</summary>
    protected Diagram()
    {
        Nodes = new NodeLayer(this);
        Links = new LinkLayer(this);
        Groups = new GroupLayer(this);
        Controls = new ControlsLayer();

        Nodes.Added += OnSelectableAdded;
        Links.Added += OnSelectableAdded;
        Groups.Added += OnSelectableAdded;

        Nodes.Removed += OnSelectableRemoved;
        Links.Removed += OnSelectableRemoved;
        Groups.Removed += OnSelectableRemoved;
    }

    /// <summary>The diagram options (supplied by the concrete diagram).</summary>
    public abstract DiagramOptions Options { get; }

    /// <summary>The node layer.</summary>
    public NodeLayer Nodes { get; }

    /// <summary>The link layer.</summary>
    public LinkLayer Links { get; }

    /// <summary>The group layer.</summary>
    public GroupLayer Groups { get; }

    /// <summary>The controls (adornments) layer.</summary>
    public ControlsLayer Controls { get; }

    /// <summary>The viewport rectangle in screen space (set by the UI), or null before first layout.</summary>
    public Rectangle? Container { get; private set; }

    /// <summary>The current pan offset.</summary>
    public Point Pan { get; private set; } = Point.Zero;

    /// <summary>The current zoom factor.</summary>
    public double Zoom { get; private set; } = 1;

    /// <summary>When true, <see cref="Refresh"/> is suppressed (used during batching).</summary>
    public bool SuspendRefresh { get; set; }

    /// <summary>When true, order changes don't trigger a re-sort.</summary>
    public bool SuspendSorting { get; set; }

    /// <summary>The selectables in z-order.</summary>
    public IReadOnlyList<SelectableModel> OrderedSelectables => _orderedSelectables;

    /// <summary>Requests a re-render (unless suspended).</summary>
    public void Refresh()
    {
        if (SuspendRefresh)
            return;

        Changed?.Invoke();
    }

    /// <summary>Runs <paramref name="action"/> with refresh suspended, then refreshes once.</summary>
    public void Batch(Action action)
    {
        if (SuspendRefresh)
        {
            // Already suspended — likely an outer batch owns the refresh.
            action();
            return;
        }

        SuspendRefresh = true;
        action();
        SuspendRefresh = false;
        Refresh();
    }

    #region Selection

    /// <summary>Enumerates the currently selected models (nodes, links, vertices, groups).</summary>
    public IEnumerable<SelectableModel> GetSelectedModels()
    {
        foreach (var node in Nodes)
            if (node.Selected)
                yield return node;

        foreach (var link in Links)
        {
            if (link.Selected)
                yield return link;

            foreach (var vertex in link.Vertices)
                if (vertex.Selected)
                    yield return vertex;
        }

        foreach (var group in Groups)
            if (group.Selected)
                yield return group;
    }

    /// <summary>Selects a model, optionally clearing the rest of the selection first.</summary>
    public void SelectModel(SelectableModel model, bool unselectOthers)
    {
        if (model.Selected)
            return;

        if (unselectOthers)
            UnselectAll();

        model.Selected = true;
        model.Refresh();
        SelectionChanged?.Invoke(model);
    }

    /// <summary>Unselects a model.</summary>
    public void UnselectModel(SelectableModel model)
    {
        if (!model.Selected)
            return;

        model.Selected = false;
        model.Refresh();
        SelectionChanged?.Invoke(model);
    }

    /// <summary>Clears the selection.</summary>
    public void UnselectAll()
    {
        foreach (var model in GetSelectedModels())
        {
            model.Selected = false;
            model.Refresh();
            SelectionChanged?.Invoke(model);
        }
    }

    /// <summary>Selects every node, link, and group.</summary>
    public void SelectAll()
    {
        Batch(() =>
        {
            foreach (var node in Nodes)
                SelectModel(node, false);
            foreach (var link in Links)
                SelectModel(link, false);
            foreach (var group in Groups)
                SelectModel(group, false);
        });
    }

    #endregion

    #region Behaviors

    /// <summary>Registers an interaction behavior (one instance per type).</summary>
    public void RegisterBehavior(Behavior behavior)
    {
        var type = behavior.GetType();
        if (_behaviors.ContainsKey(type))
            throw new NodelyException($"Behavior '{type.Name}' already registered");

        _behaviors.Add(type, behavior);
    }

    /// <summary>Gets a registered behavior of type <typeparamref name="T"/>, or null.</summary>
    public T? GetBehavior<T>() where T : Behavior
        => _behaviors.TryGetValue(typeof(T), out var behavior) ? (T)behavior : null;

    /// <summary>Unregisters and disposes the behavior of type <typeparamref name="T"/>.</summary>
    public void UnregisterBehavior<T>() where T : Behavior
    {
        var type = typeof(T);
        if (!_behaviors.TryGetValue(type, out var behavior))
            return;

        behavior.Dispose();
        _behaviors.Remove(type);
    }

    #endregion

    #region View

    /// <summary>Zooms and pans so the (selected, or all) nodes fit the container with a margin.</summary>
    public void ZoomToFit(double margin = 10)
    {
        if (Container == null || Nodes.Count == 0)
            return;

        var selectedNodes = Nodes.Where(s => s.Selected).ToList();
        var nodesToUse = selectedNodes.Any() ? selectedNodes : Nodes.ToList();
        var bounds = nodesToUse.GetBounds();
        var width = bounds.Width + 2 * margin;
        var height = bounds.Height + 2 * margin;
        var minX = bounds.Left - margin;
        var minY = bounds.Top - margin;

        SuspendRefresh = true;

        var xf = Container.Width / width;
        var yf = Container.Height / height;
        SetZoom(Math.Min(xf, yf));

        var nx = Container.Left + Pan.X + minX * Zoom;
        var ny = Container.Top + Pan.Y + minY * Zoom;
        UpdatePan(Container.Left - nx, Container.Top - ny);

        SuspendRefresh = false;
        Refresh();
    }

    /// <summary>Sets the pan to an absolute offset.</summary>
    public void SetPan(double x, double y)
    {
        Pan = new Point(x, y);
        PanChanged?.Invoke();
        Refresh();
    }

    /// <summary>Adjusts the pan by a delta.</summary>
    public void UpdatePan(double deltaX, double deltaY)
    {
        Pan = Pan.Add(deltaX, deltaY);
        PanChanged?.Invoke();
        Refresh();
    }

    /// <summary>Sets the zoom (clamped to the configured minimum; must be &gt; 0).</summary>
    public void SetZoom(double newZoom)
    {
        if (newZoom <= 0)
            throw new ArgumentException($"{nameof(newZoom)} cannot be equal to or lower than 0");

        if (newZoom < Options.Zoom.Minimum)
            newZoom = Options.Zoom.Minimum;

        Zoom = newZoom;
        ZoomChanged?.Invoke();
        Refresh();
    }

    /// <summary>Sets the container rectangle (the viewport in screen space).</summary>
    public void SetContainer(Rectangle newRect)
    {
        if (newRect.Equals(Container))
            return;

        Container = newRect;
        ContainerChanged?.Invoke();
        Refresh();
    }

    /// <summary>Converts a screen point to diagram coordinates (accounting for container, pan, and zoom).</summary>
    public Point GetRelativeMousePoint(double clientX, double clientY)
    {
        if (Container == null)
            throw new NodelyException("Container not available. Don't call this before the diagram has been laid out.");

        return new Point((clientX - Container.Left - Pan.X) / Zoom, (clientY - Container.Top - Pan.Y) / Zoom);
    }

    /// <summary>Converts a screen point to a container-relative point (no pan/zoom).</summary>
    public Point GetRelativePoint(double clientX, double clientY)
    {
        if (Container == null)
            throw new NodelyException("Container not available. Don't call this before the diagram has been laid out.");

        return new Point(clientX - Container.Left, clientY - Container.Top);
    }

    /// <summary>Converts a diagram point to a screen point.</summary>
    public Point GetScreenPoint(double clientX, double clientY)
    {
        if (Container == null)
            throw new NodelyException("Container not available. Don't call this before the diagram has been laid out.");

        return new Point(Zoom * clientX + Container.Left + Pan.X, Zoom * clientY + Container.Top + Pan.Y);
    }

    #endregion

    #region Ordering

    /// <summary>Sends a model to the back of the z-order.</summary>
    public void SendToBack(SelectableModel model)
    {
        var minOrder = GetMinOrder();
        if (model.Order == minOrder)
            return;

        if (!_orderedSelectables.Remove(model))
            return;

        _orderedSelectables.Insert(0, model);

        Batch(() =>
        {
            SuspendSorting = true;
            for (var i = 0; i < _orderedSelectables.Count; i++)
                _orderedSelectables[i].Order = i + 1;
            SuspendSorting = false;
        });
    }

    /// <summary>Brings a model to the front of the z-order.</summary>
    public void SendToFront(SelectableModel model)
    {
        var maxOrder = GetMaxOrder();
        if (model.Order == maxOrder)
            return;

        if (!_orderedSelectables.Remove(model))
            return;

        _orderedSelectables.Add(model);

        SuspendSorting = true;
        model.Order = maxOrder + 1;
        SuspendSorting = false;
        Refresh();
    }

    /// <summary>The lowest current order value.</summary>
    public int GetMinOrder() => _orderedSelectables.Count > 0 ? _orderedSelectables[0].Order : 0;

    /// <summary>The highest current order value.</summary>
    public int GetMaxOrder() => _orderedSelectables.Count > 0 ? _orderedSelectables[_orderedSelectables.Count - 1].Order : 0;

    /// <summary>Re-sorts the selectables by their order.</summary>
    public void RefreshOrders(bool refresh = true)
    {
        _orderedSelectables.Sort((a, b) => a.Order.CompareTo(b.Order));

        if (refresh)
            Refresh();
    }

    private void OnSelectableAdded(SelectableModel model)
    {
        var maxOrder = GetMaxOrder();
        _orderedSelectables.Add(model);

        if (model.Order == 0)
            model.Order = maxOrder + 1;

        model.OrderChanged += OnModelOrderChanged;
    }

    private void OnSelectableRemoved(SelectableModel model)
    {
        model.OrderChanged -= OnModelOrderChanged;
        _orderedSelectables.Remove(model);
    }

    private void OnModelOrderChanged(SelectableModel model)
    {
        if (SuspendSorting)
            return;

        RefreshOrders();
    }

    #endregion

    #region Input seam

    /// <summary>Feeds a pointer-down event into the diagram.</summary>
    public void TriggerPointerDown(Model? model, PointerEvent e) => PointerDown?.Invoke(model, e);

    /// <summary>Feeds a pointer-move event into the diagram.</summary>
    public void TriggerPointerMove(Model? model, PointerEvent e) => PointerMove?.Invoke(model, e);

    /// <summary>Feeds a pointer-up event into the diagram.</summary>
    public void TriggerPointerUp(Model? model, PointerEvent e) => PointerUp?.Invoke(model, e);

    /// <summary>Feeds a pointer-enter event into the diagram.</summary>
    public void TriggerPointerEnter(Model? model, PointerEvent e) => PointerEnter?.Invoke(model, e);

    /// <summary>Feeds a pointer-leave event into the diagram.</summary>
    public void TriggerPointerLeave(Model? model, PointerEvent e) => PointerLeave?.Invoke(model, e);

    /// <summary>Feeds a key-down event into the diagram.</summary>
    public void TriggerKeyDown(KeyboardEvent e) => KeyDown?.Invoke(e);

    /// <summary>Feeds a wheel event into the diagram.</summary>
    public void TriggerWheel(WheelEvent e) => Wheel?.Invoke(e);

    /// <summary>Feeds a click event into the diagram.</summary>
    public void TriggerPointerClick(Model? model, PointerEvent e) => PointerClick?.Invoke(model, e);

    /// <summary>Feeds a double-click event into the diagram.</summary>
    public void TriggerPointerDoubleClick(Model? model, PointerEvent e) => PointerDoubleClick?.Invoke(model, e);

    #endregion
}
