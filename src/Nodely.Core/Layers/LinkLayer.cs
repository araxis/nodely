using System.Linq;
using Nodely.Anchors;
using Nodely.Models.Base;

namespace Nodely.Layers;

/// <summary>The diagram's link layer. Wires anchor attachment and refreshes affected models.</summary>
public class LinkLayer : Layer<BaseLinkModel>
{
    /// <summary>Creates the link layer for <paramref name="diagram"/>.</summary>
    public LinkLayer(Diagram diagram) : base(diagram) => Diagram = diagram;

    /// <summary>The owning diagram.</summary>
    public Diagram Diagram { get; }

    /// <inheritdoc />
    protected override void OnItemAdded(BaseLinkModel link)
    {
        link.Diagram = Diagram;
        HandleAnchor(link, link.Source, add: true);
        HandleAnchor(link, link.Target, add: true);
        link.Refresh();

        link.SourceChanged += OnLinkSourceChanged;
        link.TargetChanged += OnLinkTargetChanged;
    }

    /// <inheritdoc />
    protected override void OnItemRemoved(BaseLinkModel link)
    {
        link.Diagram = null;
        HandleAnchor(link, link.Source, add: false);
        HandleAnchor(link, link.Target, add: false);
        link.Refresh();

        link.SourceChanged -= OnLinkSourceChanged;
        link.TargetChanged -= OnLinkTargetChanged;

        Diagram.Controls.RemoveFor(link);
        Remove(link.Links.ToList());
    }

    private static void OnLinkSourceChanged(BaseLinkModel link, Anchor old, Anchor @new)
    {
        HandleAnchor(link, old, add: false);
        HandleAnchor(link, @new, add: true);
    }

    private static void OnLinkTargetChanged(BaseLinkModel link, Anchor old, Anchor @new)
    {
        HandleAnchor(link, old, add: false);
        HandleAnchor(link, @new, add: true);
    }

    private static void HandleAnchor(BaseLinkModel link, Anchor anchor, bool add)
    {
        if (add)
            anchor.Model?.AddLink(link);
        else
            anchor.Model?.RemoveLink(link);

        if (anchor.Model is Model model)
            model.Refresh();
    }
}
