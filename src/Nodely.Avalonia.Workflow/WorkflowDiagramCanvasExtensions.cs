using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nodely.Avalonia.Controls;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Workflow;

/// <summary>Canvas registration helpers for the workflow node pack.</summary>
public static class WorkflowDiagramCanvasExtensions
{
    private static readonly IBrush StartAccent = new SolidColorBrush(Color.FromRgb(69, 157, 112));
    private static readonly IBrush EndAccent = new SolidColorBrush(Color.FromRgb(190, 88, 82));
    private static readonly IBrush TaskAccent = new SolidColorBrush(Color.FromRgb(76, 135, 196));
    private static readonly IBrush DecisionAccent = new SolidColorBrush(Color.FromRgb(194, 147, 59));
    private static readonly IBrush GatewayAccent = new SolidColorBrush(Color.FromRgb(139, 104, 184));
    private static readonly IBrush EventAccent = new SolidColorBrush(Color.FromRgb(57, 155, 166));
    private static readonly IBrush NoteAccent = new SolidColorBrush(Color.FromRgb(207, 168, 70));

    /// <summary>Registers workflow node and link renderers on the canvas.</summary>
    public static DiagramCanvas UseWorkflowNodes(this DiagramCanvas canvas)
    {
        if (canvas is null)
            throw new ArgumentNullException(nameof(canvas));

        canvas.RegisterNode<WorkflowStartNode>((node, context) =>
            BuildPillNode(node, "START", StartAccent, "workflow-start-node", context));
        canvas.RegisterNode<WorkflowEndNode>((node, context) =>
            BuildPillNode(node, "END", EndAccent, "workflow-end-node", context));
        canvas.RegisterNode<WorkflowTaskNode>(BuildTaskNode);
        canvas.RegisterNode<WorkflowDecisionNode>(BuildDecisionNode);
        canvas.RegisterNode<WorkflowGatewayNode>(BuildGatewayNode);
        canvas.RegisterNode<WorkflowEventNode>(BuildEventNode);
        canvas.RegisterNode<WorkflowNoteNode>(BuildNoteNode);
        canvas.RegisterLinkStyle<WorkflowLink>(StyleFor);
        canvas.RegisterLink<WorkflowLink>(DrawWorkflowLink);

        return canvas;
    }

    private static Control BuildPillNode(
        WorkflowNodeBase node,
        string kind,
        IBrush accent,
        string tag,
        DiagramRenderContext context)
        => BuildShell(node, kind, accent, tag, context, Array.Empty<string>(), cornerRadius: 18, minWidth: 150);

    private static Control BuildTaskNode(WorkflowTaskNode node, DiagramRenderContext context)
        => BuildShell(
            node,
            "TASK",
            TaskAccent,
            "workflow-task-node",
            context,
            new[] { node.TaskType.ToString(), node.Status.ToString() });

    private static Control BuildDecisionNode(WorkflowDecisionNode node, DiagramRenderContext context)
        => BuildShell(
            node,
            "DECISION",
            DecisionAccent,
            "workflow-decision-node",
            context,
            string.IsNullOrWhiteSpace(node.Condition) ? Array.Empty<string>() : new[] { node.Condition },
            cornerRadius: 10);

    private static Control BuildGatewayNode(WorkflowGatewayNode node, DiagramRenderContext context)
        => BuildShell(
            node,
            "GATEWAY",
            GatewayAccent,
            "workflow-gateway-node",
            context,
            new[] { node.GatewayKind.ToString() },
            cornerRadius: 10,
            minWidth: 170);

    private static Control BuildEventNode(WorkflowEventNode node, DiagramRenderContext context)
        => BuildShell(
            node,
            "EVENT",
            EventAccent,
            "workflow-event-node",
            context,
            new[] { node.EventKind.ToString() },
            cornerRadius: 18,
            minWidth: 170);

