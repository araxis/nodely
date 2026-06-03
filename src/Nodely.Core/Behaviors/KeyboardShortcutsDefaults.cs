using System.Linq;
using System.Threading.Tasks;
using Nodely.Models;
using Nodely.Models.Base;

namespace Nodely.Behaviors;

/// <summary>The default keyboard shortcut actions.</summary>
public static class KeyboardShortcutsDefaults
{
    /// <summary>Deletes the selected models (respecting the delete constraints).</summary>
    public static async ValueTask DeleteSelection(Diagram diagram)
    {
        var wasSuspended = diagram.SuspendRefresh;
        if (!wasSuspended)
            diagram.SuspendRefresh = true;

        foreach (var sm in diagram.GetSelectedModels().ToArray())
        {
            if (sm.Locked)
                continue;

            if (sm is GroupModel group && await diagram.Options.Constraints.ShouldDeleteGroup(group))
                diagram.Groups.Delete(group);
            else if (sm is NodeModel node && await diagram.Options.Constraints.ShouldDeleteNode(node))
                diagram.Nodes.Remove(node);
            else if (sm is BaseLinkModel link && await diagram.Options.Constraints.ShouldDeleteLink(link))
                diagram.Links.Remove(link);
        }

        if (!wasSuspended)
        {
            diagram.SuspendRefresh = false;
            diagram.Refresh();
        }
    }

    /// <summary>Groups the selected nodes, or ungroups them if any are already grouped.</summary>
    public static ValueTask Grouping(Diagram diagram)
    {
        if (!diagram.Options.Groups.Enabled)
            return default;

        if (!diagram.GetSelectedModels().Any())
            return default;

        var selectedNodes = diagram.Nodes.Where(n => n.Selected).ToArray();
        var nodesWithGroup = selectedNodes.Where(n => n.Group != null).ToArray();
        if (nodesWithGroup.Length > 0)
        {
            foreach (var group in nodesWithGroup.GroupBy(n => n.Group!).Select(g => g.Key))
                diagram.Groups.Remove(group);
        }
        else
        {
            if (selectedNodes.Length < 2)
                return default;

            if (selectedNodes.Any(n => n.Group != null))
                return default;

            diagram.Groups.Group(selectedNodes);
        }

        return default;
    }
}
