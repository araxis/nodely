using System.Linq;
using Nodely.Anchors;
using Nodely.Events;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Behaviors;

/// <summary>Creates a new link by dragging from a port (or an explicit source), optionally snapping to ports.</summary>
public class DragNewLinkBehavior : Behavior
{
    private PositionAnchor? _targetPositionAnchor;

    /// <summary>The link currently being dragged, if any.</summary>
    public BaseLinkModel? OngoingLink { get; private set; }

    /// <summary>Creates and wires the behavior.</summary>
    public DragNewLinkBehavior(Diagram diagram) : base(diagram)
    {
        Diagram.PointerDown += OnPointerDown;
        Diagram.PointerMove += OnPointerMove;
        Diagram.PointerUp += OnPointerUp;
    }

    /// <summary>Starts dragging a new link from a linkable source.</summary>
    public void StartFrom(ILinkable source, double clientX, double clientY)
    {
        if (OngoingLink != null)
            return;

        _targetPositionAnchor = new PositionAnchor(CalculateTargetPosition(clientX, clientY));
        OngoingLink = Diagram.Options.Links.Factory(Diagram, source, _targetPositionAnchor);
        if (OngoingLink == null)
            return;

        Diagram.Links.Add(OngoingLink);
    }

    /// <summary>Re-starts dragging an existing link's target.</summary>
    public void StartFrom(BaseLinkModel link, double clientX, double clientY)
    {
        if (OngoingLink != null)
            return;

        _targetPositionAnchor = new PositionAnchor(CalculateTargetPosition(clientX, clientY));
        OngoingLink = link;
        OngoingLink.SetTarget(_targetPositionAnchor);
        OngoingLink.Refresh();
        OngoingLink.RefreshLinks();
    }

    private void OnPointerDown(Model? model, PointerEvent e)
    {
        if (e.Button != PointerButton.Left)
            return;

        OngoingLink = null;
        _targetPositionAnchor = null;

        if (model is PortModel port)
        {
            if (port.Locked)
                return;

            _targetPositionAnchor = new PositionAnchor(CalculateTargetPosition(e.X, e.Y));
            OngoingLink = Diagram.Options.Links.Factory(Diagram, port, _targetPositionAnchor);
            if (OngoingLink == null)
                return;

            OngoingLink.SetTarget(_targetPositionAnchor);
            Diagram.Links.Add(OngoingLink);
        }
    }

    private void OnPointerMove(Model? model, PointerEvent e)
    {
        if (OngoingLink == null || model != null)
            return;

        _targetPositionAnchor!.SetPosition(CalculateTargetPosition(e.X, e.Y));

        if (Diagram.Options.Links.EnableSnapping)
        {
            var nearPort = FindNearPortToAttachTo();
            if (nearPort != null || OngoingLink.Target is not PositionAnchor)
                OngoingLink.SetTarget(nearPort is null ? _targetPositionAnchor : new SinglePortAnchor(nearPort));
        }

        OngoingLink.Refresh();
        OngoingLink.RefreshLinks();
    }

    private void OnPointerUp(Model? model, PointerEvent e)
    {
        if (OngoingLink == null)
            return;

        if (OngoingLink.IsAttached) // Snapped already
        {
            OngoingLink.TriggerTargetAttached();
            OngoingLink = null;
            return;
        }

        if (model is ILinkable linkable &&
            (OngoingLink.Source.Model == null || OngoingLink.Source.Model.CanAttachTo(linkable)) &&
            (Diagram.Options.Links.CanConnect?.Invoke(OngoingLink, linkable) ?? true))
        {
            var targetAnchor = Diagram.Options.Links.TargetAnchorFactory(Diagram, OngoingLink, linkable);
            OngoingLink.SetTarget(targetAnchor);
            OngoingLink.TriggerTargetAttached();
            OngoingLink.Refresh();
            OngoingLink.RefreshLinks();
        }
        else if (Diagram.Options.Links.RequireTarget)
        {
            Diagram.Links.Remove(OngoingLink);
        }
        else
        {
            OngoingLink.Refresh();
        }

        OngoingLink = null;
    }

    private Point CalculateTargetPosition(double clientX, double clientY)
    {
        var target = Diagram.GetRelativeMousePoint(clientX, clientY);

        if (OngoingLink == null)
            return target;

        var source = OngoingLink.Source.GetPlainPosition()!;
        var dirVector = target.Subtract(source).Normalize();
        var change = dirVector.Multiply(5);
        return target.Subtract(change);
    }

    private PortModel? FindNearPortToAttachTo()
    {
        if (OngoingLink is null || _targetPositionAnchor is null)
            return null;

        PortModel? nearestSnapPort = null;
        var nearestSnapPortDistance = double.PositiveInfinity;

        var position = _targetPositionAnchor.GetPosition(OngoingLink)!;

        foreach (var port in Diagram.Nodes.SelectMany(n => n.Ports))
        {
            var distance = position.DistanceTo(port.Position);
            if (distance <= Diagram.Options.Links.SnappingRadius &&
                OngoingLink.Source.Model?.CanAttachTo(port) != false &&
                (Diagram.Options.Links.CanConnect?.Invoke(OngoingLink, port) ?? true))
            {
                if (distance < nearestSnapPortDistance)
                {
                    nearestSnapPortDistance = distance;
                    nearestSnapPort = port;
                }
            }
        }

        return nearestSnapPort;
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        Diagram.PointerDown -= OnPointerDown;
        Diagram.PointerMove -= OnPointerMove;
        Diagram.PointerUp -= OnPointerUp;
    }
}
