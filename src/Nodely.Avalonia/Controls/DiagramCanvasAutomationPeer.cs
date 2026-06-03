using Avalonia.Automation.Peers;
using Avalonia.Controls;

namespace Nodely.Avalonia.Controls;

/// <summary>A basic automation peer so assistive tech can discover the diagram surface.</summary>
internal sealed class DiagramCanvasAutomationPeer : ControlAutomationPeer
{
    public DiagramCanvasAutomationPeer(Control owner) : base(owner)
    {
    }

    protected override string GetClassNameCore() => "DiagramCanvas";

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Custom;

    protected override string GetNameCore() => "Diagram canvas";
}
