using Nodely.Anchors;
using Nodely.Models.Base;

namespace Nodely.Models;

/// <summary>The default link: a styleable connection between two anchors, ports, or nodes.</summary>
public class LinkModel : BaseLinkModel
{
    /// <summary>The stable serialization kind for a default link.</summary>
    public const string ModelKindKey = "link";

    /// <summary>Creates a link between two anchors.</summary>
    public LinkModel(Anchor source, Anchor target) : base(source, target) { }

    /// <summary>Creates a link with the given id between two anchors.</summary>
    public LinkModel(string id, Anchor source, Anchor target) : base(id, source, target) { }

    /// <summary>Creates a link between two ports.</summary>
    public LinkModel(PortModel sourcePort, PortModel targetPort)
        : base(new SinglePortAnchor(sourcePort), new SinglePortAnchor(targetPort)) { }

    /// <summary>Creates a link between two nodes (shape-intersection anchors).</summary>
    public LinkModel(NodeModel sourceNode, NodeModel targetNode)
        : base(new ShapeIntersectionAnchor(sourceNode), new ShapeIntersectionAnchor(targetNode)) { }

    /// <summary>Creates a link with the given id between two ports.</summary>
    public LinkModel(string id, PortModel sourcePort, PortModel targetPort)
        : base(id, new SinglePortAnchor(sourcePort), new SinglePortAnchor(targetPort)) { }

    /// <summary>Creates a link with the given id between two nodes.</summary>
    public LinkModel(string id, NodeModel sourceNode, NodeModel targetNode)
        : base(id, new ShapeIntersectionAnchor(sourceNode), new ShapeIntersectionAnchor(targetNode)) { }

    /// <summary>An optional stroke color (interpreted by the renderer/theme).</summary>
    public string? Color { get; set; }

    /// <summary>An optional stroke color when selected.</summary>
    public string? SelectedColor { get; set; }

    /// <summary>The stroke width.</summary>
    public double Width { get; set; } = 2;

    /// <inheritdoc />
    public override string ModelKind => ModelKindKey;
}
