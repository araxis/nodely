using System.Linq;
using Nodely;
using Nodely.Avalonia.Uml;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;
using Shouldly;
using Xunit;

namespace Nodely.Uml.Tests;

public class UmlModelTests
{
    [Fact]
    public void Class_node_defaults_to_name_and_mutable_members()
    {
        var node = new UmlClassNode(new Point(10, 20), "Customer");

        node.Name.ShouldBe("Customer");
        node.Title.ShouldBe("Customer");

        node.Stereotypes.Add("entity");
        node.Members.Add(new UmlMember("Id", "int", UmlVisibility.Private));
        node.Operations.Add(new UmlOperation("Load", "Customer"));

        node.Stereotypes.Single().ShouldBe("entity");
        node.Members.Single().Visibility.ShouldBe(UmlVisibility.Private);
        node.Operations.Single().ReturnType.ShouldBe("Customer");
    }

    [Fact]
    public void Clone_copies_uml_node_data()
    {
        var node = new UmlClassNode(new Point(30, 40), "Order")
        {
            IsAbstract = true,
            Size = new Size(220, 160),
        };
        node.Stereotypes.Add("aggregate");
        node.Members.Add(new UmlMember("Total", "decimal") { IsStatic = true });
        var operation = new UmlOperation("Calculate", "decimal") { IsAbstract = true };
        operation.Parameters.Add(new UmlParameter("currency", "string", "USD"));
        node.Operations.Add(operation);

        var clone = node.Clone().ShouldBeOfType<UmlClassNode>();

        clone.ShouldNotBeSameAs(node);
        clone.Name.ShouldBe("Order");
        clone.IsAbstract.ShouldBeTrue();
        clone.Stereotypes.Single().ShouldBe("aggregate");
        clone.Members.Single().IsStatic.ShouldBeTrue();
        clone.Operations.Single().Parameters.Single().DefaultValue.ShouldBe("USD");
        clone.Operations.Single().ShouldNotBeSameAs(node.Operations.Single());
    }

    [Fact]
    public void Extra_data_round_trips_uml_fields_through_serializer()
    {
        var diagram = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var customer = diagram.Nodes.Add(new UmlClassNode("customer", new Point(10, 20), "Customer")
        {
            IsAbstract = true,
        });
        customer.Stereotypes.Add("entity");
        customer.Members.Add(new UmlMember("Id", "int", UmlVisibility.Private));
        var operation = new UmlOperation("Rename", "void");
        operation.Parameters.Add(new UmlParameter("name", "string"));
        customer.Operations.Add(operation);

        var repository = diagram.Nodes.Add(new UmlInterfaceNode("repo", new Point(300, 20), "ICustomerRepository"));
        repository.Operations.Add(new UmlOperation("Get", "Customer"));
        var customerPort = customer.AddPort(new UmlPortModel("customer-realization", customer, PortAlignment.Right, UmlPortKind.Realization, "Rename"));
        var repositoryPort = repository.AddPort(new UmlPortModel("repo-realization", repository, PortAlignment.Left, UmlPortKind.Realization, "Get"));
        var link = diagram.Links.Add(new UmlRelationshipLink(customerPort, repositoryPort, UmlRelationshipKind.Realization)
        {
            Label = "implements",
            SourceMultiplicity = "1",
            TargetMultiplicity = "1",
        });

        var json = DiagramSerializer.Serialize(diagram);

        var loaded = new NodelyDiagram(null, registerDefaultBehaviors: false);
        DiagramSerializer.Deserialize(loaded, json, UmlNodeFactory.CreateRegistry());

        var restored = loaded.Nodes.Single(n => n.Id == "customer").ShouldBeOfType<UmlClassNode>();
        restored.Name.ShouldBe("Customer");
        restored.IsAbstract.ShouldBeTrue();
        restored.Stereotypes.Single().ShouldBe("entity");
        restored.Members.Single().Name.ShouldBe("Id");
        restored.Operations.Single().Parameters.Single().Name.ShouldBe("name");
        var restoredPort = restored.Ports.Single().ShouldBeOfType<UmlPortModel>();
        restoredPort.Kind.ShouldBe(UmlPortKind.Realization);
        restoredPort.Name.ShouldBe("Rename");

        var restoredLink = loaded.Links.Single().ShouldBeOfType<UmlRelationshipLink>();
        restoredLink.Id.ShouldBe(link.Id);
        restoredLink.Kind.ShouldBe(UmlRelationshipKind.Realization);
        restoredLink.Label.ShouldBe("implements");
        restoredLink.Labels.Select(label => label.Content).ShouldBe(new[] { "implements", "1", "1" });
        restoredLink.SourceMultiplicity.ShouldBe("1");
        restoredLink.TargetMultiplicity.ShouldBe("1");
    }

    [Fact]
    public void Named_uml_port_attaches_to_matching_member_row()
    {
        var node = new UmlClassNode(new Point(100, 100), "Order")
        {
            Size = new Size(300, 200),
        };
        node.Members.Add(new UmlMember("Id", "Guid"));
        node.Members.Add(new UmlMember("Status", "OrderStatus"));
        node.Operations.Add(new UmlOperation("Submit", "void"));

        var port = new UmlPortModel(node, PortAlignment.Right, UmlPortKind.Association, "Status")
        {
            Size = new Size(18, 18),
        };

        var center = port.GetPortCenter();

        center.X.ShouldBe(400);
        center.Y.ShouldBe(237.5, 0.001);
    }

    [Fact]
    public void Factory_restores_structural_uml_nodes()
    {
        UmlNodeFactory.Create(new NodeSnapshot { Kind = UmlClassNode.ModelKindKey, Title = "Class", X = 1, Y = 2 })
            .ShouldBeOfType<UmlClassNode>();
        UmlNodeFactory.Create(new NodeSnapshot { Kind = UmlInterfaceNode.ModelKindKey, Title = "Interface", X = 1, Y = 2 })
            .ShouldBeOfType<UmlInterfaceNode>();
        UmlNodeFactory.Create(new NodeSnapshot { Kind = UmlEnumNode.ModelKindKey, Title = "Enum", X = 1, Y = 2 })
            .ShouldBeOfType<UmlEnumNode>();
        UmlNodeFactory.Create(new NodeSnapshot { Kind = UmlPackageNode.ModelKindKey, Title = "Package", X = 1, Y = 2 })
            .ShouldBeOfType<UmlPackageNode>();
        UmlNodeFactory.Create(new NodeSnapshot { Kind = UmlNoteNode.ModelKindKey, Title = "Note", X = 1, Y = 2 })
            .ShouldBeOfType<UmlNoteNode>();
    }

    [Fact]
    public void Relationship_link_sets_metadata_defaults_and_label()
    {
        var source = new UmlClassNode(new Point(0, 0), "Customer");
        var target = new UmlInterfaceNode(new Point(200, 0), "IRepository");
        var link = new UmlRelationshipLink(source, target, UmlRelationshipKind.Dependency)
        {
            Label = "uses",
            SourceMultiplicity = "1",
            TargetMultiplicity = "0..*",
        };

        link.Kind.ShouldBe(UmlRelationshipKind.Dependency);
        link.Segmentable.ShouldBeTrue();
        link.Width.ShouldBe(1.8);
        link.Label.ShouldBe("uses");
        link.Labels.Select(label => label.Content).ShouldBe(new[] { "uses", "1", "0..*" });
        link.SourceMultiplicity.ShouldBe("1");
        link.TargetMultiplicity.ShouldBe("0..*");
    }
}
