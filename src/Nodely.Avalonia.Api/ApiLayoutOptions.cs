namespace Nodely.Avalonia.Api;

/// <summary>Options for API diagram arrange helpers.</summary>
public sealed class ApiLayoutOptions
{
    public double OriginX { get; set; }

    public double OriginY { get; set; }

    public double ColumnSpacing { get; set; } = 285;

    public double RowSpacing { get; set; } = 44;

    public double DefaultNodeWidth { get; set; } = ApiVisualMetrics.EndpointWidth;

    public double DefaultNodeHeight { get; set; } = ApiVisualMetrics.EndpointHeight;
}
