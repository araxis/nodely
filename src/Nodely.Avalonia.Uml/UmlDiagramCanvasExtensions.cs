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
using Nodely.Models.Base;
using AvPoint = Avalonia.Point;
using NodelyPoint = Nodely.Geometry.Point;
using ShapeEllipse = Avalonia.Controls.Shapes.Ellipse;
using ShapePath = Avalonia.Controls.Shapes.Path;

namespace Nodely.Avalonia.Uml;

/// <summary>Canvas registration helpers for the UML node pack.</summary>
public static class UmlDiagramCanvasExtensions
{
    private static readonly Color ClassColor = Color.FromRgb(65, 136, 168);
    private static readonly Color InterfaceColor = Color.FromRgb(68, 154, 126);
    private static readonly Color EnumColor = Color.FromRgb(178, 130, 61);
    private static readonly Color PackageColor = Color.FromRgb(132, 112, 178);
    private static readonly Color NoteColor = Color.FromRgb(209, 166, 57);
    private static readonly Color DependencyColor = Color.FromRgb(190, 147, 61);
    private static readonly Color InheritanceColor = Color.FromRgb(82, 139, 196);
    private static readonly Color CompositionColor = Color.FromRgb(62, 148, 110);

    private static readonly StreamGeometry TargetTriangle = ClosedGeometry(new AvPoint(0, 0), new AvPoint(-16, -9), new AvPoint(-16, 9));
    private static readonly StreamGeometry SourceDiamond = ClosedGeometry(new AvPoint(0, 0), new AvPoint(10, -7), new AvPoint(21, 0), new AvPoint(10, 7));
    private static readonly StreamGeometry TargetOpenArrow = OpenGeometry(new AvPoint(-13, -7), new AvPoint(0, 0), new AvPoint(-13, 7));

    /// <summary>Registers UML node, port, and relationship renderers on the canvas.</summary>
    public static DiagramCanvas UseUmlNodes(this DiagramCanvas canvas)
    {
        if (canvas is null)
            throw new ArgumentNullException(nameof(canvas));

        canvas.RegisterNode<UmlClassNode>(BuildClassNode);
        canvas.RegisterNode<UmlInterfaceNode>(BuildInterfaceNode);
        canvas.RegisterNode<UmlEnumNode>(BuildEnumNode);
        canvas.RegisterNode<UmlPackageNode>(BuildPackageNode);
        canvas.RegisterNode<UmlNoteNode>(BuildNoteNode);
        canvas.RegisterPort<UmlPortModel>(BuildPort);
        canvas.RegisterLinkStyle<UmlRelationshipLink>(StyleFor);
        canvas.RegisterLink<UmlRelationshipLink>(DrawRelationship);

        return canvas;
    }

    private static Control BuildClassNode(UmlClassNode node, DiagramRenderContext context)
    {
        var kind = node.IsAbstract ? "ABSTRACT CLASS" : node.IsStatic ? "STATIC CLASS" : "CLASS";
        return BuildTypeShell(
            node,
            kind,
            "C",
            ClassColor,
            "uml-class-node",
            new[]
            {
                BuildMemberSection("Attributes", node.Members, context),
                BuildOperationSection("Operations", node.Operations, context),
            },
            context,
            italicName: node.IsAbstract,
            flags: Flags(node.IsAbstract, node.IsStatic));
    }

    private static Control BuildInterfaceNode(UmlInterfaceNode node, DiagramRenderContext context)
    {
        var stereotypes = node.Stereotypes.Count == 0
            ? new[] { "interface" }
            : node.Stereotypes.AsEnumerable();
        return BuildTypeShell(
            node,
            "INTERFACE",
            "I",
            InterfaceColor,
            "uml-interface-node",
            new[] { BuildOperationSection("Operations", node.Operations, context) },
            context,
            extraStereotypes: stereotypes);
    }

    private static Control BuildEnumNode(UmlEnumNode node, DiagramRenderContext context)
        => BuildTypeShell(
            node,
            "ENUM",
            "E",
            EnumColor,
            "uml-enum-node",
            new[] { BuildLiteralSection("Literals", node.Literals, context) },
            context,
            extraStereotypes: node.Stereotypes.Count == 0 ? new[] { "enumeration" } : null);

