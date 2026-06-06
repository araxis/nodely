using Avalonia;
using Avalonia.Headless;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Nodely.Demo.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace Nodely.Demo.Tests;

public sealed class DemoTestApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        RequestedThemeVariant = ThemeVariant.Dark;
    }
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<DemoTestApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
