using System.Collections.Generic;
using System.Linq;
using Nodely.Anchors;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;
using Shouldly;
using Xunit;

namespace Nodely.Core.Tests;

// A custom node kind (with an id-preserving ctor) used to exercise the deserialize factory path.
internal sealed class CustomTestNode : NodeModel
{
    public new const string ModelKindKey = "test.node";

    public CustomTestNode(string id, Point position) : base(id, position) { }

    public string Note { get; set; } = "";

    public override string ModelKind => ModelKindKey;

    public override IReadOnlyDictionary<string, object?> GetExtraData() => new Dictionary<string, object?> { ["Note"] = Note };

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Note", out var value) && value is string note)
            Note = note;
    }
}

internal sealed class CustomTestPort : PortModel
{
    public new const string ModelKindKey = "test.port";

    public CustomTestPort(string id, NodeModel parent, PortAlignment alignment) : base(id, parent, alignment) { }

    public string Role { get; set; } = "";

    public override string ModelKind => ModelKindKey;

    public override IReadOnlyDictionary<string, object?> GetExtraData() => new Dictionary<string, object?> { ["Role"] = Role };

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Role", out var value) && value is string role)
            Role = role;
    }
}

internal sealed class CustomTestLink : LinkModel
{
    public new const string ModelKindKey = "test.link";

    public CustomTestLink(string id, PortModel sourcePort, PortModel targetPort) : base(id, sourcePort, targetPort) { }

    public CustomTestLink(string id, Anchor source, Anchor target) : base(id, source, target) { }

    public bool Critical { get; set; }

    public override string ModelKind => ModelKindKey;

    public override IReadOnlyDictionary<string, object?> GetExtraData() =>
        new Dictionary<string, object?> { ["Critical"] = Critical };

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Critical", out var value) && value is bool critical)
            Critical = critical;
    }
}

internal sealed class CustomTestGroup : GroupModel
{
    public new const string ModelKindKey = "test.group";

    public CustomTestGroup(string id, IEnumerable<NodeModel> children, byte padding) : base(id, children, padding) { }

    public string Label { get; set; } = "";

    public override string ModelKind => ModelKindKey;

    public override IReadOnlyDictionary<string, object?> GetExtraData() => new Dictionary<string, object?> { ["Label"] = Label };

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Label", out var value) && value is string label)
            Label = label;
    }
}

