using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nodely.Avalonia;
using Nodely.Avalonia.Controls;
using Nodely.Models;
using Point = Nodely.Geometry.Point;

namespace Nodely.QuickStart;

public sealed class TicketNode : NodeModel
{
    public TicketNode(Point position, string title, string owner) : base(position)
    {
        Title = title;
        Owner = owner;
    }

    public string Owner { get; }
}

public sealed class MainWindow : Window
{
    private readonly DiagramCanvas _canvas;
    private NodelyPalette _palette = NodelyPalettes.Dark;

    public MainWindow()
    {
        Title = "Nodely QuickStart";
        Width = 920;
        Height = 620;

        var diagram = BuildDiagram();
        _canvas = new DiagramCanvas { Diagram = diagram, Palette = _palette };
        _canvas.RegisterNode<TicketNode>(BuildTicketNode);

        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(12),
            Children =
            {
                Button("Theme", ToggleTheme),
                Button("Fit", () => _canvas.ZoomToFit()),
                Button("+", () => _canvas.ZoomIn()),
                Button("-", () => _canvas.ZoomOut()),
            },
        };

        Content = new Grid { Children = { _canvas, toolbar } };
        Opened += (_, _) => _canvas.ZoomToFit();
    }

    private static NodelyDiagram BuildDiagram()
    {
        var diagram = new NodelyDiagram();
        diagram.Options.GridSize = 24;
        diagram.Options.Links.DefaultTargetMarker = LinkMarker.Arrow;

        var todo = diagram.Nodes.Add(new TicketNode(new Point(120, 180), "Intake", "Mira"));
        var doing = diagram.Nodes.Add(new TicketNode(new Point(420, 180), "Build", "Jon"));
        var done = diagram.Nodes.Add(new TicketNode(new Point(720, 180), "Ship", "Ari"));

        diagram.Links.Add(new LinkModel(todo.AddPort(PortAlignment.Right), doing.AddPort(PortAlignment.Left))).AddLabel("ready");
        diagram.Links.Add(new LinkModel(doing.AddPort(PortAlignment.Right), done.AddPort(PortAlignment.Left))).AddLabel("done");
        return diagram;
    }

    private static Control BuildTicketNode(TicketNode node) => new Border
    {
        Background = new SolidColorBrush(Color.FromRgb(32, 47, 64)),
        BorderBrush = new SolidColorBrush(Color.FromRgb(80, 170, 255)),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(6),
        Padding = new Thickness(14, 10),
        Child = new StackPanel
        {
            Spacing = 3,
            Children =
            {
                new TextBlock { Text = node.Title, Foreground = Brushes.White, FontWeight = FontWeight.SemiBold },
                new TextBlock { Text = node.Owner, Foreground = Brushes.LightGray, FontSize = 11 },
            },
        },
    };

    private static Button Button(string text, System.Action action)
    {
        var button = new Button { Content = text, MinWidth = 42 };
        button.Click += (_, _) => action();
        return button;
    }

    private void ToggleTheme()
    {
        _palette = _palette == NodelyPalettes.Dark ? NodelyPalettes.Light : NodelyPalettes.Dark;
        _canvas.Palette = _palette;
    }
}
