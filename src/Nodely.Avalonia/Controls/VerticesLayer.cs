using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Nodely.Models;
using Nodely.Models.Base;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Controls;

/// <summary>
/// Draws a small handle at each link vertex (bend point) and resolves which vertex is under a pointer.
/// Vertices are <see cref="LinkVertexModel"/> (movable), so a resolved handle selects + drags through the
/// normal behaviors, which reroutes the link. Handles show for any link that has vertices.
/// </summary>
internal sealed class VerticesLayer : Control
{
    private const double HandleSize = 10;

    private readonly DiagramCanvas _owner;
    private Diagram? _diagram;

    public VerticesLayer(DiagramCanvas owner)
    {
        _owner = owner;
        IsHitTestVisible = false; // hit-testing is geometric (via HitTest), not visual-tree based
        ClipToBounds = false;
        RenderTransformOrigin = RelativePoint.TopLeft;
    }

    public void SetDiagram(Diagram? diagram)
    {
        if (ReferenceEquals(_diagram, diagram))
            return;

        if (_diagram != null)
        {
            _diagram.Links.Added -= OnLinkAdded;
            _diagram.Links.Removed -= OnLinkRemoved;
            foreach (var link in _diagram.Links)
                link.Changed -= OnLinkChanged;
        }

        _diagram = diagram;

        if (diagram != null)
        {
            diagram.Links.Added += OnLinkAdded;
            diagram.Links.Removed += OnLinkRemoved;
            foreach (var link in diagram.Links)
                link.Changed += OnLinkChanged;
        }

        UpdateTransform();
        InvalidateVisual();
    }

    public void UpdateTransform()
    {
        var d = _diagram;
        RenderTransform = d == null
            ? null
            : new TransformGroup
            {
                Children = { new ScaleTransform(d.Zoom, d.Zoom), new TranslateTransform(d.Pan.X, d.Pan.Y) },
            };
    }

    private void OnLinkAdded(BaseLinkModel link)
    {
        link.Changed += OnLinkChanged;
        InvalidateVisual();
    }

    private void OnLinkRemoved(BaseLinkModel link)
    {
        link.Changed -= OnLinkChanged;
        InvalidateVisual();
    }

    private void OnLinkChanged(Model m) => InvalidateVisual();

    public override void Render(DrawingContext context)
    {
        var d = _diagram;
        if (d == null)
            return;

        var fill = _owner.Palette.CanvasBackground;
        var pen = new Pen(_owner.Palette.Selection, 1.5);
        const double half = HandleSize / 2;

        foreach (var link in d.Links)
            foreach (var vertex in link.Vertices)
            {
                var rect = new Rect(vertex.Position.X - half, vertex.Position.Y - half, HandleSize, HandleSize);
                context.DrawRectangle(fill, pen, rect, 2, 2);
            }
    }

    /// <summary>Returns the topmost link vertex within the handle radius of <paramref name="point"/> (diagram space), or null.</summary>
    public LinkVertexModel? HitTest(NodelyPoint point, double zoom)
    {
        var d = _diagram;
        if (d == null)
            return null;

        var tolerance = (HandleSize / 2 + 2) / (zoom <= 0 ? 1 : zoom);
        var toleranceSq = tolerance * tolerance;
        LinkVertexModel? hit = null;

        foreach (var link in d.Links)
            foreach (var vertex in link.Vertices)
            {
                var dx = point.X - vertex.Position.X;
                var dy = point.Y - vertex.Position.Y;
                if (dx * dx + dy * dy <= toleranceSq)
                    hit = vertex; // last match -> topmost in draw order
            }

        return hit;
    }
}
