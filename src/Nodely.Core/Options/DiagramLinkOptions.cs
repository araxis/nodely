using System;
using Nodely.Anchors;
using Nodely.Models;
using Nodely.Models.Base;
using Nodely.PathGenerators;
using Nodely.Routers;

namespace Nodely.Options;

/// <summary>Link configuration: default routing/path generation, snapping, and factories.</summary>
public class DiagramLinkOptions
{
    private double _snappingRadius = 50;

    /// <summary>The default router used when a link doesn't specify its own.</summary>
    public Router DefaultRouter { get; set; } = new NormalRouter();

    /// <summary>The default path generator used when a link doesn't specify its own.</summary>
    public PathGenerator DefaultPathGenerator { get; set; } = new SmoothPathGenerator();

    /// <summary>A marker drawn at every link's source end unless the link sets its own (null = none).</summary>
    public LinkMarker? DefaultSourceMarker { get; set; }

    /// <summary>A marker drawn at every link's target end unless the link sets its own (null = none).
    /// Set to <see cref="LinkMarker.Arrow"/> for directed diagrams (workflows, state machines).</summary>
    public LinkMarker? DefaultTargetMarker { get; set; }

    /// <summary>Whether dragging a new link snaps to nearby ports/nodes.</summary>
    public bool EnableSnapping { get; set; }

    /// <summary>Whether a dragged link must end on a valid target (else it's discarded).</summary>
    public bool RequireTarget { get; set; } = true;

    /// <summary>
    /// An optional rule deciding whether a dragged link may attach to a target (no self-loops, type rules,
    /// max-connections, …). Return false to reject. Runs in addition to <see cref="ILinkable.CanAttachTo"/>.
    /// </summary>
    public Func<BaseLinkModel, ILinkable, bool>? CanConnect { get; set; }

    /// <summary>The radius within which snapping engages (must be &gt; 0).</summary>
    public double SnappingRadius
    {
        get => _snappingRadius;
        set
        {
            if (value <= 0)
                throw new ArgumentException("SnappingRadius must be greater than zero");

            _snappingRadius = value;
        }
    }

    /// <summary>Creates a link when the user drags a new connection from a source.</summary>
    public LinkFactory Factory { get; set; } = (diagram, source, targetAnchor) =>
    {
        Anchor sourceAnchor = source switch
        {
            NodeModel node => new ShapeIntersectionAnchor(node),
            PortModel port => new SinglePortAnchor(port),
            _ => throw new NotImplementedException()
        };
        return new LinkModel(sourceAnchor, targetAnchor);
    };

    /// <summary>Creates the target anchor when a dragged link lands on a model.</summary>
    public AnchorFactory TargetAnchorFactory { get; set; } = (diagram, link, model) => model switch
    {
        NodeModel node => new ShapeIntersectionAnchor(node),
        PortModel port => new SinglePortAnchor(port),
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
    };
}
