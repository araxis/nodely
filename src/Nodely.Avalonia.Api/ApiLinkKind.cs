namespace Nodely.Avalonia.Api;

/// <summary>Relationship kind for API diagrams.</summary>
public enum ApiLinkKind
{
    Request,
    Response,
    Publishes,
    Consumes,
    DependsOn,
    Secures,
}
