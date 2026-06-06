using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Nodely;
using Nodely.Avalonia;
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Designer;
using Nodely.Models;
using Nodely.Models.Base;
using Shouldly;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Tests;

public class DesignerControlsTests
{
    [AvaloniaFact]
    public void Property_registry_returns_base_and_custom_descriptors()
    {
        var registry = DiagramPropertyRegistry.CreateDefault()
            .Register<DesignerTestNode>(
                DiagramProperty.Text<DesignerTestNode>("Status", node => node.Status, (node, value) => node.Status = value ?? "", "Test"));
        var node = new DesignerTestNode(new NodelyPoint(12, 24));

        var labels = registry.GetDescriptors(node).Select(descriptor => descriptor.Label).ToArray();

        labels.ShouldContain("Locked");
        labels.ShouldContain("Title");
        labels.ShouldContain("X");
        labels.ShouldContain("Y");
        labels.ShouldContain("Status");
    }

    [AvaloniaFact]
    public void Inspector_builds_registered_fields_for_selection()
    {
        var diagram = new NodelyDiagram();
        var node = diagram.Nodes.Add(new DesignerTestNode(new NodelyPoint(0, 0)) { Title = "Work", Status = "Ready" });
        var canvas = new DiagramCanvas { Diagram = diagram, Palette = NodelyPalettes.Light };
        var registry = DiagramPropertyRegistry.CreateDefault()
            .Register<DesignerTestNode>(
                DiagramProperty.Text<DesignerTestNode>("Status", item => item.Status, (item, value) => item.Status = value ?? "", "Test"));
        var inspector = new DiagramPropertyInspector
        {
            Canvas = canvas,
            Diagram = diagram,
            Registry = registry,
        };

        diagram.SelectModel(node, true);
        inspector.Refresh();

        Descendants(inspector).OfType<TextBox>().ShouldContain(box => Equals(box.Tag, "designer-property-Title"));
        Descendants(inspector).OfType<TextBox>().ShouldContain(box => Equals(box.Tag, "designer-property-Status"));
    }

    [AvaloniaFact]
    public void Toolbox_adds_nodes_through_canvas_history()
    {
        var diagram = new NodelyDiagram();
        var canvas = new DiagramCanvas { Diagram = diagram };
        var toolbox = new DiagramToolbox { Canvas = canvas };
        toolbox.AddSections(new[]
        {
            new DesignerToolboxSection("Nodes", new[]
            {
                new DesignerToolboxItem("Task", point => new DesignerTestNode(point) { Title = "Task" }),
            }),
        });

        var button = Descendants(toolbox).OfType<Button>().Single(button => Equals(button.Tag, "designer-toolbox-Task"));
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        diagram.Nodes.Count.ShouldBe(1);
        diagram.Nodes.Single().ShouldBeOfType<DesignerTestNode>();
        canvas.CanUndo.ShouldBeTrue();

        canvas.Undo();
        diagram.Nodes.Count.ShouldBe(0);
    }

    [AvaloniaFact]
    public void Command_bar_tracks_selection_state()
    {
        var diagram = new NodelyDiagram();
        var node = diagram.Nodes.Add(new NodeModel(new NodelyPoint(0, 0)));
        var canvas = new DiagramCanvas { Diagram = diagram };
        var bar = new DiagramCommandBar { Canvas = canvas };
        var copy = Descendants(bar).OfType<Button>().Single(button => Equals(button.Tag, "copy"));

        copy.IsEnabled.ShouldBeFalse();

        diagram.SelectModel(node, true);
        bar.Refresh();

        copy.IsEnabled.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Designer_shell_composes_canvas_chrome_and_palette_refresh()
    {
        var diagram = new NodelyDiagram();
        var shell = new DiagramDesignerShell(diagram, new DiagramDesignerOptions
        {
            ToolboxSections = new[]
            {
                new DesignerToolboxSection("Nodes", new[]
                {
                    new DesignerToolboxItem("Task", point => new DesignerTestNode(point)),
                }),
            },
        });

        shell.Canvas.Diagram.ShouldBe(diagram);
        Descendants(shell).OfType<DiagramToolbox>().Any().ShouldBeTrue();
        Descendants(shell).OfType<DiagramPropertyInspector>().Any().ShouldBeTrue();
        Descendants(shell).OfType<DiagramCommandBar>().Any().ShouldBeTrue();
        Descendants(shell).OfType<DiagramStatusBar>().Any().ShouldBeTrue();

        shell.Palette = NodelyPalettes.Light;
        shell.Canvas.Palette.ShouldBe(NodelyPalettes.Light);
    }

    private static IEnumerable<Control> Descendants(Control control)
    {
        yield return control;

        switch (control)
        {
            case ContentControl { Content: Control content }:
                foreach (var child in Descendants(content))
                    yield return child;
                break;
            case Decorator { Child: Control child }:
                foreach (var descendant in Descendants(child))
                    yield return descendant;
                break;
            case Panel panel:
                foreach (var child in panel.Children.OfType<Control>())
                    foreach (var descendant in Descendants(child))
                        yield return descendant;
                break;
        }
    }

    private sealed class DesignerTestNode : NodeModel
    {
        public DesignerTestNode(NodelyPoint position) : base(position)
        {
        }

        public string Status { get; set; } = "Pending";
    }
}
