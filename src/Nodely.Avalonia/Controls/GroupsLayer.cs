using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Avalonia.Controls;

/// <summary>
/// Hosts a <see cref="GroupView"/> per group, arranged at the group's (auto-computed) bounds under the
/// shared pan/zoom transform. Groups sit below links/nodes so they read as containers behind their children.
/// </summary>
internal sealed class GroupsLayer : Panel
{
    private readonly DiagramCanvas _owner;
    private readonly Dictionary<GroupModel, GroupView> _views = new();
    private Diagram? _diagram;

    public GroupsLayer(DiagramCanvas owner)
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
            _diagram.Groups.Added -= OnGroupAdded;
            _diagram.Groups.Removed -= OnGroupRemoved;
            foreach (var group in _views.Keys)
                Unhook(group);
        }

        Children.Clear();
        _views.Clear();
        _diagram = diagram;

        if (diagram != null)
        {
            diagram.Groups.Added += OnGroupAdded;
            diagram.Groups.Removed += OnGroupRemoved;
            foreach (var group in diagram.Groups)
                OnGroupAdded(group);
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

    private void OnGroupAdded(GroupModel group)
    {
        if (_views.ContainsKey(group))
            return;

        var view = new GroupView(group, _owner);
        _views[group] = view;
        Children.Add(view);
        group.Changed += OnGroupChanged;
        group.SizeChanged += OnGroupSizeChanged;
        InvalidateMeasure();
    }

    /// <summary>Recreates all group views (e.g. after a palette change), keeping subscriptions.</summary>
    public void Rebuild()
    {
        var groups = new List<GroupModel>(_views.Keys);
        foreach (var view in _views.Values)
            Children.Remove(view);
        _views.Clear();

        foreach (var group in groups)
        {
            var view = new GroupView(group, _owner);
            _views[group] = view;
            Children.Add(view);
        }

        InvalidateMeasure();
    }

    private void OnGroupRemoved(GroupModel group)
    {
        Unhook(group);
        if (_views.Remove(group, out var view))
            Children.Remove(view);
        InvalidateMeasure();
    }

    private void Unhook(GroupModel group)
    {
        group.Changed -= OnGroupChanged;
        group.SizeChanged -= OnGroupSizeChanged;
    }

    private void OnGroupChanged(Model m)
    {
        if (m is GroupModel g && _views.TryGetValue(g, out var view))
            view.InvalidateVisual();
        InvalidateArrange();
    }

    private void OnGroupSizeChanged(NodeModel n) => InvalidateArrange();

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var view in _views.Values)
        {
            var size = view.Group.Size;
            view.Measure(size == null ? Size.Infinity : new Size(size.Width, size.Height));
        }

        var w = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
        var h = double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height;
        return new Size(w, h);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var view in _views.Values)
        {
            var group = view.Group;
            if (group.Size == null)
                continue;

            view.Arrange(new Rect(group.Position.X, group.Position.Y, group.Size.Width, group.Size.Height));
        }

        return finalSize;
    }
}
