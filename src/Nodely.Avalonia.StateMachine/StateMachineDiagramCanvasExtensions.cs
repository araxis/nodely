using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nodely.Anchors;
using Nodely.Avalonia.Controls;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Models.Base;
using AvPoint = Avalonia.Point;
using NodelyRectangle = Nodely.Geometry.Rectangle;
using NodelySize = Nodely.Geometry.Size;
using ShapeEllipse = Avalonia.Controls.Shapes.Ellipse;

namespace Nodely.Avalonia.StateMachine;

/// <summary>Canvas registration helpers for the state-machine node pack.</summary>
public static class StateMachineDiagramCanvasExtensions
{
    private static readonly Color InitialColor = Color.FromRgb(77, 158, 255);
    private static readonly Color StateColor = Color.FromRgb(55, 167, 121);
    private static readonly Color FinalColor = Color.FromRgb(196, 85, 82);
    private static readonly Color ChoiceColor = Color.FromRgb(139, 104, 184);
    private static readonly Color NoteColor = Color.FromRgb(216, 156, 53);
    private static readonly Color TimeoutColor = Color.FromRgb(209, 139, 48);
    private static readonly Color SelfColor = Color.FromRgb(57, 155, 166);

    /// <summary>Registers state-machine node, port, and transition renderers on the canvas.</summary>
    public static DiagramCanvas UseStateMachineNodes(this DiagramCanvas canvas)
    {
        if (canvas is null)
            throw new ArgumentNullException(nameof(canvas));

        canvas.RegisterNode<StateMachineInitialNode>(BuildInitialNode);
        canvas.RegisterNode<StateMachineStateNode>(BuildStateNode);
        canvas.RegisterNode<StateMachineFinalNode>(BuildFinalNode);
        canvas.RegisterNode<StateMachineChoiceNode>(BuildChoiceNode);
        canvas.RegisterNode<StateMachineNoteNode>(BuildNoteNode);
        canvas.RegisterPort<StateMachinePortModel>(BuildPort);
        canvas.RegisterLinkStyle<StateMachineTransitionLink>(StyleFor);
        canvas.RegisterLink<StateMachineTransitionLink>(DrawTransition);

        return canvas;
    }

    private static Control BuildInitialNode(StateMachineInitialNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node.AccentColor, InitialColor);
        return new Grid
        {
            Tag = "statemachine-initial-node",
            Width = StateMachineVisualMetrics.CircleSize,
            Height = StateMachineVisualMetrics.CircleSize,
            Children =
            {
                new ShapeEllipse
                {
                    Fill = Brush(accent),
                    Stroke = context.Palette.NodeBackground,
                    StrokeThickness = 4,
                },
                new TextBlock
                {
                    Text = node.Name,
                    Foreground = Brushes.White,
                    FontSize = 10,
                    FontWeight = FontWeight.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        };
    }

    private static Control BuildFinalNode(StateMachineFinalNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node.AccentColor, FinalColor);
        var grid = new Grid
        {
            Tag = "statemachine-final-node",
            Width = StateMachineVisualMetrics.CircleSize,
            Height = StateMachineVisualMetrics.CircleSize,
        };

        grid.Children.Add(new ShapeEllipse
        {
            Fill = context.Palette.NodeBackground,
            Stroke = Brush(accent),
            StrokeThickness = 3,
        });
        grid.Children.Add(new ShapeEllipse
        {
            Width = StateMachineVisualMetrics.CircleSize - 16,
            Height = StateMachineVisualMetrics.CircleSize - 16,
            Fill = Brush(accent),
            Stroke = context.Palette.NodeBackground,
            StrokeThickness = 2,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        });
        grid.Children.Add(new TextBlock
        {
            Text = node.Name,
            Foreground = Brushes.White,
            FontSize = 10,
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        });

        return grid;
    }

    private static Control BuildStateNode(StateMachineStateNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node.AccentColor, StateColor);
        var details = new StackPanel { Spacing = 3 };
        if (!string.IsNullOrWhiteSpace(node.Description))
            details.Children.Add(DetailText(node.Description!, context));
        if (!string.IsNullOrWhiteSpace(node.EntryAction))
            details.Children.Add(DetailRow("entry", node.EntryAction!, context));
        if (!string.IsNullOrWhiteSpace(node.ExitAction))
            details.Children.Add(DetailRow("exit", node.ExitAction!, context));

