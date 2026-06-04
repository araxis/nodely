---
id: undo-redo
title: Undo / redo
sidebar_position: 4
---

# Undo / redo

Editing through the canvas is undoable out of the box. There's nothing to switch on — `Ctrl`+`Z` undoes,
`Ctrl`+`Y` (or `Ctrl`+`Shift`+`Z`) redoes, and you can drive it from code when you need to:

```csharp
canvas.Undo();
canvas.Redo();
bool canUndo = canvas.CanUndo;             // handy for enabling a toolbar button
canvas.HistoryChanged += () => RefreshToolbar();
```

## What's recorded

Behind the canvas, a `DiagramHistory` watches the diagram and records the edits worth undoing:

```mermaid
flowchart LR
    edit["You add, drag, or delete"] --> record["History records a command"]
    record --> undo["Ctrl+Z reverses it"]
    undo --> redo["Ctrl+Y reapplies it"]
```

That covers adding nodes and links, finishing a drag, and deleting (including the links a deleted node took with
it, so undo brings them all back). Anything the history itself does while undoing or redoing is deliberately not
re-recorded, and the diagram's starting contents aren't undoable — only the edits you make from there.

## A gesture is one step

A whole pointer gesture collapses into a single undo. Drag five selected nodes and one `Ctrl`+`Z` puts all five
back, not one at a time. You can fold your own bulk edits into a single step the same way:

```csharp
// auto-layout (or any batch reposition) becomes one undo
canvas.RunAsUndoableMove(() => LayeredLayout.Arrange(diagram));
```

Under the covers that opens a transaction, runs your change, and records the net movement as one command.

## Going lower

The whole thing is built on a small command model in `Nodely.Commands` — `IDiagramCommand`, `UndoRedoStack`,
and concrete commands like `AddNodeCommand`, `MoveNodeCommand`, and `CompositeCommand`. You can drive that
directly for a headless or custom editor, but for the normal canvas you won't usually need to.

::: info Not yet undoable
Editing vertices and labels, group operations, and z-order changes don't go through the history yet, so they
can't be undone today.
:::
