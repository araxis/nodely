using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Nodely.Avalonia.Designer;
using Nodely.Models;
using Nodely.Models.Base;
using Shouldly;
using Xunit;

namespace Nodely.Demo.Tests;

public class DemoGalleryStartupTests
{
    public static IEnumerable<object[]> GalleryScenes => MainWindow.GallerySceneNames.Select(name => new object[] { name });

    [AvaloniaTheory]
    [MemberData(nameof(GalleryScenes))]
    public void Gallery_scene_renders_visible_links_on_first_layout(string sceneName)
    {
        var window = new MainWindow();
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            window.ShowScene(sceneName);
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var diagram = window.CurrentDiagram.ShouldNotBeNull();
            var canvas = window.CurrentCanvas.ShouldNotBeNull();
            canvas.IsVisible.ShouldBeTrue();
            canvas.Bounds.Width.ShouldBeGreaterThan(0);
            canvas.Bounds.Height.ShouldBeGreaterThan(0);

            var visibleNodes = diagram.Nodes.Where(node => node.Visible).ToArray();
            visibleNodes.Length.ShouldBeGreaterThan(0);
            foreach (var node in visibleNodes)
                AssertMeasured(sceneName, node);

            var visibleLinks = diagram.Links.Where(link => link.Visible).ToArray();
            visibleLinks.Length.ShouldBeGreaterThan(0);
            foreach (var link in visibleLinks)
                AssertGenerated(sceneName, link);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Gallery_uses_one_vertical_scene_rail_with_top_stencils()
    {
        var window = new MainWindow();
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var scene = window.ShowScene("API");
            Dispatcher.UIThread.RunJobs();

            var controls = Descendants(scene).ToArray();
            controls.OfType<DiagramToolbox>().Any().ShouldBeFalse();
            controls.ShouldContain(control => Equals(control.Tag, "scene-stencil-bar"));

            var diagram = window.CurrentDiagram.ShouldNotBeNull();
            var canvas = window.CurrentCanvas.ShouldNotBeNull();
            var before = diagram.Nodes.Count;
            var stencil = controls.OfType<Button>().Single(button => Equals(button.Tag, "scene-stencil-Endpoint"));

            stencil.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            diagram.Nodes.Count.ShouldBe(before + 1);
            canvas.CanUndo.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertMeasured(string sceneName, NodeModel node)
    {
        var size = node.Size.ShouldNotBeNull(sceneName + " node " + node.Id + " was not measured.");
        size.Width.ShouldBeGreaterThan(0, sceneName + " node " + node.Id + " measured zero width.");
        size.Height.ShouldBeGreaterThan(0, sceneName + " node " + node.Id + " measured zero height.");
    }

    private static void AssertGenerated(string sceneName, BaseLinkModel link)
    {
        var result = link.PathGeneratorResult.ShouldNotBeNull(sceneName + " link " + link.Id + " has no initial path.");
        result.FullPath.Operations.Count.ShouldBeGreaterThan(1, sceneName + " link " + link.Id + " has an empty initial path.");
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
}
