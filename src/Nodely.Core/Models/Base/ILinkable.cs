using System.Collections.Generic;

namespace Nodely.Models.Base;

/// <summary>
/// Something a link can attach to (a node, port, or link). The <see cref="AddLink"/>/<see cref="RemoveLink"/>
/// members are implemented explicitly by models, so normal callers don't see them (they're engine plumbing).
/// </summary>
public interface ILinkable
{
    /// <summary>The links currently attached to this model.</summary>
    IReadOnlyList<BaseLinkModel> Links { get; }

    /// <summary>Whether a link may attach from this model to <paramref name="other"/>.</summary>
    bool CanAttachTo(ILinkable other);

    /// <summary>Engine plumbing: registers an attached link. Implemented explicitly.</summary>
    void AddLink(BaseLinkModel link);

    /// <summary>Engine plumbing: unregisters an attached link. Implemented explicitly.</summary>
    void RemoveLink(BaseLinkModel link);
}
