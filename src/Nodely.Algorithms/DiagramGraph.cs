using System.Collections.Generic;
using Nodely.Anchors;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Algorithms;

/// <summary>Graph queries over a diagram's nodes and links (traversal, components, adjacency).</summary>
public static class DiagramGraph
{
    /// <summary>The directed edges (from-node, to-node) implied by the diagram's links.</summary>
    public static IReadOnlyList<(NodeModel From, NodeModel To)> GetEdges(Diagram diagram)
    {
        var edges = new List<(NodeModel, NodeModel)>();
        foreach (var link in diagram.Links)
        {
            var from = NodeOf(link.Source);
            var to = NodeOf(link.Target);
            if (from != null && to != null && !ReferenceEquals(from, to))
                edges.Add((from, to));
        }

        return edges;
    }

    /// <summary>The node an anchor attaches to (port's parent or the node), or null for a free position.</summary>
    public static NodeModel? NodeOf(Anchor anchor) => anchor switch
    {
        SinglePortAnchor spa => spa.Port.Parent,
        ShapeIntersectionAnchor sia => sia.Node,
        _ => null,
    };

    /// <summary>Breadth-first traversal order from <paramref name="start"/>.</summary>
    public static IReadOnlyList<NodeModel> Bfs(Diagram diagram, NodeModel start, bool directed = true)
    {
        var adjacency = BuildAdjacency(diagram, directed);
        var visited = new HashSet<NodeModel> { start };
        var queue = new Queue<NodeModel>();
        queue.Enqueue(start);
        var order = new List<NodeModel>();

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            order.Add(node);
            foreach (var next in adjacency[node])
                if (visited.Add(next))
                    queue.Enqueue(next);
        }

        return order;
    }

    /// <summary>Depth-first traversal order from <paramref name="start"/>.</summary>
    public static IReadOnlyList<NodeModel> Dfs(Diagram diagram, NodeModel start, bool directed = true)
    {
        var adjacency = BuildAdjacency(diagram, directed);
        var visited = new HashSet<NodeModel>();
        var order = new List<NodeModel>();
        var stack = new Stack<NodeModel>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (!visited.Add(node))
                continue;

            order.Add(node);
            foreach (var next in adjacency[node])
                if (!visited.Contains(next))
                    stack.Push(next);
        }

        return order;
    }

    /// <summary>The connected components of the diagram (treating links as undirected).</summary>
    public static IReadOnlyList<IReadOnlyList<NodeModel>> ConnectedComponents(Diagram diagram)
    {
        var adjacency = BuildAdjacency(diagram, directed: false);
        var visited = new HashSet<NodeModel>();
        var components = new List<IReadOnlyList<NodeModel>>();

        foreach (var start in diagram.Nodes)
        {
            if (visited.Contains(start))
                continue;

            var component = new List<NodeModel>();
            var stack = new Stack<NodeModel>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (!visited.Add(node))
                    continue;

                component.Add(node);
                foreach (var next in adjacency[node])
                    if (!visited.Contains(next))
                        stack.Push(next);
            }

            components.Add(component);
        }

        return components;
    }

    internal static Dictionary<NodeModel, List<NodeModel>> BuildAdjacency(Diagram diagram, bool directed)
    {
        var adjacency = new Dictionary<NodeModel, List<NodeModel>>();
        foreach (var node in diagram.Nodes)
            adjacency[node] = new List<NodeModel>();

        foreach (var (from, to) in GetEdges(diagram))
        {
            if (!adjacency.ContainsKey(from)) adjacency[from] = new List<NodeModel>();
            if (!adjacency.ContainsKey(to)) adjacency[to] = new List<NodeModel>();

            adjacency[from].Add(to);
            if (!directed)
                adjacency[to].Add(from);
        }

        return adjacency;
    }
}