public class SerializationTests
{
    private static NodelyDiagram BuildSample()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var n1 = d.Nodes.Add(new NodeModel("n1", new Point(50, 60)) { Title = "A" });
        var n2 = d.Nodes.Add(new NodeModel("n2", new Point(300, 60)) { Title = "B" });
        n1.Size = new Size(80, 40);
        n2.Size = new Size(80, 40);
        var p1 = n1.AddPort(new PortModel("p1", n1, PortAlignment.Right));
        var p2 = n2.AddPort(new PortModel("p2", n2, PortAlignment.Left));
        d.Links.Add(new LinkModel("l1", p1, p2));
        d.Groups.Group(n1, n2);
        d.SetContainer(new Rectangle(0, 0, 400, 300));
        d.SetPan(10, 20);
        d.SetZoom(1.5);
        return d;
    }

    [Fact]
    public void Round_trips_through_json()
    {
        var original = BuildSample();
        var json = DiagramSerializer.Serialize(original);

        var loaded = new NodelyDiagram(null, registerDefaultBehaviors: false);
        DiagramSerializer.Deserialize(loaded, json);

        DiagramSerializer.Serialize(loaded).ShouldBe(json);
    }

    [Fact]
    public void Loaded_diagram_has_the_same_structure()
    {
        var json = DiagramSerializer.Serialize(BuildSample());

        var loaded = new NodelyDiagram(null, registerDefaultBehaviors: false);
        DiagramSerializer.Deserialize(loaded, json);

        loaded.Nodes.Count.ShouldBe(2);
        loaded.Links.Count.ShouldBe(1);
        loaded.Groups.Count.ShouldBe(1);
        loaded.Zoom.ShouldBe(1.5);
    }

    [Fact]
    public void Custom_node_kind_round_trips_via_the_factory_preserving_id_and_links()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var a = d.Nodes.Add(new CustomTestNode("a", new Point(0, 0)) { Title = "A", Size = new Size(20, 20) });
        var b = d.Nodes.Add(new CustomTestNode("b", new Point(100, 0)) { Title = "B", Size = new Size(20, 20) });
        d.Links.Add(new LinkModel("l", a, b)); // node-to-node link references node ids
        var json = DiagramSerializer.Serialize(d);

        var loaded = new NodelyDiagram(null, registerDefaultBehaviors: false);
        DiagramSerializer.Deserialize(loaded, json, ns => ns.Kind == CustomTestNode.ModelKindKey
            ? new CustomTestNode(ns.Id, new Point(ns.X, ns.Y))
            : new NodeModel(ns.Id, new Point(ns.X, ns.Y)));

        loaded.Nodes.Count.ShouldBe(2);
        loaded.Nodes.First().ShouldBeOfType<CustomTestNode>(); // factory produced the right type...
        loaded.Links.Count.ShouldBe(1);                    // ...and ids were preserved so the link resolved
    }

    [Fact]
    public void Custom_node_extra_data_round_trips_via_GetExtraData_SetExtraData()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        d.Nodes.Add(new CustomTestNode("a", new Point(0, 0)) { Note = "hello", Size = new Size(20, 20) });
        var json = DiagramSerializer.Serialize(d);

        var loaded = new NodelyDiagram(null, registerDefaultBehaviors: false);
        DiagramSerializer.Deserialize(loaded, json, ns => new CustomTestNode(ns.Id, new Point(ns.X, ns.Y)));

        var node = loaded.Nodes.First().ShouldBeOfType<CustomTestNode>();
        node.Note.ShouldBe("hello"); // custom field survived the round-trip
    }

    [Fact]
    public void Registry_restores_custom_nodes_ports_links_and_groups_with_extra_data()
    {
        var d = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var a = d.Nodes.Add(new CustomTestNode("a", new Point(0, 0)) { Note = "first", Size = new Size(20, 20) });
        var b = d.Nodes.Add(new CustomTestNode("b", new Point(100, 0)) { Note = "second", Size = new Size(20, 20) });
        var outPort = a.AddPort(new CustomTestPort("out", a, PortAlignment.Right) { Role = "out" });
        var inPort = b.AddPort(new CustomTestPort("in", b, PortAlignment.Left) { Role = "in" });
        d.Links.Add(new CustomTestLink("flow", outPort, inPort) { Critical = true });
        d.Groups.Add(new CustomTestGroup("box", new[] { a, b }, 24) { Label = "pair" });

        var json = DiagramSerializer.Serialize(d);
        var registry = new DiagramSerializationRegistry()
            .RegisterNode(CustomTestNode.ModelKindKey, ns => new CustomTestNode(ns.Id, new Point(ns.X, ns.Y)))
            .RegisterPort(CustomTestPort.ModelKindKey, (ps, parent) =>
                new CustomTestPort(ps.Id, parent, System.Enum.Parse<PortAlignment>(ps.Alignment)))
            .RegisterLink(CustomTestLink.ModelKindKey, (ls, source, target) => new CustomTestLink(ls.Id, source, target))
            .RegisterGroup(CustomTestGroup.ModelKindKey, (gs, children) => new CustomTestGroup(gs.Id, children, gs.Padding));

        var loaded = new NodelyDiagram(null, registerDefaultBehaviors: false);
        DiagramSerializer.Deserialize(loaded, json, registry);

        loaded.Nodes.Count.ShouldBe(2);
        loaded.Nodes[0].ShouldBeOfType<CustomTestNode>().Note.ShouldBe("first");
        loaded.Nodes[0].Ports[0].ShouldBeOfType<CustomTestPort>().Role.ShouldBe("out");
        loaded.Links.Single().ShouldBeOfType<CustomTestLink>().Critical.ShouldBeTrue();
        loaded.Groups.Single().ShouldBeOfType<CustomTestGroup>().Label.ShouldBe("pair");
    }
}
