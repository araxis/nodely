using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Nodely.Anchors;
using Nodely.Behaviors;
using Nodely.Commands;
using Nodely.Events;
using Nodely.Models;
using Nodely.Models.Base;
using NodelyPoint = Nodely.Geometry.Point;
using NodelyRect = Nodely.Geometry.Rectangle;

namespace Nodely.Avalonia.Controls;

/// <summary>
/// The Nodely diagramming surface: a viewport bound to a <see cref="Diagram"/>. It hosts the rendering
/// layers (background grid + nodes; links/adorners in later phases), translates Avalonia pointer/wheel/key
/// input into framework-neutral events fed to the diagram's <c>Trigger*</c> seam (resolving the hit node so
/// input carries the right model), and reports its size to the diagram via <c>SetContainer</c>.
/// </summary>
public class DiagramCanvas : Panel
{
    /// <summary>Defines the <see cref="Diagram"/> property.</summary>
    public static readonly StyledProperty<Diagram?> DiagramProperty =
        AvaloniaProperty.Register<DiagramCanvas, Diagram?>(nameof(Diagram));

    /// <summary>Defines the <see cref="GridBrush"/> property.</summary>
    public static readonly StyledProperty<IBrush?> GridBrushProperty =
        AvaloniaProperty.Register<DiagramCanvas, IBrush?>(nameof(GridBrush));

    /// <summary>Defines the <see cref="GridSize"/> property.</summary>
    public static readonly StyledProperty<double> GridSizeProperty =
        AvaloniaProperty.Register<DiagramCanvas, double>(nameof(GridSize), 24d);

    /// <summary>Defines the <see cref="Palette"/> property.</summary>
    public static readonly StyledProperty<NodelyPalette> PaletteProperty =
        AvaloniaProperty.Register<DiagramCanvas, NodelyPalette>(nameof(Palette), NodelyPalettes.Dark);

    /// <summary>Defines the <see cref="IsReadOnly"/> property.</summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<DiagramCanvas, bool>(nameof(IsReadOnly));

    private readonly GridLayer _grid;
    private readonly GroupsLayer _groups;
    private readonly LinksLayer _links;
    private readonly NodesLayer _nodes;
    private readonly PortsLayer _ports;
    private readonly VerticesLayer _vertices;
    private readonly AdornersLayer _adorners;
    private readonly SelectionBoxLayer _selectionBox;
    private readonly Dictionary<Type, Func<NodeModel, Control>> _nodeFactories = new();
    private readonly Dictionary<Type, Func<PortModel, Control>> _portFactories = new();
    private readonly Dictionary<Type, Func<GroupModel, Control>> _groupFactories = new();
    private readonly Dictionary<Type, LinkDrawer> _linkDrawers = new();
    private readonly ContextMenu _contextMenu = new();
    private readonly List<NodeModel> _clipboard = new();
    private readonly List<Func<NodeModel, Control?>> _adornerFactories = new();
    private readonly List<(Control Layer, bool World)> _customLayers = new();
    private double _pasteOffset;
    private Diagram? _subscribed;
    private DiagramHistory? _history;
    private bool _marqueeing;
    private bool _inGesture;
    private Point _marqueeStart;

    /// <summary>Raised when the undo/redo state changes (so toolbars can refresh their enabled state).</summary>
    public event Action? HistoryChanged;

    /// <summary>Creates the canvas with sensible defaults.</summary>
    public DiagramCanvas()
    {
        // Layers are created before the property setters below (whose change handlers touch them).
        // Z-order (bottom to top): grid, groups, links, nodes, ports, vertices, adorners, selection box.
        _grid = new GridLayer(this);
        _groups = new GroupsLayer(this);
        _links = new LinksLayer(this);
        _nodes = new NodesLayer(this);
        _ports = new PortsLayer(this);
        _vertices = new VerticesLayer(this);
        _adorners = new AdornersLayer(this);
        _selectionBox = new SelectionBoxLayer();
        Children.Add(_grid);
        Children.Add(_groups);
        Children.Add(_links);
        Children.Add(_nodes);
        Children.Add(_ports);
        Children.Add(_vertices);
        Children.Add(_adorners);
        Children.Add(_selectionBox);

        Background = Palette.CanvasBackground;
        GridBrush = Palette.GridLine;
        Focusable = true;
        ClipToBounds = true;
        ContextMenu = _contextMenu;
        ContextRequested += OnContextRequested;
    }

