namespace Nodely.Geometry;

/// <summary>An immutable width/height pair in diagram space.</summary>
public record Size
{
    /// <summary>An empty size (0, 0).</summary>
    public static Size Zero { get; } = new(0, 0);

    /// <summary>Creates a size.</summary>
    public Size(double width, double height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>The width.</summary>
    public double Width { get; init; }

    /// <summary>The height.</summary>
    public double Height { get; init; }
}
