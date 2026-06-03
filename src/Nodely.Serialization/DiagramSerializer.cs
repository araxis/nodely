using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Nodely.Anchors;
using Nodely.Models;
using Nodely.Models.Base;
using NodelyPoint = Nodely.Geometry.Point;
using NodelySize = Nodely.Geometry.Size;

namespace Nodely.Serialization;

/// <summary>Converts a <see cref="Diagram"/> to/from a versioned <see cref="DiagramSnapshot"/> and JSON.</summary>
public static class DiagramSerializer
{
    /// <summary>The current snapshot schema version.</summary>
    public const int CurrentVersion = 1;

    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = false,
    };

    /// <summary>Builds a snapshot from the diagram.</summary>
    public static DiagramSnapshot ToSnapshot(Diagram diagram)
    {
        var nodes = diagram.Nodes.Select(n => new NodeSnapshot
        {
            Id = n.Id,
            Kind = n.GetType().Name,
            X = n.Position.X,
            Y = n.Position.Y,
            Width = n.Size?.Width,
            Height = n.Size?.Height,
            Title = n.Title,
            Ports = n.Ports.Select(p => new PortSnapshot { Id = p.Id, Alignment = p.Alignment.ToString() }).ToList(),
            Extra = BuildExtra(n),
        }).ToList();

        var links = diagram.Links.Select(l => new LinkSnapshot
        {
            Id = l.Id,
            Source = EndpointOf(l.Source),
            Target = EndpointOf(l.Target),
            Vertices = l.Vertices.Select(v => new PointSnapshot { X = v.Position.X, Y = v.Position.Y }).ToList(),
        }).ToList();

        var groups = diagram.Groups.Select(g => new GroupSnapshot
        {
            Id = g.Id,
            ChildIds = g.Children.Select(c => c.Id).ToList(),
            Padding = g.Padding,
        }).ToList();

        return new DiagramSnapshot
        {
            Version = CurrentVersion,
            Nodes = nodes,
            Links = links,
            Groups = groups,
            Viewport = new ViewportSnapshot { PanX = diagram.Pan.X, PanY = diagram.Pan.Y, Zoom = diagram.Zoom },
        };
    }

    /// <summary>Loads a snapshot into the diagram. <paramref name="nodeFactory"/> can create custom node types by <see cref="NodeSnapshot.Kind"/>.</summary>
    public static void Load(Diagram diagram, DiagramSnapshot snapshot, Func<NodeSnapshot, NodeModel>? nodeFactory = null)
    {
        var nodesById = new Dictionary<string, NodeModel>();
        var portsById = new Dictionary<string, PortModel>();

        diagram.Batch(() =>
        {
            foreach (var ns in snapshot.Nodes)
            {
                var node = nodeFactory?.Invoke(ns) ?? new NodeModel(ns.Id, new NodelyPoint(ns.X, ns.Y));
                node.Title = ns.Title;
                if (ns.Width.HasValue && ns.Height.HasValue)
                    node.Size = new NodelySize(ns.Width.Value, ns.Height.Value);

                foreach (var ps in ns.Ports)
                {
                    var alignment = (PortAlignment)Enum.Parse(typeof(PortAlignment), ps.Alignment);
                    var port = node.AddPort(new PortModel(ps.Id, node, alignment, node.Position));
                    portsById[ps.Id] = port;
                }

                if (ns.Extra is { Count: > 0 })
                    node.SetExtraData(ToClrDictionary(ns.Extra));

                nodesById[node.Id] = node;
                diagram.Nodes.Add(node);
            }

            foreach (var gs in snapshot.Groups)
            {
                var children = gs.ChildIds.Where(nodesById.ContainsKey).Select(id => nodesById[id]).ToList();
                diagram.Groups.Add(new GroupModel(gs.Id, children, gs.Padding));
            }

            foreach (var ls in snapshot.Links)
            {
                var source = AnchorFromEndpoint(ls.Source, nodesById, portsById);
                var target = AnchorFromEndpoint(ls.Target, nodesById, portsById);
                if (source == null || target == null)
                    continue;

                var link = new LinkModel(ls.Id, source, target);
                foreach (var v in ls.Vertices)
                    link.AddVertex(new NodelyPoint(v.X, v.Y));
                diagram.Links.Add(link);
            }

            if (snapshot.Viewport is { } vp)
            {
                diagram.SetPan(vp.PanX, vp.PanY);
                if (vp.Zoom > 0)
                    diagram.SetZoom(vp.Zoom);
            }
        });
    }

    /// <summary>Serializes the diagram to JSON.</summary>
    public static string Serialize(Diagram diagram, JsonSerializerOptions? options = null)
        => JsonSerializer.Serialize(ToSnapshot(diagram), options ?? DefaultOptions);

    /// <summary>Deserializes JSON into the diagram.</summary>
    public static void Deserialize(Diagram diagram, string json, Func<NodeSnapshot, NodeModel>? nodeFactory = null, JsonSerializerOptions? options = null)
    {
        var snapshot = JsonSerializer.Deserialize<DiagramSnapshot>(json, options ?? DefaultOptions)
            ?? throw new ArgumentException("The JSON did not contain a diagram snapshot.", nameof(json));
        Load(diagram, snapshot, nodeFactory);
    }

    private static EndpointSnapshot EndpointOf(Anchor anchor) => anchor switch
    {
        SinglePortAnchor spa => new EndpointSnapshot { Kind = "Port", PortId = spa.Port.Id },
        ShapeIntersectionAnchor sia => new EndpointSnapshot { Kind = "Node", NodeId = sia.Node.Id },
        PositionAnchor pa => PositionEndpoint(pa),
        _ => new EndpointSnapshot { Kind = "Position" },
    };

    private static EndpointSnapshot PositionEndpoint(PositionAnchor anchor)
    {
        var p = anchor.GetPlainPosition()!;
        return new EndpointSnapshot { Kind = "Position", X = p.X, Y = p.Y };
    }

    private static Anchor? AnchorFromEndpoint(
        EndpointSnapshot endpoint,
        Dictionary<string, NodeModel> nodes,
        Dictionary<string, PortModel> ports)
        => endpoint.Kind switch
        {
            "Port" => endpoint.PortId != null && ports.TryGetValue(endpoint.PortId, out var port) ? new SinglePortAnchor(port) : null,
            "Node" => endpoint.NodeId != null && nodes.TryGetValue(endpoint.NodeId, out var node) ? new ShapeIntersectionAnchor(node) : null,
            "Position" => new PositionAnchor(new NodelyPoint(endpoint.X, endpoint.Y)),
            _ => null,
        };

    private static Dictionary<string, JsonElement>? BuildExtra(NodeModel node)
    {
        var extra = node.GetExtraData();
        if (extra == null || extra.Count == 0)
            return null;

        var result = new Dictionary<string, JsonElement>(extra.Count);
        foreach (var kvp in extra)
            result[kvp.Key] = JsonSerializer.SerializeToElement(kvp.Value, DefaultOptions);
        return result;
    }

    private static IReadOnlyDictionary<string, object?> ToClrDictionary(Dictionary<string, JsonElement> extra)
    {
        var result = new Dictionary<string, object?>(extra.Count);
        foreach (var kvp in extra)
            result[kvp.Key] = FromElement(kvp.Value);
        return result;
    }

    // JsonElement -> a plain CLR primitive, so node authors read clean values in SetExtraData (not JsonElement).
    private static object? FromElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        _ => element.GetRawText(),
    };
}
