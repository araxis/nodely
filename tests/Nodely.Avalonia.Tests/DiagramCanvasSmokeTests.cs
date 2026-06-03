using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nodely.Avalonia.Controls;
using Shouldly;

namespace Nodely.Avalonia.Tests;

public class DiagramCanvasSmokeTests
{
    [AvaloniaFact]
    public void Canvas_can_be_hosted_in_a_window()
    {
        var canvas = new DiagramCanvas { Background = Brushes.Black };
        var window = new Window { Width = 400, Height = 300, Content = canvas };

        window.Show();

        canvas.Background.ShouldBe(Brushes.Black);
        window.Content.ShouldBe(canvas);
    }
}
