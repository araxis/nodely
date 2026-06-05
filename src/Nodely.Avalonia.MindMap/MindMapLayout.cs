using System;
using System.Collections.Generic;
using System.Linq;
using Nodely;
using Nodely.Anchors;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Avalonia.MindMap;

/// <summary>Simple mind-map arrange and collapse helpers.</summary>
public static class MindMapLayout
{
    /// <summary>Arranges the mind-map around its root and reapplies collapse visibility.</summary>
    public static void Arrange(Diagram diagram, MindMapLayoutOptions? options = null)
    {
        if (diagram is null)
            throw new ArgumentNullException(nameof(diagram));

        options ??= new MindMapLayoutOptions();
        var topics = diagram.Nodes.OfType<MindMapTopicNode>().ToList();
        if (topics.Count == 0)
            return;

        var graph = BuildBranchGraph(diagram);
        var root = topics.OfType<MindMapRootNode>().FirstOrDefault()
            ?? topics.FirstOrDefault(topic => !graph.ParentOf.ContainsKey(topic))
            ?? topics[0];

        var rootSize = SizeOf(root, options);
        root.SetPosition(options.OriginX - rootSize.Width / 2, options.OriginY - rootSize.Height / 2);

        var children = graph.ChildrenOf.TryGetValue(root, out var rootChildren)
            ? rootChildren
            : new List<MindMapTopicNode>();
        var left = new List<MindMapTopicNode>();
        var right = new List<MindMapTopicNode>();
        var autoIndex = 0;

        foreach (var child in children)
        {
            var side = child.Side;
            if (side == MindMapTopicSide.Auto)
                side = autoIndex++ % 2 == 0 ? MindMapTopicSide.Right : MindMapTopicSide.Left;

            if (side == MindMapTopicSide.Left)
                left.Add(child);
            else
                right.Add(child);
        }

        ArrangeSide(left, direction: -1, graph.ChildrenOf, options);
        ArrangeSide(right, direction: 1, graph.ChildrenOf, options);
        ApplyCollapseState(diagram);
    }

    /// <summary>Applies collapse visibility to descendant topics and mind-map links.</summary>
    public static void ApplyCollapseState(Diagram diagram)
    {
        if (diagram is null)
            throw new ArgumentNullException(nameof(diagram));

        var graph = BuildBranchGraph(diagram);
        var topics = diagram.Nodes.OfType<MindMapTopicNode>().ToList();
        foreach (var topic in topics)
        {
            topic.Visible = true;
            foreach (var port in topic.Ports)
                port.Visible = true;
        }

        foreach (var link in diagram.Links.OfType<MindMapLink>())
            link.Visible = true;

        var roots = topics.Where(topic => !graph.ParentOf.ContainsKey(topic)).ToList();
        if (roots.Count == 0 && topics.Count > 0)
            roots.Add(topics[0]);

        foreach (var root in roots)
            ApplyVisibility(root, hiddenByAncestor: false, graph, new HashSet<MindMapTopicNode>());

        foreach (var link in diagram.Links.OfType<MindMapLink>())
        {
            if (link.Kind == MindMapLinkKind.Branch)
                continue;

            var source = TopicFromAnchor(link.Source);
            var target = TopicFromAnchor(link.Target);
            link.Visible = source?.Visible == true && target?.Visible == true;
        }

        diagram.Refresh();
    }

    internal static MindMapTopicNode? TopicFromAnchor(Anchor anchor) => anchor switch
    {
        SinglePortAnchor spa => spa.Port.Parent as MindMapTopicNode,
        ShapeIntersectionAnchor sia => sia.Node as MindMapTopicNode,
        _ => anchor.Model as MindMapTopicNode,
    };

    internal static MindMapBranchGraph BuildBranchGraph(Diagram diagram)
    {
        var childrenOf = new Dictionary<MindMapTopicNode, List<MindMapTopicNode>>();
        var parentOf = new Dictionary<MindMapTopicNode, MindMapTopicNode>();
        var branchLinkByChild = new Dictionary<MindMapTopicNode, MindMapLink>();

        foreach (var link in diagram.Links.OfType<MindMapLink>().Where(link => link.Kind == MindMapLinkKind.Branch))
        {
            var source = TopicFromAnchor(link.Source);
            var target = TopicFromAnchor(link.Target);
            if (source == null || target == null || ReferenceEquals(source, target))
                continue;

            if (!childrenOf.TryGetValue(source, out var children))
            {
                children = new List<MindMapTopicNode>();
                childrenOf[source] = children;
            }

            if (!children.Contains(target))
                children.Add(target);

            if (!parentOf.ContainsKey(target))
                parentOf[target] = source;
            if (!branchLinkByChild.ContainsKey(target))
                branchLinkByChild[target] = link;
        }

        return new MindMapBranchGraph(childrenOf, parentOf, branchLinkByChild);
    }

