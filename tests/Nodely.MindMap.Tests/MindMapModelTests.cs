using System.Linq;
using Nodely;
using Nodely.Avalonia.MindMap;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;
using Shouldly;
using Xunit;

namespace Nodely.MindMap.Tests;

public class MindMapModelTests
{
    [Fact]
    public void Topic_defaults_to_title_and_mutable_metadata()
    {
        var root = new MindMapRootNode(new Point(10, 20), "Planning")
        {
            Notes = "Release map",
            AccentColor = "#37A779",
            IconKey = "P",
            Collapsed = true,
            Side = MindMapTopicSide.Right,
        };

        root.Topic.ShouldBe("Planning");
        root.Title.ShouldBe("Planning");
        root.Notes.ShouldBe("Release map");
        root.AccentColor.ShouldBe("#37A779");
        root.IconKey.ShouldBe("P");
        root.Collapsed.ShouldBeTrue();
        root.Side.ShouldBe(MindMapTopicSide.Right);
    }

    [Fact]
    public void Clone_copies_mind_map_topic_data()
    {
        var node = new MindMapBranchNode(new Point(30, 40), "Adoption")
        {
            Notes = "Docs and samples",
            AccentColor = "#D89C35",
            Collapsed = true,
            IconKey = "A",
            Side = MindMapTopicSide.Left,
            Size = new Size(220, 82),
        };

        var clone = node.Clone().ShouldBeOfType<MindMapBranchNode>();

        clone.ShouldNotBeSameAs(node);
        clone.Topic.ShouldBe("Adoption");
        clone.Notes.ShouldBe("Docs and samples");
        clone.AccentColor.ShouldBe("#D89C35");
        clone.Collapsed.ShouldBeTrue();
        clone.IconKey.ShouldBe("A");
        clone.Side.ShouldBe(MindMapTopicSide.Left);
        clone.Size.ShouldBe(new Size(220, 82));
    }

    [Fact]
    public void Extra_data_round_trips_mind_map_fields_through_serializer()
    {
        var diagram = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var root = diagram.Nodes.Add(new MindMapRootNode("root", new Point(0, 0), "Plan")
        {
            Notes = "Main topic",
            AccentColor = "#4D9EFF",
            IconKey = "P",
        });
        var branch = diagram.Nodes.Add(new MindMapBranchNode("branch", new Point(240, 0), "Build")
        {
            Notes = "Implementation",
            AccentColor = "#37A779",
            Collapsed = true,
            Side = MindMapTopicSide.Right,
        });
        var leaf = diagram.Nodes.Add(new MindMapLeafNode("leaf", new Point(480, 0), "Tests")
        {
            AccentColor = "#D89C35",
        });

        var rootPort = root.AddPort(new MindMapPortModel(root, PortAlignment.Right, MindMapPortRole.Branch, "out"));
        var branchIn = branch.AddPort(new MindMapPortModel(branch, PortAlignment.Left, MindMapPortRole.Branch, "in"));
        var branchOut = branch.AddPort(new MindMapPortModel(branch, PortAlignment.Right, MindMapPortRole.Branch, "out"));
        var leafIn = leaf.AddPort(new MindMapPortModel(leaf, PortAlignment.Left, MindMapPortRole.Branch, "in"));

        diagram.Links.Add(new MindMapLink(rootPort, branchIn, MindMapLinkKind.Branch)
        {
            Label = "scope",
            AccentColor = "#37A779",
        });
        diagram.Links.Add(new MindMapLink(branchOut, leafIn, MindMapLinkKind.Branch)
        {
            Label = "verify",
            AccentColor = "#D89C35",
        });

        var json = DiagramSerializer.Serialize(diagram);

        var loaded = new NodelyDiagram(null, registerDefaultBehaviors: false);
        DiagramSerializer.Deserialize(loaded, json, MindMapNodeFactory.CreateRegistry());

        var restoredRoot = loaded.Nodes.Single(n => n.Id == "root").ShouldBeOfType<MindMapRootNode>();
        restoredRoot.Topic.ShouldBe("Plan");
        restoredRoot.Notes.ShouldBe("Main topic");
        restoredRoot.IconKey.ShouldBe("P");

        var restoredBranch = loaded.Nodes.Single(n => n.Id == "branch").ShouldBeOfType<MindMapBranchNode>();
        restoredBranch.Collapsed.ShouldBeTrue();
        restoredBranch.Side.ShouldBe(MindMapTopicSide.Right);
        restoredBranch.Ports.OfType<MindMapPortModel>().ShouldAllBe(port => port.Role == MindMapPortRole.Branch);

        var restoredLinks = loaded.Links.OfType<MindMapLink>().ToList();
        restoredLinks.Count.ShouldBe(2);
        restoredLinks.ShouldContain(link => link.Label == "scope" && link.AccentColor == "#37A779");
        restoredLinks.ShouldContain(link => link.Label == "verify" && link.AccentColor == "#D89C35");
    }

