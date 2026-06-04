using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nodely.Serialization;

/// <summary>A versioned, serializable snapshot of a diagram's structure and viewport.</summary>
public sealed record DiagramSnapshot
{
    /// <summary>The snapshot schema version (bump when the shape changes; migrate on load).</summary>
    public int Version { get; init; } = 2;

    /// <summary>The nodes (excluding groups).</summary>
    public List<NodeSnapshot> Nodes { get; init; } = new();

    /// <summary>The links.</summary>
    public List<LinkSnapshot> Links { get; init; } = new();

    /// <summary>The groups.</summary>
    public List<GroupSnapshot> Groups { get; init; } = new();

    /// <summary>The viewport (pan/zoom), if captured.</summary>
    public ViewportSnapshot? Viewport { get; init; }
}

/// <summary>A serialized node.</summary>
public sealed record NodeSnapshot
{
    /// <summary>The node id.</summary>
    public string Id { get; init; } = "";

    /// <summary>The node's stable model kind.</summary>
    public string Kind { get; init; } = "node";

    /// <summary>X position.</summary>
    public double X { get; init; }

    /// <summary>Y position.</summary>
    public double Y { get; init; }

    /// <summary>Width, if measured.</summary>
    public double? Width { get; init; }

    /// <summary>Height, if measured.</summary>
    public double? Height { get; init; }

    /// <summary>Optional title.</summary>
    public string? Title { get; init; }

    /// <summary>The node's ports.</summary>
    public List<PortSnapshot> Ports { get; init; } = new();

    /// <summary>Custom per-node data from <c>GetExtraData</c> (restored via <c>SetExtraData</c> on load).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

/// <summary>A serialized port.</summary>
public sealed record PortSnapshot
{
    /// <summary>The port id.</summary>
    public string Id { get; init; } = "";

    /// <summary>The port's stable model kind.</summary>
    public string Kind { get; init; } = "port";

    /// <summary>The port alignment (enum name).</summary>
    public string Alignment { get; init; } = "Bottom";

    /// <summary>Custom per-port data.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

/// <summary>A serialized link.</summary>
public sealed record LinkSnapshot
{
    /// <summary>The link id.</summary>
    public string Id { get; init; } = "";

    /// <summary>The link's stable model kind.</summary>
    public string Kind { get; init; } = "link";

    /// <summary>The source endpoint.</summary>
    public EndpointSnapshot Source { get; init; } = new();

    /// <summary>The target endpoint.</summary>
    public EndpointSnapshot Target { get; init; } = new();

    /// <summary>The link's user vertices.</summary>
    public List<PointSnapshot> Vertices { get; init; } = new();

    /// <summary>Custom per-link data.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

/// <summary>A serialized link endpoint (a port, a node, or a free position).</summary>
public sealed record EndpointSnapshot
{
    /// <summary>One of "Port", "Node", or "Position".</summary>
    public string Kind { get; init; } = "Position";

    /// <summary>The port id (when <see cref="Kind"/> is "Port").</summary>
    public string? PortId { get; init; }

    /// <summary>The node id (when <see cref="Kind"/> is "Node").</summary>
    public string? NodeId { get; init; }

    /// <summary>X (when <see cref="Kind"/> is "Position").</summary>
    public double X { get; init; }

    /// <summary>Y (when <see cref="Kind"/> is "Position").</summary>
    public double Y { get; init; }
}

/// <summary>A serialized group.</summary>
public sealed record GroupSnapshot
{
    /// <summary>The group id.</summary>
    public string Id { get; init; } = "";

    /// <summary>The group's stable model kind.</summary>
    public string Kind { get; init; } = "group";

    /// <summary>The ids of the group's child nodes.</summary>
    public List<string> ChildIds { get; init; } = new();

    /// <summary>The group padding.</summary>
    public byte Padding { get; init; } = 30;

    /// <summary>Custom per-group data.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

/// <summary>A serialized viewport.</summary>
public sealed record ViewportSnapshot
{
    /// <summary>Pan X.</summary>
    public double PanX { get; init; }

    /// <summary>Pan Y.</summary>
    public double PanY { get; init; }

    /// <summary>Zoom.</summary>
    public double Zoom { get; init; } = 1;
}

/// <summary>A serialized point.</summary>
public sealed record PointSnapshot
{
    /// <summary>X.</summary>
    public double X { get; init; }

    /// <summary>Y.</summary>
    public double Y { get; init; }
}
