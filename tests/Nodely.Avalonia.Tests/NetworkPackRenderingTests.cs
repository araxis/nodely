using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nodely;
using Nodely.Avalonia;
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Database;
using Nodely.Avalonia.MindMap;
using Nodely.Avalonia.Network;
using Nodely.Avalonia.StateMachine;
using Nodely.Avalonia.Uml;
using Nodely.Avalonia.Workflow;
using Nodely.Models;
using Nodely.Serialization;
using Shouldly;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Avalonia.Tests;

public class NetworkPackRenderingTests
{
    [AvaloniaFact]
    public void UseNetworkNodes_registers_device_port_and_link_renderers()
    {
        var diagram = new NodelyDiagram();
        var cloud = diagram.Nodes.Add(new NetworkCloudNode(new NodelyPoint(0, 0), "Internet"));
        var router = diagram.Nodes.Add(new NetworkRouterNode(new NodelyPoint(220, 0), "Edge"));
        var firewall = diagram.Nodes.Add(new NetworkFirewallNode(new NodelyPoint(440, 0), "Policy"));
        var balancer = diagram.Nodes.Add(new NetworkLoadBalancerNode(new NodelyPoint(660, 0), "Traffic"));
        var switchNode = diagram.Nodes.Add(new NetworkSwitchNode(new NodelyPoint(880, 0), "Core") { PortCount = 16, ActivePorts = 12 });
        var server = diagram.Nodes.Add(new NetworkServerNode(new NodelyPoint(1100, 0), "Orders host"));
        var client = diagram.Nodes.Add(new NetworkClientNode(new NodelyPoint(0, 180), "Admin"));
        var service = diagram.Nodes.Add(new NetworkServiceNode(new NodelyPoint(660, 180), "API"));
        var zone = diagram.Nodes.Add(new NetworkZoneNode(new NodelyPoint(880, 180), "App subnet") { Address = "10.0.2.0/24" });

        var cloudPort = cloud.AddPort(new NetworkPortModel(cloud, PortAlignment.Right, NetworkPortRole.Wan, "wan"));
        var routerPort = router.AddPort(new NetworkPortModel(router, PortAlignment.Left, NetworkPortRole.Wan, "edge"));
        var vpn = diagram.Links.Add(new NetworkLink(cloudPort, routerPort, NetworkLinkKind.VpnTunnel)
        {
            Label = "site tunnel",
            Protocol = "IPsec",
        });
        var blocked = diagram.Links.Add(new NetworkLink(firewall, service, NetworkLinkKind.Blocked)
        {
            Label = "denied",
            Status = NetworkStatus.Blocked,
        });

        var canvas = new DiagramCanvas { Diagram = diagram, Palette = NodelyPalettes.Light }.UseNetworkNodes();

        canvas.BuildNodeContent(cloud).ShouldBeOfType<Border>().Tag.ShouldBe("network-cloud-node");
        canvas.BuildNodeContent(router).ShouldBeOfType<Border>().Tag.ShouldBe("network-router-node");
        canvas.BuildNodeContent(firewall).ShouldBeOfType<Border>().Tag.ShouldBe("network-firewall-node");
        canvas.BuildNodeContent(balancer).ShouldBeOfType<Border>().Tag.ShouldBe("network-loadbalancer-node");
        canvas.BuildNodeContent(switchNode).ShouldBeOfType<Border>().Tag.ShouldBe("network-switch-node");
        canvas.BuildNodeContent(server).ShouldBeOfType<Border>().Tag.ShouldBe("network-server-node");
        canvas.BuildNodeContent(client).ShouldBeOfType<Border>().Tag.ShouldBe("network-client-node");
        canvas.BuildNodeContent(service).ShouldBeOfType<Border>().Tag.ShouldBe("network-service-node");
        canvas.BuildNodeContent(zone).ShouldBeOfType<Border>().Tag.ShouldBe("network-zone-node");
        canvas.BuildPortContent((NetworkPortModel)cloudPort).ShouldBeOfType<Grid>().Tag.ShouldBe("network-port");
        canvas.ResolveLinkDrawer(vpn).ShouldNotBeNull();
        canvas.ResolveLinkDrawer(blocked).ShouldNotBeNull();
        canvas.ResolveLinkStyle(vpn).DashStyle.ShouldNotBeNull();
        canvas.ResolveLinkStyle(blocked).DashStyle.ShouldNotBeNull();

        canvas.Palette = NodelyPalettes.Dark;
        canvas.BuildNodeContent(router).ShouldBeOfType<Border>().Tag.ShouldBe("network-router-node");
    }

    [AvaloniaFact]
    public void Network_pack_registrations_compose_on_canvas_and_serializer_registry()
    {
        var diagram = new NodelyDiagram();
        var table = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(0, 0), "Customers"));
        var root = diagram.Nodes.Add(new MindMapRootNode(new NodelyPoint(260, 0), "Plan"));
        var network = diagram.Nodes.Add(new NetworkRouterNode(new NodelyPoint(520, 0), "Edge"));
        var server = diagram.Nodes.Add(new NetworkServerNode(new NodelyPoint(780, 0), "Host"));
        var entity = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(1040, 0), "Customer"));
        var state = diagram.Nodes.Add(new StateMachineStateNode(new NodelyPoint(1300, 0), "Waiting"));
        var task = diagram.Nodes.Add(new WorkflowTaskNode(new NodelyPoint(1560, 0), "Sync"));
        var link = diagram.Links.Add(new NetworkLink(network, server, NetworkLinkKind.Ethernet)
        {
            Label = "rack",
            Bandwidth = "1Gbps",
        });

        var canvas = new DiagramCanvas { Diagram = diagram }
            .UseDatabaseNodes()
            .UseMindMapNodes()
            .UseNetworkNodes()
            .UseStateMachineNodes()
            .UseUmlNodes()
            .UseWorkflowNodes();
        canvas.BuildNodeContent(table).ShouldBeOfType<Border>().Tag.ShouldBe("database-table-node");
        canvas.BuildNodeContent(root).ShouldBeOfType<Border>().Tag.ShouldBe("mindmap-root-node");
        canvas.BuildNodeContent(network).ShouldBeOfType<Border>().Tag.ShouldBe("network-router-node");
        canvas.BuildNodeContent(entity).ShouldBeOfType<Border>().Tag.ShouldBe("uml-class-node");
        canvas.BuildNodeContent(state).ShouldBeOfType<Border>().Tag.ShouldBe("statemachine-state-node");
        canvas.BuildNodeContent(task).ShouldBeOfType<Border>().Tag.ShouldBe("workflow-task-node");
        canvas.ResolveLinkDrawer(link).ShouldNotBeNull();

        var registry = DatabaseNodeFactory.CreateRegistry()
            .UseMindMapNodes()
            .UseNetworkNodes()
            .UseStateMachineNodes()
            .UseUmlNodes()
            .UseWorkflowNodes();
        var json = DiagramSerializer.Serialize(diagram);
        var loaded = new NodelyDiagram();
        DiagramSerializer.Deserialize(loaded, json, registry);

        loaded.Nodes.ShouldContain(n => n is DatabaseTableNode);
        loaded.Nodes.ShouldContain(n => n is MindMapRootNode);
        loaded.Nodes.ShouldContain(n => n is NetworkRouterNode);
        loaded.Nodes.ShouldContain(n => n is UmlClassNode);
        loaded.Nodes.ShouldContain(n => n is StateMachineStateNode);
        loaded.Nodes.ShouldContain(n => n is WorkflowTaskNode);
        loaded.Links.Single().ShouldBeOfType<NetworkLink>();
    }
}