    [Fact]
    public void Factory_restores_mind_map_nodes()
    {
        MindMapNodeFactory.Create(new NodeSnapshot { Kind = MindMapRootNode.ModelKindKey, Title = "Root", X = 1, Y = 2 })
            .ShouldBeOfType<MindMapRootNode>();
        MindMapNodeFactory.Create(new NodeSnapshot { Kind = MindMapBranchNode.ModelKindKey, Title = "Branch", X = 1, Y = 2 })
            .ShouldBeOfType<MindMapBranchNode>();
        MindMapNodeFactory.Create(new NodeSnapshot { Kind = MindMapLeafNode.ModelKindKey, Title = "Leaf", X = 1, Y = 2 })
            .ShouldBeOfType<MindMapLeafNode>();
    }

    [Fact]
    public void Mind_map_link_sets_metadata_defaults_and_labels()
    {
        var source = new MindMapBranchNode(new Point(0, 0), "Source");
        var target = new MindMapLeafNode(new Point(200, 0), "Target");
        var link = new MindMapLink(source, target, MindMapLinkKind.Association)
        {
            Label = "related",
            AccentColor = "#9779CD",
        };

        link.Kind.ShouldBe(MindMapLinkKind.Association);
        link.Segmentable.ShouldBeTrue();
        link.SourceMarker.ShouldNotBeNull();
        link.TargetMarker.ShouldNotBeNull();
        link.Width.ShouldBe(2.1);
        link.Label.ShouldBe("related");
        link.AccentColor.ShouldBe("#9779CD");
        link.Labels.Single().Content.ShouldBe("related");
    }

    [Fact]
    public void Arrange_honors_root_sides_and_inherits_descendant_side()
    {
        var diagram = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var root = diagram.Nodes.Add(new MindMapRootNode(new Point(0, 0), "Root") { Size = new Size(200, 80) });
        var right = diagram.Nodes.Add(new MindMapBranchNode(new Point(0, 0), "Right") { Side = MindMapTopicSide.Right, Size = new Size(160, 60) });
        var left = diagram.Nodes.Add(new MindMapBranchNode(new Point(0, 0), "Left") { Side = MindMapTopicSide.Left, Size = new Size(160, 60) });
        var inherited = diagram.Nodes.Add(new MindMapLeafNode(new Point(0, 0), "Inherited") { Side = MindMapTopicSide.Right, Size = new Size(120, 40) });

        diagram.Links.Add(new MindMapLink(root, right));
        diagram.Links.Add(new MindMapLink(root, left));
        diagram.Links.Add(new MindMapLink(left, inherited));

        MindMapLayout.Arrange(diagram, new MindMapLayoutOptions { OriginX = 0, OriginY = 0, LevelSpacing = 220 });

        right.Position.X.ShouldBeGreaterThan(root.Position.X);
        left.Position.X.ShouldBeLessThan(root.Position.X);
        inherited.Position.X.ShouldBeLessThan(left.Position.X);
    }

    [Fact]
    public void Collapse_state_hides_descendant_topics_and_links()
    {
        var diagram = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var root = diagram.Nodes.Add(new MindMapRootNode(new Point(0, 0), "Root"));
        var branch = diagram.Nodes.Add(new MindMapBranchNode(new Point(200, 0), "Branch") { Collapsed = true });
        var child = diagram.Nodes.Add(new MindMapLeafNode(new Point(400, 0), "Hidden child"));
        var related = diagram.Nodes.Add(new MindMapLeafNode(new Point(400, 120), "Related"));
        var rootLink = diagram.Links.Add(new MindMapLink(root, branch));
        var childLink = diagram.Links.Add(new MindMapLink(branch, child));
        var association = diagram.Links.Add(new MindMapLink(child, related, MindMapLinkKind.Association));

        MindMapLayout.ApplyCollapseState(diagram);

        root.Visible.ShouldBeTrue();
        branch.Visible.ShouldBeTrue();
        child.Visible.ShouldBeFalse();
        rootLink.Visible.ShouldBeTrue();
        childLink.Visible.ShouldBeFalse();
        association.Visible.ShouldBeFalse();
    }
}
