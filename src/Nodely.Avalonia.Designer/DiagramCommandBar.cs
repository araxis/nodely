using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Nodely.Avalonia.Controls;

namespace Nodely.Avalonia.Designer;

/// <summary>Reusable command bar for common canvas actions.</summary>
public sealed class DiagramCommandBar : UserControl
{
    private readonly StackPanel _bar = new()
    {
        Orientation = Orientation.Horizontal,
        Spacing = 4,
        HorizontalAlignment = HorizontalAlignment.Right,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(8),
    };

    private readonly List<Button> _buttons = new();
    private DiagramCanvas? _canvas;

    /// <summary>Creates the command bar.</summary>
    public DiagramCommandBar()
    {
        Content = _bar;
    }

    /// <summary>The canvas controlled by the bar.</summary>
    public DiagramCanvas? Canvas
    {
        get => _canvas;
        set
        {
            if (ReferenceEquals(_canvas, value))
                return;

            if (_canvas != null)
                _canvas.CommandStateChanged -= Refresh;

            _canvas = value;

            if (_canvas != null)
                _canvas.CommandStateChanged += Refresh;

            Build();
        }
    }

    /// <summary>Optional layout action.</summary>
    public Action<DiagramCanvas, Diagram>? LayoutAction { get; set; }

    /// <summary>Whether the layout button is visible.</summary>
    public bool ShowLayout { get; set; } = true;

    /// <summary>Rebuilds enabled states.</summary>
    public void Refresh()
    {
        var canvas = Canvas;
        if (canvas == null)
            return;

        foreach (var button in _buttons)
        {
            button.IsEnabled = button.Tag switch
            {
                "copy" => canvas.CanCopySelection,
                "cut" => canvas.CanCutSelection,
                "paste" => canvas.CanPasteClipboard,
                "duplicate" => canvas.CanDuplicateSelection,
                "group" => canvas.CanGroupSelection,
                "ungroup" => canvas.CanUngroupSelection,
                "front" => !canvas.IsReadOnly && canvas.HasSelection,
                "back" => !canvas.IsReadOnly && canvas.HasSelection,
                "undo" => !canvas.IsReadOnly && canvas.CanUndo,
                "redo" => !canvas.IsReadOnly && canvas.CanRedo,
                "layout" => !canvas.IsReadOnly && canvas.Diagram != null,
                _ => true,
            };
        }
    }

    private void Build()
    {
        _bar.Children.Clear();
        _buttons.Clear();
        var canvas = Canvas;
        if (canvas == null)
            return;

        Add("Zoom +", canvas.ZoomIn);
        Add("Zoom -", canvas.ZoomOut);
        Add("Fit", () => canvas.ZoomToFit());
        if (ShowLayout)
            Add("Layout", () =>
            {
                if (canvas.Diagram != null)
                    LayoutAction?.Invoke(canvas, canvas.Diagram);
            }, "layout");

        Add("Copy", canvas.CopySelection, "copy");
        Add("Cut", canvas.CutSelection, "cut");
        Add("Paste", canvas.PasteClipboard, "paste");
        Add("Duplicate", canvas.DuplicateSelection, "duplicate");
        Add("Group", canvas.GroupSelection, "group");
        Add("Ungroup", canvas.UngroupSelection, "ungroup");
        Add("Front", canvas.BringSelectionToFront, "front");
        Add("Back", canvas.SendSelectionToBack, "back");
        Add("Undo", canvas.Undo, "undo");
        Add("Redo", canvas.Redo, "redo");
        Refresh();
    }

    private void Add(string text, Action action, string? tag = null)
    {
        var button = new Button
        {
            Content = text,
            MinWidth = 40,
            Padding = new Thickness(10, 6),
            Tag = tag,
        };
        button.Click += (_, _) => action();
        _buttons.Add(button);
        _bar.Children.Add(button);
    }
}