    /// <summary>The diagram this canvas renders and drives.</summary>
    public Diagram? Diagram
    {
        get => GetValue(DiagramProperty);
        set => SetValue(DiagramProperty, value);
    }

    /// <summary>The brush used for the background grid lines.</summary>
    public IBrush? GridBrush
    {
        get => GetValue(GridBrushProperty);
        set => SetValue(GridBrushProperty, value);
    }

    /// <summary>The grid spacing in diagram units (0 or less hides the grid).</summary>
    public double GridSize
    {
        get => GetValue(GridSizeProperty);
        set => SetValue(GridSizeProperty, value);
    }

    /// <summary>The color palette (e.g. <see cref="NodelyPalettes.Dark"/> or <see cref="NodelyPalettes.Light"/>).</summary>
    public NodelyPalette Palette
    {
        get => GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    /// <summary>When true, the diagram is an inspector: pan/zoom/select work but moving, connecting, and deleting are blocked.</summary>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Registers how nodes of type <typeparamref name="TNode"/> are rendered. The most-derived registered
    /// type wins; unregistered node types fall back to a built-in default. This is the easy custom-node API.
    /// </summary>
    public void RegisterNode<TNode>(Func<TNode, Control> factory) where TNode : NodeModel
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        _nodeFactories[typeof(TNode)] = node => factory((TNode)node);
    }

    /// <summary>
    /// Registers a custom drawer for links of type <typeparamref name="TLink"/> (and subclasses). The drawer
    /// fully controls appearance; call <c>ctx.DrawDefault()</c> to render the standard link and augment it.
    /// </summary>
    public void RegisterLink<TLink>(LinkDrawer drawer) where TLink : BaseLinkModel
    {
        if (drawer is null)
            throw new ArgumentNullException(nameof(drawer));

        _linkDrawers[typeof(TLink)] = drawer;
        _links?.InvalidateVisual();
    }

    /// <summary>
    /// Optional per-link style override (stroke/width/dash). Cheaper than a full <see cref="RegisterLink{TLink}"/>
    /// drawer when you only want to restyle the standard link. Return null fields to keep the defaults.
    /// </summary>
    public Func<BaseLinkModel, LinkStyle>? LinkStyleResolver { get; set; }

    /// <summary>Resolves the most-derived registered link drawer for <paramref name="link"/>, or null.</summary>
    internal LinkDrawer? ResolveLinkDrawer(BaseLinkModel link)
    {
        for (var type = link.GetType(); type != null && typeof(BaseLinkModel).IsAssignableFrom(type); type = type.BaseType)
            if (_linkDrawers.TryGetValue(type, out var drawer))
                return drawer;

        return null;
    }

    /// <summary>Resolves the style overrides for <paramref name="link"/> (defaults when no resolver is set).</summary>
    internal LinkStyle ResolveLinkStyle(BaseLinkModel link) => LinkStyleResolver?.Invoke(link) ?? LinkStyle.Default;

    /// <summary>Builds the content control for a node (used by <see cref="NodeView"/>).</summary>
    internal Control BuildNodeContent(NodeModel node)
    {
        for (var type = node.GetType(); type != null && typeof(NodeModel).IsAssignableFrom(type); type = type.BaseType)
        {
            if (_nodeFactories.TryGetValue(type, out var factory))
                return factory(node);
        }

        return BuildDefaultNodeContent(node);
    }

    private Control BuildDefaultNodeContent(NodeModel node) => new Border
    {
        Background = Palette.NodeBackground,
        BorderBrush = Palette.NodeBorder,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(6),
        Padding = new Thickness(12, 8),
        Child = new TextBlock
        {
            Text = string.IsNullOrEmpty(node.Title) ? "Node" : node.Title,
            Foreground = Palette.NodeText,
        },
    };