        return new Border
        {
            Tag = "statemachine-state-node",
            MinWidth = StateMachineVisualMetrics.StateWidth,
            MinHeight = StateMachineVisualMetrics.StateHeight,
            Background = context.Palette.NodeBackground,
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1.2),
            CornerRadius = new CornerRadius(7),
            ClipToBounds = true,
            Child = new StackPanel
            {
                Children =
                {
                    new Border
                    {
                        Background = Brush(accent),
                        Padding = new Thickness(13, 9),
                        Child = new StackPanel
                        {
                            Spacing = 1,
                            Children =
                            {
                                new TextBlock { Text = "STATE", Foreground = Brushes.White, FontSize = 10, FontWeight = FontWeight.SemiBold },
                                new TextBlock
                                {
                                    Text = node.Name,
                                    Foreground = Brushes.White,
                                    FontSize = 15,
                                    FontWeight = FontWeight.SemiBold,
                                    TextWrapping = TextWrapping.Wrap,
                                    MaxWidth = 205,
                                },
                            },
                        },
                    },
                    new Border
                    {
                        IsVisible = details.Children.Count > 0,
                        Padding = new Thickness(13, 8),
                        Child = details,
                    },
                },
            },
        };
    }

    private static Control BuildChoiceNode(StateMachineChoiceNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node.AccentColor, ChoiceColor);
        return new Grid
        {
            Tag = "statemachine-choice-node",
            Width = StateMachineVisualMetrics.ChoiceSize,
            Height = StateMachineVisualMetrics.ChoiceSize,
            Children =
            {
                new Border
                {
                    Width = 62,
                    Height = 62,
                    Background = context.Palette.NodeBackground,
                    BorderBrush = Brush(accent),
                    BorderThickness = new Thickness(2),
                    RenderTransformOrigin = RelativePoint.Center,
                    RenderTransform = new RotateTransform(45),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                new StackPanel
                {
                    Spacing = 1,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "CHOICE",
                            Foreground = Brush(accent),
                            FontSize = 9,
                            FontWeight = FontWeight.SemiBold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                        },
                        new TextBlock
                        {
                            Text = node.Name,
                            Foreground = context.Palette.NodeText,
                            FontSize = 11,
                            FontWeight = FontWeight.SemiBold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            TextTrimming = TextTrimming.CharacterEllipsis,
                            MaxWidth = 54,
                        },
                    },
                },
            },
        };
    }

    private static Control BuildNoteNode(StateMachineNoteNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node.AccentColor, NoteColor);
        return new Border
        {
            Tag = "statemachine-note-node",
            MinWidth = StateMachineVisualMetrics.NoteWidth,
            MinHeight = StateMachineVisualMetrics.NoteHeight,
            Background = new SolidColorBrush(Color.FromArgb(54, accent.R, accent.G, accent.B)),
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(13, 10),
            Child = new TextBlock
            {
                Text = node.Text,
                Foreground = context.Palette.NodeText,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 250,
            },
        };
    }

    private static Control BuildPort(StateMachinePortModel port, DiagramRenderContext context)
    {
        var color = port.Role switch
        {
            StateMachinePortRole.Entry => InitialColor,
            StateMachinePortRole.Exit => StateColor,
            _ => ChoiceColor,
        };

        var view = new Grid
        {
            Tag = "statemachine-port",
            Width = 18,
            Height = 18,
            IsVisible = port.Visible && port.Parent.Visible,
            Background = Brushes.Transparent,
        };

        if (port.Role == StateMachinePortRole.Entry)
        {
            view.Children.Add(new ShapeEllipse
            {
                Width = 13,
                Height = 13,
                Fill = context.Palette.NodeBackground,
                Stroke = Brush(color),
                StrokeThickness = 2,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }
        else
        {
            view.Children.Add(new Border
            {
                Width = port.Role == StateMachinePortRole.Exit ? 13 : 12,
                Height = port.Role == StateMachinePortRole.Exit ? 13 : 12,
                Background = Brush(color),
                BorderBrush = context.Palette.NodeBackground,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(port.Role == StateMachinePortRole.Exit ? 3 : 6),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }

        return view;
    }

    private static LinkStyle StyleFor(StateMachineTransitionLink link)
    {
        var stroke = ResolveAccent(link.AccentColor, link.Kind switch
        {
            StateMachineTransitionKind.Error => FinalColor,
            StateMachineTransitionKind.Timeout => TimeoutColor,
            StateMachineTransitionKind.Choice => ChoiceColor,
            StateMachineTransitionKind.Self => SelfColor,
            _ => StateColor,
        });

        return new LinkStyle
        {
            Stroke = Brush(stroke),
            SelectedStroke = Brushes.White,
            DashStyle = link.Kind is StateMachineTransitionKind.Timeout or StateMachineTransitionKind.Choice
                ? DashStyle.Dash
                : null,
            Width = link.Width,
        };
    }

    private static void DrawTransition(DrawingContext context, LinkRenderContext ctx)
    {
        var link = (StateMachineTransitionLink)ctx.Link;
        var brush = ctx.IsSelected
            ? ctx.Palette.Selection
            : StyleFor(link).Stroke ?? ctx.Palette.LinkStroke;
        var pen = new Pen(brush, link.Width) { DashStyle = StyleFor(link).DashStyle };

        if (link.Kind == StateMachineTransitionKind.Self &&
            TryGetNode(link.Source, out var node))
        {
            DrawSelfTransition(context, ctx, link, node, pen);
            return;
        }

        ctx.DrawDefault();
        DrawKindGlyph(context, ctx, link, brush, pen);
    }

    private static void DrawSelfTransition(
        DrawingContext context,
        LinkRenderContext ctx,
        StateMachineTransitionLink link,
        NodeModel node,
        Pen pen)
    {
        var bounds = node.GetBounds() ?? new NodelyRectangle(node.Position, new NodelySize(StateMachineVisualMetrics.StateWidth, StateMachineVisualMetrics.StateHeight));
        var start = new AvPoint(bounds.Right - bounds.Width * 0.26, bounds.Top + 4);
        var end = new AvPoint(bounds.Left + bounds.Width * 0.26, bounds.Top + 4);
        var c1 = new AvPoint(bounds.Right + 76, bounds.Top - 88);
        var c2 = new AvPoint(bounds.Left - 18, bounds.Top - 88);

        var geometry = new StreamGeometry();
        using (var g = geometry.Open())
        {
            g.BeginFigure(start, isFilled: false);
            g.CubicBezierTo(c1, c2, end);
            g.EndFigure(isClosed: false);
        }

        context.DrawGeometry(null, pen, geometry);
        DrawArrow(context, pen, end);

        var label = link.FormatLabel();
        if (!string.IsNullOrWhiteSpace(label))
            DrawChip(context, label, new AvPoint(bounds.Left + bounds.Width / 2, bounds.Top - 70), ctx.Palette);
    }

    private static void DrawArrow(DrawingContext context, Pen pen, AvPoint end)
    {
        context.DrawLine(pen, end, new AvPoint(end.X - 7, end.Y - 11));
        context.DrawLine(pen, end, new AvPoint(end.X + 7, end.Y - 11));
    }

    private static void DrawKindGlyph(
        DrawingContext context,
        LinkRenderContext ctx,
        StateMachineTransitionLink link,
        IBrush brush,
        Pen pen)
    {
        if (link.Kind == StateMachineTransitionKind.Normal)
            return;

        var point = ctx.Path.PointAtDistance(ctx.Path.Length() / 2);
        if (point is not { } middle)
            return;

        var center = new AvPoint(middle.X, middle.Y);
        if (link.Kind == StateMachineTransitionKind.Choice)
        {
            var geometry = new StreamGeometry();
            using (var g = geometry.Open())
            {
                g.BeginFigure(new AvPoint(center.X, center.Y - 8), isFilled: true);
                g.LineTo(new AvPoint(center.X + 8, center.Y));
                g.LineTo(new AvPoint(center.X, center.Y + 8));
                g.LineTo(new AvPoint(center.X - 8, center.Y));
                g.EndFigure(isClosed: true);
            }

            context.DrawGeometry(ctx.Palette.NodeBackground, pen, geometry);
        }
        else if (link.Kind == StateMachineTransitionKind.Error)
        {
            context.DrawEllipse(ctx.Palette.NodeBackground, pen, center, 8, 8);
            DrawCenteredText(context, "!", center, brush, 12, FontWeight.SemiBold);
        }
        else if (link.Kind == StateMachineTransitionKind.Timeout)
        {
            context.DrawEllipse(ctx.Palette.NodeBackground, pen, center, 8, 8);
            context.DrawLine(pen, center, new AvPoint(center.X, center.Y - 5));
            context.DrawLine(pen, center, new AvPoint(center.X + 4, center.Y + 2));
        }
    }

    private static TextBlock DetailText(string text, DiagramRenderContext context)
        => new()
        {
            Text = text,
            Foreground = context.Palette.LinkStroke,
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 210,
        };

    private static Control DetailRow(string label, string value, DiagramRenderContext context)
        => new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("50,*"),
            Children =
            {
                new TextBlock
                {
                    Text = label,
                    Foreground = context.Palette.LinkStroke,
                    FontSize = 10,
                    FontWeight = FontWeight.SemiBold,
                },
                WithColumn(new TextBlock
                {
                    Text = value,
                    Foreground = context.Palette.NodeText,
                    FontSize = 11,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 160,
                }, 1),
            },
        };

    private static T WithColumn<T>(T control, int column)
        where T : Control
    {
        Grid.SetColumn(control, column);
        return control;
    }

    private static void DrawChip(DrawingContext context, string text, AvPoint center, NodelyPalette palette)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            12,
            palette.LabelForeground);

        const double padX = 7, padY = 4;
        var rect = new Rect(
            center.X - formatted.Width / 2 - padX,
            center.Y - formatted.Height / 2 - padY,
            formatted.Width + padX * 2,
            formatted.Height + padY * 2);

        context.DrawRectangle(palette.LabelBackground, null, rect, 4, 4);
        context.DrawText(formatted, new AvPoint(rect.X + padX, rect.Y + padY));
    }

    private static void DrawCenteredText(
        DrawingContext context,
        string text,
        AvPoint center,
        IBrush brush,
        double size,
        FontWeight weight)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(Typeface.Default.FontFamily, FontStyle.Normal, weight),
            size,
            brush);

        context.DrawText(formatted, new AvPoint(center.X - formatted.Width / 2, center.Y - formatted.Height / 2));
    }

    private static bool TryGetNode(Anchor anchor, out NodeModel node)
    {
        node = StateMachineLayout.NodeFromAnchor(anchor)!;
        return node != null;
    }

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
