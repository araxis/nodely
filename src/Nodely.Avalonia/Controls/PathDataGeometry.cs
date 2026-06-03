using Avalonia.Media;
using Nodely.Geometry;
using AvPoint = Avalonia.Point;
using AvGeometry = Avalonia.Media.Geometry;

namespace Nodely.Avalonia.Controls;

/// <summary>Converts a rendering-neutral <see cref="PathData"/> into an Avalonia geometry.</summary>
internal static class PathDataGeometry
{
    /// <summary>
    /// Converts the path. Pass <paramref name="filled"/> = true for closed, fillable shapes (e.g. arrowhead
    /// markers); leave false for open stroked paths such as link routes.
    /// </summary>
    public static AvGeometry ToGeometry(PathData data, bool filled = false)
    {
        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();

        var figureOpen = false;
        foreach (var op in data.Operations)
        {
            switch (op.Command)
            {
                case PathCommand.MoveTo:
                    if (figureOpen) ctx.EndFigure(filled);
                    ctx.BeginFigure(P(op.Point), isFilled: filled);
                    figureOpen = true;
                    break;
                case PathCommand.LineTo:
                    ctx.LineTo(P(op.Point));
                    break;
                case PathCommand.CubicTo:
                    ctx.CubicBezierTo(P(op.Control1), P(op.Control2), P(op.Point));
                    break;
                case PathCommand.QuadTo:
                    ctx.QuadraticBezierTo(P(op.Control1), P(op.Point));
                    break;
                case PathCommand.Close:
                    if (figureOpen)
                    {
                        ctx.EndFigure(true);
                        figureOpen = false;
                    }
                    break;
            }
        }

        if (figureOpen)
            ctx.EndFigure(filled);

        return geometry;
    }

    private static AvPoint P(Point? p) => p is null ? default : new AvPoint(p.X, p.Y);
}
