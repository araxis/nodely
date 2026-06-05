using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Nodely.Avalonia.Controls;
using Nodely.Geometry;
using AvPoint = Avalonia.Point;
using ShapeEllipse = Avalonia.Controls.Shapes.Ellipse;
using ShapeLine = Avalonia.Controls.Shapes.Line;
using ShapeRectangle = Avalonia.Controls.Shapes.Rectangle;

namespace Nodely.Avalonia.Api;

/// <summary>Canvas registration helpers for the API node pack.</summary>
public static class ApiDiagramCanvasExtensions
{
    private static readonly Color ServiceColor = Color.FromRgb(77, 158, 255);
    private static readonly Color EndpointColor = Color.FromRgb(55, 167, 121);
    private static readonly Color ContractColor = Color.FromRgb(209, 139, 48);
    private static readonly Color OperationColor = Color.FromRgb(139, 104, 184);
    private static readonly Color ClientColor = Color.FromRgb(51, 166, 184);
    private static readonly Color GatewayColor = Color.FromRgb(76, 139, 220);
    private static readonly Color AuthColor = Color.FromRgb(196, 85, 82);
    private static readonly Color GroupColor = Color.FromRgb(120, 144, 156);

    /// <summary>Registers API node, port, and link renderers on the canvas.</summary>
    public static DiagramCanvas UseApiNodes(this DiagramCanvas canvas)
    {
        if (canvas is null)
            throw new ArgumentNullException(nameof(canvas));

        canvas.RegisterNode<ApiServiceNode>(BuildService);
        canvas.RegisterNode<ApiEndpointNode>(BuildEndpoint);
        canvas.RegisterNode<ApiContractNode>(BuildContract);
        canvas.RegisterNode<ApiOperationNode>(BuildOperation);
        canvas.RegisterNode<ApiClientNode>(BuildClient);
        canvas.RegisterNode<ApiGatewayNode>(BuildGateway);
        canvas.RegisterNode<ApiAuthNode>(BuildAuth);
        canvas.RegisterNode<ApiGroupNode>(BuildGroup);
        canvas.RegisterPort<ApiPortModel>(BuildPort);
        canvas.RegisterLinkStyle<ApiLink>(StyleFor);
        canvas.RegisterLink<ApiLink>(DrawApiLink);

        return canvas;
    }

    private static Control BuildService(ApiServiceNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node.AccentColor, ServiceColor);
        var details = new StackPanel { Spacing = 3 };
        if (!string.IsNullOrWhiteSpace(node.BaseUrl))
            details.Children.Add(DetailRow("host", node.BaseUrl!, context));
        if (!string.IsNullOrWhiteSpace(node.Owner))
            details.Children.Add(DetailRow("owner", node.Owner!, context));
        if (!string.IsNullOrWhiteSpace(node.Summary))
            details.Children.Add(WrapText(node.Summary!, context, 186));

