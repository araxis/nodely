using Nodely.Geometry;

namespace Nodely.PathGenerators;

/// <summary>
/// The output of a <see cref="PathGenerator"/>: the full drawable path, its per-segment sub-paths, and the
/// resolved marker angles/positions. Uses rendering-neutral <see cref="PathData"/> (no SVG strings).
/// </summary>
public class PathGeneratorResult
{
    /// <summary>Creates a result.</summary>
    public PathGeneratorResult(
        PathData fullPath,
        PathData[] paths,
        double? sourceMarkerAngle = null,
        Point? sourceMarkerPosition = null,
        double? targetMarkerAngle = null,
        Point? targetMarkerPosition = null)
    {
        FullPath = fullPath;
        Paths = paths;
        SourceMarkerAngle = sourceMarkerAngle;
        SourceMarkerPosition = sourceMarkerPosition;
        TargetMarkerAngle = targetMarkerAngle;
        TargetMarkerPosition = targetMarkerPosition;
    }

    /// <summary>The complete path from source to target.</summary>
    public PathData FullPath { get; }

    /// <summary>The individual sub-paths (one per segment), e.g. for per-segment labels.</summary>
    public PathData[] Paths { get; }

    /// <summary>The angle (degrees) at which the source marker should be drawn.</summary>
    public double? SourceMarkerAngle { get; }

    /// <summary>The position at which the source marker should be drawn.</summary>
    public Point? SourceMarkerPosition { get; }

    /// <summary>The angle (degrees) at which the target marker should be drawn.</summary>
    public double? TargetMarkerAngle { get; }

    /// <summary>The position at which the target marker should be drawn.</summary>
    public Point? TargetMarkerPosition { get; }
}
