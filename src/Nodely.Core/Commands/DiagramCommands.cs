using System.Collections.Generic;
using System.Linq;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Commands;

/// <summary>Adds a node to the diagram.</summary>
public sealed class AddNodeCommand : IDiagramCommand
{
    private readonly Diagram _diagram;
    private readonly NodeModel _node;

    /// <summary>Creates the command.</summary>
    public AddNodeCommand(Diagram diagram, NodeModel node)
    {
        _diagram = diagram;
        _node = node;
    }

    /// <inheritdoc />
    public void Execute() => _diagram.Nodes.Add(_node);

    /// <inheritdoc />
    public void Undo() => _diagram.Nodes.Remove(_node);
}

/// <summary>Removes a node (and restores its links on undo).</summary>
public sealed class RemoveNodeCommand : IDiagramCommand
{
    private readonly Diagram _diagram;
    private readonly NodeModel _node;
    private List<BaseLinkModel> _removedLinks = new();

    /// <summary>Creates the command.</summary>
    public RemoveNodeCommand(Diagram diagram, NodeModel node)
    {
        _diagram = diagram;
        _node = node;
    }

    /// <inheritdoc />
    public void Execute()
    {
        _removedLinks = _node.PortLinks.Concat(_node.Links).Distinct().ToList();
        _diagram.Nodes.Remove(_node); // cascades link removal
    }

    /// <inheritdoc />
    public void Undo()
    {
        _diagram.Nodes.Add(_node);
        foreach (var link in _removedLinks)
            _diagram.Links.Add(link);
    }
}

/// <summary>Moves a node from one position to another.</summary>
public sealed class MoveNodeCommand : IDiagramCommand
{
    private readonly NodeModel _node;
    private readonly Point _from;
    private readonly Point _to;

    /// <summary>Creates the command.</summary>
    public MoveNodeCommand(NodeModel node, Point from, Point to)
    {
        _node = node;
        _from = from;
        _to = to;
    }

    /// <inheritdoc />
    public void Execute() => _node.SetPosition(_to.X, _to.Y);

    /// <inheritdoc />
    public void Undo() => _node.SetPosition(_from.X, _from.Y);
}

/// <summary>Adds a link to the diagram.</summary>
public sealed class AddLinkCommand : IDiagramCommand
{
    private readonly Diagram _diagram;
    private readonly BaseLinkModel _link;

    /// <summary>Creates the command.</summary>
    public AddLinkCommand(Diagram diagram, BaseLinkModel link)
    {
        _diagram = diagram;
        _link = link;
    }

    /// <inheritdoc />
    public void Execute() => _diagram.Links.Add(_link);

    /// <inheritdoc />
    public void Undo() => _diagram.Links.Remove(_link);
}

/// <summary>Removes a link from the diagram.</summary>
public sealed class RemoveLinkCommand : IDiagramCommand
{
    private readonly Diagram _diagram;
    private readonly BaseLinkModel _link;

    /// <summary>Creates the command.</summary>
    public RemoveLinkCommand(Diagram diagram, BaseLinkModel link)
    {
        _diagram = diagram;
        _link = link;
    }

    /// <inheritdoc />
    public void Execute() => _diagram.Links.Remove(_link);

    /// <inheritdoc />
    public void Undo() => _diagram.Links.Add(_link);
}

/// <summary>Adds a group around existing nodes.</summary>
public sealed class AddGroupCommand : IDiagramCommand
{
    private readonly Diagram _diagram;
    private readonly GroupModel _group;
    private readonly IReadOnlyList<NodeModel> _children;

    /// <summary>Creates the command.</summary>
    public AddGroupCommand(Diagram diagram, GroupModel group)
    {
        _diagram = diagram;
        _group = group;
        _children = group.Children.ToArray();
    }

    /// <inheritdoc />
    public void Execute()
    {
        RestoreChildren();
        if (!_diagram.Groups.Contains(_group))
            _diagram.Groups.Add(_group);
    }

    /// <inheritdoc />
    public void Undo()
    {
        if (_diagram.Groups.Contains(_group))
            _diagram.Groups.Remove(_group);
    }

    private void RestoreChildren()
    {
        foreach (var child in _children)
            if (!_group.Children.Contains(child))
                _group.AddChild(child);
    }
}

/// <summary>Removes a group while keeping its child nodes on the diagram.</summary>
public sealed class RemoveGroupCommand : IDiagramCommand
{
    private readonly Diagram _diagram;
    private readonly GroupModel _group;
    private IReadOnlyList<NodeModel>? _children;

