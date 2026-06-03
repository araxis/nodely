using Nodely.Models;

namespace Nodely.Geometry;

/// <summary>
/// Factory helpers for building <see cref="IShape"/> instances from a position/size or from a node/port.
/// </summary>
public static class Shapes
{
    /// <summary>The bounding rectangle shape of <paramref name="node"/>.</summary>
    public static IShape Rectangle(NodeModel node) => Rectangle(node.Position, node.Size!);

    /// <summary>The inscribed circle shape of <paramref name="node"/>.</summary>
    public static IShape Circle(NodeModel node) => Circle(node.Position, node.Size!);

    /// <summary>The inscribed ellipse shape of <paramref name="node"/>.</summary>
    public static IShape Ellipse(NodeModel node) => Ellipse(node.Position, node.Size!);

    /// <summary>The bounding rectangle shape of <paramref name="port"/>.</summary>
    public static IShape Rectangle(PortModel port) => Rectangle(port.Position, port.Size);

    /// <summary>The inscribed circle shape of <paramref name="port"/>.</summary>
    public static IShape Circle(PortModel port) => Circle(port.Position, port.Size);

    /// <summary>The inscribed ellipse shape of <paramref name="port"/>.</summary>
    public static IShape Ellipse(PortModel port) => Ellipse(port.Position, port.Size);

    /// <summary>A rectangle at <paramref name="position"/> with the given <paramref name="size"/>.</summary>
    public static IShape Rectangle(Point position, Size size) => new Rectangle(position, size);

    /// <summary>A circle inscribed in the box at <paramref name="position"/> with the given <paramref name="size"/>.</summary>
    public static IShape Circle(Point position, Size size)
    {
        var halfWidth = size.Width / 2;
        var centerX = position.X + halfWidth;
        var centerY = position.Y + size.Height / 2;
        return new Ellipse(centerX, centerY, halfWidth, halfWidth);
    }

    /// <summary>An ellipse inscribed in the box at <paramref name="position"/> with the given <paramref name="size"/>.</summary>
    public static IShape Ellipse(Point position, Size size)
    {
        var halfWidth = size.Width / 2;
        var halfHeight = size.Height / 2;
        var centerX = position.X + halfWidth;
        var centerY = position.Y + halfHeight;
        return new Ellipse(centerX, centerY, halfWidth, halfHeight);
    }
}
