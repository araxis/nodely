using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nodely.Avalonia.Controls;
using Nodely.Geometry;
using Nodely.Models.Base;
using AvPoint = Avalonia.Point;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Uml;

/// <summary>Canvas registration helpers for the UML node pack.</summary>
public static class UmlDiagramCanvasExtensions
{
    private static readonly IBrush ClassAccent = new SolidColorBrush(Color.FromRgb(73, 131, 166));
    private static readonly IBrush InterfaceAccent = new SolidColorBrush(Color.FromRgb(83, 154, 128));
    private static readonly IBrush EnumAccent = new SolidColorBrush(Color.FromRgb(163, 119, 74));
    private static readonly IBrush PackageAccent = new SolidColorBrush(Color.FromRgb(144, 113, 173));
    private static readonly IBrush NoteAccent = new SolidColorBrush(Color.FromRgb(207, 168, 70));
    private static readonly StreamGeometry TargetTriangle = ClosedGeometry(new AvPoint(0, 0), new AvPoint(-14, -8), new AvPoint(-14, 8));
    private static readonly StreamGeometry SourceDiamond = ClosedGeometry(new AvPoint(0, 0), new AvPoint(10, -7), new AvPoint(20, 0), new AvPoint(10, 7));
    private static readonly StreamGeometry TargetOpenArrow = OpenGeometry(new AvPoint(-12, -7), new AvPoint(0, 0), new AvPoint(-12, 7));

    /// <summary>Registers UML node and relationship renderers on the canvas.</summary>
    public static DiagramCanvas UseUmlNodes(this DiagramCanvas canvas)
    {
        if (canvas is null)
            throw new ArgumentNullException(nameof(canvas));

        canvas.RegisterNode<UmlClassNode>(BuildClassNode);
        canvas.RegisterNode<UmlInterfaceNode>(BuildInterfaceNode);
        canvas.RegisterNode<UmlEnumNode>(BuildEnumNode);
        canvas.RegisterNode<UmlPackageNode>(BuildPackageNode);
        canvas.RegisterNode<UmlNoteNode>(BuildNoteNode);
        canvas.RegisterLinkStyle<UmlRelationshipLink>(StyleFor);
        canvas.RegisterLink<UmlRelationshipLink>(DrawRelationship);

        return canvas;
    }

    private static Control BuildClassNode(UmlClassNode node, DiagramRenderContext context)
        => BuildTypeShell(
            node,
            node.IsAbstract ? "ABSTRACT CLASS" : node.IsStatic ? "STATIC CLASS" : "CLASS",
            ClassAccent,
            "uml-class-node",
            new[]
            {
                BuildRows(node.Members.Select(FormatMember), "No members", context),
                BuildRows(node.Operations.Select(FormatOperation), "No operations", context),
            },
            context,
            italicName: node.IsAbstract);

    private static Control BuildInterfaceNode(UmlInterfaceNode node, DiagramRenderContext context)
    {
        IEnumerable<string> stereotypes = node.Stereotypes.Count == 0
            ? new[] { "interface" }
            : node.Stereotypes;

        return BuildTypeShell(
            node,
            "INTERFACE",
            InterfaceAccent,
            "uml-interface-node",
            new[] { BuildRows(node.Operations.Select(FormatOperation), "No operations", context) },
            context,
            extraStereotypes: stereotypes);
    }

    private static Control BuildEnumNode(UmlEnumNode node, DiagramRenderContext context)
        => BuildTypeShell(
            node,
            "ENUM",
            EnumAccent,
            "uml-enum-node",
            new[] { BuildRows(node.Literals, "No literals", context) },
            context,
            extraStereotypes: node.Stereotypes.Count == 0 ? new[] { "enumeration" } : null);

    private static Control BuildPackageNode(UmlPackageNode node, DiagramRenderContext context)
    {
        var content = BuildHeader(node, "PACKAGE", PackageAccent, context, node.Stereotypes);
        return new Border
        {
            Tag = "uml-package-node",
            MinWidth = 190,
            Background = context.Palette.NodeBackground,
            BorderBrush = context.Palette.NodeBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            ClipToBounds = true,
            Child = content,
        };
    }

