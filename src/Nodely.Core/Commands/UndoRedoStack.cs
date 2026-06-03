using System.Collections.Generic;

namespace Nodely.Commands;

/// <summary>A reversible diagram operation.</summary>
public interface IDiagramCommand
{
    /// <summary>Applies the operation.</summary>
    void Execute();

    /// <summary>Reverses the operation.</summary>
    void Undo();
}

/// <summary>Runs <see cref="IDiagramCommand"/>s and maintains undo/redo history.</summary>
public sealed class UndoRedoStack
{
    private readonly Stack<IDiagramCommand> _undo = new();
    private readonly Stack<IDiagramCommand> _redo = new();

    /// <summary>Whether there's an operation to undo.</summary>
    public bool CanUndo => _undo.Count > 0;

    /// <summary>Whether there's an operation to redo.</summary>
    public bool CanRedo => _redo.Count > 0;

    /// <summary>Executes a command and pushes it onto the undo stack (clearing the redo stack).</summary>
    public void Execute(IDiagramCommand command)
    {
        command.Execute();
        _undo.Push(command);
        _redo.Clear();
    }

    /// <summary>
    /// Records a command that has <em>already</em> been applied (pushes to the undo stack and clears redo
    /// without executing). Use for user actions captured after the fact, e.g. a completed drag.
    /// </summary>
    public void Push(IDiagramCommand command)
    {
        _undo.Push(command);
        _redo.Clear();
    }

    /// <summary>Undoes the most recent command.</summary>
    public void Undo()
    {
        if (_undo.Count == 0)
            return;

        var command = _undo.Pop();
        command.Undo();
        _redo.Push(command);
    }

    /// <summary>Redoes the most recently undone command.</summary>
    public void Redo()
    {
        if (_redo.Count == 0)
            return;

        var command = _redo.Pop();
        command.Execute();
        _undo.Push(command);
    }

    /// <summary>Clears all history.</summary>
    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();
    }
}
