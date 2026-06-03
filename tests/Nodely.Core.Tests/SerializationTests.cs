using System.Collections.Generic;
using System.Linq;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;
using Shouldly;
using Xunit;

namespace Nodely.Core.Tests;

// A custom node kind (with an id-preserving ctor) used to exercise the deserialize factory path.
// Not 'file'-local: a file-local type's metadata name is mangled, so GetType().Name wouldn't match nameof
// (the Kind seam keys off the simple type name — which is fine for normal public custom nodes like the demo's).
internal sealed class CustomTestNode : NodeModel
{
    public CustomTestNode(string id, Point position) : base(id, position) { }

    public string Note { get; set; } = "";

    public override IReadOnlyDictionary<string, object?> GetExtraData() => new Dictionary<string, object?> { ["Note"] = Note };

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Note", out var value) && value is string note)
            Note = note;
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
        DiagramSerializer.Deserialize(loaded, json, ns => ns.Kind == nameof(CustomTestNode)
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
}
