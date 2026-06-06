using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nodely.Avalonia;
using Nodely.Avalonia.Controls;
using Nodely.Models.Base;

namespace Nodely.Avalonia.Designer;

/// <summary>Small status bar for diagram counts, selection, and zoom.</summary>
public sealed class DiagramStatusBar : UserControl
{
    private readonly TextBlock _text = new()
    {
        FontSize = 12,
        VerticalAlignment = VerticalAlignment.Center,
    };

    private DiagramCanvas? _canvas;
    private Diagram? _diagram;

    /// <summary>Creates a status bar.</summary>
    public DiagramStatusBar()
    {
        Content = new Border
        {
            Padding = new Thickness(10, 5),
            Child = _text,
        };
    }

    /// <summary>The canvas to observe.</summary>
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

            Diagram = _canvas?.Diagram;
            Refresh();
        }
    }

    /// <summary>The observed diagram.</summary>
    public Diagram? Diagram
    {
        get => _diagram;
        set
        {
            if (ReferenceEquals(_diagram, value))
                return;

            if (_diagram != null)
            {
                _diagram.Changed -= Refresh;
                _diagram.SelectionChanged -= OnSelectionChanged;
                _diagram.PanChanged -= Refresh;
                _diagram.ZoomChanged -= Refresh;
            }

            _diagram = value;

            if (_diagram != null)
            {
                _diagram.Changed += Refresh;
                _diagram.SelectionChanged += OnSelectionChanged;
                _diagram.PanChanged += Refresh;
                _diagram.ZoomChanged += Refresh;
            }

            Refresh();
        }
    }

    /// <summary>Refreshes the displayed status.</summary>
    public void Refresh()
    {
        var palette = Canvas?.Palette ?? NodelyPalettes.Dark;
        _text.Foreground = palette.NodeText;

        var diagram = Diagram;
        if (diagram == null)
        {
            _text.Text = "No diagram";
            return;
        }

        var selected = diagram.GetSelectedModels().Count();
        _text.Text = string.Create(
            CultureInfo.InvariantCulture,
            $"Nodes {diagram.Nodes.Count}  Links {diagram.Links.Count}  Groups {diagram.Groups.Count}  Selected {selected}  Zoom {diagram.Zoom:0.##}x");
    }

    private void OnSelectionChanged(SelectableModel model) => Refresh();
}
