using System.Linq;
using Nodely.Models;

namespace Nodely.Layers;

/// <summary>The diagram's group layer, with helpers to create and delete groups (and their children).</summary>
public class GroupLayer : Layer<GroupModel>
{
    /// <summary>Creates the group layer for <paramref name="diagram"/>.</summary>
    public GroupLayer(Diagram diagram) : base(diagram) => Diagram = diagram;

    /// <summary>The owning diagram.</summary>
    public Diagram Diagram { get; }

    /// <summary>Creates and adds a group around the given children using the configured factory.</summary>
    public GroupModel Group(params NodeModel[] children)
        => Add(Diagram.Options.Groups.Factory(Diagram, children));

    /// <summary>Removes the group AND its children (recursively for nested groups).</summary>
    public void Delete(GroupModel group)
    {
        Diagram.Batch(() =>
        {
            var children = group.Children.ToArray();

            Remove(group);

            foreach (var child in children)
            {
                if (child is GroupModel g)
                    Delete(g);
                else
                    Diagram.Nodes.Remove(child);
            }
        });
    }

    /// <inheritdoc />
    protected override void OnItemRemoved(GroupModel group)
    {
        Diagram.Links.Remove(group.PortLinks.ToArray());
        Diagram.Links.Remove(group.Links.ToArray());
        group.Ungroup();
        group.Group?.RemoveChild(group);
        Diagram.Controls.RemoveFor(group);
    }
}
