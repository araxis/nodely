using System.Linq;
using Nodely.Geometry;
using Nodely.Models.Base;

namespace Nodely.Routers;

/// <summary>The simplest router: the route is just the link's user-defined vertices (if any).</summary>
public class NormalRouter : Router
{
    /// <inheritdoc />
    public override Point[] GetRoute(Diagram diagram, BaseLinkModel link)
        => link.Vertices.Select(v => v.Position).ToArray();
}
