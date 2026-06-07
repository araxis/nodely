using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nodely.Avalonia;
using Nodely.Avalonia.Controls;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Avalonia.Designer;

/// <summary>Runtime property inspector backed by explicit descriptors.</summary>
public sealed class DiagramPropertyInspector : UserControl, IDisposable
{
    private readonly Border _panel = new() { BorderThickness = new Thickness(1, 0, 0, 0) };
    private Diagram? _subscribed;
    private bool _disposed;

    /// <summary>Creates an inspector.</summary>
    public DiagramPropertyInspector()
    {
        Registry = DiagramPropertyRegistry.CreateDefault();
        Content = _panel;
    }

    /// <summary>The canvas whose history and palette drive edits.</summary>
    public DiagramCanvas? Canvas { get; set; }

    /// <summary>The inspected diagram.</summary>
    public Diagram? Diagram
    {
        get => _subscribed;
        set => Subscribe(value);
    }

    /// <summary>Property registry used for selected models.</summary>
    public DiagramPropertyRegistry Registry { get; set; }

    /// <summary>Disables property mutations when true.</summary>
    public bool IsReadOnly { get; set; }

    private bool CanEdit => !IsReadOnly && Canvas?.IsReadOnly != true;

    /// <summary>Rebuilds the inspector content.</summary>
    public void Refresh()
    {
        if (_disposed)
            return;

        var canvas = Canvas;
        var palette = canvas?.Palette ?? NodelyPalettes.Dark;
        _panel.Background = palette.NodeBackground;
        _panel.BorderBrush = palette.NodeBorder;

        var content = new StackPanel
        {
            Margin = new Thickness(14),
            Spacing = 12,
        };

        content.Children.Add(Title("Properties"));
        if (!CanEdit)
            content.Children.Add(Note("Read-only"));

        var selected = Diagram?.GetSelectedModels().ToList() ?? new List<SelectableModel>();
        if (selected.Count == 0)
        {
            content.Children.Add(Note("Select one item."));
        }
        else if (selected.Count > 1)
        {
            content.Children.Add(Note(selected.Count.ToString(CultureInfo.InvariantCulture) + " items selected"));
        }
        else
        {
            BuildModel(content, selected[0]);
        }

        _panel.Child = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = content,
        };
    }

    private void BuildModel(StackPanel content, Model model)
    {
        content.Children.Add(Note(model.GetType().Name));
        var descriptors = Registry.GetDescriptors(model);
        if (descriptors.Count == 0)
        {
            content.Children.Add(Note("No editable fields registered."));
            return;
        }

        foreach (var section in descriptors.GroupBy(descriptor => descriptor.Section))
        {
            content.Children.Add(Section(section.Key));
            foreach (var descriptor in section)
                content.Children.Add(BuildField(model, descriptor));
        }
    }

    private Control BuildField(Model model, DiagramPropertyDescriptor descriptor) => descriptor.Kind switch
    {
        DesignerPropertyKind.Text => TextField(model, descriptor, multiline: false, color: false),
        DesignerPropertyKind.MultilineText => TextField(model, descriptor, multiline: true, color: false),
        DesignerPropertyKind.Color => TextField(model, descriptor, multiline: false, color: true),
        DesignerPropertyKind.Number => NumberField(model, descriptor),
        DesignerPropertyKind.Boolean => BooleanField(model, descriptor),
        DesignerPropertyKind.Enum => EnumField(model, descriptor),
        DesignerPropertyKind.Collection => CollectionField(model, descriptor),
        _ => Note("Unsupported field"),
    };

    private Control TextField(Model model, DiagramPropertyDescriptor descriptor, bool multiline, bool color)
    {
        var original = descriptor.GetValue(model)?.ToString() ?? string.Empty;
        var box = new TextBox
        {
            Text = original,
            IsEnabled = CanEdit,
            AcceptsReturn = multiline,
            TextWrapping = multiline ? TextWrapping.Wrap : TextWrapping.NoWrap,
            MinHeight = multiline ? 78 : 0,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Tag = "designer-property-" + descriptor.Label,
        };
        var committed = false;

        void Commit()
        {
            if (committed)
                return;

            var next = box.Text ?? string.Empty;
            if (string.Equals(next, original, StringComparison.Ordinal))
                return;

            committed = true;
            Apply(model, () => descriptor.SetValue(model, next), () => descriptor.SetValue(model, original));
        }

        box.LostFocus += (_, _) => Commit();
        if (!multiline)
        {
            box.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Enter)
                    Commit();
            };
        }

        if (!color)
            return Field(descriptor.Label, box);

        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("18,*"),
            ColumnSpacing = 8,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children =
            {
                new Border
                {
                    Width = 18,
                    Height = 18,
                    CornerRadius = new CornerRadius(4),
                    Background = TryBrush(original),
                    BorderBrush = Palette.NodeBorder,
                    BorderThickness = new Thickness(1),
                    VerticalAlignment = VerticalAlignment.Center,
                },
                box,
            },
        };
        Grid.SetColumn(box, 1);
        return Field(descriptor.Label, row);
    }

    private Control NumberField(Model model, DiagramPropertyDescriptor descriptor)
    {
        var original = Convert.ToDouble(descriptor.GetValue(model) ?? 0d, CultureInfo.InvariantCulture);
        var box = new TextBox
        {
            Text = original.ToString("0.###", CultureInfo.InvariantCulture),
            IsEnabled = CanEdit,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Tag = "designer-property-" + descriptor.Label,
        };
        var committed = false;

        void Commit()
        {
            if (committed)
                return;
            if (!double.TryParse(box.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var next))
                return;
            if (descriptor.Minimum is { } min)
                next = Math.Max(min, next);
            if (Math.Abs(next - original) < 0.001)
                return;

            committed = true;
            Apply(model, () => descriptor.SetValue(model, next), () => descriptor.SetValue(model, original));
        }

        box.LostFocus += (_, _) => Commit();
        box.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
                Commit();
        };

        return Field(descriptor.Label, box);
    }

    private Control BooleanField(Model model, DiagramPropertyDescriptor descriptor)
    {
        var original = descriptor.GetValue(model) is bool value && value;
        var box = new CheckBox
        {
            Content = descriptor.Label,
            IsChecked = original,
            IsEnabled = CanEdit,
            Foreground = Palette.NodeText,
            Tag = "designer-property-" + descriptor.Label,
        };
        box.Click += (_, _) =>
        {
            var next = box.IsChecked == true;
            if (next != original)
                Apply(model, () => descriptor.SetValue(model, next), () => descriptor.SetValue(model, original));
        };
        return box;
    }

    private Control EnumField(Model model, DiagramPropertyDescriptor descriptor)
    {
        var original = descriptor.GetValue(model);
        var combo = new ComboBox
        {
            ItemsSource = descriptor.Options,
            SelectedItem = original,
            IsEnabled = CanEdit,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Tag = "designer-property-" + descriptor.Label,
        };
        combo.SelectionChanged += (_, _) =>
        {
            var next = combo.SelectedItem;
            if (!Equals(next, original))
                Apply(model, () => descriptor.SetValue(model, next), () => descriptor.SetValue(model, original));
        };
        return Field(descriptor.Label, combo);
    }

    private Control CollectionField(Model model, DiagramPropertyDescriptor descriptor)
    {
        var panel = new StackPanel
        {
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        var rows = descriptor.GetCollectionItems?.Invoke(model) ?? Array.Empty<DiagramCollectionItem>();
        foreach (var row in rows)
        {
            var item = row.Item;
            var line = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                ColumnSpacing = 8,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Children =
                {
                    new TextBlock
                    {
                        Text = row.Text,
                        Foreground = Palette.NodeText,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                    },
                    SmallButton("Remove", () =>
                    {
                        if (descriptor.RemoveCollectionItem == null || descriptor.InsertCollectionItem == null)
                            return;

                        var index = rows.ToList().FindIndex(candidate => ReferenceEquals(candidate.Item, item) || Equals(candidate.Item, item));
                        Apply(
                            model,
                            () => descriptor.RemoveCollectionItem(model, item),
                            () => descriptor.InsertCollectionItem(model, Math.Max(0, index), item));
                    }),
                },
            };
            Grid.SetColumn(line.Children[1], 1);
            panel.Children.Add(line);
        }

        if (descriptor.CreateCollectionItem != null && descriptor.AddCollectionItem != null && descriptor.RemoveCollectionItem != null)
        {
            panel.Children.Add(SmallButton(descriptor.AddLabel, () =>
            {
                var item = descriptor.CreateCollectionItem(model);
                Apply(
                    model,
                    () => descriptor.AddCollectionItem(model, item),
                    () => descriptor.RemoveCollectionItem(model, item));
            }));
        }

        return Field(descriptor.Label, panel);
    }

    private Button SmallButton(string text, Action onClick)
    {
        var button = new Button
        {
            Content = text,
            IsEnabled = CanEdit,
            HorizontalAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(10, 4),
        };
        button.Click += (_, _) => onClick();
        return button;
    }

    private void Apply(Model model, Action apply, Action undo)
    {
        if (!CanEdit)
            return;

        if (Canvas == null)
        {
            apply();
            RefreshModel(model);
            Refresh();
            return;
        }

        Canvas.RunAsUndoableEdit(
            () =>
            {
                apply();
                RefreshModel(model);
            },
            () =>
            {
                undo();
                RefreshModel(model);
            });
        Refresh();
    }

    private static void RefreshModel(Model model)
    {
        switch (model)
        {
            case NodeModel node:
                node.ReinitializePorts();
                node.RefreshAll();
                node.RefreshLinks();
                foreach (var link in node.PortLinks.Distinct())
                    link.Refresh();
                break;
            case BaseLinkModel link:
                link.Refresh();
                break;
            default:
                model.Refresh();
                break;
        }
    }

    private Control Field(string label, Control control)
    {
        control.HorizontalAlignment = HorizontalAlignment.Stretch;
        return new StackPanel
        {
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children =
            {
                new TextBlock
                {
                    Text = label,
                    Foreground = Palette.NodeText,
                    Opacity = 0.72,
                    FontSize = 12,
                },
                control,
            },
        };
    }

    private TextBlock Title(string text) => new()
    {
        Text = text,
        Foreground = Palette.NodeText,
        FontSize = 17,
        FontWeight = FontWeight.SemiBold,
    };

    private TextBlock Section(string text) => new()
    {
        Text = text,
        Foreground = Palette.NodeText,
        FontSize = 13,
        FontWeight = FontWeight.SemiBold,
        Margin = new Thickness(0, 4, 0, 0),
    };

    private TextBlock Note(string text) => new()
    {
        Text = text,
        Foreground = Palette.NodeText,
        Opacity = 0.72,
        TextWrapping = TextWrapping.Wrap,
    };

    private NodelyPalette Palette => Canvas?.Palette ?? NodelyPalettes.Dark;

    private static IBrush? TryBrush(string text)
    {
        if (Color.TryParse(text, out var color))
            return new SolidColorBrush(color);

        return null;
    }

    private void Subscribe(Diagram? diagram)
    {
        if (ReferenceEquals(_subscribed, diagram))
            return;

        if (_subscribed != null)
            _subscribed.SelectionChanged -= OnSelectionChanged;

        _subscribed = diagram;

        if (_subscribed != null)
            _subscribed.SelectionChanged += OnSelectionChanged;

        Refresh();
    }

    private void OnSelectionChanged(SelectableModel model) => Refresh();

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        if (_subscribed != null)
            _subscribed.SelectionChanged -= OnSelectionChanged;
    }
}
