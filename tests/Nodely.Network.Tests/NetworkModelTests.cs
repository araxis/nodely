using System.Collections.Generic;
using System.Linq;
using Nodely;
using Nodely.Avalonia.Network;
using Nodely.Geometry;
using Nodely.Models;
using Nodely.Serialization;
using Shouldly;
using Xunit;

namespace Nodely.Network.Tests;

public class NetworkModelTests
{
    [Fact]
    public void Router_node_defaults_to_name_role_status_and_accent()
    {
        var router = new NetworkRouterNode(new Point(10, 20), "Edge router")
        {
            Address = "203.0.113.10",
            Zone = "edge",
            Notes = "Primary internet edge",
        };

        router.Name.ShouldBe("Edge router");
        router.Title.ShouldBe("Edge router");
        router.Role.ShouldBe("Router");
        router.Status.ShouldBe(NetworkStatus.Online);
        router.AccentColor.ShouldBe("#4D9EFF");
        router.IconKey.ShouldBe("RTR");
        router.Address.ShouldBe("203.0.113.10");
        router.Zone.ShouldBe("edge");
        router.Notes.ShouldBe("Primary internet edge");
    }

    [Fact]
    public void Clone_copies_network_node_data()
    {
        var node = new NetworkSwitchNode(new Point(30, 40), "Core switch")
        {
            Address = "10.0.0.2",
            Status = NetworkStatus.Warning,
            Role = "Core switch",
            Notes = "One uplink degraded",
            AccentColor = "#37A779",
            IconKey = "CORE",
            Zone = "core",
            PortCount = 32,
            ActivePorts = 27,
            Size = new Size(230, 112),
        };

        var clone = node.Clone().ShouldBeOfType<NetworkSwitchNode>();

        clone.ShouldNotBeSameAs(node);
        clone.Name.ShouldBe("Core switch");
        clone.Address.ShouldBe("10.0.0.2");
        clone.Status.ShouldBe(NetworkStatus.Warning);
        clone.Role.ShouldBe("Core switch");
        clone.Notes.ShouldBe("One uplink degraded");
        clone.AccentColor.ShouldBe("#37A779");
        clone.IconKey.ShouldBe("CORE");
        clone.Zone.ShouldBe("core");
        clone.PortCount.ShouldBe(32);
        clone.ActivePorts.ShouldBe(27);
        clone.Size.ShouldBe(new Size(230, 112));
    }

