using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Database;
using Nodely.Models;
using Shouldly;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Tests;

public class DatabasePackRenderingTests
{
    [AvaloniaFact]
    public void UseDatabaseNodes_registers_database_renderers()
    {
        var diagram = new NodelyDiagram();
        var table = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(40, 40), "Customers"));
        table.Columns.Add(new DatabaseColumn("Id", "int", isPrimaryKey: true, isNullable: false));
        var view = diagram.Nodes.Add(new DatabaseViewNode(new NodelyPoint(320, 40), "CustomerSummary"));
        view.Columns.Add(new DatabaseColumn("CustomerId", "int"));
        var procedure = diagram.Nodes.Add(new DatabaseProcedureNode(new NodelyPoint(600, 40), "RefreshSummary"));
        procedure.Parameters.Add(new DatabaseParameter("@customerId", "int"));

        var tableOut = table.AddPort(new DatabasePortModel(table, PortAlignment.Right, DatabasePortKind.Relationship));
        var viewIn = view.AddPort(new DatabasePortModel(view, PortAlignment.Left, DatabasePortKind.Relationship));
        var relationship = diagram.Links.Add(new DatabaseRelationshipLink(tableOut, viewIn, RelationshipKind.OneToMany));

        var canvas = new DiagramCanvas { Diagram = diagram }.UseDatabaseNodes();

        canvas.BuildNodeContent(table).ShouldBeOfType<Border>().Tag.ShouldBe("database-table-node");
        canvas.BuildNodeContent(view).ShouldBeOfType<Border>().Tag.ShouldBe("database-view-node");
        canvas.BuildNodeContent(procedure).ShouldBeOfType<Border>().Tag.ShouldBe("database-procedure-node");
        canvas.BuildPortContent((DatabasePortModel)tableOut).ShouldBeOfType<Ellipse>().Tag.ShouldBe("database-port");
        canvas.ResolveLinkStyle(relationship).Width.ShouldBe(relationship.Width);
    }
}
