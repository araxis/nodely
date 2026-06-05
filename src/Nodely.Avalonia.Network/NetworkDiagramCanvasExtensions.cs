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

namespace Nodely.Avalonia.Network;

/// <summary>Canvas registration helpers for the network node pack.</summary>
public static class NetworkDiagramCanvasExtensions
{
    private static readonly Color RouterColor = Color.FromRgb(77, 158, 255);
    private static readonly Color SwitchColor = Color.FromRgb(55, 167, 121);
    private static readonly Color FirewallColor = Color.FromRgb(196, 85, 82);
    private static readonly Color BalancerColor = Color.FromRgb(139, 104, 184);
    private static readonly Color ServerColor = Color.FromRgb(209, 139, 48);
    private static readonly Color ClientColor = Color.FromRgb(51, 166, 184);
    private static readonly Color CloudColor = Color.FromRgb(76, 139, 220);
    private static readonly Color ServiceColor = Color.FromRgb(150, 112, 199);
    private static readonly Color ZoneColor = Color.FromRgb(120, 144, 156);

    /// <summary>Registers network node, port, and link renderers on the canvas.</summary>
    public static DiagramCanvas UseNetworkNodes(this DiagramCanvas canvas)
    {
        if (canvas is null)
            throw new ArgumentNullException(nameof(canvas));

        canvas.RegisterNode<NetworkRouterNode>((node, context) => BuildDevice(node, RouterColor, "network-router-node", context, BuildRouterGlyph));
        canvas.RegisterNode<NetworkSwitchNode>(BuildSwitch);
        canvas.RegisterNode<NetworkFirewallNode>(BuildFirewall);
        canvas.RegisterNode<NetworkLoadBalancerNode>((node, context) => BuildDevice(node, BalancerColor, "network-loadbalancer-node", context, BuildBalancerGlyph));
        canvas.RegisterNode<NetworkServerNode>((node, context) => BuildDevice(node, ServerColor, "network-server-node", context, BuildServerGlyph, NetworkVisualMetrics.DeviceWidth, 116));
        canvas.RegisterNode<NetworkClientNode>((node, context) => BuildDevice(node, ClientColor, "network-client-node", context, BuildClientGlyph, NetworkVisualMetrics.ClientWidth, NetworkVisualMetrics.ClientHeight));
        canvas.RegisterNode<NetworkCloudNode>(BuildCloud);
        canvas.RegisterNode<NetworkServiceNode>((node, context) => BuildDevice(node, ServiceColor, "network-service-node", context, BuildServiceGlyph));
        canvas.RegisterNode<NetworkZoneNode>(BuildZone);
        canvas.RegisterPort<NetworkPortModel>(BuildPort);
        canvas.RegisterLinkStyle<NetworkLink>(StyleFor);
        canvas.RegisterLink<NetworkLink>(DrawNetworkLink);

        return canvas;
    }

