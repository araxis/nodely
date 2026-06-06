using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Nodely.Models;
using Nodely.Models.Base;
using NodelySize = Nodely.Geometry.Size;

namespace Nodely.Avalonia.Controls;

/// <summary>
/// Hosts a <see cref="NodeView"/> per node, arranged at the node's diagram-space position under a shared
/// pan/zoom render transform. Each node's measured size is written back to
/// <see cref="NodeModel.Size"/> — the native replacement for the browser ResizeObserver.
/// </summary>
internal sealed class NodesLayer : Panel
{
    private readonly DiagramCanvas _owner;
    private readonly Dictionary<NodeModel, NodeView> _views = new();
    private Diagram? _diagram;

    public NodesLayer(DiagramCanvas owner)
    {
        _owner = owner;
        ClipToBounds = false;
        RenderTransformOrigin = RelativePoint.TopLeft;
    }

    /// <summary>Binds the layer to a diagram, (re)creating a view per node.</summary>
    public void SetDiagram(Diagram? diagram)
    {
        if (ReferenceEquals(_diagram, diagram))
            return;

        if (_diagram != null)
        {
            _diagram.Nodes.Added -= OnNodeAdded;
            _diagram.Nodes.Removed -= OnNodeRemoved;
            foreach (var node in _views.Keys)
                Unhook(node);
        }

        Children.Clear();
        _views.Clear();
        _diagram = diagram;

        if (diagram != null)
        {
            diagram.Nodes.Added += OnNodeAdded;
            diagram.Nodes.Removed += OnNodeRemoved;
            foreach (var node in diagram.Nodes)
                OnNodeAdded(node);
        }

        UpdateTransform();
        InvalidateMeasure();
    }

    /// <summary>Refreshes the pan/zoom transform from the diagram's view state.</summary>
    public void UpdateTransform()
    {
        var d = _diagram;
        if (d == null)
        {
            RenderTransform = null;
            return;
        }

        // p_screen = p_diagram * zoom + pan  (scale then translate; pan is in screen pixels).
        RenderTransform = new TransformGroup
        {
            Children =
            {
                new ScaleTransform(d.Zoom, d.Zoom),
                new TranslateTransform(d.Pan.X, d.Pan.Y),
            },
        };
    }

    private void OnNodeAdded(NodeModel node)
    {
        if (_views.ContainsKey(node))
            return;

        var view = new NodeView(node, _owner) { IsVisible = node.Visible, ZIndex = node.Order };
        _views[node] = view;
        Children.Add(view);
        node.Changed += OnNodeChanged;
        node.SizeChanged += OnNodeSizeChanged;
        node.VisibilityChanged += OnNodeVisibilityChanged;
        node.OrderChanged += OnNodeOrderChanged;
        InvalidateMeasure();
    }

    /// <summary>Recreates all node views (e.g. after a palette change), keeping subscriptions.</summary>
    public void Rebuild()
    {
        var nodes = new List<NodeModel>(_views.Keys);
        foreach (var view in _views.Values)
            Children.Remove(view);
        _views.Clear();

        foreach (var node in nodes)
        {
            var view = new NodeView(node, _owner) { IsVisible = node.Visible, ZIndex = node.Order };
            _views[node] = view;
            Children.Add(view);
        }

        InvalidateMeasure();
    }

    private void OnNodeRemoved(NodeModel node)
    {
        Unhook(node);
        if (_views.Remove(node, out var view))
            Children.Remove(view);
        InvalidateMeasure();
    }

    private void Unhook(NodeModel node)
    {
        node.Changed -= OnNodeChanged;
        node.SizeChanged -= OnNodeSizeChanged;
        node.VisibilityChanged -= OnNodeVisibilityChanged;
        node.OrderChanged -= OnNodeOrderChanged;
    }

    private void OnNodeOrderChanged(SelectableModel m)
    {
        if (m is NodeModel n && _views.TryGetValue(n, out var view))
            view.ZIndex = n.Order;
    }

    private void OnNodeChanged(Model m)
    {
        if (m is NodeModel n && _views.TryGetValue(n, out var view))
            view.InvalidateVisual();

        InvalidateArrange();
    }

    private void OnNodeSizeChanged(NodeModel n)
    {
        n.RefreshLinks();
        InvalidateMeasure();
    }

    // Virtualization: when VirtualizationBehavior toggles a node's Visible flag, collapse/realize its view.
    private void OnNodeVisibilityChanged(Model m)
    {
        if (m is NodeModel n && _views.TryGetValue(n, out var view))
        {
            view.IsVisible = n.Visible;
            InvalidateMeasure();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var view in _views.Values)
        {
            var node = view.Node;
            if (node.ControlledSize && node.Size != null)
            {
                view.Measure(new Size(node.Size.Width, node.Size.Height));
            }
            else
            {
                view.Measure(Size.Infinity);
                var d = view.DesiredSize;
                node.Size = new NodelySize(d.Width, d.Height); // size feedback (setter guards equality)
            }
        }

        var w = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
        var h = double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height;
        return new Size(w, h);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var view in _views.Values)
        {
            var node = view.Node;
            var size = node.Size ?? new NodelySize(view.DesiredSize.Width, view.DesiredSize.Height);
            view.Arrange(new Rect(node.Position.X, node.Position.Y, size.Width, size.Height));
        }

        return finalSize;
    }
}
