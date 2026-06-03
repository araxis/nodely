using System;
using System.Threading.Tasks;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Options;

/// <summary>Hooks that decide whether deletions are allowed (e.g. to confirm with the user).</summary>
public class DiagramConstraintsOptions
{
    /// <summary>Returns whether a node may be deleted.</summary>
    public Func<NodeModel, ValueTask<bool>> ShouldDeleteNode { get; set; } = _ => new ValueTask<bool>(true);

    /// <summary>Returns whether a link may be deleted.</summary>
    public Func<BaseLinkModel, ValueTask<bool>> ShouldDeleteLink { get; set; } = _ => new ValueTask<bool>(true);

    /// <summary>Returns whether a group may be deleted.</summary>
    public Func<GroupModel, ValueTask<bool>> ShouldDeleteGroup { get; set; } = _ => new ValueTask<bool>(true);
}
