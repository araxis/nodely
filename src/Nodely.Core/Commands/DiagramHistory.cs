using System;
using System.Collections.Generic;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Commands;

/// <summary>
/// Records reversible edits for a diagram and drives undo/redo. Node adds and completed moves, and link adds,
/// are captured automatically by observing the diagram; deletions are recorded by calling <see cref="Execute"/>
/// at the delete site (so a removed node's links are captured before the cascade). Edits applied while undoing
/// or redoing are not re-recorded. History only covers edits made <em>after</em> the instance is created — the
/// diagram's initial content is treated as the baseline.
/// </summary>
public sealed class DiagramHistory : IDisposable
{
    private readonly Diagram _diagram;
    private readonly UndoRedoStack _stack = new();
    private readonly Dictionary<NodeModel, Point> _lastPosition = new();
    private readonly Dictionary<BaseLinkModel, IDiagramCommand> _bufferedLinkAdds = new();
    private List<IDiagramCommand> _transactionBuffer = new();
    private int _transactionDepth;

    /// <summary>Raised whenever the undo/redo state changes (a record, undo, redo, or clear).</summary>
    public event Action? Changed;

    /// <summary>Creates a history bound to <paramref name="diagram"/>; records edits made from now on.</summary>
    public DiagramHistory(Diagram diagram)
    {
        _diagram = diagram ?? throw new ArgumentNullException(nameof(diagram));

        foreach (var node in diagram.Nodes)
            Track(node);

        diagram.Nodes.Added += OnNodeAdded;
        diagram.Nodes.Removed += OnNodeRemoved;
        diagram.Links.Added += OnLinkAdded;
        diagram.Links.Removed += OnLinkRemoved;
    }

    /// <summary>True while an undo/redo/explicit-execute is being applied — observers skip recording then.</summary>
    public bool IsApplying { get; private set; }

    /// <summary>Whether there's an edit to undo.</summary>
    public bool CanUndo => _stack.CanUndo;

    /// <summary>Whether there's an edit to redo.</summary>
    public bool CanRedo => _stack.CanRedo;

    /// <summary>Executes a command and records it. Use for actions that aren't auto-observed, e.g. deletions.</summary>
    public void Execute(IDiagramCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        Apply(() => _stack.Execute(command));
    }

    /// <summary>Undoes the most recent edit.</summary>
    public void Undo()
    {
        if (_stack.CanUndo)
            Apply(_stack.Undo);
    }

    /// <summary>Redoes the most recently undone edit.</summary>
    public void Redo()
    {
        if (_stack.CanRedo)
            Apply(_stack.Redo);
    }

    /// <summary>Clears all history (e.g. after a load).</summary>
    public void Clear()
    {
        _stack.Clear();
        Changed?.Invoke();
    }

    /// <summary>Begins an edit transaction; auto-recorded edits buffer until the matching <see cref="EndTransaction"/>.</summary>
    public void BeginTransaction() => _transactionDepth++;

    /// <summary>Ends the current transaction, committing its buffered edits as a single undo unit.</summary>
    public void EndTransaction()
    {
        if (_transactionDepth == 0)
            return;

        _transactionDepth--;
        if (_transactionDepth > 0)
            return;

        _bufferedLinkAdds.Clear();
        if (_transactionBuffer.Count == 0)
            return;

        var commands = _transactionBuffer;
        _transactionBuffer = new List<IDiagramCommand>();
        _stack.Push(commands.Count == 1 ? commands[0] : new CompositeCommand(commands));
        Changed?.Invoke();
    }

    /// <summary>Opens a transaction scope; dispose it (e.g. a <c>using</c> block) to commit the grouped edits.</summary>
    public IDisposable Transaction()
    {
        BeginTransaction();
        return new TransactionScope(this);
    }

    /// <summary>
    /// Records a command that has <em>already</em> been applied (so it can be undone) — for edits the observers
    /// don't capture, e.g. a bulk auto-layout move. No-op while an undo/redo is being applied.
    /// </summary>
    public void RecordApplied(IDiagramCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        if (!IsApplying)
            Add(command);
    }

    private void Apply(Action action)
    {
        IsApplying = true;
        try
        {
            action();
        }
        finally
        {
            IsApplying = false;
        }

        SyncTracking();
        Changed?.Invoke();
    }

    // Records an edit: buffered while a transaction is open, otherwise pushed (and undoable) immediately.
    private void Add(IDiagramCommand command)
    {
        if (_transactionDepth > 0)
        {
            _transactionBuffer.Add(command);
            return;
        }

        _stack.Push(command);
        Changed?.Invoke();
    }

    private void OnNodeAdded(NodeModel node)
    {
        Track(node);
        if (!IsApplying)
            Add(new AddNodeCommand(_diagram, node));
    }

    private void OnNodeRemoved(NodeModel node)
    {
        node.Moved -= OnNodeMoved;
        _lastPosition.Remove(node);
        // Deletions are recorded explicitly via Execute(RemoveNodeCommand) so the cascaded links are captured.
    }

    private void OnLinkAdded(BaseLinkModel link)
    {
        if (IsApplying)
            return;

        var command = new AddLinkCommand(_diagram, link);
        Add(command);
        if (_transactionDepth > 0)
            _bufferedLinkAdds[link] = command;
    }

    // A link added then removed within the same transaction (e.g. a drag-new-link discarded for want of a
    // target) is cancelled, so the transient link never reaches the history.
    private void OnLinkRemoved(BaseLinkModel link)
    {
        if (_transactionDepth > 0 && _bufferedLinkAdds.TryGetValue(link, out var command))
        {
            _transactionBuffer.Remove(command);
            _bufferedLinkAdds.Remove(link);
        }
    }

    private void OnNodeMoved(MovableModel movable)
    {
        if (IsApplying || movable is not NodeModel node)
            return;

        var from = _lastPosition.TryGetValue(node, out var previous) ? previous : node.Position;
        var to = node.Position;
        _lastPosition[node] = to;

        if (from.X != to.X || from.Y != to.Y)
            Add(new MoveNodeCommand(node, from, to));
    }

    private void Track(NodeModel node)
    {
        _lastPosition[node] = node.Position;
        node.Moved -= OnNodeMoved;
        node.Moved += OnNodeMoved;
    }

    // After an undo/redo/execute, refresh tracked positions so the next move delta is measured from here.
    private void SyncTracking()
    {
        foreach (var node in _diagram.Nodes)
            _lastPosition[node] = node.Position;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _diagram.Nodes.Added -= OnNodeAdded;
        _diagram.Nodes.Removed -= OnNodeRemoved;
        _diagram.Links.Added -= OnLinkAdded;
        _diagram.Links.Removed -= OnLinkRemoved;
        foreach (var node in _diagram.Nodes)
            node.Moved -= OnNodeMoved;
        _lastPosition.Clear();
    }

    private sealed class TransactionScope : IDisposable
    {
        private DiagramHistory? _history;

        public TransactionScope(DiagramHistory history) => _history = history;

        public void Dispose()
        {
            _history?.EndTransaction();
            _history = null;
        }
    }
}
