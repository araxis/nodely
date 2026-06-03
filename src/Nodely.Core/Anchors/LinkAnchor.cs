using Nodely.Geometry;
using Nodely.Models.Base;

namespace Nodely.Anchors;

/// <summary>
/// An anchor that attaches a link endpoint to another link (link-to-link), at the middle of that link's
/// generated path. Falls back to the midpoint of the target link's endpoints before its path exists.
/// </summary>
public sealed class LinkAnchor : Anchor
{
    /// <summary>Creates an anchor on <paramref name="link"/>.</summary>
    public LinkAnchor(BaseLinkModel link) : base(link) => Link = link;

    /// <summary>The link this anchor attaches to.</summary>
    public BaseLinkModel Link { get; }

    /// <inheritdoc />
    public override Point? GetPosition(BaseLinkModel link, Point[] route) => Midpoint();

    /// <inheritdoc />
    public override Point? GetPlainPosition() => Midpoint();

    private Point? Midpoint()
    {
        var path = Link.PathGeneratorResult?.FullPath;
        if (path != null)
        {
            var point = path.PointAtDistance(path.Length() / 2);
            if (point != null)
                return point;
        }

        var source = Link.Source.GetPlainPosition();
        var target = Link.Target.GetPlainPosition();
        if (source is null || target is null)
            return null;

        return new Point((source.X + target.X) / 2, (source.Y + target.Y) / 2);
    }
}