    private static Control BuildNoteNode(WorkflowNoteNode node, DiagramRenderContext context)
        => new Border
        {
            Tag = "workflow-note-node",
            MinWidth = 180,
            Background = new SolidColorBrush(Color.FromRgb(255, 246, 204)),
            BorderBrush = NoteAccent,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12, 10),
            Child = new TextBlock
            {
                Text = node.Text,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.Black,
                MaxWidth = 260,
            },
        };

    private static Control BuildShell(
        WorkflowNodeBase node,
        string kind,
        IBrush accent,
        string tag,
        DiagramRenderContext context,
        string[] details,
        double cornerRadius = 7,
        double minWidth = 210)
    {
        var detailPanel = new StackPanel { Spacing = 2 };
        foreach (var detail in details)
            if (!string.IsNullOrWhiteSpace(detail))
                detailPanel.Children.Add(new TextBlock { Text = detail, Foreground = context.Palette.LinkStroke, FontSize = 11 });

        if (!string.IsNullOrWhiteSpace(node.Notes))
            detailPanel.Children.Add(new TextBlock
            {
                Text = node.Notes,
                Foreground = context.Palette.LinkStroke,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 240,
            });

        return new Border
        {
            Tag = tag,
            MinWidth = minWidth,
            Background = context.Palette.NodeBackground,
            BorderBrush = context.Palette.NodeBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(cornerRadius),
            ClipToBounds = true,
            Child = new StackPanel
            {
                Children =
                {
                    new Border
                    {
                        Background = accent,
                        Padding = new Thickness(12, 8),
                        Child = new StackPanel
                        {
                            Spacing = 1,
                            Children =
                            {
                                new TextBlock { Text = kind, Foreground = Brushes.White, FontSize = 10, FontWeight = FontWeight.SemiBold },
                                new TextBlock { Text = node.Label, Foreground = Brushes.White, FontWeight = FontWeight.SemiBold },
                            },
                        },
                    },
                    new Border
                    {
                        IsVisible = detailPanel.Children.Count > 0,
                        Padding = new Thickness(12, 7),
                        Child = detailPanel,
                    },
                },
            },
        };
    }

    private static LinkStyle StyleFor(WorkflowLink link)
    {
        var stroke = link.Kind switch
        {
            WorkflowLinkKind.Conditional => DecisionAccent,
            WorkflowLinkKind.Error => Brushes.IndianRed,
            WorkflowLinkKind.Message => EventAccent,
            _ => Brushes.DimGray,
        };

        return new LinkStyle
        {
            Stroke = stroke,
            SelectedStroke = Brushes.White,
            DashStyle = link.Kind is WorkflowLinkKind.Conditional or WorkflowLinkKind.Message
                ? DashStyle.Dash
                : null,
            Width = link.Width,
        };
    }

    private static void DrawWorkflowLink(DrawingContext context, LinkRenderContext ctx)
    {
        ctx.DrawDefault();

        var link = (WorkflowLink)ctx.Link;
        if (link.Kind == WorkflowLinkKind.Sequence)
            return;

        var pathLength = ctx.Path.Length();
        var midpoint = ctx.Path.PointAtDistance(pathLength / 2);
        if (midpoint is null)
            return;

        var stroke = ctx.IsSelected ? ctx.Palette.Selection : StyleFor(link).Stroke ?? ctx.Palette.LinkStroke;
        var pen = new Pen(stroke, 1.4);
        var fill = ctx.Palette.NodeBackground;
        var point = new Point(midpoint.X, midpoint.Y);

        if (link.Kind == WorkflowLinkKind.Message)
        {
            var rect = new Rect(point.X - 8, point.Y - 6, 16, 12);
            context.DrawRectangle(fill, pen, rect);
            context.DrawLine(pen, new Point(rect.Left, rect.Top), new Point(point.X, point.Y + 1));
            context.DrawLine(pen, new Point(rect.Right, rect.Top), new Point(point.X, point.Y + 1));
        }
        else if (link.Kind == WorkflowLinkKind.Error)
        {
            context.DrawEllipse(fill, pen, point, 7, 7);
            context.DrawLine(pen, new Point(point.X - 4, point.Y - 4), new Point(point.X + 4, point.Y + 4));
            context.DrawLine(pen, new Point(point.X + 4, point.Y - 4), new Point(point.X - 4, point.Y + 4));
        }
        else if (link.Kind == WorkflowLinkKind.Conditional)
        {
            var geometry = new StreamGeometry();
            using (var g = geometry.Open())
            {
                g.BeginFigure(new Point(point.X, point.Y - 8), isFilled: true);
                g.LineTo(new Point(point.X + 8, point.Y));
                g.LineTo(new Point(point.X, point.Y + 8));
                g.LineTo(new Point(point.X - 8, point.Y));
                g.EndFigure(isClosed: true);
            }

            context.DrawGeometry(fill, pen, geometry);
        }
    }
}
