using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Nodely.Models;
using Nodely.Models.Base;
using NodelyPoint = Nodely.Geometry.Point;
using NodelySize = Nodely.Geometry.Size;

namespace Nodely.Avalonia.Controls;

/// <summary>
/// Renders ports as hit-testable dots positioned on their node's edges. Computes and initializes each
/// port's diagram-space position/size from the node's bounds + alignment (so anchors can resolve), under
/// the shared pan/zoom transform.
/// </summary>
internal sealed class PortsLayer : Panel
{
    private const double DefaultPortSize = 12;

    private readonly DiagramCanvas _owner;
    private readonly Dictionary<PortModel, PortView> _views = new();
    private Diagram? _diagram;

    public PortsLayer(DiagramCanvas owner)
    {
        _owner = owner;
        ClipToBounds = false;
        RenderTransformOrigin = RelativePoint.TopLeft;
    }

    public void SetDiagram(Diagram? diagram)
    {
        if (ReferenceEquals(_diagram, diagram))
            return;

        if (_diagram != null)
        {
            _diagram.Nodes.Added -= OnNodeAdded;
            _diagram.Nodes.Removed -= OnNodeRemoved;
            foreach (var node in _diagram.Nodes)
                node.Changed -= OnNodeChanged;
        }

        Children.Clear();
        _views.Clear();
        _diagram = diagram;

        if (diagram != null)
        {
            diagram.Nodes.Added += OnNodeAdded;
            diagram.Nodes.Removed += OnNodeRemoved;
            foreach (var node in diagram.Nodes)
                node.Changed += OnNodeChanged;
        }

        UpdateTransform();
        InvalidateMeasure();
    }

    public void UpdateTransform()
    {
        var d = _diagram;
        RenderTransform = d == null
            ? null
            : new TransformGroup
            {
                Children = { new ScaleTransform(d.Zoom, d.Zoom), new TranslateTransform(d.Pan.X, d.Pan.Y) },
            };
    }

    private void OnNodeAdded(NodeModel node)
    {
        node.Changed += OnNodeChanged;
        InvalidateMeasure();
    }

    private void OnNodeRemoved(NodeModel node)
    {
        node.Changed -= OnNodeChanged;
        foreach (var port in node.Ports)
            if (_views.Remove(port, out var view))
                Children.Remove(view);
        InvalidateMeasure();
    }

    private void OnNodeChanged(Model m) => InvalidateArrange();

    private void Sync()
    {
        if (_diagram == null)
            return;

        foreach (var node in _diagram.Nodes)
        foreach (var port in node.Ports)
        {
            if (_views.ContainsKey(port))
                continue;

            var view = new PortView(port, _owner);
            _views[port] = view;
            Children.Add(view);
        }
    }

    /// <summary>Recreates all port views (e.g. after a palette change).</summary>
    public void Rebuild()
    {
        Children.Clear();
        _views.Clear();
        InvalidateMeasure();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        Sync();
        foreach (var view in _views.Values)
            view.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        var w = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
        var h = double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height;
        return new Size(w, h);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_diagram != null)
        {
            foreach (var node in _diagram.Nodes)
            {
                if (node.Size == null)
                    continue;

                foreach (var port in node.Ports)
                {
                    if (!_views.TryGetValue(port, out var view))
                        continue;

                    var viewSize = ResolveViewSize(view);
                    port.Size = new NodelySize(viewSize.Width, viewSize.Height);
                    var center = port.GetPortCenter();
                    var newPos = new NodelyPoint(center.X - viewSize.Width / 2, center.Y - viewSize.Height / 2);
                    var oldPos = port.Position;

                    port.Position = newPos;
                    var wasInitialized = port.Initialized;
                    port.Initialized = true;

                    view.Arrange(new Rect(newPos.X, newPos.Y, viewSize.Width, viewSize.Height));

                    // (Re)generate attached links when a port first initializes or actually moves.
                    if (!wasInitialized || !oldPos.Equals(newPos))
                        port.RefreshLinks();
                }
            }
        }

        return finalSize;
    }

    private static Size ResolveViewSize(Control view)
    {
        var desired = view.DesiredSize;
        var width = IsUsable(desired.Width) ? desired.Width : DefaultPortSize;
        var height = IsUsable(desired.Height) ? desired.Height : DefaultPortSize;
        return new Size(width, height);
    }

    private static bool IsUsable(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
}
