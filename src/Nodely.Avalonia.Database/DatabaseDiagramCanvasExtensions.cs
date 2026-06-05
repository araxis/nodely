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
using Nodely.Models.Base;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Database;

/// <summary>Canvas registration helpers for the database node pack.</summary>
public static class DatabaseDiagramCanvasExtensions
{
    private static readonly Color TableColor = Color.FromRgb(55, 155, 122);
    private static readonly Color ViewColor = Color.FromRgb(67, 128, 214);
    private static readonly Color ProcedureColor = Color.FromRgb(151, 103, 201);
    private static readonly Color KeyColor = Color.FromRgb(220, 171, 53);
    private static readonly Color ForeignKeyColor = Color.FromRgb(74, 153, 218);
    private static readonly Color DependencyColor = Color.FromRgb(51, 166, 184);
    private static readonly Color InputColor = Color.FromRgb(82, 169, 104);
    private static readonly Color OutputColor = Color.FromRgb(223, 178, 66);

    /// <summary>Registers database node, port, and relationship renderers on the canvas.</summary>
    public static DiagramCanvas UseDatabaseNodes(this DiagramCanvas canvas)
    {
        if (canvas is null)
            throw new ArgumentNullException(nameof(canvas));

        canvas.RegisterNode<DatabaseTableNode>((node, context) =>
            BuildColumnObject(node, "TABLE", "T", node.Columns, TableColor, "database-table-node", context));
        canvas.RegisterNode<DatabaseViewNode>((node, context) =>
            BuildColumnObject(node, "VIEW", "V", node.Columns, ViewColor, "database-view-node", context));
        canvas.RegisterNode<DatabaseProcedureNode>(BuildProcedureObject);
        canvas.RegisterPort<DatabasePortModel>(BuildPort);
        canvas.RegisterLinkStyle<DatabaseRelationshipLink>(StyleFor);
        canvas.RegisterLink<DatabaseRelationshipLink>(DrawRelationship);

        return canvas;
    }

    private static Control BuildColumnObject(
        DatabaseObjectNode node,
        string kind,
        string icon,
        IEnumerable<DatabaseColumn> columns,
        Color accent,
        string tag,
        DiagramRenderContext context)
        => BuildShell(node, kind, icon, accent, tag, BuildColumnRows(columns, context), context);

    private static Control BuildProcedureObject(DatabaseProcedureNode node, DiagramRenderContext context)
        => BuildShell(
            node,
            "PROCEDURE",
            "P",
            ProcedureColor,
            "database-procedure-node",
            BuildParameterRows(node.Parameters, context),
            context);

    private static Control BuildShell(
        DatabaseObjectNode node,
        string kind,
        string icon,
        Color accent,
        string tag,
        IEnumerable<Control> rows,
        DiagramRenderContext context)
    {
        var rowPanel = new StackPanel();
        foreach (var row in rows)
            rowPanel.Children.Add(row);

        if (rowPanel.Children.Count == 0)
            rowPanel.Children.Add(BuildEmptyRow(context));

        var content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("5,*"),
            Children =
            {
                new Border { Background = Brush(accent) },
                new StackPanel
                {
                    Children =
                    {
                        BuildHeader(node, kind, icon, accent, context),
                        new Border
                        {
                            Padding = new Thickness(0, DatabaseVisualMetrics.BodyTopPadding, 0, DatabaseVisualMetrics.BodyBottomPadding),
                            Child = rowPanel,
                        },
                    },
                },
            },
        };
        Grid.SetColumn(content.Children[1], 1);