    [Fact]
    public void Extra_data_round_trips_network_fields_through_serializer()
    {
        var diagram = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var cloud = diagram.Nodes.Add(new NetworkCloudNode("cloud", new Point(0, 0), "Internet")
        {
            Address = "0.0.0.0/0",
            Zone = "external",
        });
        var router = diagram.Nodes.Add(new NetworkRouterNode("router", new Point(250, 0), "Edge router")
        {
            Address = "203.0.113.10",
            Notes = "Primary path",
        });
        var firewall = diagram.Nodes.Add(new NetworkFirewallNode("firewall", new Point(500, 0), "Policy")
        {
            Status = NetworkStatus.Maintenance,
            Zone = "edge",
        });
        var service = diagram.Nodes.Add(new NetworkServiceNode("service", new Point(750, 0), "API")
        {
            Address = "api.internal",
            Status = NetworkStatus.Online,
        });
        var switchNode = diagram.Nodes.Add(new NetworkSwitchNode("switch", new Point(1000, 0), "Core switch")
        {
            PortCount = 16,
            ActivePorts = 12,
        });
        var server = diagram.Nodes.Add(new NetworkServerNode("server", new Point(1250, 0), "Orders host")
        {
            Address = "10.0.2.41",
            Status = NetworkStatus.Warning,
        });

        var cloudWan = cloud.AddPort(new NetworkPortModel(cloud, PortAlignment.Right, NetworkPortRole.Wan, "wan"));
        var routerWan = router.AddPort(new NetworkPortModel(router, PortAlignment.Left, NetworkPortRole.Wan, "internet"));
        var routerLan = router.AddPort(new NetworkPortModel(router, PortAlignment.Right, NetworkPortRole.Lan, "lan"));
        var firewallWan = firewall.AddPort(new NetworkPortModel(firewall, PortAlignment.Left, NetworkPortRole.Wan, "outside"));
        var firewallLan = firewall.AddPort(new NetworkPortModel(firewall, PortAlignment.Right, NetworkPortRole.Lan, "inside"));
        var servicePort = service.AddPort(new NetworkPortModel(service, PortAlignment.Left, NetworkPortRole.Service, "https"));
        var switchUplink = switchNode.AddPort(new NetworkPortModel(switchNode, PortAlignment.Left, NetworkPortRole.Uplink, "uplink", index: 0));
        var serverPort = server.AddPort(new NetworkPortModel(server, PortAlignment.Left, NetworkPortRole.Service, "app"));

        diagram.Links.Add(new NetworkLink(cloudWan, routerWan, NetworkLinkKind.Fiber)
        {
            Label = "internet",
            Protocol = "BGP",
            Bandwidth = "10Gbps",
            Latency = "3ms",
            Direction = NetworkLinkDirection.Bidirectional,
        });
        diagram.Links.Add(new NetworkLink(routerLan, firewallWan, NetworkLinkKind.VpnTunnel)
        {
            Label = "site tunnel",
            Protocol = "IPsec",
            Status = NetworkStatus.Warning,
            AccentColor = "#8B68B8",
        });
        diagram.Links.Add(new NetworkLink(firewallLan, servicePort, NetworkLinkKind.Dependency)
        {
            Label = "allow 443",
            Direction = NetworkLinkDirection.SourceToTarget,
        });
        diagram.Links.Add(new NetworkLink(switchUplink, serverPort, NetworkLinkKind.Ethernet)
        {
            Label = "rack",
            Bandwidth = "1Gbps",
        });

        var json = DiagramSerializer.Serialize(diagram);

        var loaded = new NodelyDiagram(null, registerDefaultBehaviors: false);
        DiagramSerializer.Deserialize(loaded, json, NetworkNodeFactory.CreateRegistry());

        var restoredSwitch = loaded.Nodes.Single(n => n.Id == "switch").ShouldBeOfType<NetworkSwitchNode>();
        restoredSwitch.PortCount.ShouldBe(16);
        restoredSwitch.ActivePorts.ShouldBe(12);
        restoredSwitch.Ports.OfType<NetworkPortModel>().Single().Role.ShouldBe(NetworkPortRole.Uplink);
        restoredSwitch.Ports.OfType<NetworkPortModel>().Single().Index.ShouldBe(0);

        var restoredFirewall = loaded.Nodes.Single(n => n.Id == "firewall").ShouldBeOfType<NetworkFirewallNode>();
        restoredFirewall.Status.ShouldBe(NetworkStatus.Maintenance);
        restoredFirewall.Zone.ShouldBe("edge");

        var restoredVpn = loaded.Links.OfType<NetworkLink>().Single(link => link.Kind == NetworkLinkKind.VpnTunnel);
        restoredVpn.Label.ShouldBe("site tunnel");
        restoredVpn.Protocol.ShouldBe("IPsec");
        restoredVpn.Status.ShouldBe(NetworkStatus.Warning);
        restoredVpn.AccentColor.ShouldBe("#8B68B8");
        restoredVpn.Labels.Single().Content.ShouldBe("site tunnel · IPsec");
    }

    [Fact]
    public void Factory_restores_network_nodes()
    {
        NetworkNodeFactory.Create(new NodeSnapshot { Kind = NetworkRouterNode.ModelKindKey, Title = "Router", X = 1, Y = 2 })
            .ShouldBeOfType<NetworkRouterNode>();
        NetworkNodeFactory.Create(new NodeSnapshot { Kind = NetworkSwitchNode.ModelKindKey, Title = "Switch", X = 1, Y = 2 })
            .ShouldBeOfType<NetworkSwitchNode>();
        NetworkNodeFactory.Create(new NodeSnapshot { Kind = NetworkFirewallNode.ModelKindKey, Title = "Firewall", X = 1, Y = 2 })
            .ShouldBeOfType<NetworkFirewallNode>();
        NetworkNodeFactory.Create(new NodeSnapshot { Kind = NetworkLoadBalancerNode.ModelKindKey, Title = "Load balancer", X = 1, Y = 2 })
            .ShouldBeOfType<NetworkLoadBalancerNode>();
        NetworkNodeFactory.Create(new NodeSnapshot { Kind = NetworkServerNode.ModelKindKey, Title = "Server", X = 1, Y = 2 })
            .ShouldBeOfType<NetworkServerNode>();
        NetworkNodeFactory.Create(new NodeSnapshot { Kind = NetworkClientNode.ModelKindKey, Title = "Client", X = 1, Y = 2 })
            .ShouldBeOfType<NetworkClientNode>();
        NetworkNodeFactory.Create(new NodeSnapshot { Kind = NetworkCloudNode.ModelKindKey, Title = "Cloud", X = 1, Y = 2 })
            .ShouldBeOfType<NetworkCloudNode>();
        NetworkNodeFactory.Create(new NodeSnapshot { Kind = NetworkServiceNode.ModelKindKey, Title = "Service", X = 1, Y = 2 })
            .ShouldBeOfType<NetworkServiceNode>();
        NetworkNodeFactory.Create(new NodeSnapshot { Kind = NetworkZoneNode.ModelKindKey, Title = "Zone", X = 1, Y = 2 })
            .ShouldBeOfType<NetworkZoneNode>();
    }

