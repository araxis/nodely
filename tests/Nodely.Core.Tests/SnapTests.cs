using Nodely.Events;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Options;
using Shouldly;
using Xunit;

namespace Nodely.Core.Tests;

public class SnapTests
{
    private static PointerEvent P(double x, double y)
        => new(x, y, PointerButton.Left, false, false, false, false);

    [Fact]
    public void Dragging_snaps_node_to_the_grid()
    {
        var d = new NodelyDiagram(new DiagramOptions { GridSize = 20 });
        var n = d.Nodes.Add(new NodeModel(new Point(0, 0)));
        n.Size = new Size(50, 50);

        d.TriggerPointerDown(n, P(10, 10));
        d.TriggerPointerMove(n, P(43, 10)); // +33 in x
        d.TriggerPointerUp(n, P(43, 10));

        // 0 + 33 = 33 -> snapped to the nearest multiple of 20 = 40
        n.Position.X.ShouldBe(40, 0.001);
        n.Position.Y.ShouldBe(0, 0.001);
    }
}
