using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Routers;

/// <summary>Computes the ordered waypoints a link follows between its endpoints.</summary>
public abstract class Router
{
    /// <summary>Returns the route (excluding the resolved source/target points) for <paramref name="link"/>.</summary>
    public abstract Point[] GetRoute(Diagram diagram, BaseLinkModel link);

    /// <summary>The point on a port's edge implied by its alignment.</summary>
    protected static Point GetPortPositionBasedOnAlignment(PortModel port)
    {
        var pt = port.Position;
        return port.Alignment switch
        {
            PortAlignment.Top => new Point(pt.X + port.Size.Width / 2, pt.Y),
            PortAlignment.TopRight => new Point(pt.X + port.Size.Width, pt.Y),
            PortAlignment.Right => new Point(pt.X + port.Size.Width, pt.Y + port.Size.Height / 2),
            PortAlignment.BottomRight => new Point(pt.X + port.Size.Width, pt.Y + port.Size.Height),
            PortAlignment.Bottom => new Point(pt.X + port.Size.Width / 2, pt.Y + port.Size.Height),
            PortAlignment.BottomLeft => new Point(pt.X, pt.Y + port.Size.Height),
            PortAlignment.Left => new Point(pt.X, pt.Y + port.Size.Height / 2),
            _ => pt,
        };
    }
}
