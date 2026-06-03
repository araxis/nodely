namespace Nodely.Options;

/// <summary>Controls which model kinds are virtualized (culled when off-screen).</summary>
public class DiagramVirtualizationOptions
{
    /// <summary>Whether virtualization is enabled at all.</summary>
    public bool Enabled { get; set; }

    /// <summary>Virtualize nodes.</summary>
    public bool OnNodes { get; set; } = true;

    /// <summary>Virtualize groups.</summary>
    public bool OnGroups { get; set; }

    /// <summary>Virtualize links.</summary>
    public bool OnLinks { get; set; }
}
