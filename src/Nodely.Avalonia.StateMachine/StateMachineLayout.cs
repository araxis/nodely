using System;
using System.Collections.Generic;
using System.Linq;
using Nodely;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;

namespace Nodely.Avalonia.StateMachine;

/// <summary>Simple left-to-right arrange helper for state-machine diagrams.</summary>
public static class StateMachineLayout
{
    /// <summary>Arranges state-machine nodes by transition reachability.</summary>
    public static void Arrange(Diagram diagram, StateMachineLayoutOptions? options = null)
    {
        if (diagram is null)
            throw new ArgumentNullException(nameof(diagram));

        options ??= new StateMachineLayoutOptions();
        var nodes = diagram.Nodes
            .Where(node => node is StateMachineNodeBase)
            .ToList();
        if (nodes.Count == 0)
            return;

        var graph = BuildTransitionGraph(diagram);
        var roots = nodes.OfType<StateMachineInitialNode>().Cast<NodeModel>().ToList();
        if (roots.Count == 0)
            roots = nodes.Where(node => !graph.Incoming.ContainsKey(node)).ToList();
        if (roots.Count == 0)
            roots.Add(nodes[0]);

        var levels = AssignLevels(nodes, roots, graph.Outgoing);
        var grouped = nodes
            .GroupBy(node => levels.TryGetValue(node, out var level) ? level : 0)
            .OrderBy(group => group.Key)
            .ToList();

        foreach (var group in grouped)
        {
            var column = group.OrderBy(node => node.Position.Y).ThenBy(node => node.Position.X).ToList();
            var totalHeight = column.Sum(node => SizeOf(node, options).Height)
                + Math.Max(0, column.Count - 1) * options.RowSpacing;
            var y = options.OriginY - totalHeight / 2;

            foreach (var node in column)
            {
                var size = SizeOf(node, options);
                node.SetPosition(
                    options.OriginX + group.Key * options.ColumnSpacing - size.Width / 2,
                    y);
                y += size.Height + options.RowSpacing;
            }
        }

        diagram.Refresh();
    }

    internal static StateMachineGraph BuildTransitionGraph(Diagram diagram)
    {
        var outgoing = new Dictionary<NodeModel, List<NodeModel>>();
        var incoming = new Dictionary<NodeModel, List<NodeModel>>();

        foreach (var link in diagram.Links.OfType<StateMachineTransitionLink>())
        {
            if (link.Kind == StateMachineTransitionKind.Self)
                continue;

            var source = NodeFromAnchor(link.Source);
            var target = NodeFromAnchor(link.Target);
            if (source == null || target == null || ReferenceEquals(source, target))
                continue;

            Add(outgoing, source, target);
            Add(incoming, target, source);
        }

        return new StateMachineGraph(outgoing, incoming);
    }

    internal static NodeModel? NodeFromAnchor(Anchor anchor) => anchor switch
    {
        SinglePortAnchor spa => spa.Port.Parent,
        ShapeIntersectionAnchor sia => sia.Node,
        _ => anchor.Model as NodeModel,
    };

    private static Dictionary<NodeModel, int> AssignLevels(
        IReadOnlyList<NodeModel> nodes,
        IReadOnlyList<NodeModel> roots,
        IReadOnlyDictionary<NodeModel, List<NodeModel>> outgoing)
    {
        var levels = new Dictionary<NodeModel, int>();
        var queue = new Queue<NodeModel>();
        foreach (var root in roots)
        {
            levels[root] = 0;
            queue.Enqueue(root);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var nextLevel = levels[current] + 1;
            if (!outgoing.TryGetValue(current, out var targets))
                continue;

            foreach (var target in targets)
            {
                if (!levels.TryGetValue(target, out var existing) || nextLevel > existing)
                {
                    levels[target] = nextLevel;
                    queue.Enqueue(target);
                }
            }
        }

        var fallbackLevel = levels.Count == 0 ? 0 : levels.Values.Max() + 1;
        foreach (var node in nodes)
            if (!levels.ContainsKey(node))
                levels[node] = fallbackLevel++;

        return levels;
    }

    private static NodeSize SizeOf(NodeModel node, StateMachineLayoutOptions options)
    {
        if (node.Size is { } size)
            return new NodeSize(size.Width, size.Height);

        return node switch
        {
            StateMachineInitialNode or StateMachineFinalNode => new NodeSize(StateMachineVisualMetrics.CircleSize, StateMachineVisualMetrics.CircleSize),
            StateMachineChoiceNode => new NodeSize(StateMachineVisualMetrics.ChoiceSize, StateMachineVisualMetrics.ChoiceSize),
            StateMachineNoteNode => new NodeSize(StateMachineVisualMetrics.NoteWidth, StateMachineVisualMetrics.NoteHeight),
            _ => new NodeSize(options.DefaultNodeWidth, options.DefaultNodeHeight),
        };
    }

    private static void Add(Dictionary<NodeModel, List<NodeModel>> map, NodeModel key, NodeModel value)
    {
        if (!map.TryGetValue(key, out var list))
        {
            list = new List<NodeModel>();
            map[key] = list;
        }

        if (!list.Contains(value))
            list.Add(value);
    }

    internal sealed record StateMachineGraph(
        IReadOnlyDictionary<NodeModel, List<NodeModel>> Outgoing,
        IReadOnlyDictionary<NodeModel, List<NodeModel>> Incoming);

    private readonly record struct NodeSize(double Width, double Height);
}
