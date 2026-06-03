---
id: selection-and-clipboard
title: Selection, clipboard & context menu
sidebar_position: 3
---

# Selection, clipboard & context menu

## Selecting

Selection works the way you'd expect from any editor. Click to select, hold `Ctrl` to add or remove from the
selection, drag a box with `Shift` held to marquee-select, and `Ctrl`+`A` to grab everything. `Esc` or a click on
empty space clears it.

The same operations are available from code, which is useful when you're building toolbars or reacting to what's
selected:

```csharp
diagram.SelectModel(node, unselectOthers: true);
diagram.SelectAll();
diagram.UnselectAll();

foreach (var model in diagram.GetSelectedModels())
{
    // ...
}
```

## Clipboard

Copy, cut, paste, and duplicate act on the selected nodes — links between them aren't carried along, only the
nodes themselves. Each is on the usual shortcut and also exposed on the canvas:

| Action | Shortcut | Method |
| --- | --- | --- |
| Copy | `Ctrl`+`C` | `canvas.CopySelection()` |
| Cut | `Ctrl`+`X` | `canvas.CutSelection()` |
| Paste | `Ctrl`+`V` | `canvas.PasteClipboard()` |
| Duplicate | `Ctrl`+`D` | `canvas.DuplicateSelection()` |

Pasting and duplicating drop the copies at a small offset, select just the new copies, and count as a single
undo. The copies are made with `NodeModel.Clone()`, so if your node carries extra data, override `Clone` to
bring it along — see [Custom nodes](./custom-nodes.md).

## Z-order

```csharp
canvas.BringSelectionToFront();
canvas.SendSelectionToBack();
// or one model at a time:
diagram.SendToFront(model);
diagram.SendToBack(model);
```

## The right-click menu

Right-clicking the canvas opens a menu with Delete, Duplicate, Bring to front, Send to back, Select all, and
Zoom to fit. If you right-click a model that isn't selected, it gets selected first, so the menu always acts on
whatever you clicked.

## Read-only mode

Set `canvas.IsReadOnly = true` to turn the canvas into an inspector. People can still pan, zoom, and select — so
it's genuinely browsable — but moving, connecting, deleting, and the clipboard are all switched off.
