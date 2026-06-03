using Avalonia.Controls;
using Nodely.Models;

namespace Nodely.Avalonia.Controls;

/// <summary>A hit-testable view of a single <see cref="PortModel"/> (a dot by default). Drag from it to link.</summary>
internal sealed class PortView : Decorator
{
    public PortView(PortModel port, DiagramCanvas owner)
    {
        Port = port;
        Child = owner.BuildPortContent(port); // default dot, or a RegisterPort template
    }

    /// <summary>The port this view represents.</summary>
    public PortModel Port { get; }
}
