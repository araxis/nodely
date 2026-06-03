using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Nodely.Models;

namespace Nodely.Avalonia.Controls;

/// <summary>A container visual for a <see cref="GroupModel"/> (semi-transparent box) with a selection outline.</summary>
internal sealed class GroupView : Decorator
{
    private readonly DiagramCanvas _owner;

    public GroupView(GroupModel group, DiagramCanvas owner)
    {
        _owner = owner;
        Group = group;
        Child = owner.BuildGroupContent(group); // default box, or a RegisterGroup template
    }

    /// <summary>The group this view represents.</summary>
    public GroupModel Group { get; }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Group.Selected)
            context.DrawRectangle(null, new Pen(_owner.Palette.Selection, 2), new Rect(Bounds.Size).Inflate(1.5));
    }
}
