using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Database;
using Nodely.Avalonia.Uml;
using Nodely.Models;
using Nodely.Serialization;
using Shouldly;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Tests;

public class UmlPackRenderingTests
{
    [AvaloniaFact]
    public void UseUmlNodes_registers_uml_renderers_and_relationship_drawing()
    {
        var diagram = new NodelyDiagram();
        var customer = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(40, 40), "Customer"));
        customer.Members.Add(new UmlMember("Id", "int"));
        var repository = diagram.Nodes.Add(new UmlInterfaceNode(new NodelyPoint(340, 40), "ICustomerRepository"));
        repository.Operations.Add(new UmlOperation("Get", "Customer"));
        var customerPort = customer.AddPort(new UmlPortModel(customer, PortAlignment.Right, UmlPortKind.Realization, "Id"));
        var repositoryPort = repository.AddPort(new UmlPortModel(repository, PortAlignment.Left, UmlPortKind.Realization, "Get"));
        var status = diagram.Nodes.Add(new UmlEnumNode(new NodelyPoint(640, 40), "OrderStatus"));
        status.Literals.Add("Pending");
        var package = diagram.Nodes.Add(new UmlPackageNode(new NodelyPoint(40, 280), "Sales"));
        var note = diagram.Nodes.Add(new UmlNoteNode(new NodelyPoint(340, 280), "Keep domain objects small."));
        var relationship = diagram.Links.Add(new UmlRelationshipLink(customerPort, repositoryPort, UmlRelationshipKind.Realization)
        {
            Label = "implements",
        });

        var canvas = new DiagramCanvas { Diagram = diagram }.UseUmlNodes();

        canvas.BuildNodeContent(customer).ShouldBeOfType<Border>().Tag.ShouldBe("uml-class-node");
        canvas.BuildNodeContent(repository).ShouldBeOfType<Border>().Tag.ShouldBe("uml-interface-node");
        canvas.BuildNodeContent(status).ShouldBeOfType<Border>().Tag.ShouldBe("uml-enum-node");
        canvas.BuildNodeContent(package).ShouldBeOfType<Border>().Tag.ShouldBe("uml-package-node");
        canvas.BuildNodeContent(note).ShouldBeOfType<Border>().Tag.ShouldBe("uml-note-node");
        canvas.BuildPortContent((UmlPortModel)customerPort).ShouldBeOfType<Grid>().Tag.ShouldBe("uml-port");
        canvas.ResolveLinkDrawer(relationship).ShouldNotBeNull();
        canvas.ResolveLinkStyle(relationship).DashStyle.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public void Side_pack_registrations_compose_on_canvas_and_serializer_registry()
    {
        var diagram = new NodelyDiagram();
        var table = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(0, 0), "Customers"));
        var entity = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(260, 0), "Customer"));
        var relationship = diagram.Links.Add(new UmlRelationshipLink(entity, table, UmlRelationshipKind.Association)
        {
            Label = "self",
        });

        var canvas = new DiagramCanvas { Diagram = diagram }.UseDatabaseNodes().UseUmlNodes();
        canvas.BuildNodeContent(table).ShouldBeOfType<Border>().Tag.ShouldBe("database-table-node");
        canvas.BuildNodeContent(entity).ShouldBeOfType<Border>().Tag.ShouldBe("uml-class-node");
        canvas.ResolveLinkDrawer(relationship).ShouldNotBeNull();

        var registry = DatabaseNodeFactory.CreateRegistry().UseUmlNodes();
        var json = DiagramSerializer.Serialize(diagram);
        var loaded = new NodelyDiagram();
        DiagramSerializer.Deserialize(loaded, json, registry);

        loaded.Nodes.ShouldContain(n => n is DatabaseTableNode);
        loaded.Nodes.ShouldContain(n => n is UmlClassNode);
        loaded.Links.Single().ShouldBeOfType<UmlRelationshipLink>();
    }
}