    /// <summary>Registers how ports of type <typeparamref name="TPort"/> render (default: a dot).</summary>
    public void RegisterPort<TPort>(Func<TPort, Control> factory) where TPort : PortModel
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        _portFactories[typeof(TPort)] = port => factory((TPort)port);
        _ports?.Rebuild();
    }

    /// <summary>Registers how groups of type <typeparamref name="TGroup"/> render (default: a translucent box).</summary>
    public void RegisterGroup<TGroup>(Func<TGroup, Control> factory) where TGroup : GroupModel
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        _groupFactories[typeof(TGroup)] = group => factory((TGroup)group);
        _groups?.Rebuild();
    }

    /// <summary>Builds the content control for a port (used by <see cref="PortView"/>).</summary>
    internal Control BuildPortContent(PortModel port)
    {
        for (var type = port.GetType(); type != null && typeof(PortModel).IsAssignableFrom(type); type = type.BaseType)
            if (_portFactories.TryGetValue(type, out var factory))
                return factory(port);

        return new Ellipse { Fill = Palette.PortFill, Stroke = Palette.PortStroke, StrokeThickness = 1.5 };
    }

    /// <summary>Builds the content control for a group (used by <see cref="GroupView"/>).</summary>
    internal Control BuildGroupContent(GroupModel group)
    {
        for (var type = group.GetType(); type != null && typeof(GroupModel).IsAssignableFrom(type); type = type.BaseType)
            if (_groupFactories.TryGetValue(type, out var factory))
                return factory(group);

        return new Border
        {
            Background = Palette.GroupBackground,
            BorderBrush = Palette.GroupBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
        };
    }

    /// <summary>
    /// Registers a provider of screen-space adorners for selected nodes (selection toolbars, badges, custom
    /// handles, …). The returned control is anchored at the node's top-left corner; arrange it from there.
    /// Return null to skip a node.
    /// </summary>
    public void RegisterAdorner(Func<NodeModel, Control?> factory)
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        _adornerFactories.Add(factory);
        _adorners?.Refresh();
    }

    internal IEnumerable<Control> BuildAdorners(NodeModel node)
    {
        foreach (var factory in _adornerFactories)
        {
            var control = factory(node);
            if (control != null)
                yield return control;
        }
    }

    /// <summary>
    /// Adds a custom overlay layer (rulers, guides, heatmaps, annotations, …) above the content and below the
    /// adorners. World-space layers (the default) share the diagram's pan/zoom transform and draw in diagram
    /// coordinates; screen-space layers draw in viewport pixels. Subclass <see cref="DiagramLayer"/> for easy
    /// access to the diagram, or pass any <see cref="Control"/>.
    /// </summary>
    public void AddLayer(Control layer, bool worldSpace = true)
    {
        if (layer is null)
            throw new ArgumentNullException(nameof(layer));

        if (worldSpace)
        {
            layer.RenderTransformOrigin = RelativePoint.TopLeft;
            layer.ClipToBounds = false;
        }

        if (layer is DiagramLayer diagramLayer)
            diagramLayer.Attach(this);

        _customLayers.Add((layer, worldSpace));
        var index = Children.IndexOf(_adorners);
        Children.Insert(index < 0 ? Children.Count : index, layer);
        UpdateCustomLayer(layer, worldSpace);
    }

    /// <summary>Removes a layer added with <see cref="AddLayer"/>.</summary>
    public void RemoveLayer(Control layer)
    {
        _customLayers.RemoveAll(entry => ReferenceEquals(entry.Layer, layer));
        Children.Remove(layer);
    }

    private void UpdateCustomLayer(Control layer, bool world)
    {
        if (world)
        {
            var d = Diagram;
            layer.RenderTransform = d == null
                ? null
                : new TransformGroup { Children = { new ScaleTransform(d.Zoom, d.Zoom), new TranslateTransform(d.Pan.X, d.Pan.Y) } };
        }

        layer.InvalidateVisual();
    }

    /// <summary>Registers a custom interaction <see cref="Behavior"/> on the diagram (one per type).</summary>
    public void RegisterBehavior(Behavior behavior) => Diagram?.RegisterBehavior(behavior);

    /// <summary>Gets the registered behavior of type <typeparamref name="T"/>, or null.</summary>
    public T? GetBehavior<T>() where T : Behavior => Diagram?.GetBehavior<T>();

    /// <summary>Unregisters (and disposes) the behavior of type <typeparamref name="T"/>.</summary>
    public void UnregisterBehavior<T>() where T : Behavior => Diagram?.UnregisterBehavior<T>();

    /// <summary>Zooms and pans so the diagram's content fits the viewport.</summary>
    public void ZoomToFit(double margin = 20) => Diagram?.ZoomToFit(margin);

    /// <summary>Zooms in around the viewport center.</summary>
    public void ZoomIn() => ZoomAroundCenter(1.2);

    /// <summary>Zooms out around the viewport center.</summary>
    public void ZoomOut() => ZoomAroundCenter(1 / 1.2);

    /// <summary>Resets zoom to 1 and pan to the origin.</summary>
    public void ResetView()
    {
        var d = Diagram;
        if (d == null)
            return;

        d.Batch(() =>
        {
            d.SetZoom(1);
            d.SetPan(0, 0);
        });
    }

    /// <summary>Whether there's an edit to undo.</summary>
    public bool CanUndo => _history?.CanUndo ?? false;

    /// <summary>Whether there's an edit to redo.</summary>
    public bool CanRedo => _history?.CanRedo ?? false;

    /// <summary>Undoes the last edit (no-op when read-only).</summary>
    public void Undo()
    {
        if (!IsReadOnly)
            _history?.Undo();
    }

    /// <summary>Redoes the last undone edit (no-op when read-only).</summary>
    public void Redo()
    {
        if (!IsReadOnly)
            _history?.Redo();
    }

    /// <summary>
    /// Runs <paramref name="mutate"/> (which repositions nodes directly, e.g. auto-layout) and records the net
    /// position changes as a single undo step. Auto-layout moves via <c>SetPosition</c> don't raise per-node
    /// Moved events, so they must be captured explicitly.
    /// </summary>
    public void RunAsUndoableMove(Action mutate)
    {
        if (mutate == null)
            throw new ArgumentNullException(nameof(mutate));

        var d = Diagram;
        if (d == null || _history == null)
        {
            mutate();
            return;
        }

        var before = new Dictionary<NodeModel, NodelyPoint>();
        foreach (var node in d.Nodes)
            before[node] = node.Position;

        using (_history.Transaction())
        {
            mutate();
            foreach (var node in d.Nodes)
                if (before.TryGetValue(node, out var from) && (from.X != node.Position.X || from.Y != node.Position.Y))
                    _history.RecordApplied(new MoveNodeCommand(node, from, node.Position));
        }
    }

    /// <summary>Brings the selected models to the front of the z-order.</summary>
    public void BringSelectionToFront()
    {
        var d = Diagram;
        if (d == null)
            return;

        foreach (var model in new List<SelectableModel>(d.GetSelectedModels()))
            d.SendToFront(model);
    }

    /// <summary>Sends the selected models to the back of the z-order.</summary>
    public void SendSelectionToBack()
    {
        var d = Diagram;
        if (d == null)
            return;

        foreach (var model in new List<SelectableModel>(d.GetSelectedModels()))
            d.SendToBack(model);
    }

    /// <summary>Copies the selected nodes to the clipboard (as clones).</summary>
    public void CopySelection()
    {
        var d = Diagram;
        if (d == null)
            return;

        _clipboard.Clear();
        _pasteOffset = 0;
        foreach (var model in d.GetSelectedModels())
            if (model is NodeModel node)
                _clipboard.Add(node.Clone());
    }

    /// <summary>Copies the selection, then deletes it (undoable).</summary>
    public void CutSelection()
    {
        var d = Diagram;
        if (d == null || IsReadOnly)
            return;

        CopySelection();
        DeleteModels(new List<Model>(d.GetSelectedModels()));
    }

    /// <summary>Pastes the clipboard nodes at a growing offset, selecting them (one undo step).</summary>
    public void PasteClipboard()
    {
        var d = Diagram;
        if (d == null || IsReadOnly || _clipboard.Count == 0)
            return;

        _pasteOffset += 24;
        AddClones(d, _clipboard, _pasteOffset);
    }

    /// <summary>Duplicates the selected nodes in place at a small offset, selecting the copies (one undo step).</summary>
    public void DuplicateSelection()
    {
        var d = Diagram;
        if (d == null || IsReadOnly)
            return;

        var sources = new List<NodeModel>();
        foreach (var model in d.GetSelectedModels())
            if (model is NodeModel node)
                sources.Add(node);

        if (sources.Count > 0)
            AddClones(d, sources, 24);
    }

    // Clones each source node at +offset, adds them in one history transaction, and selects only the copies.
    // Links between the sources aren't copied (nodes only).
    private void AddClones(Diagram d, List<NodeModel> sources, double offset)
    {
        var clones = new List<NodeModel>(sources.Count);
        foreach (var source in sources)
        {
            var clone = source.Clone();
            clone.SetPosition(source.Position.X + offset, source.Position.Y + offset);
            clones.Add(clone);
        }

        using (_history?.Transaction())
        {
            d.Batch(() =>
            {
                d.UnselectAll();
                foreach (var clone in clones)
                {
                    d.Nodes.Add(clone);
                    d.SelectModel(clone, unselectOthers: false);
                }
            });
        }
    }

    private void OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        var d = Diagram;
        if (d == null)
        {
            e.Handled = true;
            return;
        }

        // Select the model under the cursor so the menu acts on it.
        if (e.TryGetPosition(this, out var pos))
        {
            var model = ResolveModelAt(pos);
            if (model is SelectableModel sm && !sm.Selected)
                d.SelectModel(sm, unselectOthers: true);
        }

        BuildContextMenu(d);
    }

    private void BuildContextMenu(Diagram d)
    {
        _contextMenu.Items.Clear();

        var hasSelection = false;
        foreach (var _ in d.GetSelectedModels())
        {
            hasSelection = true;
            break;
        }

        if (!IsReadOnly && hasSelection)
        {
            _contextMenu.Items.Add(BuildMenuItem("Delete", () => DeleteModels(new List<Model>(d.GetSelectedModels()))));
            _contextMenu.Items.Add(BuildMenuItem("Duplicate", DuplicateSelection));
            _contextMenu.Items.Add(new Separator());
            _contextMenu.Items.Add(BuildMenuItem("Bring to front", BringSelectionToFront));
            _contextMenu.Items.Add(BuildMenuItem("Send to back", SendSelectionToBack));
            _contextMenu.Items.Add(new Separator());
        }

        _contextMenu.Items.Add(BuildMenuItem("Select all", d.SelectAll));
        _contextMenu.Items.Add(BuildMenuItem("Zoom to fit", () => ZoomToFit()));
    }

    private static MenuItem BuildMenuItem(string header, Action onClick)
    {
        var item = new MenuItem { Header = header };
        item.Click += (_, _) => onClick();
        return item;
    }

    private void RebuildHistory()
    {
        if (_history != null)
        {
            _history.Changed -= OnHistoryChanged;
            _history.Dispose();
            _history = null;
        }

        if (Diagram != null)
        {
            _history = new DiagramHistory(Diagram);
            _history.Changed += OnHistoryChanged;
        }

        OnHistoryChanged();
    }

    private void OnHistoryChanged() => HistoryChanged?.Invoke();

    // Deletes models through the history so the removal is undoable. Nodes cascade their links, so links whose
    // endpoint is one of the deleted nodes are skipped (they would otherwise be double-restored on undo).
    internal void DeleteModels(IReadOnlyCollection<Model> models)
    {
        var d = Diagram;
        if (d == null || _history == null || IsReadOnly || models.Count == 0)
            return;

        var nodes = new List<NodeModel>();
        foreach (var model in models)
            if (model is NodeModel node && !node.Locked)
                nodes.Add(node);

        var deletedNodes = new HashSet<NodeModel>(nodes);
        var commands = new List<IDiagramCommand>();

        foreach (var node in nodes)
            commands.Add(new RemoveNodeCommand(d, node));

        foreach (var model in models)
        {
            if (model is not BaseLinkModel link)
                continue;

            var a = EndpointNode(link.Source);
            var b = EndpointNode(link.Target);
            if ((a != null && deletedNodes.Contains(a)) || (b != null && deletedNodes.Contains(b)))
                continue; // removed via its node command

            commands.Add(new RemoveLinkCommand(d, link));
        }

        if (commands.Count == 0)
            return;

        _history.Execute(commands.Count == 1 ? commands[0] : new CompositeCommand(commands));
    }

    private static NodeModel? EndpointNode(Anchor anchor) => anchor switch
    {
        SinglePortAnchor spa => spa.Port.Parent,
        ShapeIntersectionAnchor sia => sia.Node,
        _ => null,
    };

    private void ZoomAroundCenter(double factor)
    {
        var d = Diagram;
        if (d?.Container == null)
            return;

        var oldZoom = d.Zoom;
        var newZoom = Math.Clamp(oldZoom * factor, d.Options.Zoom.Minimum, d.Options.Zoom.Maximum);
        if (newZoom == oldZoom)
            return;

        var cx = d.Container.Width / 2;
        var cy = d.Container.Height / 2;
        var ratio = newZoom / oldZoom;
        d.Batch(() =>
        {
            d.SetPan(cx - (cx - d.Pan.X) * ratio, cy - (cy - d.Pan.Y) * ratio);
            d.SetZoom(newZoom);
        });
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DiagramProperty)
        {
            Subscribe(change.GetNewValue<Diagram?>());
            _groups?.SetDiagram(Diagram);
            _links?.SetDiagram(Diagram);
            _nodes?.SetDiagram(Diagram);
            _ports?.SetDiagram(Diagram);
            _vertices?.SetDiagram(Diagram);
            _adorners?.SetDiagram(Diagram);
            RebuildHistory();
            ApplyReadOnly();
            InvalidateArrange(); // re-run SetContainer for the new diagram
            _grid?.InvalidateVisual();
        }
        else if (change.Property == GridBrushProperty || change.Property == GridSizeProperty)
        {
            _grid?.InvalidateVisual();
        }
        else if (change.Property == PaletteProperty)
        {
            ApplyPalette();
        }
        else if (change.Property == IsReadOnlyProperty)
        {
            ApplyReadOnly();
        }
    }

    private void ApplyPalette()
    {
        if (_grid == null)
            return; // not constructed yet

        Background = Palette.CanvasBackground;
        GridBrush = Palette.GridLine;
        _links.InvalidateVisual();
        _nodes.Rebuild();
        _ports.Rebuild();
        _groups.Rebuild();
        _vertices.InvalidateVisual();
        _grid.InvalidateVisual();
    }

    private void ApplyReadOnly()
    {
        var d = Diagram;
        _adorners?.SetReadOnly(IsReadOnly);
        if (d == null)
            return;

        if (IsReadOnly)
        {
            d.UnregisterBehavior<DragMovablesBehavior>();
            d.UnregisterBehavior<DragNewLinkBehavior>();
        }
        else
        {
            if (d.GetBehavior<DragMovablesBehavior>() == null)
                d.RegisterBehavior(new DragMovablesBehavior(d));
            if (d.GetBehavior<DragNewLinkBehavior>() == null)
                d.RegisterBehavior(new DragNewLinkBehavior(d));
        }
    }

    private void Subscribe(Diagram? diagram)
    {
        if (ReferenceEquals(_subscribed, diagram))
            return;

        if (_subscribed != null)
        {
            _subscribed.Changed -= OnStructureChanged;
            _subscribed.PanChanged -= OnViewChanged;
            _subscribed.ZoomChanged -= OnViewChanged;
        }

        _subscribed = diagram;

        if (diagram != null)
        {
            diagram.Changed += OnStructureChanged;
            diagram.PanChanged += OnViewChanged;
            diagram.ZoomChanged += OnViewChanged;
        }
    }

    private void OnStructureChanged()
    {
        _grid.InvalidateVisual();
        _groups.InvalidateMeasure();
        _links.InvalidateVisual();
        _nodes.InvalidateMeasure();
        _ports.InvalidateMeasure();
        _vertices.InvalidateVisual();
        _adorners.Reposition();
        foreach (var entry in _customLayers)
            UpdateCustomLayer(entry.Layer, entry.World);
    }

    private void OnViewChanged()
    {
        _grid.InvalidateVisual();
        _groups.UpdateTransform();
        _links.UpdateTransform();
        _nodes.UpdateTransform();
        _ports.UpdateTransform();
        _vertices.UpdateTransform();
        _adorners.Reposition();
        foreach (var entry in _customLayers)
            UpdateCustomLayer(entry.Layer, entry.World);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var child in Children)
            child.Measure(availableSize);

        var width = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
        var height = double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height;
        return new Size(width, height);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var rect = new Rect(finalSize);
        foreach (var child in Children)
            child.Arrange(rect);

        Diagram?.SetContainer(new NodelyRect(0, 0, finalSize.Width, finalSize.Height));
        return finalSize;
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.Handled) // an adorner (e.g. delete button) handled it
            return;

        Focus();

        var screen = e.GetPosition(this);
        var model = ResolveModelAt(screen);

        // Double-click edits link bend points: on a vertex removes it; on a segmentable link adds one.
        if (e.ClickCount == 2 && !IsReadOnly && Diagram is { } d && TryEditVertex(model, screen, d))
        {
            e.Handled = true;
            return;
        }

        // Group everything this pointer gesture records (e.g. a multi-node drag) into one undo unit.
        _history?.BeginTransaction();
        _inGesture = _history != null;

        e.Pointer.Capture(this);
        Diagram?.TriggerPointerDown(model, ToPointerEvent(e));

        // Shift-drag on empty canvas starts a marquee selection (panning is suppressed when Shift is held).
        if (model == null && e.KeyModifiers.HasFlag(KeyModifiers.Shift) && Diagram?.Container != null)
        {
            _marqueeing = true;
            _marqueeStart = screen;
        }
    }

    private bool TryEditVertex(Model? model, global::Avalonia.Point screen, Diagram diagram)
    {
        if (model is LinkVertexModel vertex)
        {
            vertex.Parent.Vertices.Remove(vertex);
            vertex.Parent.Refresh();
            return true;
        }

        if (model is BaseLinkModel { Segmentable: true } link)
        {
            var point = diagram.GetRelativeMousePoint(screen.X, screen.Y);
            link.Vertices.Insert(BestVertexIndex(link, point), new LinkVertexModel(link, point));
            link.Refresh();
            return true;
        }

        return false;
    }

    // Insert the new vertex into the segment it is closest to (least added detour), preserving route order.
    private static int BestVertexIndex(BaseLinkModel link, NodelyPoint point)
    {
        var points = new List<NodelyPoint> { link.Source.GetPosition(link) ?? point };
        foreach (var vertex in link.Vertices)
            points.Add(vertex.Position);
        points.Add(link.Target.GetPosition(link) ?? point);

        var bestIndex = 0;
        var bestDetour = double.PositiveInfinity;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var detour = points[i].DistanceTo(point) + point.DistanceTo(points[i + 1]) - points[i].DistanceTo(points[i + 1]);
            if (detour < bestDetour)
            {
                bestDetour = detour;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.Handled)
            return;

        if (_marqueeing)
            _selectionBox.SetRect(RectFromPoints(_marqueeStart, e.GetPosition(this)));

        Diagram?.TriggerPointerMove(null, ToPointerEvent(e));
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        try
        {
            if (e.Handled)
            {
                e.Pointer.Capture(null);
                return;
            }

            var screen = e.GetPosition(this);
            Diagram?.TriggerPointerUp(ResolveModelAt(screen), ToPointerEvent(e));

            if (_marqueeing)
            {
                _marqueeing = false;
                _selectionBox.Clear();
                SelectWithin(_marqueeStart, screen);
            }

            e.Pointer.Capture(null);
        }
        finally
        {
            // Close the gesture transaction even on the early-handled path, so begin/end stay balanced.
            if (_inGesture)
            {
                _inGesture = false;
                _history?.EndTransaction();
            }
        }
    }

    /// <inheritdoc />
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var p = e.GetPosition(this);
        var m = e.KeyModifiers;
        Diagram?.TriggerWheel(new WheelEvent(
            p.X, p.Y, e.Delta.X, e.Delta.Y, 0,
            m.HasFlag(KeyModifiers.Control), m.HasFlag(KeyModifiers.Shift),
            m.HasFlag(KeyModifiers.Alt), m.HasFlag(KeyModifiers.Meta)));
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        var d = Diagram;
        if (d == null)
            return;

        // Accessibility: Escape clears the selection; arrow keys nudge the selected nodes (always available).
        if (e.Key == Key.Escape)
        {
            d.UnselectAll();
            e.Handled = true;
            return;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.A)
        {
            d.SelectAll();
            e.Handled = true;
            return;
        }

        if (!IsReadOnly && TryGetArrowNudge(e.Key, out var dx, out var dy))
        {
            NudgeSelection(d, dx, dy);
            e.Handled = true;
            return;
        }

        // Read-only inspectors don't forward keyboard edits (e.g. Delete).
        if (IsReadOnly)
            return;

        // Undo/redo and delete are handled here (not forwarded) so the edits route through the history.
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && (e.Key == Key.Z || e.Key == Key.Y))
        {
            if (e.Key == Key.Y || e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                Redo();
            else
                Undo();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Delete || e.Key == Key.Back)
        {
            DeleteModels(new List<Model>(d.GetSelectedModels()));
            e.Handled = true;
            return;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            switch (e.Key)
            {
                case Key.C: CopySelection(); e.Handled = true; return;
                case Key.X: CutSelection(); e.Handled = true; return;
                case Key.V: PasteClipboard(); e.Handled = true; return;
                case Key.D: DuplicateSelection(); e.Handled = true; return;
            }
        }

        var m = e.KeyModifiers;
        var key = e.Key.ToString();
        d.TriggerKeyDown(new KeyboardEvent(
            key, key,
            m.HasFlag(KeyModifiers.Control), m.HasFlag(KeyModifiers.Shift),
            m.HasFlag(KeyModifiers.Alt), m.HasFlag(KeyModifiers.Meta)));
    }

    private static bool TryGetArrowNudge(Key key, out double dx, out double dy)
    {
        (dx, dy) = key switch
        {
            Key.Left => (-1d, 0d),
            Key.Right => (1d, 0d),
            Key.Up => (0d, -1d),
            Key.Down => (0d, 1d),
            _ => (0d, 0d),
        };
        return dx != 0 || dy != 0;
    }

    private static void NudgeSelection(Diagram d, double dx, double dy)
    {
        var step = d.Options.GridSize ?? 1;
        d.Batch(() =>
        {
            foreach (var model in d.GetSelectedModels())
                if (model is NodeModel node && !node.Locked)
                    node.SetPosition(node.Position.X + dx * step, node.Position.Y + dy * step);
        });
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new DiagramCanvasAutomationPeer(this);

    private Model? ResolveModelAt(global::Avalonia.Point point)
    {
        GroupModel? group = null;

        foreach (var visual in this.GetVisualsAt(point))
        {
            var current = visual;
            while (current != null)
            {
                if (current is PortView portView)
                    return portView.Port;
                if (current is NodeView nodeView)
                    return nodeView.Node;
                if (current is GroupView groupView)
                {
                    group ??= groupView.Group; // remember it, but prefer a link hit over a background group
                    break;
                }
                current = current.GetVisualParent();
            }
        }

        // Vertices and links are immediate-mode (not in the visual tree). Resolve them geometrically, ranking
        // a vertex handle above the link line, and a link above a group container — all below nodes/ports.
        var d = Diagram;
        if (d != null)
        {
            var dp = d.GetRelativeMousePoint(point.X, point.Y);

            var vertex = _vertices.HitTest(dp, d.Zoom);
            if (vertex != null)
                return vertex;

            var link = _links.HitTest(dp, d.Zoom);
            if (link != null)
                return link;
        }

        return group;
    }

    private static Rect RectFromPoints(Point a, Point b)
        => new(
            new Point(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)),
            new Size(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y)));

    private void SelectWithin(Point startScreen, Point endScreen)
    {
        var d = Diagram;
        if (d?.Container == null)
            return;

        var p1 = d.GetRelativeMousePoint(startScreen.X, startScreen.Y);
        var p2 = d.GetRelativeMousePoint(endScreen.X, endScreen.Y);
        var rect = new NodelyRect(
            Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y),
            Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));

        d.Batch(() =>
        {
            foreach (var node in d.Nodes)
            {
                var bounds = node.GetBounds();
                if (bounds != null && bounds.Intersects(rect))
                    d.SelectModel(node, false);
            }

            foreach (var group in d.Groups)
            {
                var bounds = group.GetBounds();
                if (bounds != null && bounds.Intersects(rect))
                    d.SelectModel(group, false);
            }
        });
    }

    private PointerEvent ToPointerEvent(PointerEventArgs e)
    {
        var p = e.GetPosition(this);
        var props = e.GetCurrentPoint(this).Properties;
        var m = e.KeyModifiers;
        return new PointerEvent(
            p.X, p.Y, ToButton(props.PointerUpdateKind),
            m.HasFlag(KeyModifiers.Control), m.HasFlag(KeyModifiers.Shift),
            m.HasFlag(KeyModifiers.Alt), m.HasFlag(KeyModifiers.Meta),
            e.Pointer.Id);
    }

    private static PointerButton ToButton(PointerUpdateKind kind) => kind switch
    {
        PointerUpdateKind.LeftButtonPressed or PointerUpdateKind.LeftButtonReleased => PointerButton.Left,
        PointerUpdateKind.MiddleButtonPressed or PointerUpdateKind.MiddleButtonReleased => PointerButton.Middle,
        PointerUpdateKind.RightButtonPressed or PointerUpdateKind.RightButtonReleased => PointerButton.Right,
        PointerUpdateKind.XButton1Pressed or PointerUpdateKind.XButton1Released => PointerButton.XButton1,
        PointerUpdateKind.XButton2Pressed or PointerUpdateKind.XButton2Released => PointerButton.XButton2,
        _ => PointerButton.None,
    };
}