    private static Control BuildTypeShell(
        UmlNodeBase node,
        string kind,
        string icon,
        Color accent,
        string tag,
        IEnumerable<Control> sections,
        DiagramRenderContext context,
        IEnumerable<string>? extraStereotypes = null,
        IEnumerable<string>? flags = null,
        bool italicName = false)
    {
        var panel = new StackPanel();
        panel.Children.Add(BuildHeader(node, kind, icon, accent, context, extraStereotypes, flags, italicName));

        foreach (var section in sections)
            panel.Children.Add(section);

        return new Border
        {
            Tag = tag,
            MinWidth = UmlVisualMetrics.MinTypeWidth,
            Background = context.Palette.NodeBackground,
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1.2),
            CornerRadius = new CornerRadius(6),
            ClipToBounds = true,
            Child = panel,
        };
    }

    private static Control BuildHeader(
        UmlNodeBase node,
        string kind,
        string icon,
        Color accent,
        DiagramRenderContext context,
        IEnumerable<string>? extraStereotypes = null,
        IEnumerable<string>? flags = null,
        bool italicName = false)
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

        var titlePanel = new StackPanel { Spacing = 1 };
        titlePanel.Children.Add(new TextBlock
        {
            Text = node.Name,
            Foreground = context.Palette.NodeText,
            FontSize = 15,
            FontWeight = FontWeight.SemiBold,
            FontStyle = italicName ? FontStyle.Italic : FontStyle.Normal,
        });
        titlePanel.Children.Add(new TextBlock
        {
            Text = kind,
            Foreground = context.Palette.LinkStroke,
            FontSize = 10,
            FontWeight = FontWeight.SemiBold,
        });

        var stereotypes = (extraStereotypes ?? node.Stereotypes)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim('<', '>', ' '))
            .ToList();
        if (stereotypes.Count > 0)
        {
            titlePanel.Children.Add(new TextBlock
            {
                Text = "<<" + string.Join(", ", stereotypes) + ">>",
                Foreground = context.Palette.LinkStroke,
                FontSize = 10,
            });
        }

        Grid.SetColumn(titlePanel, 1);
        grid.Children.Add(titlePanel);

        var flagsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Top,
        };
        foreach (var flag in flags ?? Array.Empty<string>())
            flagsPanel.Children.Add(BuildChip(flag, accent, context));
        Grid.SetColumn(flagsPanel, 2);
        grid.Children.Add(flagsPanel);

        return new Border
        {
            Height = UmlVisualMetrics.HeaderHeight,
            Background = new SolidColorBrush(Color.FromArgb(38, accent.R, accent.G, accent.B)),
            Padding = new Thickness(12, 9, 12, 8),
            Child = grid,
        };
    }

    private static Control BuildMemberSection(string title, IReadOnlyList<UmlMember> members, DiagramRenderContext context)
        => BuildSection(title, members.Select(member => BuildMemberRow(member, context)), members.Count == 0, context);

    private static Control BuildOperationSection(string title, IReadOnlyList<UmlOperation> operations, DiagramRenderContext context)
        => BuildSection(title, operations.Select(operation => BuildOperationRow(operation, context)), operations.Count == 0, context);

    private static Control BuildLiteralSection(string title, IReadOnlyList<string> literals, DiagramRenderContext context)
        => BuildSection(title, literals.Select(literal => BuildLiteralRow(literal, context)), literals.Count == 0, context);

    private static Control BuildSection(string title, IEnumerable<Control> rows, bool empty, DiagramRenderContext context)
    {
        var panel = new StackPanel();
        panel.Children.Add(new Border
        {
            Height = UmlVisualMetrics.SectionHeaderHeight,
            Background = new SolidColorBrush(Color.FromArgb(18, 255, 255, 255)),
            BorderBrush = context.Palette.NodeBorder,
            BorderThickness = new Thickness(0, 1, 0, 1),
            Padding = new Thickness(12, 0),
            Child = new TextBlock
            {
                Text = title,
                Foreground = context.Palette.LinkStroke,
                FontSize = 10,
                FontWeight = FontWeight.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
            },
        });

        if (empty)
        {
            panel.Children.Add(new Border
            {
                Height = UmlVisualMetrics.EmptyRowHeight,
                Padding = new Thickness(12, 0),
                Child = new TextBlock
                {
                    Text = "No entries",
                    Foreground = context.Palette.LinkStroke,
                    FontSize = 11,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            });
        }
        else
        {
            foreach (var row in rows)
                panel.Children.Add(row);
        }

        return panel;
    }

    private static Control BuildMemberRow(UmlMember member, DiagramRenderContext context)
    {
        var chips = new List<string>();
        if (member.IsStatic)
            chips.Add("static");
        if (member.IsAbstract)
            chips.Add("abstract");

        return BuildRow(
            Visibility(member.Visibility),
            member.Name,
            member.Type,
            chips,
            context,
            important: member.Visibility == UmlVisibility.Public);
    }

    private static Control BuildOperationRow(UmlOperation operation, DiagramRenderContext context)
    {
        var chips = new List<string>();
        if (operation.IsStatic)
            chips.Add("static");
        if (operation.IsAbstract)
            chips.Add("abstract");

        var signature = operation.Name + "(" + string.Join(", ", operation.Parameters.Select(FormatParameter)) + ")";
        return BuildRow(
            Visibility(operation.Visibility),
            signature,
            operation.ReturnType,
            chips,
            context,
            important: operation.Visibility == UmlVisibility.Public);
    }

    private static Control BuildLiteralRow(string literal, DiagramRenderContext context)
        => BuildRow("", literal, "", Array.Empty<string>(), context, important: false);

    private static Control BuildRow(
        string badge,
        string name,
        string type,
        IEnumerable<string> chips,
        DiagramRenderContext context,
        bool important)
    {
        var grid = new Grid
        {
            Height = UmlVisualMetrics.RowHeight,
            ColumnDefinitions = new ColumnDefinitions("30,*,Auto,Auto"),
        };

        grid.Children.Add(BuildVisibilityBadge(badge, context));
        grid.Children.Add(Cell(name, context.Palette.NodeText, 1, important ? FontWeight.SemiBold : FontWeight.Normal));
        grid.Children.Add(Cell(type, context.Palette.LinkStroke, 2, FontWeight.Normal, monospace: true));
        grid.Children.Add(BuildChipPanel(chips, context));

        return new Border
        {
            Tag = "uml-member-row",
            Height = UmlVisualMetrics.RowHeight,
            Padding = new Thickness(12, 0, 12, 0),
            BorderBrush = context.Palette.NodeBorder,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Background = important
                ? new SolidColorBrush(Color.FromArgb(16, 255, 255, 255))
                : Brushes.Transparent,
            Child = grid,
        };
    }

    private static Control BuildVisibilityBadge(string text, DiagramRenderContext context)
    {
        var accent = text switch
        {
            "+" => ClassColor,
            "#" => InterfaceColor,
            "-" => Color.FromRgb(185, 95, 86),
            "~" => PackageColor,
            _ => Color.FromArgb(0, 0, 0, 0),
        };

        var badge = new Border
        {
            Width = 20,
            Height = 16,
            CornerRadius = new CornerRadius(4),
            Background = string.IsNullOrWhiteSpace(text) ? Brushes.Transparent : Brush(accent),
            BorderBrush = string.IsNullOrWhiteSpace(text) ? context.Palette.NodeBorder : null,
            BorderThickness = string.IsNullOrWhiteSpace(text) ? new Thickness(1) : new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(text) ? " " : text,
                Foreground = string.IsNullOrWhiteSpace(text) ? context.Palette.LinkStroke : Brushes.White,
                FontSize = 11,
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

    private static Control BuildChipPanel(IEnumerable<string> chips, DiagramRenderContext context)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };

        foreach (var chip in chips)
            panel.Children.Add(BuildChip(chip, Color.FromRgb(120, 120, 120), context));

        Grid.SetColumn(panel, 3);
        return panel;
    }

    private static Control BuildChip(string text, Color color, DiagramRenderContext context)
        => new Border
        {
            MinWidth = 38,
            Height = 18,
            CornerRadius = new CornerRadius(9),
            Background = new SolidColorBrush(Color.FromArgb(55, color.R, color.G, color.B)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(95, color.R, color.G, color.B)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(7, 0),
            Child = new TextBlock
            {
                Text = text,
                Foreground = context.Palette.NodeText,
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };

    private static Control BuildPackageNode(UmlPackageNode node, DiagramRenderContext context)
    {
        var panel = new Grid();
        panel.Children.Add(new Border
        {
            Width = 108,
            Height = 24,
            Background = new SolidColorBrush(Color.FromArgb(55, PackageColor.R, PackageColor.G, PackageColor.B)),
            BorderBrush = Brush(PackageColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6, 6, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Child = new TextBlock
            {
                Text = "package",
                Foreground = context.Palette.NodeText,
                FontSize = 10,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        });

        panel.Children.Add(new Border
        {
            Margin = new Thickness(0, 20, 0, 0),
            MinWidth = 220,
            Padding = new Thickness(14, 12),
            Background = context.Palette.NodeBackground,
            BorderBrush = Brush(PackageColor),
            BorderThickness = new Thickness(1.2),
            CornerRadius = new CornerRadius(0, 6, 6, 6),
            Child = BuildPackageContent(node, context),
        });

        return new Border
        {
            Tag = "uml-package-node",
            Background = Brushes.Transparent,
            Child = panel,
        };
    }

    private static Control BuildPackageContent(UmlPackageNode node, DiagramRenderContext context)
    {
        var panel = new StackPanel { Spacing = 4 };
        panel.Children.Add(new TextBlock
        {
            Text = node.Name,
            Foreground = context.Palette.NodeText,
            FontWeight = FontWeight.SemiBold,
            FontSize = 14,
        });
        if (node.Stereotypes.Count > 0)
        {
            panel.Children.Add(new TextBlock
            {
                Text = "<<" + string.Join(", ", node.Stereotypes.Select(s => s.Trim('<', '>', ' '))) + ">>",
                Foreground = context.Palette.LinkStroke,
                FontSize = 10,
            });
        }

        return panel;
    }

    private static Control BuildNoteNode(UmlNoteNode node, DiagramRenderContext context)
    {
        var grid = new Grid();
        grid.Children.Add(new Border
        {
            MinWidth = 210,
            Padding = new Thickness(14, 12),
            Background = new SolidColorBrush(Color.FromRgb(255, 247, 204)),
            BorderBrush = Brush(NoteColor),
            BorderThickness = new Thickness(1.2),
            CornerRadius = new CornerRadius(4),
            Child = new TextBlock
            {
                Text = node.Text,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.Black,
                MaxWidth = 270,
            },
        });

        grid.Children.Add(new ShapePath
        {
            Data = ClosedGeometry(new AvPoint(0, 0), new AvPoint(22, 0), new AvPoint(22, 22)),
            Fill = new SolidColorBrush(Color.FromRgb(237, 222, 158)),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Width = 22,
            Height = 22,
            Stretch = Stretch.Fill,
        });

        return new Border
        {
            Tag = "uml-note-node",
            Background = Brushes.Transparent,
            Child = grid,
        };
    }

    private static Control BuildPort(UmlPortModel port, DiagramRenderContext context)
    {
        var color = port.Kind switch
        {
            UmlPortKind.Inheritance => InheritanceColor,
            UmlPortKind.Realization => InterfaceColor,
            UmlPortKind.Dependency => DependencyColor,
            UmlPortKind.Aggregation => CompositionColor,
            UmlPortKind.Composition => CompositionColor,
            UmlPortKind.ProvidedInterface => InterfaceColor,
            UmlPortKind.RequiredInterface => DependencyColor,
            _ => ClassColor,
        };

        var container = new Grid
        {
            Tag = "uml-port",
            Width = 18,
            Height = 18,
            Background = Brushes.Transparent,
        };

        if (port.Kind is UmlPortKind.Inheritance or UmlPortKind.Realization)
        {
            container.Children.Add(new ShapePath
            {
                Data = ClosedGeometry(new AvPoint(9, 1), new AvPoint(17, 16), new AvPoint(1, 16)),
                Fill = port.Kind == UmlPortKind.Realization ? context.Palette.NodeBackground : Brush(color),
                Stroke = Brush(color),
                StrokeThickness = 1.5,
                Stretch = Stretch.None,
            });
        }
        else if (port.Kind is UmlPortKind.Aggregation or UmlPortKind.Composition)
        {
            container.Children.Add(new Border
            {
                Width = 12,
                Height = 12,
                Background = port.Kind == UmlPortKind.Composition ? Brush(color) : context.Palette.NodeBackground,
                BorderBrush = Brush(color),
                BorderThickness = new Thickness(1.5),
                RenderTransformOrigin = RelativePoint.Center,
                RenderTransform = new RotateTransform(45),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }
        else if (port.Kind == UmlPortKind.ProvidedInterface)
        {
            container.Children.Add(new ShapeEllipse
            {
                Width = 14,
                Height = 14,
                Fill = context.Palette.NodeBackground,
                Stroke = Brush(color),
                StrokeThickness = 1.7,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }
        else
        {
            container.Children.Add(new ShapeEllipse
            {
                Width = 13,
                Height = 13,
                Fill = Brush(color),
                Stroke = context.Palette.NodeBackground,
                StrokeThickness = 1.5,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }

        return container;
    }

    private static LinkStyle StyleFor(UmlRelationshipLink relationship)
    {
        var stroke = relationship.Kind switch
        {
            UmlRelationshipKind.Dependency => Brush(DependencyColor),
            UmlRelationshipKind.Realization => Brush(InterfaceColor),
            UmlRelationshipKind.Inheritance => Brush(InheritanceColor),
            UmlRelationshipKind.Aggregation => Brush(CompositionColor),
            UmlRelationshipKind.Composition => Brush(CompositionColor),
            _ => Brushes.SlateGray,
        };

        return new LinkStyle
        {
            Stroke = stroke,
            SelectedStroke = Brushes.White,
            DashStyle = relationship.Kind is UmlRelationshipKind.Dependency or UmlRelationshipKind.Realization
                ? DashStyle.Dash
                : null,
            Width = relationship.Width,
        };
    }

    private static void DrawRelationship(DrawingContext context, LinkRenderContext ctx)
    {
        ctx.DrawDefault();

        var relationship = (UmlRelationshipLink)ctx.Link;
        var brush = ctx.IsSelected ? ctx.Palette.Selection : StyleFor(relationship).Stroke ?? ctx.Palette.LinkStroke;
        var pen = new Pen(brush, 1.7);
        var fill = ctx.Palette.NodeBackground;

        if (relationship.Kind is UmlRelationshipKind.Inheritance or UmlRelationshipKind.Realization &&
            TryGetEnd(ctx.Path, target: true, out var targetPoint, out var targetAngle))
        {
            DrawClosedMarker(context, TargetTriangle, targetPoint, targetAngle, fill, pen);
        }
        else if (relationship.Kind == UmlRelationshipKind.Dependency &&
                 TryGetEnd(ctx.Path, target: true, out targetPoint, out targetAngle))
        {
            DrawOpenMarker(context, TargetOpenArrow, targetPoint, targetAngle, pen);
        }
        else if (relationship.Kind == UmlRelationshipKind.Aggregation &&
                 TryGetEnd(ctx.Path, target: false, out var sourcePoint, out var sourceAngle))
        {
            DrawClosedMarker(context, SourceDiamond, sourcePoint, sourceAngle, fill, pen);
        }
        else if (relationship.Kind == UmlRelationshipKind.Composition &&
                 TryGetEnd(ctx.Path, target: false, out sourcePoint, out sourceAngle))
        {
            DrawClosedMarker(context, SourceDiamond, sourcePoint, sourceAngle, brush, pen);
        }

        DrawKindGlyph(context, ctx, relationship);
    }

    private static void DrawKindGlyph(DrawingContext context, LinkRenderContext ctx, UmlRelationshipLink relationship)
    {
        if (relationship.Kind == UmlRelationshipKind.Association)
            return;

        var point = ctx.Path.PointAtDistance(ctx.Path.Length() / 2);
        if (point is not { } p)
            return;

        var text = relationship.Kind switch
        {
            UmlRelationshipKind.Inheritance => "extends",
            UmlRelationshipKind.Realization => "realizes",
            UmlRelationshipKind.Aggregation => "aggregates",
            UmlRelationshipKind.Composition => "owns",
            UmlRelationshipKind.Dependency => "uses",
            _ => "",
        };
        if (string.IsNullOrWhiteSpace(text))
            return;

        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            10,
            ctx.Palette.LabelForeground);

        var rect = new Rect(p.X - formatted.Width / 2 - 5, p.Y - formatted.Height / 2 - 2, formatted.Width + 10, formatted.Height + 4);
        context.DrawRectangle(ctx.Palette.LabelBackground, null, rect, 4, 4);
        context.DrawText(formatted, new AvPoint(rect.X + 5, rect.Y + 2));
    }

    private static string FormatParameter(UmlParameter parameter)
        => string.IsNullOrWhiteSpace(parameter.DefaultValue)
            ? parameter.Name + ": " + parameter.Type
            : parameter.Name + ": " + parameter.Type + " = " + parameter.DefaultValue;

    private static IEnumerable<string> Flags(bool isAbstract, bool isStatic)
    {
        if (isAbstract)
            yield return "abstract";
        if (isStatic)
            yield return "static";
    }

    private static string Visibility(UmlVisibility visibility) => visibility switch
    {
        UmlVisibility.Public => "+",
        UmlVisibility.Protected => "#",
        UmlVisibility.Private => "-",
        _ => "~",
    };

    private static bool TryGetEnd(PathData path, bool target, out NodelyPoint point, out double angle)
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
        angle = target
            ? Angle(nearPoint, endPoint)
            : Angle(endPoint, nearPoint);
        return true;
    }

    private static double Angle(NodelyPoint from, NodelyPoint to)
        => Math.Atan2(to.Y - from.Y, to.X - from.X) * 180 / Math.PI;

    private static void DrawClosedMarker(
        DrawingContext context,
        StreamGeometry geometry,
        NodelyPoint position,
        double angleDegrees,
        IBrush fill,
        Pen pen)
    {
        using (context.PushTransform(MarkerTransform(position, angleDegrees)))
            context.DrawGeometry(fill, pen, geometry);
    }

    private static void DrawOpenMarker(
        DrawingContext context,
        StreamGeometry geometry,
        NodelyPoint position,
        double angleDegrees,
        Pen pen)
    {
        using (context.PushTransform(MarkerTransform(position, angleDegrees)))
            context.DrawGeometry(null, pen, geometry);
    }

    private static Matrix MarkerTransform(NodelyPoint position, double angleDegrees)
        => Matrix.CreateRotation(angleDegrees * Math.PI / 180) * Matrix.CreateTranslation(position.X, position.Y);

    private static StreamGeometry ClosedGeometry(params AvPoint[] points)
    {
        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        ctx.BeginFigure(points[0], isFilled: true);
        for (var i = 1; i < points.Length; i++)
            ctx.LineTo(points[i]);
        ctx.EndFigure(isClosed: true);
        return geometry;
    }

    private static StreamGeometry OpenGeometry(params AvPoint[] points)
    {
        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        ctx.BeginFigure(points[0], isFilled: false);
        for (var i = 1; i < points.Length; i++)
            ctx.LineTo(points[i]);
        ctx.EndFigure(isClosed: false);
        return geometry;
    }

    private static IBrush Brush(Color color) => new SolidColorBrush(color);
}
