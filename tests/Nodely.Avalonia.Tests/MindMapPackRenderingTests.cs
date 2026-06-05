using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nodely;
using Nodely.Avalonia;
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Database;
using Nodely.Avalonia.MindMap;
using Nodely.Avalonia.Uml;
using Nodely.Avalonia.Workflow;
using Nodely.Models;
using Nodely.Serialization;
using Shouldly;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Tests;

public class MindMapPackRenderingTests
{
    [AvaloniaFact]
    public void UseMindMapNodes_registers_topic_port_and_link_renderers()
    {
        var diagram = new NodelyDiagram();
        var root = diagram.Nodes.Add(new MindMapRootNode(new NodelyPoint(0, 0), "Plan") { IconKey = "P" });
        var branch = diagram.Nodes.Add(new MindMapBranchNode(new NodelyPoint(260, 0), "Build") { Notes = "Implementation" });
        var leaf = diagram.Nodes.Add(new MindMapLeafNode(new NodelyPoint(520, 0), "Tests"));
        var rootPort = root.AddPort(new MindMapPortModel(root, PortAlignment.Right, MindMapPortRole.Branch, "out"));
        var branchIn = branch.AddPort(new MindMapPortModel(branch, PortAlignment.Left, MindMapPortRole.Branch, "in"));
        var branchOut = branch.AddPort(new MindMapPortModel(branch, PortAlignment.Right, MindMapPortRole.Association, "related"));
        var leafIn = leaf.AddPort(new MindMapPortModel(leaf, PortAlignment.Left, MindMapPortRole.Association, "related"));
        var branchLink = diagram.Links.Add(new MindMapLink(rootPort, branchIn, MindMapLinkKind.Branch) { Label = "scope" });
        var association = diagram.Links.Add(new MindMapLink(branchOut, leafIn, MindMapLinkKind.Association) { Label = "relates" });

        var canvas = new DiagramCanvas { Diagram = diagram, Palette = NodelyPalettes.Light }.UseMindMapNodes();

        canvas.BuildNodeContent(root).ShouldBeOfType<Border>().Tag.ShouldBe("mindmap-root-node");
        canvas.BuildNodeContent(branch).ShouldBeOfType<Border>().Tag.ShouldBe("mindmap-branch-node");
        canvas.BuildNodeContent(leaf).ShouldBeOfType<Border>().Tag.ShouldBe("mindmap-leaf-node");
        canvas.BuildPortContent((MindMapPortModel)rootPort).ShouldBeOfType<Grid>().Tag.ShouldBe("mindmap-port");
        canvas.ResolveLinkDrawer(branchLink).ShouldNotBeNull();
        canvas.ResolveLinkStyle(branchLink).DashStyle.ShouldBeNull();
        canvas.ResolveLinkDrawer(association).ShouldNotBeNull();
        canvas.ResolveLinkStyle(association).DashStyle.ShouldNotBeNull();

        canvas.Palette = NodelyPalettes.Dark;
        canvas.BuildNodeContent(branch).ShouldBeOfType<Border>().Tag.ShouldBe("mindmap-branch-node");
    }

    [AvaloniaFact]
    public void Side_pack_registrations_compose_on_canvas_and_serializer_registry()
    {
        var diagram = new NodelyDiagram();
        var table = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(0, 0), "Customers"));
        var entity = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(260, 0), "Customer"));
        var task = diagram.Nodes.Add(new WorkflowTaskNode(new NodelyPoint(520, 0), "Sync"));
        var root = diagram.Nodes.Add(new MindMapRootNode(new NodelyPoint(780, 0), "Plan"));
        var branch = diagram.Nodes.Add(new MindMapBranchNode(new NodelyPoint(1040, 0), "Ship"));
        var relationship = diagram.Links.Add(new MindMapLink(root, branch, MindMapLinkKind.Branch)
        {
            Label = "next",
        });

        var canvas = new DiagramCanvas { Diagram = diagram }
            .UseDatabaseNodes()
            .UseMindMapNodes()
            .UseUmlNodes()
            .UseWorkflowNodes();
        canvas.BuildNodeContent(table).ShouldBeOfType<Border>().Tag.ShouldBe("database-table-node");
        canvas.BuildNodeContent(entity).ShouldBeOfType<Border>().Tag.ShouldBe("uml-class-node");
        canvas.BuildNodeContent(task).ShouldBeOfType<Border>().Tag.ShouldBe("workflow-task-node");
        canvas.BuildNodeContent(root).ShouldBeOfType<Border>().Tag.ShouldBe("mindmap-root-node");
        canvas.ResolveLinkDrawer(relationship).ShouldNotBeNull();

        var registry = DatabaseNodeFactory.CreateRegistry()
            .UseMindMapNodes()
            .UseUmlNodes()
            .UseWorkflowNodes();
        var json = DiagramSerializer.Serialize(diagram);
        var loaded = new NodelyDiagram();
        DiagramSerializer.Deserialize(loaded, json, registry);

        loaded.Nodes.ShouldContain(n => n is DatabaseTableNode);
        loaded.Nodes.ShouldContain(n => n is UmlClassNode);
        loaded.Nodes.ShouldContain(n => n is WorkflowTaskNode);
        loaded.Nodes.ShouldContain(n => n is MindMapRootNode);
        loaded.Links.Single().ShouldBeOfType<MindMapLink>();
    }
}
