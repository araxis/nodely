using Nodely.Anchors;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely;

/// <summary>Creates a link from a source linkable to a target anchor (returns null to cancel).</summary>
public delegate BaseLinkModel? LinkFactory(Diagram diagram, ILinkable source, Anchor targetAnchor);

/// <summary>Creates the anchor used to attach a link's target to a model.</summary>
public delegate Anchor AnchorFactory(Diagram diagram, BaseLinkModel link, ILinkable model);

/// <summary>Creates a group around the given children.</summary>
public delegate GroupModel GroupFactory(Diagram diagram, NodeModel[] children);
