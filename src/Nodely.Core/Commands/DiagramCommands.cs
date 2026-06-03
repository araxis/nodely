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
