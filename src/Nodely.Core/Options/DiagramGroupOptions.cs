using Nodely.Models;

namespace Nodely.Options;

/// <summary>Group configuration.</summary>
public class DiagramGroupOptions
{
    /// <summary>Whether interactive grouping is enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>Creates a group around the given children.</summary>
    public GroupFactory Factory { get; set; } = (diagram, children) => new GroupModel(children);
}
