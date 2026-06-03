using System.Linq;
using Nodely.Models;

namespace Nodely.Layers;

/// <summary>The diagram's node layer. Cleans up a node's links/group/controls when it's removed.</summary>
public class NodeLayer : Layer<NodeModel>
{
    /// <summary>Creates the node layer for <paramref name="diagram"/>.</summary>
    public NodeLayer(Diagram diagram) : base(diagram) => Diagram = diagram;

    /// <summary>The owning diagram.</summary>
    public Diagram Diagram { get; }

    /// <inheritdoc />
    protected override void OnItemRemoved(NodeModel node)
    {
        Diagram.Links.Remove(node.PortLinks.ToList());
        Diagram.Links.Remove(node.Links.ToList());
        node.Group?.RemoveChild(node);
        Diagram.Controls.RemoveFor(node);
    }
}
