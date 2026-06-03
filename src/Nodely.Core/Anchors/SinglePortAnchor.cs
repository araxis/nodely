using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Anchors;

/// <summary>An anchor that attaches a link to a single port, honoring the port's shape and alignment.</summary>
public sealed class SinglePortAnchor : Anchor
{
    /// <summary>Creates an anchor on <paramref name="port"/>.</summary>
    public SinglePortAnchor(PortModel port) : base(port) => Port = port;

    /// <summary>The port this anchor attaches to.</summary>
    public PortModel Port { get; }

    /// <summary>If true and there is no marker on this end, attach at the port's middle.</summary>
    public bool MiddleIfNoMarker { get; set; }

    /// <summary>If true, derive the point from the port's shape and alignment; otherwise from the bounds.</summary>
    public bool UseShapeAndAlignment { get; set; } = true;

    /// <inheritdoc />
    public override Point? GetPosition(BaseLinkModel link, Point[] route)
    {
        if (!Port.Initialized)
            return null;

        if (MiddleIfNoMarker &&
            ((link.Source == this && link.SourceMarker is null) || (link.Target == this && link.TargetMarker is null)))
            return Port.MiddlePosition;

        var pt = Port.Position;
        if (UseShapeAndAlignment)
        {
            return Port.Alignment switch
            {
                PortAlignment.Top => Port.GetShape().GetPointAtAngle(270),
                PortAlignment.TopRight => Port.GetShape().GetPointAtAngle(315),
                PortAlignment.Right => Port.GetShape().GetPointAtAngle(0),
                PortAlignment.BottomRight => Port.GetShape().GetPointAtAngle(45),
                PortAlignment.Bottom => Port.GetShape().GetPointAtAngle(90),
                PortAlignment.BottomLeft => Port.GetShape().GetPointAtAngle(135),
                PortAlignment.Left => Port.GetShape().GetPointAtAngle(180),
                PortAlignment.TopLeft => Port.GetShape().GetPointAtAngle(225),
                _ => null,
            };
        }

        return Port.Alignment switch
        {
            PortAlignment.Top => new Point(pt.X + Port.Size.Width / 2, pt.Y),
            PortAlignment.TopRight => new Point(pt.X + Port.Size.Width, pt.Y),
            PortAlignment.Right => new Point(pt.X + Port.Size.Width, pt.Y + Port.Size.Height / 2),
            PortAlignment.BottomRight => new Point(pt.X + Port.Size.Width, pt.Y + Port.Size.Height),
            PortAlignment.Bottom => new Point(pt.X + Port.Size.Width / 2, pt.Y + Port.Size.Height),
            PortAlignment.BottomLeft => new Point(pt.X, pt.Y + Port.Size.Height),
            PortAlignment.Left => new Point(pt.X, pt.Y + Port.Size.Height / 2),
            _ => pt,
        };
    }

    /// <inheritdoc />
    public override Point? GetPlainPosition() => Port.MiddlePosition;
}
