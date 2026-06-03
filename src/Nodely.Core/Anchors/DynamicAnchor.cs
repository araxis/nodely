using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models.Base;

namespace Nodely.Anchors;

/// <summary>
/// An anchor that picks, among several candidate anchors, the one whose attachment point is closest to the
/// link's other endpoint — so the link snaps to whichever candidate faces the model it connects to (e.g. the
/// nearest side/port of a node).
/// </summary>
public sealed class DynamicAnchor : Anchor
{
    private readonly IReadOnlyList<Anchor> _anchors;

    /// <summary>Creates a dynamic anchor on <paramref name="model"/> over the given candidate anchors.</summary>
    public DynamicAnchor(ILinkable model, IReadOnlyList<Anchor> anchors) : base(model) => _anchors = anchors;

    /// <summary>The candidate anchors this one chooses between.</summary>
    public IReadOnlyList<Anchor> Anchors => _anchors;

    /// <inheritdoc />
    public override Point? GetPosition(BaseLinkModel link, Point[] route)
    {
        if (_anchors.Count == 0)
            return null;

        var isTarget = link.Target == this;
        var other = route.Length > 0
            ? route[isTarget ? route.Length - 1 : 0]
            : GetOtherPosition(link, isTarget);

        var candidates = new List<Point?>(_anchors.Count);
        foreach (var anchor in _anchors)
            candidates.Add(anchor.GetPosition(link, route));

        // Before the other endpoint is known, fall back to the first candidate.
        return other is null ? candidates[0] : GetClosestPointTo(candidates, other);
    }

    /// <inheritdoc />
    public override Point? GetPlainPosition() => _anchors.Count > 0 ? _anchors[0].GetPlainPosition() : null;
}
