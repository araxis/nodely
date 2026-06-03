using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nodely;
using Nodely.Algorithms;
using Nodely.Avalonia;
using Nodely.Avalonia.Controls;
using Nodely.Models;
using Nodely.Serialization;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Demo;

/// <summary>A custom node type with extra domain data, rendered by a registered template.</summary>
public sealed class TaskNode : NodeModel
{
    public TaskNode(NodelyPoint position, string title) : base(position) => Title = title;

    public TaskNode(string id, NodelyPoint position, string title) : base(id, position) => Title = title;

    public string Status { get; set; } = "Pending";

    public override NodeModel Clone() => new TaskNode(Position, Title ?? string.Empty) { Status = Status, Size = Size };

    // Persist Status across save/load (the snapshot only stores built-in fields otherwise).
    public override IReadOnlyDictionary<string, object?> GetExtraData() => new Dictionary<string, object?> { ["Status"] = Status };

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Status", out var value) && value is string status)
            Status = status;
    }
}

public sealed class MainWindow : Window
{
    private readonly ContentControl _host = new();
    private NodelyPalette _palette = NodelyPalettes.Dark;
    private NodelyDiagram? _currentDiagram;
    private string? _savedJson;

    public MainWindow()
    {
        Title = "Nodely — Gallery";
        Width = 1100;
        Height = 720;

        var bar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Margin = new Thickness(10) };
        bar.Children.Add(SceneButton("Workflow", BuildWorkflow));
        bar.Children.Add(SceneButton("State machine", BuildStateMachine));
        bar.Children.Add(SceneButton("Inspector (read-only)", BuildInspector));
        bar.Children.Add(new Border { Width = 24 });
        bar.Children.Add(ToolButton("Toggle theme", ToggleTheme));
        bar.Children.Add(ToolButton("Save", Save));
        bar.Children.Add(ToolButton("Load", Load));

        var root = new DockPanel();
        DockPanel.SetDock(bar, Dock.Top);
        root.Children.Add(bar);
        root.Children.Add(_host);
        Content = root;