    /// <summary>Creates the command.</summary>
    public RemoveGroupCommand(Diagram diagram, GroupModel group)
    {
        _diagram = diagram;
        _group = group;
    }

    /// <inheritdoc />
    public void Execute()
    {
        _children ??= _group.Children.ToArray();
        if (_diagram.Groups.Contains(_group))
            _diagram.Groups.Remove(_group);
    }

    /// <inheritdoc />
    public void Undo()
    {
        foreach (var child in _children ?? Enumerable.Empty<NodeModel>())
            if (!_group.Children.Contains(child))
                _group.AddChild(child);

        if (!_diagram.Groups.Contains(_group))
            _diagram.Groups.Add(_group);
    }
}

/// <summary>Adds a bend point to a link.</summary>
public sealed class AddLinkVertexCommand : IDiagramCommand
{
    private readonly BaseLinkModel _link;
    private readonly LinkVertexModel _vertex;
    private readonly int _index;

    /// <summary>Creates the command.</summary>
    public AddLinkVertexCommand(BaseLinkModel link, LinkVertexModel vertex, int index)
    {
        _link = link;
        _vertex = vertex;
        _index = index;
    }

    /// <inheritdoc />
    public void Execute()
    {
        if (_link.Vertices.Contains(_vertex))
            return;

        var index = _index < 0 ? 0 : _index > _link.Vertices.Count ? _link.Vertices.Count : _index;
        _link.Vertices.Insert(index, _vertex);
        _link.Refresh();
    }

    /// <inheritdoc />
    public void Undo()
    {
        if (_link.Vertices.Remove(_vertex))
            _link.Refresh();
    }
}

/// <summary>Removes a bend point from a link.</summary>
public sealed class RemoveLinkVertexCommand : IDiagramCommand
{
    private readonly BaseLinkModel _link;
    private readonly LinkVertexModel _vertex;
    private int _index;

    /// <summary>Creates the command.</summary>
    public RemoveLinkVertexCommand(BaseLinkModel link, LinkVertexModel vertex)
    {
        _link = link;
        _vertex = vertex;
        _index = link.Vertices.IndexOf(vertex);
    }

    /// <inheritdoc />
    public void Execute()
    {
        _index = _link.Vertices.IndexOf(_vertex);
        if (_index >= 0)
        {
            _link.Vertices.RemoveAt(_index);
            _link.Refresh();
        }
    }

    /// <inheritdoc />
    public void Undo()
    {
        if (_link.Vertices.Contains(_vertex))
            return;

        var index = _index < 0 ? _link.Vertices.Count : _index > _link.Vertices.Count ? _link.Vertices.Count : _index;
        _link.Vertices.Insert(index, _vertex);
        _link.Refresh();
    }
}

/// <summary>Restores a diagram's selectable z-order snapshot.</summary>
public sealed class SetModelOrdersCommand : IDiagramCommand
{
    private readonly Diagram _diagram;
    private readonly IReadOnlyDictionary<SelectableModel, int> _before;
    private readonly IReadOnlyDictionary<SelectableModel, int> _after;

    /// <summary>Creates the command.</summary>
    public SetModelOrdersCommand(
        Diagram diagram,
        IReadOnlyDictionary<SelectableModel, int> before,
        IReadOnlyDictionary<SelectableModel, int> after)
    {
        _diagram = diagram;
        _before = before;
        _after = after;
    }

    /// <inheritdoc />
    public void Execute() => Apply(_after);

    /// <inheritdoc />
    public void Undo() => Apply(_before);

    private void Apply(IReadOnlyDictionary<SelectableModel, int> orders)
    {
        _diagram.Batch(() =>
        {
            _diagram.SuspendSorting = true;
            foreach (var entry in orders)
                entry.Key.Order = entry.Value;
            _diagram.SuspendSorting = false;
            _diagram.RefreshOrders(refresh: false);
        });
    }
}

/// <summary>Groups several commands into a single reversible unit (undone in reverse order).</summary>
public sealed class CompositeCommand : IDiagramCommand
{
    private readonly IReadOnlyList<IDiagramCommand> _commands;

    /// <summary>Creates a composite of <paramref name="commands"/>.</summary>
    public CompositeCommand(IReadOnlyList<IDiagramCommand> commands) => _commands = commands;

    /// <inheritdoc />
    public void Execute()
    {
        foreach (var command in _commands)
            command.Execute();
    }

    /// <inheritdoc />
    public void Undo()
    {
        for (var i = _commands.Count - 1; i >= 0; i--)
            _commands[i].Undo();
    }
}
