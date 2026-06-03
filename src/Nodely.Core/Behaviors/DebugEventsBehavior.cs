using System;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Behaviors;

/// <summary>Logs diagram events to the console. Opt-in; not registered by default.</summary>
public class DebugEventsBehavior : Behavior
{
    /// <summary>Creates and wires the behavior.</summary>
    public DebugEventsBehavior(Diagram diagram) : base(diagram)
    {
        Diagram.Changed += OnChanged;
        Diagram.ContainerChanged += OnContainerChanged;
        Diagram.PanChanged += OnPanChanged;
        Diagram.ZoomChanged += OnZoomChanged;
        Diagram.SelectionChanged += OnSelectionChanged;
        Diagram.Nodes.Added += OnNodeAdded;
        Diagram.Nodes.Removed += OnNodeRemoved;
        Diagram.Links.Added += OnLinkAdded;
        Diagram.Links.Removed += OnLinkRemoved;
        Diagram.Groups.Added += OnGroupAdded;
        Diagram.Groups.Removed += OnGroupRemoved;
    }

    private void OnChanged() => Console.WriteLine("Changed");
    private void OnContainerChanged() => Console.WriteLine($"ContainerChanged, Container={Diagram.Container}");
    private void OnPanChanged() => Console.WriteLine($"PanChanged, Pan={Diagram.Pan}");
    private void OnZoomChanged() => Console.WriteLine($"ZoomChanged, Zoom={Diagram.Zoom}");
    private void OnSelectionChanged(SelectableModel m) => Console.WriteLine($"SelectionChanged, Model={m.GetType().Name}, Selected={m.Selected}");
    private void OnNodeAdded(NodeModel n) => Console.WriteLine($"Nodes.Added, Id={n.Id}");
    private void OnNodeRemoved(NodeModel n) => Console.WriteLine($"Nodes.Removed, Id={n.Id}");
    private void OnLinkAdded(BaseLinkModel l) => Console.WriteLine($"Links.Added, Id={l.Id}");
    private void OnLinkRemoved(BaseLinkModel l) => Console.WriteLine($"Links.Removed, Id={l.Id}");
    private void OnGroupAdded(GroupModel g) => Console.WriteLine($"Groups.Added, Id={g.Id}");
    private void OnGroupRemoved(GroupModel g) => Console.WriteLine($"Groups.Removed, Id={g.Id}");

    /// <inheritdoc />
    public override void Dispose()
    {
        Diagram.Changed -= OnChanged;
        Diagram.ContainerChanged -= OnContainerChanged;
        Diagram.PanChanged -= OnPanChanged;
        Diagram.ZoomChanged -= OnZoomChanged;
        Diagram.SelectionChanged -= OnSelectionChanged;
        Diagram.Nodes.Added -= OnNodeAdded;
        Diagram.Nodes.Removed -= OnNodeRemoved;
        Diagram.Links.Added -= OnLinkAdded;
        Diagram.Links.Removed -= OnLinkRemoved;
        Diagram.Groups.Added -= OnGroupAdded;
        Diagram.Groups.Removed -= OnGroupRemoved;
    }
}
