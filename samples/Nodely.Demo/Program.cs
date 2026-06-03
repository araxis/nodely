using System;
using Avalonia;

namespace Nodely.Demo;

internal static class Program
{
    // Avalonia configuration; do not call any Avalonia APIs before AppMain (StartWith...) is invoked.
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
