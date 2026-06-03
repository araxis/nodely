using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using Nodely.Avalonia.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace Nodely.Avalonia.Tests;

/// <summary>Minimal Avalonia application used to host headless control tests.</summary>
public sealed class TestApp : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());
}

/// <summary>Builds the headless Avalonia app for <c>[AvaloniaFact]</c> / <c>[AvaloniaTheory]</c> tests.</summary>
public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
