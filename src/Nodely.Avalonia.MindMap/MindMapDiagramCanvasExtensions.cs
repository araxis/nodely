using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Nodely.Avalonia.Controls;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;
using AvPoint = Avalonia.Point;
using ShapeEllipse = Avalonia.Controls.Shapes.Ellipse;

namespace Nodely.Avalonia.MindMap;

/// <summary>Canvas registration helpers for the mind-map node pack.</summary>
public static class MindMapDiagramCanvasExtensions
{
    private static readonly Color DefaultRootColor = Color.FromRgb(77, 158, 255);
    private static readonly Color DefaultBranchColor = Color.FromRgb(55, 167, 121);
    private static readonly Color DefaultLeafColor = Color.FromRgb(216, 156, 53);
    private static readonly Color AssociationColor = Color.FromRgb(151, 121, 205);

    /// <summary>Registers mind-map node, port, and link renderers on the canvas.</summary>
    public static DiagramCanvas UseMindMapNodes(this DiagramCanvas canvas)
    {
        if (canvas is null)
            throw new ArgumentNullException(nameof(canvas));

        canvas.RegisterNode<MindMapRootNode>(BuildRootNode);
        canvas.RegisterNode<MindMapBranchNode>(BuildBranchNode);
        canvas.RegisterNode<MindMapLeafNode>(BuildLeafNode);
        canvas.RegisterPort<MindMapPortModel>(BuildPort);
        canvas.RegisterLinkStyle<MindMapLink>(StyleFor);
        canvas.RegisterLink<MindMapLink>(DrawMindMapLink);

        return canvas;
    }

