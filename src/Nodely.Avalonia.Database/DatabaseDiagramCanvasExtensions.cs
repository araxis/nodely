using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Nodely.Avalonia.Controls;
using Nodely.Models.Base;

namespace Nodely.Avalonia.Database;

/// <summary>Canvas registration helpers for the database node pack.</summary>
public static class DatabaseDiagramCanvasExtensions
{
    /// <summary>Registers database node, port, and relationship renderers on the canvas.</summary>
    public static DiagramCanvas UseDatabaseNodes(this DiagramCanvas canvas)
    {
        if (canvas is null)
            throw new ArgumentNullException(nameof(canvas));

        canvas.RegisterNode<DatabaseTableNode>((node, context) =>
            BuildColumnNode(node, "TABLE", node.Columns, TableAccent, "database-table-node", context));
        canvas.RegisterNode<DatabaseViewNode>((node, context) =>
            BuildColumnNode(node, "VIEW", node.Columns, ViewAccent, "database-view-node", context));
        canvas.RegisterNode<DatabaseProcedureNode>(BuildProcedureNode);
        canvas.RegisterPort<DatabasePortModel>(BuildPort);
        canvas.RegisterLinkStyle<DatabaseRelationshipLink>(StyleFor);

        return canvas;
    }

    private static readonly IBrush TableAccent = new SolidColorBrush(Color.FromRgb(68, 158, 118));
    private static readonly IBrush ViewAccent = new SolidColorBrush(Color.FromRgb(76, 140, 220));
    private static readonly IBrush ProcedureAccent = new SolidColorBrush(Color.FromRgb(168, 116, 214));

    private static Control BuildColumnNode(
        DatabaseObjectNode node,
        string kind,
        IEnumerable<DatabaseColumn> columns,
        IBrush accent,
        string tag,
        DiagramRenderContext context)
        => BuildShell(node, kind, accent, tag, BuildColumnRows(columns, context), context);

    private static Control BuildProcedureNode(DatabaseProcedureNode node, DiagramRenderContext context)
        => BuildShell(node, "PROCEDURE", ProcedureAccent, "database-procedure-node", BuildParameterRows(node.Parameters, context), context);

    private static Control BuildShell(
        DatabaseObjectNode node,
        string kind,
        IBrush accent,
        string tag,
        IEnumerable<Control> rows,
        DiagramRenderContext context)
    {
        var rowPanel = new StackPanel { Spacing = 2 };
        foreach (var row in rows)
            rowPanel.Children.Add(row);

        if (rowPanel.Children.Count == 0)
            rowPanel.Children.Add(new TextBlock { Text = "No fields", Foreground = context.Palette.LinkStroke, FontSize = 11 });

        return new Border
        {
            Tag = tag,
            MinWidth = 220,
            Background = context.Palette.NodeBackground,
            BorderBrush = context.Palette.NodeBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(7),
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
                                new TextBlock { Text = node.Title ?? node.ObjectName, Foreground = Brushes.White, FontWeight = FontWeight.SemiBold },
                            },
                        },
                    },
                    new Border
                    {
                        Padding = new Thickness(12, 8),
                        Child = rowPanel,
                    },
                },
            },
        };
    }

    private static IEnumerable<Control> BuildColumnRows(IEnumerable<DatabaseColumn> columns, DiagramRenderContext context)
        => columns.Select(column => BuildRow(
            column.IsPrimaryKey ? "PK" : column.IsForeignKey ? "FK" : "",
            column.Name,
            column.DataType,
            column.IsNullable ? "null" : "not null",
            context));

    private static IEnumerable<Control> BuildParameterRows(IEnumerable<DatabaseParameter> parameters, DiagramRenderContext context)
        => parameters.Select(parameter => BuildRow(parameter.Direction, parameter.Name, parameter.DataType, "", context));

    private static Control BuildRow(string badge, string name, string type, string suffix, DiagramRenderContext context)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("34,*,Auto,Auto"),
            MinHeight = 20,
        };

        var muted = context.Palette.LinkStroke;
        grid.Children.Add(Cell(badge, muted, 0, FontWeight.SemiBold));
        grid.Children.Add(Cell(name, context.Palette.NodeText, 1, FontWeight.Normal));
        grid.Children.Add(Cell(type, muted, 2, FontWeight.Normal));
        grid.Children.Add(Cell(suffix, muted, 3, FontWeight.Normal));
        return grid;
    }

    private static TextBlock Cell(string text, IBrush brush, int column, FontWeight weight)
    {
        var block = new TextBlock
        {
            Text = text,
            Foreground = brush,
            FontSize = 11,
            FontWeight = weight,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(block, column);
        return block;
    }

    private static Control BuildPort(DatabasePortModel port)
    {
        var fill = port.Kind switch
        {
            DatabasePortKind.Dependency => Brushes.DeepSkyBlue,
            DatabasePortKind.Input => Brushes.MediumSeaGreen,
            DatabasePortKind.Output => Brushes.Gold,
            _ => Brushes.CornflowerBlue,
        };

        return new Ellipse
        {
            Tag = "database-port",
            Width = 12,
            Height = 12,
            Fill = fill,
            Stroke = Brushes.White,
            StrokeThickness = 1.5,
        };
    }

    private static LinkStyle StyleFor(DatabaseRelationshipLink relationship)
        => relationship.Kind switch
        {
            RelationshipKind.Dependency => new LinkStyle
            {
                Stroke = Brushes.DeepSkyBlue,
                SelectedStroke = Brushes.White,
                DashStyle = DashStyle.Dash,
                Width = relationship.Width,
            },
            RelationshipKind.ManyToMany => new LinkStyle
            {
                Stroke = Brushes.MediumPurple,
                SelectedStroke = Brushes.White,
                Width = relationship.Width,
            },
            _ => new LinkStyle
            {
                Stroke = Brushes.MediumSeaGreen,
                SelectedStroke = Brushes.White,
                Width = relationship.Width,
            },
        };
}
