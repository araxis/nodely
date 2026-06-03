using Nodely.Geometry;

namespace Nodely.Models;

/// <summary>
/// A small shape (arrowhead, square, circle, …) drawn at a link end. The shape is described as
/// rendering-neutral <see cref="PathData"/> (not an SVG string), so the Avalonia layer draws it directly.
/// </summary>
public class LinkMarker
{
    /// <summary>Creates a marker from its shape and width.</summary>
    public LinkMarker(PathData path, double width)
    {
        Path = path;
        Width = width;
    }

    /// <summary>The marker shape, drawn with the link end at the origin pointing along +X.</summary>
    public PathData Path { get; }

    /// <summary>The marker width (used to shorten the link so it doesn't overlap the marker).</summary>
    public double Width { get; }

    /// <summary>A default filled arrowhead.</summary>
    public static LinkMarker Arrow { get; } = NewArrow(10, 10);

    /// <summary>A default square marker.</summary>
    public static LinkMarker Square { get; } = NewRectangle(10, 10);

    /// <summary>A default filled circle marker.</summary>
    public static LinkMarker Circle { get; } = NewCircle(10);

    /// <summary>Builds an arrowhead marker of the given width/height.</summary>
    public static LinkMarker NewArrow(double width, double height)
        => new(new PathData()
            .MoveTo(new Point(0, -height / 2))
            .LineTo(new Point(width, 0))
            .LineTo(new Point(0, height / 2)), width);

    /// <summary>Builds a rectangular marker of the given width/height.</summary>
    public static LinkMarker NewRectangle(double width, double height)
        => new(new PathData()
            .MoveTo(new Point(0, -height / 2))
            .LineTo(new Point(width, -height / 2))
            .LineTo(new Point(width, height / 2))
            .LineTo(new Point(0, height / 2))
            .Close(), width);

    /// <summary>Builds a square marker of the given size.</summary>
    public static LinkMarker NewSquare(double size) => NewRectangle(size, size);

    /// <summary>
    /// Builds a circular marker of the given diameter, approximated with four cubic Béziers (the link end at
    /// the origin, the circle extending to (diameter, 0) along +X).
    /// </summary>
    public static LinkMarker NewCircle(double diameter)
    {
        var r = diameter / 2;
        var k = 0.5522847498307936 * r; // cubic-Bézier circle constant × radius

        var path = new PathData()
            .MoveTo(new Point(0, 0))
            .CubicTo(new Point(0, -k), new Point(r - k, -r), new Point(r, -r))
            .CubicTo(new Point(r + k, -r), new Point(2 * r, -k), new Point(2 * r, 0))
            .CubicTo(new Point(2 * r, k), new Point(r + k, r), new Point(r, r))
            .CubicTo(new Point(r - k, r), new Point(0, k), new Point(0, 0))
            .Close();

        return new LinkMarker(path, diameter);
    }
}
