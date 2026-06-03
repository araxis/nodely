using System.Collections.Generic;

namespace Nodely.Geometry;

/// <summary>A 2D shape that links can intersect and attach to.</summary>
public interface IShape
{
    /// <summary>Returns the points where the shape's boundary crosses <paramref name="line"/>.</summary>
    IEnumerable<Point> GetIntersectionsWithLine(Line line);

    /// <summary>Returns the boundary point at the given angle (degrees), or null if undefined.</summary>
    Point? GetPointAtAngle(double a);
}
