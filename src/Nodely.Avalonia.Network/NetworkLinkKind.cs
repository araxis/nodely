namespace Nodely.Avalonia.Network;

/// <summary>Kinds of network topology links.</summary>
public enum NetworkLinkKind
{
    Ethernet,
    Fiber,
    Wireless,
    VpnTunnel,
    Dependency,
    Blocked,
}