    private static Control BuildNoteNode(UmlNoteNode node, DiagramRenderContext context)
        => new Border
        {
            Tag = "uml-note-node",
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

    private static Control BuildTypeShell(
        UmlNodeBase node,
        string kind,
        IBrush accent,
        string tag,
        IEnumerable<Control> sections,
        DiagramRenderContext context,
        IEnumerable<string>? extraStereotypes = null,
        bool italicName = false)
    {
        var panel = new StackPanel();
        panel.Children.Add(BuildHeader(node, kind, accent, context, extraStereotypes, italicName));

        foreach (var section in sections)
            panel.Children.Add(section);

        return new Border
        {
            Tag = tag,
            MinWidth = 230,
            Background = context.Palette.NodeBackground,
            BorderBrush = context.Palette.NodeBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(7),
            ClipToBounds = true,
            Child = panel,
        };
    }

    private static Control BuildHeader(
        UmlNodeBase node,
        string kind,
        IBrush accent,
        DiagramRenderContext context,
        IEnumerable<string>? extraStereotypes = null,
        bool italicName = false)
    {
        var stereotypes = (extraStereotypes ?? node.Stereotypes).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        var panel = new StackPanel { Spacing = 1 };
        panel.Children.Add(new TextBlock { Text = kind, Foreground = Brushes.White, FontSize = 10, FontWeight = FontWeight.SemiBold });

        foreach (var stereotype in stereotypes)
            panel.Children.Add(new TextBlock { Text = "<<" + stereotype.Trim('<', '>', ' ') + ">>", Foreground = Brushes.White, FontSize = 11 });

        panel.Children.Add(new TextBlock
        {
            Text = node.Name,
            Foreground = Brushes.White,
            FontWeight = FontWeight.SemiBold,
            FontStyle = italicName ? FontStyle.Italic : FontStyle.Normal,
        });

        return new Border
        {
            Background = accent,
            Padding = new Thickness(12, 8),
            Child = panel,
        };
    }

    private static Control BuildRows(IEnumerable<string> rows, string emptyText, DiagramRenderContext context)
    {
        var panel = new StackPanel { Spacing = 2, Margin = new Thickness(0, 0, 0, 1) };
        var rowList = rows.Where(row => !string.IsNullOrWhiteSpace(row)).ToList();
        if (rowList.Count == 0)
        {
            panel.Children.Add(new TextBlock { Text = emptyText, Foreground = context.Palette.LinkStroke, FontSize = 11 });
        }
        else
        {
            foreach (var row in rowList)
                panel.Children.Add(new TextBlock { Text = row, Foreground = context.Palette.NodeText, FontSize = 11 });
        }

        return new Border
        {
            BorderBrush = context.Palette.NodeBorder,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(12, 7),
            Child = panel,
        };
    }

    private static string FormatMember(UmlMember member)
    {
        var modifier = member.IsStatic ? " {static}" : member.IsAbstract ? " {abstract}" : "";
        return Visibility(member.Visibility) + " " + member.Name + ": " + member.Type + modifier;
    }

    private static string FormatOperation(UmlOperation operation)
    {
        var parameters = string.Join(", ", operation.Parameters.Select(p =>
            string.IsNullOrWhiteSpace(p.DefaultValue)
                ? p.Name + ": " + p.Type
                : p.Name + ": " + p.Type + " = " + p.DefaultValue));
        var modifier = operation.IsStatic ? " {static}" : operation.IsAbstract ? " {abstract}" : "";
        return Visibility(operation.Visibility) + " " + operation.Name + "(" + parameters + "): " + operation.ReturnType + modifier;
    }

    private static string Visibility(UmlVisibility visibility) => visibility switch
    {
        UmlVisibility.Public => "+",
        UmlVisibility.Protected => "#",
        UmlVisibility.Private => "-",
        _ => "~",
    };

    private static LinkStyle StyleFor(UmlRelationshipLink relationship)
    {
        var stroke = relationship.Kind switch
        {
            UmlRelationshipKind.Dependency => Brushes.DarkGoldenrod,
            UmlRelationshipKind.Realization => Brushes.MediumSeaGreen,
            UmlRelationshipKind.Inheritance => Brushes.SteelBlue,
            UmlRelationshipKind.Aggregation => Brushes.SeaGreen,
            UmlRelationshipKind.Composition => Brushes.SeaGreen,
            _ => Brushes.DimGray,
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
        var pen = new Pen(brush, 1.6);
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
    }

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
}
