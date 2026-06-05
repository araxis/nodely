using System.Linq;
using Nodely;
using Nodely.Avalonia.Database;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;
using Shouldly;
using Xunit;

namespace Nodely.Database.Tests;

public class DatabaseModelTests
{
    [Fact]
    public void Table_node_defaults_to_schema_name_and_mutable_columns()
    {
        var table = new DatabaseTableNode(new Point(10, 20), "Customers");

        table.Schema.ShouldBe("dbo");
        table.ObjectName.ShouldBe("Customers");
        table.Title.ShouldBe("dbo.Customers");

        table.Columns.Add(new DatabaseColumn("Id", "int", isPrimaryKey: true, isNullable: false));
        table.Columns.Count.ShouldBe(1);
    }

    [Fact]
    public void Clone_copies_database_node_data()
    {
        var table = new DatabaseTableNode(new Point(10, 20), "Orders", "sales")
        {
            Size = new Size(220, 120),
        };
        table.Columns.Add(new DatabaseColumn("Id", "int", isPrimaryKey: true, isNullable: false));
        table.Columns.Add(new DatabaseColumn("CustomerId", "int") { IsForeignKey = true, IsNullable = false });

        var clone = table.Clone().ShouldBeOfType<DatabaseTableNode>();

        clone.ShouldNotBeSameAs(table);
        clone.Schema.ShouldBe("sales");
        clone.ObjectName.ShouldBe("Orders");
        clone.Columns.Count.ShouldBe(2);
        clone.Columns[1].IsForeignKey.ShouldBeTrue();
        clone.Columns[1].ShouldNotBeSameAs(table.Columns[1]);
    }

    [Fact]
    public void Extra_data_round_trips_database_fields_through_serializer()
    {
        var diagram = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var table = diagram.Nodes.Add(new DatabaseTableNode("customers", new Point(10, 20), "Customers", "sales"));
        table.Columns.Add(new DatabaseColumn("Id", "int", isPrimaryKey: true, isNullable: false));
        table.Columns.Add(new DatabaseColumn("Email", "nvarchar(120)"));

        var json = DiagramSerializer.Serialize(diagram);

        var loaded = new NodelyDiagram(null, registerDefaultBehaviors: false);
        DiagramSerializer.Deserialize(loaded, json, DatabaseNodeFactory.CreateRegistry());

        var restored = loaded.Nodes.Single().ShouldBeOfType<DatabaseTableNode>();
        restored.Id.ShouldBe("customers");
        restored.Schema.ShouldBe("sales");
        restored.ObjectName.ShouldBe("Customers");
        restored.Columns.Count.ShouldBe(2);
        restored.Columns[0].IsPrimaryKey.ShouldBeTrue();
        restored.Columns[1].DataType.ShouldBe("nvarchar(120)");
    }

    [Fact]
    public void Factory_restores_view_and_procedure_nodes()
    {
        var viewSnapshot = new NodeSnapshot
        {
            Id = "v",
            Kind = DatabaseViewNode.ModelKindKey,
            X = 1,
            Y = 2,
        };
        var procedureSnapshot = new NodeSnapshot
        {
            Id = "p",
            Kind = DatabaseProcedureNode.ModelKindKey,
            X = 3,
            Y = 4,
        };

        DatabaseNodeFactory.Create(viewSnapshot).ShouldBeOfType<DatabaseViewNode>();
        DatabaseNodeFactory.Create(procedureSnapshot).ShouldBeOfType<DatabaseProcedureNode>();
    }

    [Fact]
    public void Registry_round_trips_database_ports_and_relationship_links()
    {
        var diagram = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var customers = diagram.Nodes.Add(new DatabaseTableNode("customers", new Point(10, 20), "Customers", "sales"));
        var orders = diagram.Nodes.Add(new DatabaseTableNode("orders", new Point(180, 20), "Orders", "sales"));
        var customersOut = customers.AddPort(new DatabasePortModel("customers-out", customers, PortAlignment.Right,
            DatabasePortKind.Relationship, "CustomerId"));
        var ordersIn = orders.AddPort(new DatabasePortModel("orders-in", orders, PortAlignment.Left,
            DatabasePortKind.Relationship, "CustomerId"));
        diagram.Links.Add(new DatabaseRelationshipLink("rel", customersOut, ordersIn, RelationshipKind.OneToMany)
        {
            SourceCardinality = "1",
            TargetCardinality = "many",
        });

        var json = DiagramSerializer.Serialize(diagram);

        var loaded = new NodelyDiagram(null, registerDefaultBehaviors: false);
        DiagramSerializer.Deserialize(loaded, json, DatabaseNodeFactory.CreateRegistry());

        var restoredCustomers = loaded.Nodes.Single(n => n.Id == "customers").ShouldBeOfType<DatabaseTableNode>();
        var restoredPort = restoredCustomers.Ports.Single().ShouldBeOfType<DatabasePortModel>();
        restoredPort.Kind.ShouldBe(DatabasePortKind.Relationship);
        restoredPort.Name.ShouldBe("CustomerId");

        var restoredLink = loaded.Links.Single().ShouldBeOfType<DatabaseRelationshipLink>();
        restoredLink.Kind.ShouldBe(RelationshipKind.OneToMany);
        restoredLink.SourceCardinality.ShouldBe("1");
        restoredLink.TargetCardinality.ShouldBe("many");
    }

    [Fact]
    public void Named_database_port_attaches_to_matching_field_row()
    {
        var table = new DatabaseTableNode(new Point(100, 100), "Orders")
        {
            Size = new Size(270, 158),
        };
        table.Columns.Add(new DatabaseColumn("OrderId", "int", isPrimaryKey: true, isNullable: false));
        table.Columns.Add(new DatabaseColumn("CustomerId", "int") { IsForeignKey = true, IsNullable = false });
        table.Columns.Add(new DatabaseColumn("Total", "decimal(12,2)", isNullable: false));

        var port = new DatabasePortModel(table, PortAlignment.Right, DatabasePortKind.Relationship, "CustomerId")
        {
            Size = new Size(20, 16),
        };

        var center = port.GetPortCenter();

        center.X.ShouldBe(370);
        center.Y.ShouldBe(209);
    }

    [Fact]
    public void Relationship_link_sets_metadata_and_defers_endpoint_markers_to_renderer()
    {
        var source = new NodeModel(new Point(0, 0));
        var target = new NodeModel(new Point(100, 0));
        var link = new DatabaseRelationshipLink(source, target, RelationshipKind.ManyToMany)
        {
            SourceCardinality = "many",
            TargetCardinality = "many",
        };

        link.Kind.ShouldBe(RelationshipKind.ManyToMany);
        link.Segmentable.ShouldBeTrue();
        link.SourceMarker.ShouldBeNull();
        link.TargetMarker.ShouldBeNull();
        link.SourceCardinality.ShouldBe("many");
        link.TargetCardinality.ShouldBe("many");
    }
}