        return new Border
        {
            Tag = tag,
            MinWidth = DatabaseVisualMetrics.MinObjectWidth,
            Background = context.Palette.NodeBackground,
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1.2),
            CornerRadius = new CornerRadius(6),
            ClipToBounds = true,
            Child = content,
        };
    }

    private static Control BuildHeader(
        DatabaseObjectNode node,
        string kind,
        string icon,
        Color accent,
        DiagramRenderContext context)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("42,*,Auto"),
        };

        grid.Children.Add(new Border
        {
            Width = 30,
            Height = 30,
            CornerRadius = new CornerRadius(6),
            Background = Brush(accent),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = icon,
                Foreground = Brushes.White,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        });

        var title = new StackPanel { Spacing = 1 };
        title.Children.Add(new TextBlock
        {
            Text = node.ObjectName,
            Foreground = context.Palette.NodeText,
            FontWeight = FontWeight.SemiBold,
            FontSize = 14,
        });
        title.Children.Add(new TextBlock
        {
            Text = kind,
            Foreground = context.Palette.LinkStroke,
            FontSize = 10,
            FontWeight = FontWeight.SemiBold,
        });
        Grid.SetColumn(title, 1);
        grid.Children.Add(title);

        var schema = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(45, accent.R, accent.G, accent.B)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, accent.R, accent.G, accent.B)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(8, 2),
            VerticalAlignment = VerticalAlignment.Top,
            Child = new TextBlock
            {
                Text = node.Schema,
                Foreground = context.Palette.NodeText,
                FontSize = 10,
            },
        };
        Grid.SetColumn(schema, 2);
        grid.Children.Add(schema);

        return new Border
        {
            Height = DatabaseVisualMetrics.HeaderHeight,
            Background = new SolidColorBrush(Color.FromArgb(35, accent.R, accent.G, accent.B)),
            Padding = new Thickness(12, 9, 12, 8),
            Child = grid,
        };
    }

    private static IEnumerable<Control> BuildColumnRows(IEnumerable<DatabaseColumn> columns, DiagramRenderContext context)
        => columns.Select(column => BuildFieldRow(
            column.IsPrimaryKey ? "PK" : column.IsForeignKey ? "FK" : "",
            column.IsPrimaryKey ? KeyColor : column.IsForeignKey ? ForeignKeyColor : default(Color?),
            column.Name,
            column.DataType,
            column.IsNullable ? "NULL" : "NN",
            context,
            important: column.IsPrimaryKey || column.IsForeignKey));

    private static IEnumerable<Control> BuildParameterRows(IEnumerable<DatabaseParameter> parameters, DiagramRenderContext context)
        => parameters.Select(parameter => BuildFieldRow(
            parameter.Direction,
            ProcedureColor,
            parameter.Name,
            parameter.DataType,
            "",
            context,
            important: true));

    private static Control BuildFieldRow(
        string badge,
        Color? badgeColor,
        string name,
        string type,
        string suffix,
        DiagramRenderContext context,
        bool important)
    {
        var grid = new Grid
        {
            Height = DatabaseVisualMetrics.RowHeight,
            ColumnDefinitions = new ColumnDefinitions("42,*,Auto,54"),
        };

        grid.Children.Add(BuildBadge(badge, badgeColor, context));
        grid.Children.Add(Cell(name, context.Palette.NodeText, 1, important ? FontWeight.SemiBold : FontWeight.Normal));
        grid.Children.Add(Cell(type, context.Palette.LinkStroke, 2, FontWeight.Normal, monospace: true));
        grid.Children.Add(BuildSuffix(suffix, context));

        return new Border
        {
            Tag = "database-field-row",
            Height = DatabaseVisualMetrics.RowHeight,
            Padding = new Thickness(12, 0, 12, 0),
            BorderBrush = context.Palette.NodeBorder,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Background = important
                ? new SolidColorBrush(Color.FromArgb(18, 255, 255, 255))
                : Brushes.Transparent,
            Child = grid,
        };
    }

    private static Control BuildEmptyRow(DiagramRenderContext context) => new Border
    {
        Height = DatabaseVisualMetrics.RowHeight,
        Padding = new Thickness(12, 0),
        Child = new TextBlock
        {
            Text = "No fields",
            Foreground = context.Palette.LinkStroke,
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
        },
    };

    private static Control BuildBadge(string text, Color? color, DiagramRenderContext context)
    {
        var badge = new Border
        {
            Width = 28,
            Height = 16,
            CornerRadius = new CornerRadius(4),
            Background = color.HasValue ? Brush(color.Value) : Brushes.Transparent,
            BorderBrush = color.HasValue ? null : context.Palette.NodeBorder,
            BorderThickness = color.HasValue ? new Thickness(0) : new Thickness(1),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(text) ? " " : text,
                Foreground = color.HasValue ? Brushes.White : context.Palette.LinkStroke,
                FontSize = 9,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        Grid.SetColumn(badge, 0);
        return badge;
    }

    private static TextBlock Cell(string text, IBrush brush, int column, FontWeight weight, bool monospace = false)
    {
        var block = new TextBlock
        {
            Text = text,
            Foreground = brush,
            FontSize = 11,
            FontWeight = weight,
            FontFamily = monospace ? new FontFamily("Consolas") : FontFamily.Default,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(block, column);
        return block;
    }

    private static Control BuildSuffix(string suffix, DiagramRenderContext context)
    {
        var chip = new Border
        {
            IsVisible = !string.IsNullOrWhiteSpace(suffix),
            MinWidth = 34,
            Height = 16,
            CornerRadius = new CornerRadius(8),
            Background = suffix == "NN"
                ? new SolidColorBrush(Color.FromArgb(55, KeyColor.R, KeyColor.G, KeyColor.B))
                : new SolidColorBrush(Color.FromArgb(32, 255, 255, 255)),
            Padding = new Thickness(6, 0),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = suffix,
                Foreground = context.Palette.NodeText,
                FontSize = 9,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        Grid.SetColumn(chip, 3);
        return chip;
    }

    private static Control BuildPort(DatabasePortModel port, DiagramRenderContext context)
    {
        var color = port.Kind switch
        {
            DatabasePortKind.Dependency => DependencyColor,
            DatabasePortKind.Input => InputColor,
            DatabasePortKind.Output => OutputColor,
            _ => ForeignKeyColor,
        };

        var container = new Grid
        {
            Tag = port.Kind == DatabasePortKind.Relationship ? "database-relationship-port" : "database-port",
            Width = port.Kind == DatabasePortKind.Relationship ? 20 : 16,
            Height = port.Kind == DatabasePortKind.Relationship ? 16 : 16,
            Background = Brushes.Transparent,
        };

        if (port.Kind == DatabasePortKind.Relationship)
        {
            container.Children.Add(new Border
            {
                Width = 20,
                Height = 14,
                CornerRadius = new CornerRadius(5),
                Background = Brush(color),
                BorderBrush = context.Palette.NodeBackground,
                BorderThickness = new Thickness(1.5),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new Canvas
                {
                    Width = 12,
                    Height = 8,
                    Children =
                    {
                        KeyCircle(context),
                        KeyStem(context),
                        KeyTooth(context),
                    },
                },
            });
        }
        else if (port.Kind == DatabasePortKind.Dependency)
        {
            container.Children.Add(new Border
            {
                Width = 12,
                Height = 12,
                Background = Brush(color),
                BorderBrush = context.Palette.NodeBackground,
                BorderThickness = new Thickness(1.5),
                RenderTransformOrigin = RelativePoint.Center,
                RenderTransform = new RotateTransform(45),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }
        else
        {
            container.Children.Add(new Ellipse
            {
                Width = 15,
                Height = 15,
                Fill = Brush(color),
                Stroke = context.Palette.NodeBackground,
                StrokeThickness = 1.5,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }

        return container;
    }

    private static Ellipse KeyCircle(DiagramRenderContext context)
    {
        var circle = new Ellipse
        {
            Width = 5,
            Height = 5,
            Stroke = context.Palette.NodeBackground,
            StrokeThickness = 1.4,
        };
        Canvas.SetLeft(circle, 1);
        Canvas.SetTop(circle, 1);
        return circle;
    }

    private static Rectangle KeyStem(DiagramRenderContext context)
    {
        var stem = new Rectangle
        {
            Width = 8,
            Height = 2,
            Fill = context.Palette.NodeBackground,
        };
        Canvas.SetLeft(stem, 5);
        Canvas.SetTop(stem, 3);
        return stem;
    }

    private static Rectangle KeyTooth(DiagramRenderContext context)
    {
        var tooth = new Rectangle
        {
            Width = 2,
            Height = 4,
            Fill = context.Palette.NodeBackground,
        };
        Canvas.SetLeft(tooth, 10);
        Canvas.SetTop(tooth, 3);
        return tooth;
    }

    private static LinkStyle StyleFor(DatabaseRelationshipLink relationship)
        => relationship.Kind switch
        {
            RelationshipKind.Dependency => new LinkStyle
            {
                Stroke = Brush(DependencyColor),
                SelectedStroke = Brushes.White,
                DashStyle = DashStyle.Dash,
                Width = relationship.Width,
            },
            RelationshipKind.ManyToMany => new LinkStyle
            {
                Stroke = Brush(ProcedureColor),
                SelectedStroke = Brushes.White,
                Width = relationship.Width,
            },
            RelationshipKind.Association => new LinkStyle
            {
                Stroke = Brushes.SlateGray,
                SelectedStroke = Brushes.White,
                Width = relationship.Width,
            },
            _ => new LinkStyle
            {
                Stroke = Brush(TableColor),
                SelectedStroke = Brushes.White,
                Width = relationship.Width,
            },
        };

    private static void DrawRelationship(DrawingContext context, LinkRenderContext ctx)
    {
        ctx.DrawDefault();

        var relationship = (DatabaseRelationshipLink)ctx.Link;
        var style = StyleFor(relationship);
        var stroke = ctx.IsSelected ? ctx.Palette.Selection : style.Stroke ?? ctx.Palette.LinkStroke;
        var pen = new Pen(stroke, relationship.Kind == RelationshipKind.Dependency ? 1.7 : 1.9);

        if (relationship.Kind == RelationshipKind.Dependency)
        {
            if (TryGetEnd(ctx.Path, target: true, out var targetPoint, out var targetAngle))
                DrawOpenArrow(context, targetPoint, targetAngle, pen);
        }
        else if (relationship.Kind != RelationshipKind.Association)
        {
            if (TryGetEnd(ctx.Path, target: false, out var sourcePoint, out var sourceAngle))
                DrawEndpoint(context, relationship.Kind, source: true, sourcePoint, sourceAngle, pen);
            if (TryGetEnd(ctx.Path, target: true, out var targetPoint, out var targetAngle))
                DrawEndpoint(context, relationship.Kind, source: false, targetPoint, targetAngle, pen);
        }

        DrawCardinality(context, ctx, relationship.SourceCardinality, target: false);
        DrawCardinality(context, ctx, relationship.TargetCardinality, target: true);
    }

    private static void DrawEndpoint(
        DrawingContext context,
        RelationshipKind kind,
        bool source,
        NodelyPoint position,
        double angleDegrees,
        Pen pen)
    {
        var many = kind == RelationshipKind.ManyToMany || (!source && kind == RelationshipKind.OneToMany);
        using (context.PushTransform(MarkerTransform(position, angleDegrees)))
        {
            if (many)
                DrawCrowFoot(context, pen);
            else
                DrawOneBar(context, pen);
        }
    }

    private static void DrawOneBar(DrawingContext context, Pen pen)
        => context.DrawLine(pen, new Point(-7, -8), new Point(-7, 8));

    private static void DrawCrowFoot(DrawingContext context, Pen pen)
    {
        context.DrawLine(pen, new Point(0, 0), new Point(-14, -8));
        context.DrawLine(pen, new Point(0, 0), new Point(-14, 0));
        context.DrawLine(pen, new Point(0, 0), new Point(-14, 8));
    }

    private static void DrawOpenArrow(DrawingContext context, NodelyPoint position, double angleDegrees, Pen pen)
    {
        using (context.PushTransform(MarkerTransform(position, angleDegrees)))
        {
            context.DrawLine(pen, new Point(0, 0), new Point(-12, -7));
            context.DrawLine(pen, new Point(0, 0), new Point(-12, 7));
        }
    }

    private static void DrawCardinality(DrawingContext context, LinkRenderContext ctx, string? text, bool target)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var length = ctx.Path.Length();
        var distance = target ? Math.Max(0, length - 24) : Math.Min(length, 24);
        var position = ctx.Path.PointAtDistance(distance);
        if (position is not { } point)
            return;

        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            11,
            ctx.Palette.LabelForeground);

        const double padX = 5;
        const double padY = 2;
        var rect = new Rect(
            point.X - formatted.Width / 2 - padX,
            point.Y - formatted.Height / 2 - padY,
            formatted.Width + padX * 2,
            formatted.Height + padY * 2);

        context.DrawRectangle(ctx.Palette.LabelBackground, null, rect, 4, 4);
        context.DrawText(formatted, new Point(rect.X + padX, rect.Y + padY));
    }

    private static bool TryGetEnd(Nodely.Geometry.PathData path, bool target, out NodelyPoint point, out double angle)
    {
        var length = path.Length();
        var end = path.PointAtDistance(target ? length : 0);
        var near = path.PointAtDistance(target ? Math.Max(0, length - 18) : Math.Min(length, 18));

        if (end is not { } endPoint || near is not { } nearPoint)
        {
            point = new NodelyPoint(0, 0);
            angle = 0;
            return false;
        }

        point = endPoint;
        angle = Angle(nearPoint, endPoint);
        return true;
    }

    private static double Angle(NodelyPoint from, NodelyPoint to)
        => Math.Atan2(to.Y - from.Y, to.X - from.X) * 180 / Math.PI;

    private static Matrix MarkerTransform(NodelyPoint position, double angleDegrees)
        => Matrix.CreateRotation(angleDegrees * Math.PI / 180) * Matrix.CreateTranslation(position.X, position.Y);

    private static IBrush Brush(Color color) => new SolidColorBrush(color);
}
