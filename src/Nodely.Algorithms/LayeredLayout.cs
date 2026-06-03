using System;
using System.Collections.Generic;
using System.Linq;
using Nodely.Models;

namespace Nodely.Algorithms;

/// <summary>Options for <see cref="LayeredLayout"/>.</summary>
public sealed class LayeredLayoutOptions
{
    /// <summary>Gap between successive layers (edge-to-edge, along the layer-advance axis).</summary>
    public double LayerSpacing { get; set; } = 120;

    /// <summary>Gap between adjacent nodes within a layer (edge-to-edge, across the layer).</summary>
    public double NodeSpacing { get; set; } = 40;

    /// <summary>When true, layers advance left→right (x) and nodes stack down (y); otherwise the reverse.</summary>
    public bool Horizontal { get; set; } = true;

    /// <summary>The layout origin X (the cross-axis center when <see cref="Horizontal"/> is false).</summary>
    public double OriginX { get; set; }

    /// <summary>The layout origin Y (the cross-axis center when <see cref="Horizontal"/> is true).</summary>
    public double OriginY { get; set; }

    /// <summary>Barycenter sweeps used to reduce edge crossings (0 disables ordering). More = tidier, slower.</summary>
    public int CrossingIterations { get; set; } = 4;

    /// <summary>Width assumed for nodes that haven't been measured yet (<see cref="NodeModel.Size"/> is null).</summary>
    public double DefaultNodeWidth { get; set; } = 80;

    /// <summary>Height assumed for nodes that haven't been measured yet (<see cref="NodeModel.Size"/> is null).</summary>
    public double DefaultNodeHeight { get; set; } = 40;
}

/// <summary>
/// A layered (Sugiyama-style) auto-layout for directed graphs. It runs four stages: break cycles into a DAG
/// (so cyclic graphs such as state machines lay out instead of collapsing), assign layers by longest path,
/// order nodes within each layer to reduce edge crossings (barycenter heuristic), then place them with
/// size-aware spacing, centering each layer on a common spine. Trees lay out as a clean hierarchy.
/// </summary>
public static class LayeredLayout
{
    /// <summary>Computes a layered layout and applies it to the diagram's nodes.</summary>
    public static void Arrange(Diagram diagram, LayeredLayoutOptions? options = null)
    {
        if (diagram == null)
            throw new ArgumentNullException(nameof(diagram));

        options ??= new LayeredLayoutOptions();

        var nodes = diagram.Nodes.ToList();
        if (nodes.Count == 0)
            return;

        var edges = BuildEdges(diagram, nodes);
        var dag = BreakCycles(nodes, edges);
        var layer = AssignLayers(nodes, dag);
        var byLayer = GroupByLayer(nodes, layer);
        OrderLayers(byLayer, edges, layer, options.CrossingIterations);
        Apply(diagram, byLayer, options);
    }

    /// <summary>Distinct directed edges among <paramref name="nodes"/> (self-loops already excluded upstream).</summary>
    private static List<(NodeModel From, NodeModel To)> BuildEdges(Diagram diagram, List<NodeModel> nodes)
    {
        var present = new HashSet<NodeModel>(nodes);
        var seen = new HashSet<(NodeModel, NodeModel)>();
        var edges = new List<(NodeModel, NodeModel)>();

        foreach (var (from, to) in DiagramGraph.GetEdges(diagram))
            if (present.Contains(from) && present.Contains(to) && seen.Add((from, to)))
                edges.Add((from, to));

        return edges;
    }

    /// <summary>
    /// Greedy DFS cycle removal: keeps every edge except those pointing at a node currently on the DFS stack
    /// (back edges), yielding an acyclic out-adjacency. Iterative to stay safe on large/deep graphs.
    /// </summary>
    private static Dictionary<NodeModel, List<NodeModel>> BreakCycles(
        List<NodeModel> nodes, List<(NodeModel From, NodeModel To)> edges)
    {
        var outAdj = NewAdjacency(nodes);
        foreach (var (from, to) in edges)
            outAdj[from].Add(to);

        var dag = NewAdjacency(nodes);
        var state = new Dictionary<NodeModel, byte>(nodes.Count); // 0 unvisited, 1 on-stack, 2 done
        foreach (var n in nodes)
            state[n] = 0;

        foreach (var root in nodes)
        {
            if (state[root] != 0)
                continue;

            var stack = new Stack<(NodeModel Node, int Next)>();
            stack.Push((root, 0));
            state[root] = 1;

            while (stack.Count > 0)
            {
                var (node, next) = stack.Pop();
                var neighbors = outAdj[node];

                if (next >= neighbors.Count)
                {
                    state[node] = 2;
                    continue;
                }

                stack.Push((node, next + 1));
                var child = neighbors[next];
                if (state[child] == 1)
                    continue; // back edge → drop it (this is what makes the graph acyclic)

                dag[node].Add(child);
                if (state[child] == 0)
                {
                    state[child] = 1;
                    stack.Push((child, 0));
                }
            }
        }

        return dag;
    }

    /// <summary>Longest-path layering over the DAG via Kahn's algorithm (layer = max predecessor layer + 1).</summary>
    private static Dictionary<NodeModel, int> AssignLayers(
        List<NodeModel> nodes, Dictionary<NodeModel, List<NodeModel>> dag)
    {
        var layer = new Dictionary<NodeModel, int>(nodes.Count);
        var indegree = new Dictionary<NodeModel, int>(nodes.Count);
        foreach (var n in nodes)
        {
            layer[n] = 0;
            indegree[n] = 0;
        }

        foreach (var n in nodes)
            foreach (var c in dag[n])
                indegree[c]++;

        var queue = new Queue<NodeModel>(nodes.Where(n => indegree[n] == 0));
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            foreach (var c in dag[node])
            {
                if (layer[c] < layer[node] + 1)
                    layer[c] = layer[node] + 1;
                if (--indegree[c] == 0)
                    queue.Enqueue(c);
            }
        }

