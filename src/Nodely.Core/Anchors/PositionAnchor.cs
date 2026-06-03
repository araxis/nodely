using Nodely.Geometry;
using Nodely.Models.Base;

namespace Nodely.Anchors;

/// <summary>An anchor fixed to an absolute point in diagram space (used for free/dangling link ends).</summary>
public sealed class PositionAnchor : Anchor
{
    private Point _position;

    /// <summary>Creates an anchor at the given position.</summary>
    public PositionAnchor(Point position) : base(null) => _position = position;

    /// <summary>Moves the anchor.</summary>
    public void SetPosition(Point position) => _position = position;

    /// <inheritdoc />
    public override Point? GetPlainPosition() => _position;

    /// <inheritdoc />
    public override Point? GetPosition(BaseLinkModel link, Point[] route) => _position;
}
