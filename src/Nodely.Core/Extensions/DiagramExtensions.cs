using System.Collections.Generic;
using System.Linq;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Extensions;

/// <summary>Helpers over collections of diagram models.</summary>
public static class DiagramExtensions
{
    /// <summary>
    /// The combined bounds of the given nodes (ignoring nodes that don't have a size yet). Returns
    /// <see cref="Rectangle.Zero"/> when the sequence is empty.
    /// </summary>
    public static Rectangle GetBounds(this IEnumerable<NodeModel> nodes)
    {
        if (!nodes.Any())
            return Rectangle.Zero;

        var minX = double.MaxValue;
        var maxX = double.MinValue;
        var minY = double.MaxValue;
        var maxY = double.MinValue;

        foreach (var node in nodes)
        {
            if (node.Size == null)
                continue;

            var trX = node.Position.X + node.Size!.Width;
            var bY = node.Position.Y + node.Size.Height;

            if (node.Position.X < minX) minX = node.Position.X;
            if (trX > maxX) maxX = trX;
            if (node.Position.Y < minY) minY = node.Position.Y;
            if (bY > maxY) maxY = bY;
        }

        return new Rectangle(minX, minY, maxX, maxY);
    }
}