    [Fact]
    public void Network_link_sets_metadata_defaults_markers_and_label()
    {
        var source = new NetworkRouterNode(new Point(0, 0), "Router");
        var target = new NetworkFirewallNode(new Point(200, 0), "Firewall");
        var link = new NetworkLink(source, target, NetworkLinkKind.VpnTunnel)
        {
            Label = "primary",
            Protocol = "IPsec",
            Bandwidth = "1Gbps",
            Latency = "4ms",
            Status = NetworkStatus.Warning,
            Direction = NetworkLinkDirection.Bidirectional,
            AccentColor = "#8B68B8",
        };

        link.Kind.ShouldBe(NetworkLinkKind.VpnTunnel);
        link.Segmentable.ShouldBeTrue();
        link.SourceMarker.ShouldNotBeNull();
        link.TargetMarker.ShouldNotBeNull();
        link.Width.ShouldBe(2.4);
        link.Labels.Single().Content.ShouldBe("primary · IPsec · 1Gbps · 4ms");
        link.Status.ShouldBe(NetworkStatus.Warning);
        link.AccentColor.ShouldBe("#8B68B8");
    }

    [Fact]
    public void Port_model_persists_role_name_and_index()
    {
        var node = new NetworkSwitchNode(new Point(0, 0), "Core") { Size = new Size(230, 112) };
        var port = new NetworkPortModel(node, PortAlignment.Right, NetworkPortRole.Uplink, "uplink", index: 3);

        var data = port.GetExtraData();
        var restored = new NetworkPortModel(node);
        restored.SetExtraData(new Dictionary<string, object?>(data));

        restored.Role.ShouldBe(NetworkPortRole.Uplink);
        restored.Name.ShouldBe("uplink");
        restored.Index.ShouldBe(3);
        port.GetPortCenter().X.ShouldBeGreaterThan(node.Position.X);
    }

    [Fact]
    public void Arrange_places_topology_nodes_in_expected_columns()
    {
        var diagram = new NodelyDiagram(null, registerDefaultBehaviors: false);
        var cloud = diagram.Nodes.Add(new NetworkCloudNode(new Point(0, 0), "Internet") { Size = new Size(198, 112) });
        var router = diagram.Nodes.Add(new NetworkRouterNode(new Point(0, 0), "Edge") { Size = new Size(190, 104) });
        var firewall = diagram.Nodes.Add(new NetworkFirewallNode(new Point(0, 0), "Policy") { Size = new Size(188, 118) });
        var switchNode = diagram.Nodes.Add(new NetworkSwitchNode(new Point(0, 0), "Core") { Size = new Size(230, 112) });
        var service = diagram.Nodes.Add(new NetworkServiceNode(new Point(0, 0), "API") { Size = new Size(190, 104) });
        var server = diagram.Nodes.Add(new NetworkServerNode(new Point(0, 0), "Host") { Size = new Size(190, 104) });

        NetworkLayout.Arrange(diagram, new NetworkLayoutOptions { OriginX = 0, OriginY = 0, ColumnSpacing = 240 });

        router.Position.X.ShouldBeGreaterThan(cloud.Position.X);
        firewall.Position.X.ShouldBeGreaterThan(router.Position.X);
        switchNode.Position.X.ShouldBeGreaterThanOrEqualTo(firewall.Position.X);
        service.Position.X.ShouldBeGreaterThan(switchNode.Position.X);
        server.Position.X.ShouldBeGreaterThan(service.Position.X);
    }
}