    private static Control BuildRootNode(MindMapRootNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node, DefaultRootColor);
        var hidden = HiddenDescendantCount(node, context.Diagram);
        var header = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                new TextBlock
                {
                    Text = IconText(node, "ROOT"),
                    Foreground = Brushes.White,
                    FontSize = 10,
                    FontWeight = FontWeight.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                new TextBlock
                {
                    Text = node.Topic,
                    Foreground = Brushes.White,
                    FontSize = 18,
                    FontWeight = FontWeight.SemiBold,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    MaxWidth = 230,
                },
            },
        };

        if (!string.IsNullOrWhiteSpace(node.Notes))
            header.Children.Add(new TextBlock
            {
                Text = node.Notes,
                Foreground = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                MaxWidth = 230,
            });

        var grid = new Grid();
        grid.Children.Add(new Border
        {
            MinWidth = MindMapVisualMetrics.RootWidth,
            MinHeight = MindMapVisualMetrics.RootHeight,
            Background = Brush(accent),
            BorderBrush = context.Palette.Selection,
            BorderThickness = new Thickness(1.2),
            CornerRadius = new CornerRadius(28),
            Padding = new Thickness(20, 14),
            Child = header,
        });

        if (hidden > 0)
            grid.Children.Add(BuildCollapseBadge(hidden, context, accent));

        return new Border
        {
            Tag = "mindmap-root-node",
            Background = Brushes.Transparent,
            Child = grid,
        };
    }

    private static Control BuildBranchNode(MindMapBranchNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node, DefaultBranchColor);
        var hidden = HiddenDescendantCount(node, context.Diagram);
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("10,*,Auto") };
        grid.Children.Add(new Border
        {
            Background = Brush(accent),
            CornerRadius = new CornerRadius(8, 0, 0, 8),
        });

        var content = new StackPanel { Spacing = 3 };
        content.Children.Add(new TextBlock
        {
            Text = node.Topic,
            Foreground = context.Palette.NodeText,
            FontWeight = FontWeight.SemiBold,
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 178,
        });
        if (!string.IsNullOrWhiteSpace(node.Notes))
            content.Children.Add(new TextBlock
            {
                Text = node.Notes,
                Foreground = context.Palette.LinkStroke,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 178,
            });

        Grid.SetColumn(content, 1);
        grid.Children.Add(new Border
        {
            Padding = new Thickness(12, 9),
            Child = content,
        });

        var meta = new StackPanel
        {
            Spacing = 5,
            Margin = new Thickness(0, 8, 9, 8),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
        };
        if (!string.IsNullOrWhiteSpace(node.IconKey))
            meta.Children.Add(BuildSmallBadge(node.IconKey, accent, context));
        if (hidden > 0)
            meta.Children.Add(BuildSmallBadge(hidden.ToString(CultureInfo.InvariantCulture), accent, context));
        Grid.SetColumn(meta, 2);
        grid.Children.Add(meta);

        return new Border
        {
            Tag = "mindmap-branch-node",
            MinWidth = MindMapVisualMetrics.BranchWidth,
            MinHeight = MindMapVisualMetrics.BranchHeight,
            Background = context.Palette.NodeBackground,
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1.2),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
            Child = grid,
        };
    }

    private static Control BuildLeafNode(MindMapLeafNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node, DefaultLeafColor);
        var panel = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
        };
        panel.Children.Add(new ShapeEllipse
        {
            Width = 10,
            Height = 10,
            Fill = Brush(accent),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
        });

        Grid.SetColumn(panel.Children[0], 0);
        var text = new TextBlock
        {
            Text = node.Topic,
            Foreground = context.Palette.NodeText,
            FontWeight = FontWeight.SemiBold,
            FontSize = 12,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
            MaxWidth = 156,
        };
        Grid.SetColumn(text, 1);
        panel.Children.Add(text);

        return new Border
        {
            Tag = "mindmap-leaf-node",
            MinWidth = MindMapVisualMetrics.LeafWidth,
            MinHeight = MindMapVisualMetrics.LeafHeight,
            Background = new SolidColorBrush(Color.FromArgb(48, accent.R, accent.G, accent.B)),
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(13, 7),
            Child = panel,
        };
    }

    private static Control BuildPort(MindMapPortModel port, DiagramRenderContext context)
    {
        var parentAccent = port.Parent is MindMapTopicNode topic
            ? ResolveAccent(topic, port.Role == MindMapPortRole.Association ? AssociationColor : DefaultBranchColor)
            : DefaultBranchColor;
        var size = port.Role == MindMapPortRole.Branch ? 18d : 16d;
        var view = new Grid
        {
            Tag = "mindmap-port",
            Width = size,
            Height = size,
            IsVisible = port.Visible && port.Parent.Visible,
            Background = Brushes.Transparent,
        };

        if (port.Role == MindMapPortRole.Association)
        {
            view.Children.Add(new ShapeEllipse
            {
                Width = 13,
                Height = 13,
                Fill = context.Palette.NodeBackground,
                Stroke = Brush(AssociationColor),
                StrokeThickness = 2,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }
        else
        {
            view.Children.Add(new Border
            {
                Width = 13,
                Height = 13,
                Background = Brush(parentAccent),
                BorderBrush = context.Palette.NodeBackground,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(4),
                RenderTransformOrigin = RelativePoint.Center,
                RenderTransform = new RotateTransform(45),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }

        return view;
    }

    private static LinkStyle StyleFor(MindMapLink link)
    {
        var accent = ResolveAccent(link.AccentColor, link.Kind == MindMapLinkKind.Branch ? DefaultBranchColor : AssociationColor);
        return new LinkStyle
        {
            Stroke = Brush(accent),
            SelectedStroke = Brushes.White,
            DashStyle = link.Kind == MindMapLinkKind.Association ? DashStyle.Dash : null,
            Width = link.Width,
        };
    }

    private static void DrawMindMapLink(DrawingContext context, LinkRenderContext ctx)
    {
        if (!ctx.Link.Visible)
            return;

        ctx.DrawDefault();

        var link = (MindMapLink)ctx.Link;
        var accent = ctx.IsSelected
            ? ctx.Palette.Selection
            : StyleFor(link).Stroke ?? ctx.Palette.LinkStroke;
        var pen = new Pen(accent, link.Kind == MindMapLinkKind.Branch ? 1.8 : 1.4);

        if (link.Kind == MindMapLinkKind.Branch && TryGetEnd(ctx.Path, target: true, out var target, out _))
            context.DrawEllipse(ctx.Palette.NodeBackground, pen, new AvPoint(target.X, target.Y), 5.5, 5.5);

        if (link.Kind == MindMapLinkKind.Association)
        {
            var point = ctx.Path.PointAtDistance(ctx.Path.Length() / 2);
            if (point is { } middle)
                context.DrawEllipse(ctx.Palette.NodeBackground, pen, new AvPoint(middle.X, middle.Y), 4.5, 4.5);
        }
    }

    private static Control BuildCollapseBadge(int count, DiagramRenderContext context, Color accent)
        => new Border
        {
            MinWidth = 32,
            Height = 24,
            Background = context.Palette.NodeBackground,
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(8, 0),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Child = new TextBlock
            {
                Text = count.ToString(CultureInfo.InvariantCulture),
                Foreground = context.Palette.NodeText,
                FontSize = 11,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };

    private static Control BuildSmallBadge(string text, Color accent, DiagramRenderContext context)
        => new Border
        {
            MinWidth = 26,
            Height = 20,
            Background = new SolidColorBrush(Color.FromArgb(38, accent.R, accent.G, accent.B)),
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(7, 0),
            Child = new TextBlock
            {
                Text = text,
                Foreground = context.Palette.NodeText,
                FontSize = 10,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };

    private static int HiddenDescendantCount(MindMapTopicNode node, Diagram? diagram)
    {
        if (diagram == null || !node.Collapsed)
            return 0;

        var graph = MindMapLayout.BuildBranchGraph(diagram);
        return CountDescendants(node, graph.ChildrenOf, new HashSet<MindMapTopicNode>());
    }

    private static int CountDescendants(
        MindMapTopicNode node,
        IReadOnlyDictionary<MindMapTopicNode, List<MindMapTopicNode>> childrenOf,
        HashSet<MindMapTopicNode> seen)
    {
        if (!seen.Add(node) || !childrenOf.TryGetValue(node, out var children))
            return 0;

        var count = children.Count;
        foreach (var child in children)
            count += CountDescendants(child, childrenOf, seen);
        return count;
    }

    private static string IconText(MindMapTopicNode node, string fallback)
        => string.IsNullOrWhiteSpace(node.IconKey) ? fallback : node.IconKey!;

    private static bool TryGetEnd(PathData path, bool target, out Nodely.Geometry.Point point, out double angle)
    {
        var length = path.Length();
        var end = path.PointAtDistance(target ? length : 0);
        var near = path.PointAtDistance(target ? Math.Max(0, length - 18) : Math.Min(length, 18));

        if (end is not { } endPoint || near is not { } nearPoint)
        {
            point = new Nodely.Geometry.Point(0, 0);
            angle = 0;
            return false;
        }

        point = endPoint;
        angle = target
            ? Angle(nearPoint, endPoint)
            : Angle(endPoint, nearPoint);
        return true;
    }

    private static double Angle(Nodely.Geometry.Point from, Nodely.Geometry.Point to)
        => Math.Atan2(to.Y - from.Y, to.X - from.X) * 180 / Math.PI;

    private static Color ResolveAccent(MindMapTopicNode node, Color fallback)
        => ResolveAccent(node.AccentColor, fallback);

    private static Color ResolveAccent(string? value, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        var text = value.Trim();
        if (text.StartsWith("#", StringComparison.Ordinal))
            text = text.Substring(1);

        if (text.Length == 6 &&
            byte.TryParse(text.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) &&
            byte.TryParse(text.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) &&
            byte.TryParse(text.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
        {
            return Color.FromRgb(r, g, b);
        }

        return fallback;
    }

    private static IBrush Brush(Color color) => new SolidColorBrush(color);
}
