using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Nodely.Avalonia;
using Nodely.Avalonia.Controls;
using Nodely.Models;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Designer;

/// <summary>Toolbox that adds registered node stencils to the canvas.</summary>
public sealed class DiagramToolbox : UserControl
{
    private readonly Border _panel = new()
    {
        BorderThickness = new Thickness(0, 0, 1, 0),
    };

    /// <summary>Creates a toolbox.</summary>
    public DiagramToolbox()
    {
        Sections = new ObservableCollection<DesignerToolboxSection>();
        Content = _panel;
        Refresh();
    }

    /// <summary>The target canvas.</summary>
    public DiagramCanvas? Canvas { get; set; }

    /// <summary>Toolbox sections.</summary>
    public ObservableCollection<DesignerToolboxSection> Sections { get; }

    /// <summary>Rebuilds toolbox controls.</summary>
    public void Refresh()
    {
        var palette = Canvas?.Palette ?? NodelyPalettes.Dark;
        _panel.Background = palette.NodeBackground;
        _panel.BorderBrush = palette.NodeBorder;

        var content = new StackPanel
        {
            Margin = new Thickness(12, 14),
            Spacing = 11,
        };
        content.Children.Add(new TextBlock
        {
            Text = "Toolbox",
            Foreground = palette.NodeText,
            FontSize = 17,
            FontWeight = FontWeight.SemiBold,
        });

        foreach (var section in Sections)
        {
            content.Children.Add(new TextBlock
            {
                Text = section.Title,
                Foreground = palette.NodeText,
                FontSize = 13,
                FontWeight = FontWeight.SemiBold,
                Margin = new Thickness(0, 4, 0, 0),
            });

            foreach (var item in section.Items)
                content.Children.Add(ItemButton(item, palette));
        }

        _panel.Child = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = content,
        };
    }

    /// <summary>Adds sections and refreshes the view.</summary>
    public void AddSections(IEnumerable<DesignerToolboxSection> sections)
    {
        foreach (var section in sections)
            Sections.Add(section);
        Refresh();
    }

    private Control ItemButton(DesignerToolboxItem item, NodelyPalette palette)
    {
        var preview = item.PreviewFactory?.Invoke() ?? DefaultPreview(item, palette);
        var previewHost = new Border
        {
            Height = 58,
            CornerRadius = new CornerRadius(7),
            Background = palette.CanvasBackground,
            BorderBrush = palette.NodeBorder,
            BorderThickness = new Thickness(1),
            ClipToBounds = true,
            Child = preview,
        };

        var text = new StackPanel
        {
            Spacing = 1,
            Children =
            {
                new TextBlock { Text = item.Label, Foreground = palette.NodeText, FontWeight = FontWeight.SemiBold },
            },
        };
        if (!string.IsNullOrWhiteSpace(item.Detail))
        {
            text.Children.Add(new TextBlock
            {
                Text = item.Detail,
                Foreground = palette.NodeText,
                Opacity = 0.65,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
            });
        }

        var content = new StackPanel
        {
            Spacing = 7,
            Children = { previewHost, text },
        };

        var button = new Button
        {
            Content = content,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(8),
            Background = palette.GroupBackground,
            BorderBrush = palette.NodeBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Tag = "designer-toolbox-" + item.Label,
        };
        button.Click += (_, _) => AddNode(item);
        return button;
    }

    private static Control DefaultPreview(DesignerToolboxItem item, NodelyPalette palette)
    {
        var accent = item.Accent ?? palette.Selection;
        var rows = new StackPanel
        {
            Spacing = 4,
            Margin = new Thickness(10),
            Children =
            {
                new Border
                {
                    Height = 12,
                    Width = 92,
                    CornerRadius = new CornerRadius(4),
                    Background = accent,
                    HorizontalAlignment = HorizontalAlignment.Left,
                },
                new Border
                {
                    Height = 6,
                    Width = 62,
                    CornerRadius = new CornerRadius(3),
                    Background = palette.NodeBorder,
                    HorizontalAlignment = HorizontalAlignment.Left,
                },
                new Border
                {
                    Height = 6,
                    Width = 44,
                    CornerRadius = new CornerRadius(3),
                    Background = palette.NodeBorder,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Opacity = 0.72,
                },
            },
        };

        return new Border
        {
            Background = palette.NodeBackground,
            BorderBrush = accent,
            BorderThickness = new Thickness(0, 0, 0, 3),
            Child = rows,
        };
    }

    private void AddNode(DesignerToolboxItem item)
    {
        var canvas = Canvas;
        var diagram = canvas?.Diagram;
        if (canvas == null || diagram == null || canvas.IsReadOnly)
            return;

        var position = NextPosition(diagram);
        var node = item.CreateNode(position);
        var afterAddApplied = false;

        canvas.RunAsUndoableEdit(
            () =>
            {
                diagram.Nodes.Add(node);
                if (!afterAddApplied)
                {
                    item.AfterAdd?.Invoke(diagram, node);
                    afterAddApplied = true;
                }

                diagram.UnselectAll();
                diagram.SelectModel(node, true);
            },
            () =>
            {
                diagram.Nodes.Remove(node);
                diagram.UnselectAll();
            });
        canvas.RefreshVisuals();
    }

    private static NodelyPoint NextPosition(Diagram diagram)
    {
        if (diagram.Container != null)
            return diagram.GetRelativeMousePoint(diagram.Container.Width / 2, diagram.Container.Height / 2);

        var offset = diagram.Nodes.Count * 28;
        return new NodelyPoint(100 + offset, 100 + offset);
    }
}
