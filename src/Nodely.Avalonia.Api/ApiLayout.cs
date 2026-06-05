using System;
using System.Linq;
using Nodely;
using Nodely.Models;

namespace Nodely.Avalonia.Api;

/// <summary>Simple arrange helper for API design diagrams.</summary>
public static class ApiLayout
{
    /// <summary>Arranges API nodes into client, edge, service, operation, and contract columns.</summary>
    public static void Arrange(Diagram diagram, ApiLayoutOptions? options = null)
    {
        if (diagram is null)
            throw new ArgumentNullException(nameof(diagram));

        options ??= new ApiLayoutOptions();
        var nodes = diagram.Nodes.OfType<ApiNodeBase>().ToList();
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
            var columnNodes = group.OrderBy(node => node.Position.Y).ThenBy(node => node.Name).ToList();
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

    internal static int ColumnFor(ApiNodeBase node) => node switch
    {
        ApiClientNode => 0,
        ApiGatewayNode => 1,
        ApiServiceNode => 2,
        ApiEndpointNode => 3,
        ApiOperationNode => 4,
        ApiAuthNode => 4,
        ApiContractNode => 5,
        ApiGroupNode => 2,
        _ => 3,
    };

    private static NodeSize SizeOf(NodeModel node, ApiLayoutOptions options)
    {
        if (node.Size is { } size)
            return new NodeSize(size.Width, size.Height);

        return node switch
        {
            ApiServiceNode => new NodeSize(ApiVisualMetrics.ServiceWidth, ApiVisualMetrics.ServiceHeight),
            ApiEndpointNode => new NodeSize(ApiVisualMetrics.EndpointWidth, ApiVisualMetrics.EndpointHeight),
            ApiContractNode => new NodeSize(ApiVisualMetrics.ContractWidth, ApiVisualMetrics.ContractHeight),
            ApiOperationNode => new NodeSize(ApiVisualMetrics.OperationWidth, ApiVisualMetrics.OperationHeight),
            ApiClientNode => new NodeSize(ApiVisualMetrics.ClientWidth, ApiVisualMetrics.ClientHeight),
            ApiGatewayNode => new NodeSize(ApiVisualMetrics.GatewayWidth, ApiVisualMetrics.GatewayHeight),
            ApiAuthNode => new NodeSize(ApiVisualMetrics.AuthWidth, ApiVisualMetrics.AuthHeight),
            ApiGroupNode => new NodeSize(ApiVisualMetrics.GroupWidth, ApiVisualMetrics.GroupHeight),
            _ => new NodeSize(options.DefaultNodeWidth, options.DefaultNodeHeight),
        };
    }

    private readonly record struct NodeSize(double Width, double Height);
}