        return layer;
    }

    private static List<List<NodeModel>> GroupByLayer(List<NodeModel> nodes, Dictionary<NodeModel, int> layer)
    {
        var max = 0;
        foreach (var n in nodes)
            if (layer[n] > max)
                max = layer[n];

        var byLayer = new List<List<NodeModel>>(max + 1);
        for (var i = 0; i <= max; i++)
            byLayer.Add(new List<NodeModel>());

        foreach (var n in nodes)
            byLayer[layer[n]].Add(n);

        return byLayer;
    }

    /// <summary>Reduces crossings by repeatedly reordering each layer to its neighbors' barycenter (down then up).</summary>
    private static void OrderLayers(
        List<List<NodeModel>> byLayer,
        List<(NodeModel From, NodeModel To)> edges,
        Dictionary<NodeModel, int> layer,
        int iterations)
    {
        if (byLayer.Count < 2 || iterations <= 0)
            return;

        var neighbors = new Dictionary<NodeModel, List<NodeModel>>();
        foreach (var list in byLayer)
            foreach (var n in list)
                neighbors[n] = new List<NodeModel>();
        foreach (var (from, to) in edges)
        {
            neighbors[from].Add(to);
            neighbors[to].Add(from);
        }

        var order = new Dictionary<NodeModel, int>();
        RefreshOrder(byLayer, order);

        for (var iteration = 0; iteration < iterations; iteration++)
        {
            for (var l = 1; l < byLayer.Count; l++)
                SortByBarycenter(byLayer[l], neighbors, layer, order, l - 1);
            RefreshOrder(byLayer, order);

            for (var l = byLayer.Count - 2; l >= 0; l--)
                SortByBarycenter(byLayer[l], neighbors, layer, order, l + 1);
            RefreshOrder(byLayer, order);
        }
    }

    private static void RefreshOrder(List<List<NodeModel>> byLayer, Dictionary<NodeModel, int> order)
    {
        foreach (var list in byLayer)
            for (var i = 0; i < list.Count; i++)
                order[list[i]] = i;
    }

    private static void SortByBarycenter(
        List<NodeModel> list,
        Dictionary<NodeModel, List<NodeModel>> neighbors,
        Dictionary<NodeModel, int> layer,
        Dictionary<NodeModel, int> order,
        int adjacentLayer)
    {
        var barycenter = new Dictionary<NodeModel, double>(list.Count);
        for (var i = 0; i < list.Count; i++)
        {
            var node = list[i];
            double sum = 0;
            var count = 0;
            foreach (var neighbor in neighbors[node])
                if (layer[neighbor] == adjacentLayer)
                {
                    sum += order[neighbor];
                    count++;
                }

            // No neighbor in the adjacent layer → keep current position (stable OrderBy preserves ties).
            barycenter[node] = count == 0 ? i : sum / count;
        }

        var sorted = list.OrderBy(n => barycenter[n]).ToList();
        list.Clear();
        list.AddRange(sorted);
    }

    private static void Apply(Diagram diagram, List<List<NodeModel>> byLayer, LayeredLayoutOptions o)
    {
        // Layer-axis coordinate of each layer, advancing by the thickest node in the previous layer + the gap.
        var layerCoord = new double[byLayer.Count];
        var cursor = o.Horizontal ? o.OriginX : o.OriginY;
        for (var l = 0; l < byLayer.Count; l++)
        {
            layerCoord[l] = cursor;
            double thickness = 0;
            foreach (var node in byLayer[l])
                thickness = Math.Max(thickness, LayerThickness(node, o));
            if (thickness <= 0)
                thickness = o.Horizontal ? o.DefaultNodeWidth : o.DefaultNodeHeight;
            cursor += thickness + o.LayerSpacing;
        }

        var crossCenter = o.Horizontal ? o.OriginY : o.OriginX;

        diagram.Batch(() =>
        {
            for (var l = 0; l < byLayer.Count; l++)
            {
                var list = byLayer[l];

                double total = 0;
                for (var i = 0; i < list.Count; i++)
                {
                    if (i > 0)
                        total += o.NodeSpacing;
                    total += CrossExtent(list[i], o);
                }

                var run = crossCenter - total / 2; // center this layer's stack on the shared spine
                foreach (var node in list)
                {
                    if (o.Horizontal)
                        node.SetPosition(layerCoord[l], run);
                    else
                        node.SetPosition(run, layerCoord[l]);
                    run += CrossExtent(node, o) + o.NodeSpacing;
                }
            }
        });
    }

    private static double LayerThickness(NodeModel n, LayeredLayoutOptions o)
        => o.Horizontal ? (n.Size?.Width ?? o.DefaultNodeWidth) : (n.Size?.Height ?? o.DefaultNodeHeight);

    private static double CrossExtent(NodeModel n, LayeredLayoutOptions o)
        => o.Horizontal ? (n.Size?.Height ?? o.DefaultNodeHeight) : (n.Size?.Width ?? o.DefaultNodeWidth);

    private static Dictionary<NodeModel, List<NodeModel>> NewAdjacency(List<NodeModel> nodes)
    {
        var adjacency = new Dictionary<NodeModel, List<NodeModel>>(nodes.Count);
        foreach (var n in nodes)
            adjacency[n] = new List<NodeModel>();
        return adjacency;
    }
}
