using System;
using System.Collections.Generic;
using System.Linq;
using Nodely;
using Nodely.Models;

namespace Nodely.Avalonia.Network;

/// <summary>Simple arrange helper for network topology diagrams.</summary>
public static class NetworkLayout
{
    /// <summary>Arranges network nodes into recognizable topology columns.</summary>
    public static void Arrange(Diagram diagram, NetworkLayoutOptions? options = null)
    {
        if (diagram is null)
            throw new ArgumentNullException(nameof(diagram));

        options ??= new NetworkLayoutOptions();
        var nodes = diagram.Nodes.OfType<NetworkNodeBase>().ToList();
        if (nodes.Count == 0)
            return;

        var grouped = nodes
            .GroupBy(ColumnFor)
            .OrderBy(group => group.Key)
            .ToList();
        var minColumn = grouped.Min(group => group.Key);
        var maxColumn = grouped.Max(group => group.Key);
        var totalWidth = (maxColumn - minColumn) * options.ColumnSpacing;

        foreach (var group in grouped)
        {
            var columnNodes = group.OrderBy(node => ZoneOrder(node.Zone)).ThenBy(node => node.Position.Y).ThenBy(node => node.Name).ToList();
            var totalHeight = columnNodes.Sum(node => SizeOf(node, options).Height)
                + Math.Max(0, columnNodes.Count - 1) * options.RowSpacing;
            var y = options.OriginY - totalHeight / 2;
            var x = options.OriginX - totalWidth / 2 + (group.Key - minColumn) * options.ColumnSpacing;

            foreach (var node in columnNodes)
            {
                var size = SizeOf(node, options);
                node.SetPosition(x - size.Width / 2, y);
                y += size.Height + options.RowSpacing;
            }
        }

        diagram.Refresh();
    }

    internal static int ColumnFor(NetworkNodeBase node) => node switch
    {
        NetworkCloudNode => 0,
        NetworkClientNode => 0,
        NetworkRouterNode => 1,
        NetworkLoadBalancerNode => 2,
        NetworkFirewallNode => 2,
        NetworkZoneNode => 3,
        NetworkSwitchNode => 3,
        NetworkServiceNode => 4,
        NetworkServerNode => 5,
        _ => 3,
    };

    private static int ZoneOrder(string? zone)
        => string.IsNullOrWhiteSpace(zone) ? 100 : Math.Abs(StringComparer.OrdinalIgnoreCase.GetHashCode(zone)) % 50;

    private static NodeSize SizeOf(NodeModel node, NetworkLayoutOptions options)
    {
        if (node.Size is { } size)
            return new NodeSize(size.Width, size.Height);

        return node switch
        {
            NetworkSwitchNode => new NodeSize(NetworkVisualMetrics.SwitchWidth, NetworkVisualMetrics.SwitchHeight),
            NetworkFirewallNode => new NodeSize(NetworkVisualMetrics.FirewallWidth, NetworkVisualMetrics.FirewallHeight),
            NetworkCloudNode => new NodeSize(NetworkVisualMetrics.CloudWidth, NetworkVisualMetrics.CloudHeight),
            NetworkClientNode => new NodeSize(NetworkVisualMetrics.ClientWidth, NetworkVisualMetrics.ClientHeight),
            NetworkZoneNode => new NodeSize(NetworkVisualMetrics.ZoneWidth, NetworkVisualMetrics.ZoneHeight),
            _ => new NodeSize(options.DefaultNodeWidth, options.DefaultNodeHeight),
        };
    }

    private readonly record struct NodeSize(double Width, double Height);
}