        _host.Content = BuildWorkflow();
    }

    private Button SceneButton(string text, Func<Control> build)
    {
        var button = new Button { Content = text };
        button.Click += (_, _) => _host.Content = build();
        return button;
    }

    private static Button ToolButton(string text, Action onClick)
    {
        var button = new Button { Content = text, MinWidth = 36 };
        button.Click += (_, _) => onClick();
        return button;
    }

    private void ToggleTheme()
    {
        _palette = _palette == NodelyPalettes.Dark ? NodelyPalettes.Light : NodelyPalettes.Dark;
        if (_host.Content is Grid grid)
            foreach (var child in grid.Children)
                if (child is DiagramCanvas canvas)
                    canvas.Palette = _palette;
    }

    private void Save()
    {
        if (_currentDiagram != null)
            _savedJson = DiagramSerializer.Serialize(_currentDiagram);
    }

    private void Load()
    {
        if (_savedJson == null)
            return;

        var diagram = new NodelyDiagram();
        diagram.Options.Links.DefaultTargetMarker = LinkMarker.Arrow;
        DiagramSerializer.Deserialize(diagram, _savedJson, LoadNode);
        _host.Content = Editor(diagram, registerTask: true);
    }

    // Recreates the right node type (preserving its id so links/groups resolve) from a snapshot.
    private static NodeModel LoadNode(NodeSnapshot ns) => ns.Kind == nameof(TaskNode)
        ? new TaskNode(ns.Id, new NodelyPoint(ns.X, ns.Y), ns.Title ?? string.Empty)
        : new NodeModel(ns.Id, new NodelyPoint(ns.X, ns.Y));

    private Control Editor(NodelyDiagram diagram, bool readOnly = false, bool registerTask = false)
    {
        _currentDiagram = diagram;
        var canvas = new DiagramCanvas { Diagram = diagram, Palette = _palette, IsReadOnly = readOnly };
        if (registerTask)
            RegisterTaskNode(canvas);

        var navigator = new DiagramNavigator
        {
            Diagram = diagram,
            Width = 200,
            Height = 130,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(12),
        };

        var undo = ToolButton("Undo", canvas.Undo);
        var redo = ToolButton("Redo", canvas.Redo);
        void RefreshHistory()
        {
            undo.IsEnabled = canvas.CanUndo;
            redo.IsEnabled = canvas.CanRedo;
        }

        canvas.HistoryChanged += RefreshHistory;
        RefreshHistory();

        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(12),
            Children =
            {
                ToolButton("+", canvas.ZoomIn),
                ToolButton("−", canvas.ZoomOut),
                ToolButton("Fit", () => canvas.ZoomToFit()),
                ToolButton("Layout", () =>
                {
                    canvas.RunAsUndoableMove(() => LayeredLayout.Arrange(diagram));
                    canvas.ZoomToFit();
                }),
                undo,
                redo,
            },
        };

        return new Grid { Children = { canvas, navigator, toolbar } };
    }

    private static void RegisterTaskNode(DiagramCanvas canvas) => canvas.RegisterNode<TaskNode>(node => new Border
    {
        Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x4A, 0x6B)),
        BorderBrush = new SolidColorBrush(Color.FromRgb(0x4D, 0x9E, 0xFF)),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(6),
        Padding = new Thickness(14, 10),
        Child = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                new TextBlock { Text = node.Title, Foreground = Brushes.White, FontWeight = FontWeight.SemiBold },
                new TextBlock { Text = node.Status, Foreground = Brushes.LightGray, FontSize = 11 },
            },
        },
    });

    private Control BuildWorkflow()
    {
        var diagram = new NodelyDiagram();
        diagram.Options.GridSize = 24;
        diagram.Options.Groups.Enabled = true;
        diagram.Options.Links.DefaultTargetMarker = LinkMarker.Arrow;

        var start = diagram.Nodes.Add(new NodeModel(new NodelyPoint(120, 220)) { Title = "Start" });
        var build = diagram.Nodes.Add(new TaskNode(new NodelyPoint(420, 160), "Build") { Status = "Running" });
        var deploy = diagram.Nodes.Add(new TaskNode(new NodelyPoint(420, 320), "Deploy") { Status = "Pending" });

        var startOut = start.AddPort(PortAlignment.Right);
        var buildLink = diagram.Links.Add(new LinkModel(startOut, build.AddPort(PortAlignment.Left)));
        buildLink.Segmentable = true;
        buildLink.AddVertex(new NodelyPoint(300, 120)); // a draggable bend point — double-click a link to add more, a vertex to remove
        var deployLink = diagram.Links.Add(new LinkModel(startOut, deploy.AddPort(PortAlignment.Left)));
        deployLink.Segmentable = true;
        deployLink.SourceMarker = LinkMarker.Circle; // a circle at the source end (arrow stays at the target)
        diagram.Groups.Group(build, deploy);

        return Editor(diagram, registerTask: true);
    }

    private Control BuildStateMachine()
    {
        var diagram = new NodelyDiagram();
        diagram.Options.Links.DefaultTargetMarker = LinkMarker.Arrow;
        var idle = diagram.Nodes.Add(new NodeModel(new NodelyPoint(0, 0)) { Title = "Idle" });
        var running = diagram.Nodes.Add(new NodeModel(new NodelyPoint(0, 0)) { Title = "Running" });
        var done = diagram.Nodes.Add(new NodeModel(new NodelyPoint(0, 0)) { Title = "Done" });
        var error = diagram.Nodes.Add(new NodeModel(new NodelyPoint(0, 0)) { Title = "Error" });

        diagram.Links.Add(new LinkModel(idle, running)).AddLabel("start");
        diagram.Links.Add(new LinkModel(running, done)).AddLabel("ok");
        diagram.Links.Add(new LinkModel(running, error)).AddLabel("fail");
        diagram.Links.Add(new LinkModel(error, idle)).AddLabel("reset");

        return Editor(diagram); // press "Layout" to arrange
    }

    private Control BuildInspector()
    {
        var diagram = new NodelyDiagram();
        diagram.Options.Links.DefaultTargetMarker = LinkMarker.Arrow;
        var a = diagram.Nodes.Add(new TaskNode(new NodelyPoint(120, 160), "Ingest") { Status = "Done" });
        var b = diagram.Nodes.Add(new TaskNode(new NodelyPoint(420, 160), "Transform") { Status = "Done" });
        var c = diagram.Nodes.Add(new TaskNode(new NodelyPoint(720, 160), "Load") { Status = "Done" });
        diagram.Links.Add(new LinkModel(a.AddPort(PortAlignment.Right), b.AddPort(PortAlignment.Left)));
        diagram.Links.Add(new LinkModel(b.AddPort(PortAlignment.Right), c.AddPort(PortAlignment.Left)));

        return Editor(diagram, readOnly: true, registerTask: true);
    }
}