    private static Control BuildDevice(
        NetworkNodeBase node,
        Color fallback,
        string tag,
        DiagramRenderContext context,
        Func<Color, DiagramRenderContext, Control> glyphFactory,
        double minWidth = NetworkVisualMetrics.DeviceWidth,
        double minHeight = NetworkVisualMetrics.DeviceHeight)
    {
        var accent = ResolveAccent(node.AccentColor, fallback);
        var header = BuildHeader(node, accent, context, glyphFactory);
        var details = BuildDetails(node, context);

        return new Border
        {
            Tag = tag,
            MinWidth = minWidth,
            MinHeight = minHeight,
            Background = context.Palette.NodeBackground,
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1.2),
            CornerRadius = new CornerRadius(7),
            ClipToBounds = true,
            Child = new StackPanel
            {
                Children =
                {
                    header,
                    new Border
                    {
                        Padding = new Thickness(13, 9, 13, 11),
                        Child = details,
                    },
                },
            },
        };
    }

    private static Control BuildSwitch(NetworkSwitchNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node.AccentColor, SwitchColor);
        var ports = new WrapPanel
        {
            Margin = new Thickness(13, 9, 13, 11),
            ItemWidth = 13,
            ItemHeight = 12,
            MaxWidth = 184,
        };

        for (var i = 0; i < node.PortCount; i++)
        {
            ports.Children.Add(new Border
            {
                Width = 10,
                Height = 8,
                Margin = new Thickness(1.5, 2),
                Background = i < node.ActivePorts ? Brush(SwitchColor) : context.Palette.NodeBorder,
                BorderBrush = i < node.ActivePorts ? context.Palette.Selection : context.Palette.LinkStroke,
                BorderThickness = new Thickness(0.6),
                CornerRadius = new CornerRadius(2),
            });
        }

        return new Border
        {
            Tag = "network-switch-node",
            MinWidth = NetworkVisualMetrics.SwitchWidth,
            MinHeight = NetworkVisualMetrics.SwitchHeight,
            Background = context.Palette.NodeBackground,
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1.2),
            CornerRadius = new CornerRadius(7),
            ClipToBounds = true,
            Child = new StackPanel
            {
                Children =
                {
                    BuildHeader(node, accent, context, BuildSwitchGlyph),
                    ports,
                },
            },
        };
    }

    private static Control BuildFirewall(NetworkFirewallNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node.AccentColor, FirewallColor);
        var bricks = new Grid
        {
            RowDefinitions = new RowDefinitions("18,18,18"),
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            Margin = new Thickness(13, 9, 13, 12),
        };

        for (var row = 0; row < 3; row++)
            for (var column = 0; column < 4; column++)
            {
                var brick = new Border
                {
                    Margin = new Thickness(2),
                    Background = new SolidColorBrush(Color.FromArgb((byte)(row == 1 ? 82 : 52), accent.R, accent.G, accent.B)),
                    BorderBrush = Brush(accent),
                    BorderThickness = new Thickness(0.6),
                    CornerRadius = new CornerRadius(3),
                };
                Grid.SetRow(brick, row);
                Grid.SetColumn(brick, column);
                bricks.Children.Add(brick);
            }

        return new Border
        {
            Tag = "network-firewall-node",
            MinWidth = NetworkVisualMetrics.FirewallWidth,
            MinHeight = NetworkVisualMetrics.FirewallHeight,
            Background = context.Palette.NodeBackground,
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1.2),
            CornerRadius = new CornerRadius(7),
            ClipToBounds = true,
            Child = new StackPanel
            {
                Children =
                {
                    BuildHeader(node, accent, context, BuildFirewallGlyph),
                    bricks,
                },
            },
        };
    }

    private static Control BuildCloud(NetworkCloudNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node.AccentColor, CloudColor);
        var grid = new Grid();
        grid.Children.Add(new ShapeEllipse
        {
            Width = 88,
            Height = 58,
            Fill = new SolidColorBrush(Color.FromArgb(74, accent.R, accent.G, accent.B)),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(16, 18, 0, 0),
        });
        grid.Children.Add(new ShapeEllipse
        {
            Width = 92,
            Height = 74,
            Fill = new SolidColorBrush(Color.FromArgb(82, accent.R, accent.G, accent.B)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 8, 0, 0),
        });
        grid.Children.Add(new ShapeEllipse
        {
            Width = 92,
            Height = 60,
            Fill = new SolidColorBrush(Color.FromArgb(72, accent.R, accent.G, accent.B)),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 20, 16, 0),
        });
        grid.Children.Add(new Border
        {
            MinWidth = NetworkVisualMetrics.CloudWidth,
            MinHeight = NetworkVisualMetrics.CloudHeight,
            Background = Brushes.Transparent,
            BorderBrush = Brush(accent),
            BorderThickness = new Thickness(1.2),
            CornerRadius = new CornerRadius(34),
            Padding = new Thickness(18, 15),
            Child = BuildCenteredContent(node, context, accent),
        });

        return new Border
        {
            Tag = "network-cloud-node",
            Background = Brushes.Transparent,
            Child = grid,
        };
    }

    private static Control BuildZone(NetworkZoneNode node, DiagramRenderContext context)
    {
        var accent = ResolveAccent(node.AccentColor, ZoneColor);
        return new Border
        {
            Tag = "network-zone-node",
            MinWidth = NetworkVisualMetrics.ZoneWidth,
            MinHeight = NetworkVisualMetrics.ZoneHeight,
            Background = new SolidColorBrush(Color.FromArgb(24, accent.R, accent.G, accent.B)),
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
                        Text = string.IsNullOrWhiteSpace(node.Address) ? node.Role : node.Address,
                        Foreground = context.Palette.LinkStroke,
                        FontSize = 12,
                    },
                    BuildStatusBadge(node.Status, accent, context),
                },
            },
        };
    }

    private static Control BuildHeader(
        NetworkNodeBase node,
        Color accent,
        DiagramRenderContext context,
        Func<Color, DiagramRenderContext, Control> glyphFactory)
    {
        var grid = new Grid
        {
            Height = NetworkVisualMetrics.HeaderHeight,
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
            MaxWidth = 130,
        });
        title.Children.Add(new TextBlock
        {
            Text = node.Role,
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

    private static StackPanel BuildDetails(NetworkNodeBase node, DiagramRenderContext context)
    {
        var details = new StackPanel { Spacing = 3 };
        if (!string.IsNullOrWhiteSpace(node.Address))
            details.Children.Add(DetailRow("addr", node.Address!, context));
        if (!string.IsNullOrWhiteSpace(node.Zone))
            details.Children.Add(DetailRow("zone", node.Zone!, context));
        if (!string.IsNullOrWhiteSpace(node.Notes))
            details.Children.Add(new TextBlock
            {
                Text = node.Notes,
                Foreground = context.Palette.LinkStroke,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 168,
            });
        if (details.Children.Count == 0)
            details.Children.Add(new TextBlock
            {
                Text = node.IconKey ?? node.Role,
                Foreground = context.Palette.LinkStroke,
                FontSize = 11,
            });
        return details;
    }

    private static Control BuildCenteredContent(NetworkNodeBase node, DiagramRenderContext context, Color accent)
    {
        var stack = new StackPanel
        {
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        stack.Children.Add(new TextBlock
        {
            Text = node.IconKey ?? node.Role,
            Foreground = Brush(accent),
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
        });
        stack.Children.Add(new TextBlock
        {
            Text = node.Name,
            Foreground = context.Palette.NodeText,
            FontSize = 16,
            FontWeight = FontWeight.SemiBold,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 150,
        });
        if (!string.IsNullOrWhiteSpace(node.Address))
            stack.Children.Add(new TextBlock
            {
                Text = node.Address,
                Foreground = context.Palette.LinkStroke,
                FontSize = 11,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
        return stack;
    }

    private static Control DetailRow(string label, string value, DiagramRenderContext context)
        => new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("40,*"),
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
                    MaxWidth = 122,
                }, 1),
            },
        };

    private static Border BuildStatusBadge(NetworkStatus status, Color accent, DiagramRenderContext context)
    {
        var color = StatusColor(status, accent);
        return new Border
        {
            MinWidth = 24,
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

    private static Control BuildRouterGlyph(Color accent, DiagramRenderContext context)
        => new Canvas
        {
            Width = 24,
            Height = 24,
            Children =
            {
                GlyphLine(4, 12, 20, 12, context),
                GlyphLine(12, 4, 12, 20, context),
                GlyphDot(12, 12, context),
            },
        };

    private static Control BuildSwitchGlyph(Color accent, DiagramRenderContext context)
        => new Grid
        {
            Width = 22,
            Height = 22,
            Children =
            {
                new Border
                {
                    Width = 18,
                    Height = 13,
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1.5),
                    CornerRadius = new CornerRadius(3),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                new TextBlock
                {
                    Text = "SW",
                    Foreground = Brushes.White,
                    FontSize = 8,
                    FontWeight = FontWeight.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        };

    private static Control BuildFirewallGlyph(Color accent, DiagramRenderContext context)
        => new TextBlock
        {
            Text = "FW",
            Foreground = Brushes.White,
            FontWeight = FontWeight.SemiBold,
            FontSize = 10,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

    private static Control BuildBalancerGlyph(Color accent, DiagramRenderContext context)
        => new Canvas
        {
            Width = 24,
            Height = 24,
            Children =
            {
                GlyphLine(5, 12, 19, 12, context),
                GlyphLine(12, 12, 12, 5, context),
                GlyphLine(12, 12, 12, 19, context),
                GlyphDot(12, 5, context),
                GlyphDot(12, 19, context),
            },
        };

    private static Control BuildServerGlyph(Color accent, DiagramRenderContext context)
        => new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                RackRow(context),
                RackRow(context),
                RackRow(context),
            },
        };

    private static Control BuildClientGlyph(Color accent, DiagramRenderContext context)
        => new TextBlock
        {
            Text = "CLI",
            Foreground = Brushes.White,
            FontWeight = FontWeight.SemiBold,
            FontSize = 9,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

    private static Control BuildServiceGlyph(Color accent, DiagramRenderContext context)
        => new TextBlock
        {
            Text = "SVC",
            Foreground = Brushes.White,
            FontWeight = FontWeight.SemiBold,
            FontSize = 8,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

    private static ShapeRectangle RackRow(DiagramRenderContext context)
        => new()
        {
            Width = 18,
            Height = 4,
            Fill = Brushes.White,
            RadiusX = 1.5,
            RadiusY = 1.5,
        };

    private static ShapeLine GlyphLine(double x1, double y1, double x2, double y2, DiagramRenderContext context)
    {
        var line = new ShapeLine
        {
            StartPoint = new AvPoint(x1, y1),
            EndPoint = new AvPoint(x2, y2),
            Stroke = Brushes.White,
            StrokeThickness = 1.8,
        };
        return line;
    }

    private static ShapeEllipse GlyphDot(double x, double y, DiagramRenderContext context)
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

    private static Control BuildPort(NetworkPortModel port, DiagramRenderContext context)
    {
        var color = port.Role switch
        {
            NetworkPortRole.Wan => RouterColor,
            NetworkPortRole.Uplink => SwitchColor,
            NetworkPortRole.Downlink => ClientColor,
            NetworkPortRole.Management => ZoneColor,
            NetworkPortRole.Service => ServiceColor,
            NetworkPortRole.Client => ServerColor,
            _ => SwitchColor,
        };

        var view = new Grid
        {
            Tag = "network-port",
            Width = port.Role == NetworkPortRole.Service ? 22 : 18,
            Height = 18,
            IsVisible = port.Visible && port.Parent.Visible,
            Background = Brushes.Transparent,
        };

        if (port.Role == NetworkPortRole.Uplink)
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
        else if (port.Role == NetworkPortRole.Service)
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
        else if (port.Role == NetworkPortRole.Management)
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
                Width = port.Role == NetworkPortRole.Lan ? 13 : 12,
                Height = port.Role == NetworkPortRole.Lan ? 13 : 12,
                Background = Brush(color),
                BorderBrush = context.Palette.NodeBackground,
                BorderThickness = new Thickness(1.8),
                CornerRadius = new CornerRadius(port.Role == NetworkPortRole.Lan ? 3 : 6),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }

        return view;
    }

    private static LinkStyle StyleFor(NetworkLink link)
    {
        var stroke = ResolveAccent(link.AccentColor, link.Status == NetworkStatus.Blocked || link.Kind == NetworkLinkKind.Blocked
            ? FirewallColor
            : link.Kind switch
            {
                NetworkLinkKind.Fiber => CloudColor,
                NetworkLinkKind.Wireless => ClientColor,
                NetworkLinkKind.VpnTunnel => BalancerColor,
                NetworkLinkKind.Dependency => ZoneColor,
                _ => SwitchColor,
            });

        if (link.Status == NetworkStatus.Offline)
            stroke = ZoneColor;
        if (link.Status == NetworkStatus.Warning)
            stroke = ServerColor;

        return new LinkStyle
        {
            Stroke = Brush(stroke),
            SelectedStroke = Brushes.White,
            DashStyle = link.Kind is NetworkLinkKind.Wireless or NetworkLinkKind.VpnTunnel or NetworkLinkKind.Dependency or NetworkLinkKind.Blocked
                ? DashStyle.Dash
                : null,
            Width = link.Width,
        };
    }

    private static void DrawNetworkLink(DrawingContext context, LinkRenderContext ctx)
    {
        var link = (NetworkLink)ctx.Link;
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
            case NetworkLinkKind.Wireless:
                DrawWirelessGlyph(context, center, pen);
                break;
            case NetworkLinkKind.VpnTunnel:
                DrawChip(context, "VPN", center, ctx.Palette, stroke);
                break;
            case NetworkLinkKind.Blocked:
                DrawBlockedGlyph(context, center, pen);
                break;
            case NetworkLinkKind.Fiber:
                context.DrawEllipse(ctx.Palette.NodeBackground, pen, center, 6, 6);
                context.DrawEllipse(null, pen, center, 11, 11);
                break;
            case NetworkLinkKind.Dependency:
                context.DrawEllipse(ctx.Palette.NodeBackground, pen, center, 4.5, 4.5);
                break;
        }

        if (link.Status is NetworkStatus.Warning or NetworkStatus.Offline or NetworkStatus.Maintenance)
            DrawChip(context, StatusText(link.Status), new AvPoint(center.X, center.Y + 21), ctx.Palette, stroke);
    }

    private static void DrawWirelessGlyph(DrawingContext context, AvPoint center, Pen pen)
    {
        for (var i = 0; i < 3; i++)
        {
            var radius = 6 + i * 5;
            var geometry = new StreamGeometry();
            using (var g = geometry.Open())
            {
                g.BeginFigure(new AvPoint(center.X - radius, center.Y + radius / 2), isFilled: false);
                g.CubicBezierTo(
                    new AvPoint(center.X - radius / 2, center.Y - radius),
                    new AvPoint(center.X + radius / 2, center.Y - radius),
                    new AvPoint(center.X + radius, center.Y + radius / 2));
                g.EndFigure(isClosed: false);
            }

            context.DrawGeometry(null, pen, geometry);
        }
    }

    private static void DrawBlockedGlyph(DrawingContext context, AvPoint center, Pen pen)
    {
        context.DrawEllipse(Brush(FirewallColor), null, center, 10, 10);
        var crossPen = new Pen(Brushes.White, 2);
        context.DrawLine(crossPen, new AvPoint(center.X - 5, center.Y - 5), new AvPoint(center.X + 5, center.Y + 5));
        context.DrawLine(crossPen, new AvPoint(center.X + 5, center.Y - 5), new AvPoint(center.X - 5, center.Y + 5));
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

    private static Color StatusColor(NetworkStatus status, Color fallback)
        => status switch
        {
            NetworkStatus.Online => SwitchColor,
            NetworkStatus.Warning => ServerColor,
            NetworkStatus.Offline => ZoneColor,
            NetworkStatus.Maintenance => BalancerColor,
            NetworkStatus.Blocked => FirewallColor,
            _ => fallback,
        };

    private static string StatusText(NetworkStatus status)
        => status switch
        {
            NetworkStatus.Online => "ON",
            NetworkStatus.Warning => "WARN",
            NetworkStatus.Offline => "OFF",
            NetworkStatus.Maintenance => "MAINT",
            NetworkStatus.Blocked => "BLOCK",
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