    private static void ArrangeSide(
        IReadOnlyList<MindMapTopicNode> nodes,
        int direction,
        IReadOnlyDictionary<MindMapTopicNode, List<MindMapTopicNode>> childrenOf,
        MindMapLayoutOptions options)
    {
        if (nodes.Count == 0)
            return;

        var totalHeight = SumSubtreeHeights(nodes, childrenOf, options);
        var y = options.OriginY - totalHeight / 2;
        foreach (var node in nodes)
        {
            var height = SubtreeHeight(node, childrenOf, options, new HashSet<MindMapTopicNode>());
            ArrangeSubtree(node, direction, depth: 1, centerY: y + height / 2, childrenOf, options, new HashSet<MindMapTopicNode>());
            y += height + options.TopicSpacing;
        }
    }

    private static void ArrangeSubtree(
        MindMapTopicNode node,
        int direction,
        int depth,
        double centerY,
        IReadOnlyDictionary<MindMapTopicNode, List<MindMapTopicNode>> childrenOf,
        MindMapLayoutOptions options,
        HashSet<MindMapTopicNode> seen)
    {
        if (!seen.Add(node))
            return;

        var size = SizeOf(node, options);
        var centerX = options.OriginX + direction * depth * options.LevelSpacing;
        node.SetPosition(centerX - size.Width / 2, centerY - size.Height / 2);

        if (!childrenOf.TryGetValue(node, out var children) || children.Count == 0)
            return;

        var totalHeight = SumSubtreeHeights(children, childrenOf, options);
        var y = centerY - totalHeight / 2;
        foreach (var child in children)
        {
            var childHeight = SubtreeHeight(child, childrenOf, options, new HashSet<MindMapTopicNode>(seen));
            ArrangeSubtree(child, direction, depth + 1, y + childHeight / 2, childrenOf, options, new HashSet<MindMapTopicNode>(seen));
            y += childHeight + options.TopicSpacing;
        }
    }

    private static double SumSubtreeHeights(
        IReadOnlyList<MindMapTopicNode> nodes,
        IReadOnlyDictionary<MindMapTopicNode, List<MindMapTopicNode>> childrenOf,
        MindMapLayoutOptions options)
    {
        var total = 0d;
        for (var i = 0; i < nodes.Count; i++)
        {
            total += SubtreeHeight(nodes[i], childrenOf, options, new HashSet<MindMapTopicNode>());
            if (i < nodes.Count - 1)
                total += options.TopicSpacing;
        }

        return total;
    }

    private static double SubtreeHeight(
        MindMapTopicNode node,
        IReadOnlyDictionary<MindMapTopicNode, List<MindMapTopicNode>> childrenOf,
        MindMapLayoutOptions options,
        HashSet<MindMapTopicNode> seen)
    {
        if (!seen.Add(node))
            return 0;

        var own = SizeOf(node, options).Height;
        if (!childrenOf.TryGetValue(node, out var children) || children.Count == 0)
            return own;

        return Math.Max(own, SumSubtreeHeights(children, childrenOf, options));
    }

    private static TopicSize SizeOf(MindMapTopicNode node, MindMapLayoutOptions options)
    {
        if (node.Size is { } size)
            return new TopicSize(size.Width, size.Height);

        return node switch
        {
            MindMapRootNode => new TopicSize(MindMapVisualMetrics.RootWidth, MindMapVisualMetrics.RootHeight),
            MindMapLeafNode => new TopicSize(MindMapVisualMetrics.LeafWidth, MindMapVisualMetrics.LeafHeight),
            _ => new TopicSize(options.DefaultNodeWidth, options.DefaultNodeHeight),
        };
    }

    private static void ApplyVisibility(
        MindMapTopicNode node,
        bool hiddenByAncestor,
        MindMapBranchGraph graph,
        HashSet<MindMapTopicNode> seen)
    {
        if (!seen.Add(node))
            return;

        node.Visible = !hiddenByAncestor;
        foreach (var port in node.Ports)
            port.Visible = node.Visible;

        if (!graph.ChildrenOf.TryGetValue(node, out var children))
            return;

        foreach (var child in children)
        {
            var childHidden = hiddenByAncestor || node.Collapsed;
            if (graph.BranchLinkByChild.TryGetValue(child, out var link))
                link.Visible = !childHidden && node.Visible;
            ApplyVisibility(child, childHidden, graph, seen);
        }
    }

    internal sealed record MindMapBranchGraph(
        IReadOnlyDictionary<MindMapTopicNode, List<MindMapTopicNode>> ChildrenOf,
        IReadOnlyDictionary<MindMapTopicNode, MindMapTopicNode> ParentOf,
        IReadOnlyDictionary<MindMapTopicNode, MindMapLink> BranchLinkByChild);

    private readonly record struct TopicSize(double Width, double Height);
}
