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
        DiagramSerializer.Deserialize(loaded, json, DatabaseNodeFactory.Create);

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
            Kind = nameof(DatabaseViewNode),
            X = 1,
            Y = 2,
        };
        var procedureSnapshot = new NodeSnapshot
        {
            Id = "p",
            Kind = nameof(DatabaseProcedureNode),
            X = 3,
            Y = 4,
        };

        DatabaseNodeFactory.Create(viewSnapshot).ShouldBeOfType<DatabaseViewNode>();
        DatabaseNodeFactory.Create(procedureSnapshot).ShouldBeOfType<DatabaseProcedureNode>();
    }

    [Fact]
    public void Relationship_link_sets_metadata_and_markers()
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
        link.SourceMarker.ShouldNotBeNull();
        link.TargetMarker.ShouldNotBeNull();
        link.SourceCardinality.ShouldBe("many");
        link.TargetCardinality.ShouldBe("many");
    }
}
