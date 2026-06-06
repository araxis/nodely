using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Nodely.Avalonia;
using Nodely.Avalonia.Controls;

namespace Nodely.Avalonia.Designer;

/// <summary>Ready-to-compose editor shell with toolbox, command bar, canvas, navigator, inspector, and status bar.</summary>
public sealed class DiagramDesignerShell : UserControl, IDisposable
{
    private readonly DiagramPropertyInspector? _inspector;
    private readonly DiagramStatusBar? _statusBar;
    private readonly DiagramToolbox? _toolbox;
    private readonly DiagramCommandBar? _commandBar;
    private bool _disposed;

    /// <summary>Creates a designer shell for a diagram.</summary>
    public DiagramDesignerShell(Diagram diagram, DiagramDesignerOptions? options = null)
    {
        options ??= new DiagramDesignerOptions();
        Diagram = diagram ?? throw new ArgumentNullException(nameof(diagram));
        Canvas = new DiagramCanvas
        {
            Diagram = diagram,
            Palette = options.Palette,
            IsReadOnly = options.IsReadOnly,
        };
        options.ConfigureCanvas?.Invoke(Canvas);

        Canvas.AttachedToVisualTree += (_, _) =>
            Dispatcher.UIThread.Post(() => Canvas.ZoomToFit(48), DispatcherPriority.Loaded);

        var surface = BuildSurface(diagram, options);
        var body = new Grid();
        var currentColumn = 0;

        if (options.ShowToolbox)
        {
            body.ColumnDefinitions.Add(new ColumnDefinition(options.ToolboxWidth, GridUnitType.Pixel));
            _toolbox = new DiagramToolbox { Canvas = Canvas, Width = options.ToolboxWidth };
            _toolbox.AddSections(options.ToolboxSections);
            body.Children.Add(_toolbox);
            currentColumn++;
        }

        body.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
        Grid.SetColumn(surface, currentColumn);
        body.Children.Add(surface);
        currentColumn++;

        if (options.ShowInspector)
        {
            body.ColumnDefinitions.Add(new ColumnDefinition(options.InspectorWidth, GridUnitType.Pixel));
            _inspector = new DiagramPropertyInspector
            {
                Canvas = Canvas,
                Diagram = diagram,
                Registry = options.PropertyRegistry,
                IsReadOnly = options.IsReadOnly,
                Width = options.InspectorWidth,
            };
            Canvas.CommandStateChanged += _inspector.Refresh;
            Grid.SetColumn(_inspector, currentColumn);
            body.Children.Add(_inspector);
        }

        var root = new DockPanel();
        if (options.ShowCommandBar)
        {
            _commandBar = new DiagramCommandBar
            {
                Canvas = Canvas,
                LayoutAction = options.LayoutAction,
            };
            DockPanel.SetDock(_commandBar, Dock.Top);
            root.Children.Add(_commandBar);
        }

        if (options.ShowStatusBar)
        {
            _statusBar = new DiagramStatusBar
            {
                Canvas = Canvas,
                Diagram = diagram,
            };
            DockPanel.SetDock(_statusBar, Dock.Bottom);
            root.Children.Add(_statusBar);
        }

        root.Children.Add(body);
        Content = root;
    }

    /// <summary>The edited diagram.</summary>
    public Diagram Diagram { get; }

    /// <summary>The canvas hosted by the shell.</summary>
    public DiagramCanvas Canvas { get; }

    /// <summary>The active palette.</summary>
    public NodelyPalette Palette
    {
        get => Canvas.Palette;
        set
        {
            Canvas.Palette = value;
            _inspector?.Refresh();
            _toolbox?.Refresh();
            _statusBar?.Refresh();
            _commandBar?.Refresh();
        }
    }

    /// <summary>Refreshes shell chrome after external model changes.</summary>
    public void Refresh()
    {
        Canvas.RefreshVisuals();
        _inspector?.Refresh();
        _toolbox?.Refresh();
        _statusBar?.Refresh();
        _commandBar?.Refresh();
    }

    private Control BuildSurface(Diagram diagram, DiagramDesignerOptions options)
    {
        if (!options.ShowNavigator)
            return Canvas;

        var navigator = new DiagramNavigator
        {
            Diagram = diagram,
            Width = 210,
            Height = 136,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(12),
        };

        return new Grid { Children = { Canvas, navigator } };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        if (_inspector != null)
        {
            Canvas.CommandStateChanged -= _inspector.Refresh;
            _inspector.Dispose();
        }
    }
}