        return new Border
        {
            Tag = "api-service-node",
            MinWidth = ApiVisualMetrics.ServiceWidth,
            MinHeight = ApiVisualMetrics.ServiceHeight,
            Background = context.Palette.NodeBackground,
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1.3),
            CornerRadius = new CornerRadius(7),
            ClipToBounds = true,
            Child = new StackPanel
            {
                Children =
                {
                    BuildHeader(node, "SERVICE", accent, context, BuildServiceGlyph),
                    new Border
                    {
                        Padding = new Thickness(13, 9, 13, 11),
                        Child = details.Children.Count == 0 ? DetailText(node.IconKey ?? "SVC", context) : details,
                    },
                },
            },
        };
    }

    private static Control BuildEndpoint(ApiEndpointNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node.AccentColor, MethodColor(node.Method));
        var details = new StackPanel { Spacing = 4 };
        if (!string.IsNullOrWhiteSpace(node.RequestType))
            details.Children.Add(DetailRow("req", node.RequestType!, context));
        if (!string.IsNullOrWhiteSpace(node.ResponseType))
            details.Children.Add(DetailRow("res", node.ResponseType!, context));
        if (!string.IsNullOrWhiteSpace(node.Summary))
            details.Children.Add(WrapText(node.Summary!, context, 210));

        var header = new Grid
        {
            Height = ApiVisualMetrics.HeaderHeight,
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            Background = new SolidColorBrush(Color.FromArgb(36, accent.R, accent.G, accent.B)),
        };
        header.Children.Add(MethodBadge(node.Method, accent));
        var title = new StackPanel { Spacing = 0, VerticalAlignment = VerticalAlignment.Center };
        title.Children.Add(new TextBlock
        {
            Text = node.Route,
            Foreground = context.Palette.NodeText,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 154,
        });
        title.Children.Add(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(node.Version) ? "endpoint" : node.Version,
            Foreground = context.Palette.LinkStroke,
            FontSize = 10,
            FontWeight = FontWeight.SemiBold,
        });
        Grid.SetColumn(title, 1);
        header.Children.Add(title);
        var status = BuildStatusBadge(node.Status, accent, context);
        status.Margin = new Thickness(0, 0, 10, 0);
        Grid.SetColumn(status, 2);
        header.Children.Add(status);

        return new Border
        {
            Tag = "api-endpoint-node",
            MinWidth = ApiVisualMetrics.EndpointWidth,
            MinHeight = ApiVisualMetrics.EndpointHeight,
            Background = context.Palette.NodeBackground,
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1.3),
            CornerRadius = new CornerRadius(7),
            ClipToBounds = true,
            Child = new StackPanel
            {
                Children =
                {
                    header,
                    new Border
                    {
                        Padding = new Thickness(14, 10, 14, 12),
                        Child = details.Children.Count == 0 ? DetailText(node.IconKey ?? "HTTP", context) : details,
                    },
                },
            },
        };
    }

    private static Control BuildContract(ApiContractNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node.AccentColor, ContractColor);
        var fields = new StackPanel { Spacing = 3 };
        foreach (var field in node.Fields)
            fields.Children.Add(FieldRow(field, context));
        if (fields.Children.Count == 0)
            fields.Children.Add(DetailText("schema", context));

        return new Border
        {
            Tag = "api-contract-node",
            MinWidth = ApiVisualMetrics.ContractWidth,
            MinHeight = ApiVisualMetrics.ContractHeight,
            Background = context.Palette.NodeBackground,
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1.3),
            CornerRadius = new CornerRadius(7),
            ClipToBounds = true,
            Child = new StackPanel
            {
                Children =
                {
                    BuildHeader(node, "CONTRACT", accent, context, BuildContractGlyph),
                    new Border
                    {
                        Padding = new Thickness(13, 9, 13, 12),
                        Child = fields,
                    },
                },
            },
        };
    }

    private static Control BuildOperation(ApiOperationNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node.AccentColor, OperationColor);
        var details = new StackPanel { Spacing = 4 };
        if (!string.IsNullOrWhiteSpace(node.Input))
            details.Children.Add(DetailRow("in", node.Input!, context));
        if (!string.IsNullOrWhiteSpace(node.Output))
            details.Children.Add(DetailRow("out", node.Output!, context));
        details.Children.Add(new Border
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(Color.FromArgb(34, accent.R, accent.G, accent.B)),
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(7, 2),
            Child = new TextBlock
            {
                Text = node.SideEffectFree ? "READ" : "WRITE",
                Foreground = context.Palette.NodeText,
                FontSize = 9,
                FontWeight = FontWeight.SemiBold,
            },
        });

        return new Border
        {
            Tag = "api-operation-node",
            MinWidth = ApiVisualMetrics.OperationWidth,
            MinHeight = ApiVisualMetrics.OperationHeight,
            Background = context.Palette.NodeBackground,
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1.3),
            CornerRadius = new CornerRadius(7),
            ClipToBounds = true,
            Child = new StackPanel
            {
                Children =
                {
                    BuildHeader(node, "OPERATION", accent, context, BuildOperationGlyph),
                    new Border { Padding = new Thickness(13, 9, 13, 11), Child = details },
                },
            },
        };
    }

    private static Control BuildClient(ApiClientNode node, DiagramRenderContext context)
        => BuildCompactDevice(node, "api-client-node", "CLIENT", ClientColor, context, BuildClientGlyph,
            ApiVisualMetrics.ClientWidth, ApiVisualMetrics.ClientHeight, node.Platform);

    private static Control BuildGateway(ApiGatewayNode node, DiagramRenderContext context)
        => BuildCompactDevice(node, "api-gateway-node", "GATEWAY", GatewayColor, context, BuildGatewayGlyph,
            ApiVisualMetrics.GatewayWidth, ApiVisualMetrics.GatewayHeight, node.Host);

    private static Control BuildAuth(ApiAuthNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node.AccentColor, AuthColor);
        var details = new StackPanel { Spacing = 4 };
        if (!string.IsNullOrWhiteSpace(node.Scheme))
            details.Children.Add(DetailRow("scheme", node.Scheme!, context));
        if (!string.IsNullOrWhiteSpace(node.Scopes))
            details.Children.Add(DetailRow("scope", node.Scopes!, context));

        return new Border
        {
            Tag = "api-auth-node",
            MinWidth = ApiVisualMetrics.AuthWidth,
            MinHeight = ApiVisualMetrics.AuthHeight,
            Background = context.Palette.NodeBackground,
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1.3),
            CornerRadius = new CornerRadius(7),
            ClipToBounds = true,
            Child = new StackPanel
            {
                Children =
                {
                    BuildHeader(node, "AUTH", accent, context, BuildAuthGlyph),
                    new Border { Padding = new Thickness(13, 9, 13, 11), Child = details.Children.Count == 0 ? DetailText("policy", context) : details },
                },
            },
        };
    }

    private static Control BuildGroup(ApiGroupNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node.AccentColor, GroupColor);
        return new Border
        {
            Tag = "api-group-node",
            MinWidth = ApiVisualMetrics.GroupWidth,
            MinHeight = ApiVisualMetrics.GroupHeight,
            Background = new SolidColorBrush(Color.FromArgb(22, accent.R, accent.G, accent.B)),
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1.4),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 12),
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new TextBlock
                    {
                        Text = node.Name,
                        Foreground = context.Palette.NodeText,
                        FontSize = 16,
                        FontWeight = FontWeight.SemiBold,
                    },
                    new TextBlock
                    {
                        Text = node.Summary ?? "service boundary",
                        Foreground = context.Palette.LinkStroke,
                        FontSize = 12,
                    },
                    BuildStatusBadge(node.Status, accent, context),
                },
            },
        };
    }

    private static Control BuildCompactDevice(
        ApiNodeBase node,
        string tag,
        string label,
        Color fallback,
        DiagramRenderContext context,
        Func<Color, DiagramRenderContext, Control> glyphFactory,
        double width,
        double height,
        string? detail)
    {
        var accent = ResolveAccent(node.AccentColor, fallback);
        return new Border
        {
            Tag = tag,
            MinWidth = width,
            MinHeight = height,
            Background = context.Palette.NodeBackground,
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1.3),
            CornerRadius = new CornerRadius(7),
            ClipToBounds = true,
            Child = new StackPanel
            {
                Children =
                {
                    BuildHeader(node, label, accent, context, glyphFactory),
                    new Border
                    {
                        Padding = new Thickness(13, 9, 13, 11),
                        Child = string.IsNullOrWhiteSpace(detail) ? DetailText(node.Summary ?? node.IconKey ?? label, context) : DetailRow("info", detail!, context),
                    },
                },
            },
        };
    }

    private static Control BuildHeader(
        ApiNodeBase node,
        string label,
        Color accent,
        DiagramRenderContext context,
        Func<Color, DiagramRenderContext, Control> glyphFactory)
    {
        var grid = new Grid
        {
            Height = ApiVisualMetrics.HeaderHeight,
            ColumnDefinitions = new ColumnDefinitions("42,*,Auto"),
            Background = new SolidColorBrush(Color.FromArgb(34, accent.R, accent.G, accent.B)),
        };

        grid.Children.Add(new Border
        {
            Width = 30,
            Height = 30,
            Background = Brush(accent),
            CornerRadius = new CornerRadius(8),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = glyphFactory(accent, context),
        });

        var title = new StackPanel { Spacing = 0, VerticalAlignment = VerticalAlignment.Center };
        title.Children.Add(new TextBlock
        {
            Text = node.Name,
            Foreground = context.Palette.NodeText,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 134,
        });
        title.Children.Add(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(node.Version) ? label : $"{label} {node.Version}",
            Foreground = context.Palette.LinkStroke,
            FontSize = 10,
            FontWeight = FontWeight.SemiBold,
        });
        Grid.SetColumn(title, 1);
        grid.Children.Add(title);

        var status = BuildStatusBadge(node.Status, accent, context);
        status.Margin = new Thickness(0, 0, 10, 0);
        Grid.SetColumn(status, 2);
        grid.Children.Add(status);

        return grid;
    }

    private static Border MethodBadge(ApiEndpointMethod method, Color accent)
        => new()
        {
            MinWidth = 56,
            Height = 26,
            Margin = new Thickness(10, 0, 10, 0),
            Background = Brush(accent),
            CornerRadius = new CornerRadius(6),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = MethodText(method),
                Foreground = Brushes.White,
                FontSize = 11,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };

    private static Control FieldRow(ApiContractField field, DiagramRenderContext context)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        grid.Children.Add(new StackPanel
        {
            Spacing = 0,
            Children =
            {
                new TextBlock
                {
                    Text = field.Name,
                    Foreground = context.Palette.NodeText,
                    FontSize = 12,
                    FontWeight = FontWeight.SemiBold,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = 140,
                },
                new TextBlock
                {
                    Text = field.Type,
                    Foreground = context.Palette.LinkStroke,
                    FontSize = 10,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = 140,
                },
            },
        });

        var badge = new Border
        {
            Background = field.Required ? new SolidColorBrush(Color.FromArgb(38, ContractColor.R, ContractColor.G, ContractColor.B)) : Brushes.Transparent,
            BorderBrush = field.Required ? Brush(ContractColor) : context.Palette.NodeBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(7),
            Padding = new Thickness(6, 1),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = field.Required ? "REQ" : "OPT",
                Foreground = context.Palette.NodeText,
                FontSize = 9,
                FontWeight = FontWeight.SemiBold,
            },
        };
        Grid.SetColumn(badge, 1);
        grid.Children.Add(badge);
        return grid;
    }

    private static Control DetailRow(string label, string value, DiagramRenderContext context)
        => new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("48,*"),
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
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = 154,
                }, 1),
            },
        };

    private static TextBlock DetailText(string text, DiagramRenderContext context)
        => new()
        {
            Text = text,
            Foreground = context.Palette.LinkStroke,
            FontSize = 11,
        };

    private static TextBlock WrapText(string text, DiagramRenderContext context, double maxWidth)
        => new()
        {
            Text = text,
            Foreground = context.Palette.LinkStroke,
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = maxWidth,
        };

    private static Border BuildStatusBadge(ApiEndpointStatus status, Color accent, DiagramRenderContext context)
    {
        var color = StatusColor(status, accent);
        return new Border
        {
            MinWidth = 28,
            Height = 18,
            Padding = new Thickness(6, 0),
            Background = new SolidColorBrush(Color.FromArgb(44, color.R, color.G, color.B)),
            BorderBrush = Brush(color),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(9),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = StatusText(status),
                Foreground = context.Palette.NodeText,
                FontSize = 9,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
    }

    private static Control BuildServiceGlyph(Color accent, DiagramRenderContext context)
        => new Canvas
        {
            Width = 24,
            Height = 24,
            Children =
            {
                GlyphLine(5, 7, 19, 7),
                GlyphLine(5, 12, 19, 12),
                GlyphLine(5, 17, 19, 17),
                GlyphDot(5, 7),
                GlyphDot(5, 12),
                GlyphDot(5, 17),
            },
        };

    private static Control BuildContractGlyph(Color accent, DiagramRenderContext context)
        => new TextBlock
        {
            Text = "{}",
            Foreground = Brushes.White,
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

    private static Control BuildOperationGlyph(Color accent, DiagramRenderContext context)
        => new TextBlock
        {
            Text = "fn",
            Foreground = Brushes.White,
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

    private static Control BuildClientGlyph(Color accent, DiagramRenderContext context)
        => new TextBlock
        {
            Text = "APP",
            Foreground = Brushes.White,
            FontSize = 9,
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

    private static Control BuildGatewayGlyph(Color accent, DiagramRenderContext context)
        => new Canvas
        {
            Width = 24,
            Height = 24,
            Children =
            {
                GlyphLine(4, 12, 20, 12),
                GlyphLine(12, 4, 12, 20),
                GlyphDot(12, 12),
            },
        };

    private static Control BuildAuthGlyph(Color accent, DiagramRenderContext context)
        => new Grid
        {
            Width = 24,
            Height = 24,
            Children =
            {
                new ShapeRectangle
                {
                    Width = 14,
                    Height = 12,
                    Fill = Brushes.White,
                    RadiusX = 2,
                    RadiusY = 2,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 0, 3),
                },
                new ShapeEllipse
                {
                    Width = 14,
                    Height = 14,
                    Stroke = Brushes.White,
                    StrokeThickness = 2,
                    Fill = Brushes.Transparent,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 2, 0, 0),
                },
            },
        };

    private static ShapeLine GlyphLine(double x1, double y1, double x2, double y2)
        => new()
        {
            StartPoint = new AvPoint(x1, y1),
            EndPoint = new AvPoint(x2, y2),
            Stroke = Brushes.White,
            StrokeThickness = 1.8,
        };

    private static ShapeEllipse GlyphDot(double x, double y)
    {
        var dot = new ShapeEllipse
        {
            Width = 5,
            Height = 5,
            Fill = Brushes.White,
        };
        Canvas.SetLeft(dot, x - 2.5);
        Canvas.SetTop(dot, y - 2.5);
        return dot;
    }

    private static Control BuildPort(ApiPortModel port, DiagramRenderContext context)
    {
        var color = port.Role switch
        {
            ApiPortRole.Request => EndpointColor,
            ApiPortRole.Response => ServiceColor,
            ApiPortRole.Event => OperationColor,
            ApiPortRole.Dependency => GroupColor,
            ApiPortRole.Auth => AuthColor,
            _ => EndpointColor,
        };

        var view = new Grid
        {
            Tag = "api-port",
            Width = port.Role == ApiPortRole.Auth ? 20 : 18,
            Height = 18,
            IsVisible = port.Visible && port.Parent.Visible,
            Background = Brushes.Transparent,
        };

        if (port.Role == ApiPortRole.Response)
        {
            view.Children.Add(new Border
            {
                Width = 20,
                Height = 12,
                Background = Brush(color),
                BorderBrush = context.Palette.NodeBackground,
                BorderThickness = new Thickness(1.6),
                CornerRadius = new CornerRadius(6),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }
        else if (port.Role == ApiPortRole.Event)
        {
            view.Children.Add(new Border
            {
                Width = 12,
                Height = 12,
                Background = Brush(color),
                BorderBrush = context.Palette.NodeBackground,
                BorderThickness = new Thickness(2),
                RenderTransformOrigin = RelativePoint.Center,
                RenderTransform = new RotateTransform(45),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }
        else if (port.Role == ApiPortRole.Auth)
        {
            view.Children.Add(new ShapeEllipse
            {
                Width = 14,
                Height = 14,
                Fill = context.Palette.NodeBackground,
                Stroke = Brush(color),
                StrokeThickness = 2.2,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }
        else
        {
            view.Children.Add(new Border
            {
                Width = port.Role == ApiPortRole.Dependency ? 13 : 12,
                Height = port.Role == ApiPortRole.Dependency ? 13 : 12,
                Background = Brush(color),
                BorderBrush = context.Palette.NodeBackground,
                BorderThickness = new Thickness(1.8),
                CornerRadius = new CornerRadius(port.Role == ApiPortRole.Dependency ? 3 : 6),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }

        return view;
    }

    private static LinkStyle StyleFor(ApiLink link)
    {
        var stroke = ResolveAccent(link.AccentColor, link.Kind switch
        {
            ApiLinkKind.Response => ServiceColor,
            ApiLinkKind.Publishes or ApiLinkKind.Consumes => OperationColor,
            ApiLinkKind.DependsOn => GroupColor,
            ApiLinkKind.Secures => AuthColor,
            _ => EndpointColor,
        });

        if (link.Status == ApiEndpointStatus.Deprecated)
            stroke = ContractColor;
        if (link.Status == ApiEndpointStatus.Internal)
            stroke = GroupColor;

        return new LinkStyle
        {
            Stroke = Brush(stroke),
            SelectedStroke = Brushes.White,
            DashStyle = link.Kind is ApiLinkKind.Response or ApiLinkKind.Publishes or ApiLinkKind.Consumes or ApiLinkKind.DependsOn or ApiLinkKind.Secures
                ? DashStyle.Dash
                : null,
            Width = link.Width,
        };
    }

    private static void DrawApiLink(DrawingContext context, LinkRenderContext ctx)
    {
        var link = (ApiLink)ctx.Link;
        if (!link.Visible)
            return;

        ctx.DrawDefault();

        var style = StyleFor(link);
        var stroke = ctx.IsSelected ? ctx.Palette.Selection : style.Stroke ?? ctx.Palette.LinkStroke;
        var pen = new Pen(stroke, Math.Max(1.4, link.Width - 0.7));
        var point = ctx.Path.PointAtDistance(ctx.Path.Length() / 2);
        if (point is not { } middle)
            return;

        var center = new AvPoint(middle.X, middle.Y);
        switch (link.Kind)
        {
            case ApiLinkKind.Request:
                DrawChip(context, "REQ", center, ctx.Palette, stroke);
                break;
            case ApiLinkKind.Response:
                DrawChip(context, "RES", center, ctx.Palette, stroke);
                break;
            case ApiLinkKind.Publishes:
                DrawEventGlyph(context, center, pen);
                break;
            case ApiLinkKind.Consumes:
                DrawChip(context, "SUB", center, ctx.Palette, stroke);
                break;
            case ApiLinkKind.DependsOn:
                context.DrawEllipse(ctx.Palette.NodeBackground, pen, center, 4.5, 4.5);
                break;
            case ApiLinkKind.Secures:
                DrawLockGlyph(context, center, pen, stroke);
                break;
        }

        if (link.Status is ApiEndpointStatus.Preview or ApiEndpointStatus.Deprecated or ApiEndpointStatus.Internal)
            DrawChip(context, StatusText(link.Status), new AvPoint(center.X, center.Y + 21), ctx.Palette, stroke);
    }

    private static void DrawEventGlyph(DrawingContext context, AvPoint center, Pen pen)
    {
        var geometry = new StreamGeometry();
        using (var g = geometry.Open())
        {
            g.BeginFigure(new AvPoint(center.X, center.Y - 11), isFilled: false);
            g.LineTo(new AvPoint(center.X + 10, center.Y));
            g.LineTo(new AvPoint(center.X, center.Y + 11));
            g.LineTo(new AvPoint(center.X - 10, center.Y));
            g.EndFigure(isClosed: true);
        }

        context.DrawGeometry(null, pen, geometry);
    }

    private static void DrawLockGlyph(DrawingContext context, AvPoint center, Pen pen, IBrush stroke)
    {
        var body = new Rect(center.X - 8, center.Y - 2, 16, 12);
        context.DrawRectangle(null, pen, body, 3, 3);
        context.DrawEllipse(null, pen, new AvPoint(center.X, center.Y - 4), 7, 7);
        context.DrawRectangle(Brush(AuthColor), null, new Rect(center.X - 2, center.Y + 2, 4, 5), 2, 2);
    }

    private static void DrawChip(DrawingContext context, string text, AvPoint center, NodelyPalette palette, IBrush stroke)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(Typeface.Default.FontFamily, FontStyle.Normal, FontWeight.SemiBold),
            10,
            palette.LabelForeground);

        const double padX = 7;
        const double padY = 3;
        var rect = new Rect(
            center.X - formatted.Width / 2 - padX,
            center.Y - formatted.Height / 2 - padY,
            formatted.Width + padX * 2,
            formatted.Height + padY * 2);

        context.DrawRectangle(palette.LabelBackground, new Pen(stroke, 1), rect, 5, 5);
        context.DrawText(formatted, new AvPoint(rect.X + padX, rect.Y + padY));
    }

    private static T WithColumn<T>(T control, int column)
        where T : Control
    {
        Grid.SetColumn(control, column);
        return control;
    }

    private static Color MethodColor(ApiEndpointMethod method)
        => method switch
        {
            ApiEndpointMethod.Get => EndpointColor,
            ApiEndpointMethod.Post => OperationColor,
            ApiEndpointMethod.Put or ApiEndpointMethod.Patch => ContractColor,
            ApiEndpointMethod.Delete => AuthColor,
            _ => GatewayColor,
        };

    private static string MethodText(ApiEndpointMethod method)
        => method switch
        {
            ApiEndpointMethod.Get => "GET",
            ApiEndpointMethod.Post => "POST",
            ApiEndpointMethod.Put => "PUT",
            ApiEndpointMethod.Patch => "PATCH",
            ApiEndpointMethod.Delete => "DELETE",
            ApiEndpointMethod.Head => "HEAD",
            ApiEndpointMethod.Options => "OPTIONS",
            _ => "HTTP",
        };

    private static Color StatusColor(ApiEndpointStatus status, Color fallback)
        => status switch
        {
            ApiEndpointStatus.Stable => EndpointColor,
            ApiEndpointStatus.Preview => OperationColor,
            ApiEndpointStatus.Deprecated => AuthColor,
            ApiEndpointStatus.Internal => GroupColor,
            _ => fallback,
        };

    private static string StatusText(ApiEndpointStatus status)
        => status switch
        {
            ApiEndpointStatus.Stable => "OK",
            ApiEndpointStatus.Preview => "PRE",
            ApiEndpointStatus.Deprecated => "DEP",
            ApiEndpointStatus.Internal => "INT",
            _ => "UNK",
        };

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
