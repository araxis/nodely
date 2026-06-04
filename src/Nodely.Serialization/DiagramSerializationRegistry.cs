using System;
using System.Collections.Generic;
using Nodely.Anchors;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Serialization;

/// <summary>Registry used by optional packages to restore typed models from diagram snapshots.</summary>
public sealed class DiagramSerializationRegistry
{
    private readonly Dictionary<string, Func<NodeSnapshot, NodeModel>> _nodeFactories = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<PortSnapshot, NodeModel, PortModel>> _portFactories = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<LinkSnapshot, Anchor, Anchor, BaseLinkModel>> _linkFactories = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<GroupSnapshot, IReadOnlyList<NodeModel>, GroupModel>> _groupFactories = new(StringComparer.Ordinal);

    /// <summary>Registers a node restore factory for a stable snapshot kind.</summary>
    public DiagramSerializationRegistry RegisterNode<TNode>(string kind, Func<NodeSnapshot, TNode> factory)
        where TNode : NodeModel
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        _nodeFactories[Normalize(kind)] = snapshot => factory(snapshot);
        return this;
    }

    /// <summary>Registers a port factory for a stable snapshot kind.</summary>
    public DiagramSerializationRegistry RegisterPort<TPort>(string kind, Func<PortSnapshot, NodeModel, TPort> factory)
        where TPort : PortModel
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        _portFactories[Normalize(kind)] = (snapshot, parent) => factory(snapshot, parent);
        return this;
    }

    /// <summary>Registers a link factory for a stable snapshot kind.</summary>
    public DiagramSerializationRegistry RegisterLink<TLink>(string kind, Func<LinkSnapshot, Anchor, Anchor, TLink> factory)
        where TLink : BaseLinkModel
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        _linkFactories[Normalize(kind)] = (snapshot, source, target) => factory(snapshot, source, target);
        return this;
    }

    /// <summary>Registers a group factory for a stable snapshot kind.</summary>
    public DiagramSerializationRegistry RegisterGroup<TGroup>(string kind, Func<GroupSnapshot, IReadOnlyList<NodeModel>, TGroup> factory)
        where TGroup : GroupModel
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        _groupFactories[Normalize(kind)] = (snapshot, children) => factory(snapshot, children);
        return this;
    }

    internal NodeModel? CreateNode(NodeSnapshot snapshot)
        => _nodeFactories.TryGetValue(snapshot.Kind, out var factory) ? factory(snapshot) : null;

    internal PortModel? CreatePort(PortSnapshot snapshot, NodeModel parent)
        => _portFactories.TryGetValue(snapshot.Kind, out var factory) ? factory(snapshot, parent) : null;

    internal BaseLinkModel? CreateLink(LinkSnapshot snapshot, Anchor source, Anchor target)
        => _linkFactories.TryGetValue(snapshot.Kind, out var factory) ? factory(snapshot, source, target) : null;

    internal GroupModel? CreateGroup(GroupSnapshot snapshot, IReadOnlyList<NodeModel> children)
        => _groupFactories.TryGetValue(snapshot.Kind, out var factory) ? factory(snapshot, children) : null;

    private static string Normalize(string kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
            throw new ArgumentException("A snapshot kind is required.", nameof(kind));

        return kind.Trim();
    }
}
