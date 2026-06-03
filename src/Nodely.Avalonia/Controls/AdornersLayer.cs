using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nodely.Models;
using Nodely.Models.Base;
using NodelySize = Nodely.Geometry.Size;

namespace Nodely.Avalonia.Controls;

/// <summary>
/// Screen-space overlay for each selected node: a delete (✕) button at the top-right and a resize handle at
/// the bottom-right. Fixed size (doesn't scale with zoom). Hidden when the canvas is read-only.
/// </summary>
internal sealed class AdornersLayer : Canvas
{
    private const double HandleSize = 14;

    private readonly DiagramCanvas _owner;
    private readonly Dictionary<NodeModel, (Control Delete, Control Resize)> _adorners = new();
    private readonly Dictionary<NodeModel, List<Control>> _customAdorners = new();
    private Diagram? _diagram;
    private bool _readOnly;

    public AdornersLayer(DiagramCanvas owner) => _owner = owner;

    /// <summary>Rebuilds adorners (e.g. after a custom adorner is registered).</summary>
    public void Refresh() => Rebuild();

    public void SetDiagram(Diagram? diagram)
    {
        if (ReferenceEquals(_diagram, diagram))
            return;

        if (_diagram != null)
            _diagram.SelectionChanged -= OnSelectionChanged;

        _diagram = diagram;

        if (diagram != null)
            diagram.SelectionChanged += OnSelectionChanged;

        Rebuild();
    }

    public void SetReadOnly(bool readOnly)
    {
        if (_readOnly == readOnly)
            return;

        _readOnly = readOnly;
        Rebuild();
    }

    /// <summary>Repositions every adorner from the current pan/zoom (called when the view or models change).</summary>
    public void Reposition()
    {
        if (_diagram == null)
            return;

        foreach (var kvp in _adorners)
            Position(kvp.Key, kvp.Value.Delete, kvp.Value.Resize);
        foreach (var kvp in _customAdorners)
            PositionCustom(kvp.Key, kvp.Value);
    }

    private void Position(NodeModel node, Control delete, Control resize)
    {
        var d = _diagram;
        if (d == null || node.Size == null)
            return;

        var rightX = (node.Position.X + node.Size.Width) * d.Zoom + d.Pan.X;
        var topY = node.Position.Y * d.Zoom + d.Pan.Y;
        var bottomY = (node.Position.Y + node.Size.Height) * d.Zoom + d.Pan.Y;

        SetLeft(delete, rightX - HandleSize / 2);
        SetTop(delete, topY - HandleSize / 2);
        SetLeft(resize, rightX - HandleSize / 2);
        SetTop(resize, bottomY - HandleSize / 2);
    }

    // Custom adorners are anchored at the node's top-left corner (screen space). The provider arranges itself
    // relative to that (e.g. a Margin/transform to float a toolbar above the node).
    private void PositionCustom(NodeModel node, List<Control> customs)
    {
        var d = _diagram;
        if (d == null || node.Size == null)
            return;

        var leftX = node.Position.X * d.Zoom + d.Pan.X;
        var topY = node.Position.Y * d.Zoom + d.Pan.Y;
        foreach (var control in customs)
        {
            SetLeft(control, leftX);
            SetTop(control, topY);
        }
    }

    private void OnSelectionChanged(SelectableModel m) => Rebuild();

    // A node moved/resized by a pointer drag raises its own Changed event, not Diagram.Changed, so the
    // canvas-level reposition hooks (OnStructureChanged/OnViewChanged) never fire mid-drag. Track each
    // adorned node directly so its handles follow it live instead of freezing at the pre-drag position.
    private void OnAdornedNodeChanged(Model m)
    {
        if (m is not NodeModel node)
            return;

        if (_adorners.TryGetValue(node, out var pair))
            Position(node, pair.Delete, pair.Resize);
        if (_customAdorners.TryGetValue(node, out var customs))
            PositionCustom(node, customs);
    }

    private void Rebuild()
    {
        foreach (var node in _adorners.Keys)
            node.Changed -= OnAdornedNodeChanged;

        Children.Clear();
        _adorners.Clear();
        _customAdorners.Clear();

        var d = _diagram;
        if (d == null || _readOnly)
            return;

        foreach (var node in d.Nodes)
        {
            if (!node.Selected || node.Locked)
                continue;

            var delete = CreateDeleteButton(d, node);
            var resize = CreateResizeHandle(d, node);
            _adorners[node] = (delete, resize);
            Children.Add(resize);
            Children.Add(delete);

            // User-provided adorners (RegisterAdorner) — selection toolbars, badges, custom handles, etc.
            List<Control>? customs = null;
            foreach (var control in _owner.BuildAdorners(node))
            {
                (customs ??= new List<Control>()).Add(control);
                Children.Add(control);
            }

            if (customs != null)
                _customAdorners[node] = customs;

            node.Changed += OnAdornedNodeChanged; // follow live drag/resize (no Diagram.Changed during drag)
        }

        Reposition();
    }

    private Control CreateDeleteButton(Diagram diagram, NodeModel node)
    {
        var button = new Button
        {
            Width = HandleSize,
            Height = HandleSize,
            Padding = new Thickness(0),
            Content = "✕",
            FontSize = 10,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(Color.FromRgb(0xC0, 0x3A, 0x3A)),
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(HandleSize / 2),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };

        button.Click += (_, e) =>
        {
            _owner.DeleteModels(new Model[] { node }); // routed through the undo/redo history
            e.Handled = true;
        };

        return button;
    }

    private Control CreateResizeHandle(Diagram diagram, NodeModel node)
    {
        var handle = new Border
        {
            Width = HandleSize,
            Height = HandleSize,
            CornerRadius = new CornerRadius(3),
            Background = _owner.Palette.Selection,
            Cursor = new Cursor(StandardCursorType.BottomRightCorner),
        };

        Point? last = null;

        handle.PointerPressed += (_, e) =>
        {
            last = e.GetPosition(this);
            e.Pointer.Capture(handle);
            e.Handled = true;
        };

        handle.PointerMoved += (_, e) =>
        {
            if (last is not { } prev || node.Size == null)
                return;

            var current = e.GetPosition(this);
            var zoom = diagram.Zoom <= 0 ? 1 : diagram.Zoom;
            node.ControlledSize = true;
            node.Size = new NodelySize(
                Math.Max(20, node.Size.Width + (current.X - prev.X) / zoom),
                Math.Max(20, node.Size.Height + (current.Y - prev.Y) / zoom));
            last = current;
            Reposition();
            e.Handled = true;
        };

        handle.PointerReleased += (_, e) =>
        {
            last = null;
            e.Pointer.Capture(null);
            e.Handled = true;
        };

        return handle;
    }
}
