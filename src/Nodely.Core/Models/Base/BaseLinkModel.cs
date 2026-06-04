using System;
using System.Collections.Generic;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.PathGenerators;
using Nodely.Routers;

namespace Nodely.Models.Base;

/// <summary>
/// The base of every link. Carries its source/target <see cref="Anchor"/>s, an optional router and path
/// generator, vertices, labels, and markers, and produces a <see cref="PathGeneratorResult"/> when a
/// router + path generator are available (the concrete routers/generators arrive in Phase 4).
/// </summary>
public abstract class BaseLinkModel : SelectableModel, IHasBounds, ILinkable
{
    private readonly List<BaseLinkModel> _links = new();
    private bool _refreshing;

    /// <summary>Raised when the source anchor changes (link, old, new).</summary>
    public event Action<BaseLinkModel, Anchor, Anchor>? SourceChanged;

    /// <summary>Raised when the target anchor changes (link, old, new).</summary>
    public event Action<BaseLinkModel, Anchor, Anchor>? TargetChanged;

    /// <summary>Raised when the link's target becomes attached to a real model.</summary>
    public event Action<BaseLinkModel>? TargetAttached;

    /// <summary>Creates a link between two anchors.</summary>
    protected BaseLinkModel(Anchor source, Anchor target)
    {
        Source = source;
        Target = target;
    }

    /// <summary>Creates a link with the given id between two anchors.</summary>
    protected BaseLinkModel(string id, Anchor source, Anchor target) : base(id)
    {
        Source = source;
        Target = target;
    }

    /// <summary>The source endpoint anchor.</summary>
    public Anchor Source { get; private set; }

    /// <summary>The target endpoint anchor.</summary>
    public Anchor Target { get; private set; }

    /// <summary>The owning diagram, set when the link is added to the diagram's link layer.</summary>
    public Diagram? Diagram { get; internal set; }

    /// <summary>The computed route waypoints (null until a router runs).</summary>
    public Point[]? Route { get; private set; }

    /// <summary>The computed drawable path (null until a path generator runs).</summary>
    public PathGeneratorResult? PathGeneratorResult { get; private set; }

    /// <summary>True when both endpoints attach to real models (not free <see cref="PositionAnchor"/>s).</summary>
    public bool IsAttached => Source is not PositionAnchor && Target is not PositionAnchor;

    /// <summary>An optional per-link router overriding the diagram default.</summary>
    public Router? Router { get; set; }

    /// <summary>An optional per-link path generator overriding the diagram default.</summary>
    public PathGenerator? PathGenerator { get; set; }

    /// <summary>An optional marker drawn at the source end.</summary>
    public LinkMarker? SourceMarker { get; set; }

    /// <summary>An optional marker drawn at the target end.</summary>
    public LinkMarker? TargetMarker { get; set; }

    /// <summary>The source marker actually used: the per-link <see cref="SourceMarker"/>, else the diagram default.</summary>
    public LinkMarker? EffectiveSourceMarker => SourceMarker ?? Diagram?.Options.Links.DefaultSourceMarker;

    /// <summary>The target marker actually used: the per-link <see cref="TargetMarker"/>, else the diagram default.</summary>
    public LinkMarker? EffectiveTargetMarker => TargetMarker ?? Diagram?.Options.Links.DefaultTargetMarker;

    /// <summary>Whether the link supports user-added segments/vertices.</summary>
    public bool Segmentable { get; set; }

    /// <summary>The user-draggable bend points along the link.</summary>
    public List<LinkVertexModel> Vertices { get; } = new();

    /// <summary>The labels placed along the link.</summary>
    public List<LinkLabelModel> Labels { get; } = new();

    /// <summary>Links attached to this link (link-to-link).</summary>
    public IReadOnlyList<BaseLinkModel> Links => _links;

    /// <inheritdoc />
    public override void Refresh()
    {
        if (_refreshing)
            return;

        _refreshing = true;
        try
        {
            GeneratePath();
            base.Refresh();
            RefreshLinks();
        }
        finally
        {
            _refreshing = false;
        }
    }

    /// <summary>Refreshes every link attached to this one.</summary>
    public void RefreshLinks()
    {
        foreach (var link in Links)
            link.Refresh();
    }

    /// <summary>Adds a label to the link.</summary>
    public LinkLabelModel AddLabel(string content, double? distance = null, Point? offset = null)
    {
        var label = new LinkLabelModel(this, content, distance, offset);
        Labels.Add(label);
        return label;
    }

    /// <summary>Adds a vertex (bend point) to the link.</summary>
    public LinkVertexModel AddVertex(Point? position = null)
    {
        var vertex = new LinkVertexModel(this, position);
        Vertices.Add(vertex);
        return vertex;
    }

    /// <summary>Replaces the source anchor.</summary>
    public void SetSource(Anchor anchor)
    {
        if (anchor is null)
            throw new ArgumentNullException(nameof(anchor));

        if (Source == anchor)
            return;

        var old = Source;
        Source = anchor;
        SourceChanged?.Invoke(this, old, Source);
    }

    /// <summary>Replaces the target anchor.</summary>
    public void SetTarget(Anchor anchor)
    {
        if (anchor is null)
            throw new ArgumentNullException(nameof(anchor));

        if (Target == anchor)
            return;

        var old = Target;
        Target = anchor;
        TargetChanged?.Invoke(this, old, Target);
    }

    /// <summary>The bounding box of the generated path, or null if not yet generated.</summary>
    public Rectangle? GetBounds()
    {
        if (PathGeneratorResult == null)
            return null;

        var bbox = PathGeneratorResult.FullPath.GetBBox();
        return new Rectangle(bbox.Left, bbox.Top, bbox.Right, bbox.Bottom);
    }

    /// <inheritdoc />
    public bool CanAttachTo(ILinkable other) => true;

    /// <summary>Raises <see cref="TargetAttached"/>.</summary>
    public void TriggerTargetAttached() => TargetAttached?.Invoke(this);

    private void GeneratePath()
    {
        if (Diagram != null)
        {
            var router = Router ?? Diagram.Options.Links.DefaultRouter;
            var pathGenerator = PathGenerator ?? Diagram.Options.Links.DefaultPathGenerator;

            // Consumers can set either default to null to attach links without producing drawable paths.
            if (router != null && pathGenerator != null)
            {
                var route = router.GetRoute(Diagram, this);
                var source = Source.GetPosition(this, route);
                var target = Target.GetPosition(this, route);
                if (source != null && target != null)
                {
                    Route = route;
                    PathGeneratorResult = pathGenerator.GetResult(Diagram, this, route, source, target);
                    return;
                }
            }
        }

        Route = null;
        PathGeneratorResult = null;
    }

    void ILinkable.AddLink(BaseLinkModel link) => _links.Add(link);

    void ILinkable.RemoveLink(BaseLinkModel link) => _links.Remove(link);
}
