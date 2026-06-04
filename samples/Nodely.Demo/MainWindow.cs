using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nodely;
using Nodely.Algorithms;
using Nodely.Anchors;
using Nodely.Avalonia;
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Database;
using Nodely.Avalonia.Uml;
using Nodely.Models;
using Nodely.Serialization;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Demo;

/// <summary>A custom node type with extra domain data, rendered by a registered template.</summary>
public sealed class TaskNode : NodeModel
{
    public new const string ModelKindKey = "demo.task";

    public TaskNode(NodelyPoint position, string title) : base(position) => Title = title;

    public TaskNode(string id, NodelyPoint position, string title) : base(id, position) => Title = title;

    public string Status { get; set; } = "Pending";

    public override string ModelKind => ModelKindKey;

    public override NodeModel Clone() => new TaskNode(Position, Title ?? string.Empty) { Status = Status, Size = Size };

    public override IReadOnlyDictionary<string, object?> GetExtraData() =>
        new Dictionary<string, object?> { ["Status"] = Status };

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Status", out var value) && value is string status)
            Status = status;
    }
}

public sealed class SignalPort : PortModel
{
    public SignalPort(NodeModel parent, PortAlignment alignment, string role) : base(parent, alignment) => Role = role;

    public SignalPort(string id, NodeModel parent, PortAlignment alignment) : base(id, parent, alignment) { }

    public new const string ModelKindKey = "demo.signal-port";

    public string Role { get; set; } = "in";

    public override string ModelKind => ModelKindKey;

    public override IReadOnlyDictionary<string, object?> GetExtraData() =>
        new Dictionary<string, object?> { ["Role"] = Role };

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Role", out var role) && role is string roleText)
            Role = roleText;
    }
}

public sealed class FlowLink : LinkModel
{
    public FlowLink(PortModel sourcePort, PortModel targetPort) : base(sourcePort, targetPort) { }

    public FlowLink(string id, Anchor source, Anchor target) : base(id, source, target) { }

    public new const string ModelKindKey = "demo.flow";

    public bool Critical { get; set; }

    public override string ModelKind => ModelKindKey;

    public override IReadOnlyDictionary<string, object?> GetExtraData() =>
        new Dictionary<string, object?> { ["Critical"] = Critical };

    public override void SetExtraData(IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue("Critical", out var critical) && critical is bool value)
            Critical = value;
    }
}

public sealed class HighlightGroup : GroupModel
{
    public HighlightGroup(IEnumerable<NodeModel> children, string title) : base(children, padding: 34) => Title = title;
}

public sealed class GuideLayer : DiagramLayer
{
    public override void Render(DrawingContext context)
    {
        var diagram = Diagram;
        if (diagram == null)
            return;

        var pen = new Pen(new SolidColorBrush(Color.FromArgb(90, 86, 205, 255)), 1, DashStyle.Dash);
        foreach (var node in diagram.Nodes)
            if (node.Size is { } size)
            {
                var y = node.Position.Y + size.Height + 24;
                context.DrawLine(pen, new Point(node.Position.X - 18, y), new Point(node.Position.X + size.Width + 18, y));
            }
    }
}

public sealed class MainWindow : Window
{
    private readonly ContentControl _host = new();
    private NodelyPalette _palette = NodelyPalettes.Dark;
    private NodelyDiagram? _currentDiagram;
    private DiagramCanvas? _currentCanvas;
    private string? _savedJson;

    public MainWindow()
    {
        Title = "Nodely Gallery";
        Width = 1180;
        Height = 760;

        var scenes = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Margin = new Thickness(10) };
        scenes.Children.Add(SceneButton("Workflow", BuildWorkflow));
        scenes.Children.Add(SceneButton("State machine", BuildStateMachine));
        scenes.Children.Add(SceneButton("Inspector", BuildInspector));
        scenes.Children.Add(SceneButton("Extensibility", BuildExtensibility));
        scenes.Children.Add(SceneButton("Database", BuildDatabase));
        scenes.Children.Add(SceneButton("UML", BuildUml));
        scenes.Children.Add(new Border { Width = 24 });
        scenes.Children.Add(ToolButton("Theme", ToggleTheme));
        scenes.Children.Add(ToolButton("Save", Save));
        scenes.Children.Add(ToolButton("Load", Load));

        var root = new DockPanel();
        DockPanel.SetDock(scenes, Dock.Top);
        root.Children.Add(scenes);
        root.Children.Add(_host);
        Content = root;

        _host.Content = BuildWorkflow();
    }

    private Button SceneButton(string text, Func<Control> build)
    {
        var button = new Button { Content = text, MinWidth = 96 };
        button.Click += (_, _) => _host.Content = build();
        return button;
    }

    private static Button ToolButton(string text, Action onClick)
    {
        var button = new Button { Content = text, MinWidth = 42 };
        button.Click += (_, _) => onClick();
        return button;
    }

    private void ToggleTheme()
    {
        _palette = _palette == NodelyPalettes.Dark ? NodelyPalettes.Light : NodelyPalettes.Dark;
        if (_currentCanvas != null)
            _currentCanvas.Palette = _palette;
    }

    private void Save()
    {
        if (_currentDiagram != null)
            _savedJson = DiagramSerializer.Serialize(_currentDiagram);
    }

    private void Load()
    {
        if (_savedJson == null)
            return;

        var diagram = NewDiagram();
        DiagramSerializer.Deserialize(diagram, _savedJson, CreateSerializationRegistry());
        _host.Content = Editor(diagram);
    }

    private static DiagramSerializationRegistry CreateSerializationRegistry() => DatabaseNodeFactory.CreateRegistry()
        .UseUmlNodes()
        .RegisterNode(TaskNode.ModelKindKey, ns => new TaskNode(ns.Id, new NodelyPoint(ns.X, ns.Y), ns.Title ?? string.Empty))
        .RegisterPort(SignalPort.ModelKindKey, (ps, parent) =>
            new SignalPort(ps.Id, parent, Enum.Parse<PortAlignment>(ps.Alignment)))
        .RegisterLink(FlowLink.ModelKindKey, (ls, source, target) => new FlowLink(ls.Id, source, target));

    private Control Editor(NodelyDiagram diagram, bool readOnly = false, Action<DiagramCanvas>? configureCanvas = null)
    {
        _currentDiagram = diagram;

        var canvas = new DiagramCanvas { Diagram = diagram, Palette = _palette, IsReadOnly = readOnly };
        _currentCanvas = canvas;
        RegisterTaskNode(canvas);
        configureCanvas?.Invoke(canvas);

        var navigator = new DiagramNavigator
        {
            Diagram = diagram,
            Width = 210,
            Height = 136,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(12),
        };

        var toolbar = BuildEditorToolbar(canvas, diagram, readOnly);
        return new Grid { Children = { canvas, navigator, toolbar } };
    }

    private StackPanel BuildEditorToolbar(DiagramCanvas canvas, NodelyDiagram diagram, bool readOnly)
    {
        var bar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(12),
        };

        bar.Children.Add(ToolButton("+", canvas.ZoomIn));
        bar.Children.Add(ToolButton("-", canvas.ZoomOut));
        bar.Children.Add(ToolButton("Fit", () => canvas.ZoomToFit()));

        var layout = ToolButton("Layout", () =>
        {
            canvas.RunAsUndoableMove(() => LayeredLayout.Arrange(diagram));
            canvas.ZoomToFit();
        });
        layout.IsEnabled = !readOnly;
        bar.Children.Add(layout);

        var copy = ToolButton("Copy", canvas.CopySelection);
        var cut = ToolButton("Cut", canvas.CutSelection);
        var paste = ToolButton("Paste", canvas.PasteClipboard);
        var duplicate = ToolButton("Duplicate", canvas.DuplicateSelection);
        var group = ToolButton("Group", canvas.GroupSelection);
        var ungroup = ToolButton("Ungroup", canvas.UngroupSelection);
        var front = ToolButton("Front", canvas.BringSelectionToFront);
        var back = ToolButton("Back", canvas.SendSelectionToBack);
        var undo = ToolButton("Undo", canvas.Undo);
        var redo = ToolButton("Redo", canvas.Redo);

        foreach (var button in new[] { copy, cut, paste, duplicate, group, ungroup, front, back, undo, redo })
            bar.Children.Add(button);

        void Refresh()
        {
            copy.IsEnabled = canvas.CanCopySelection;
            cut.IsEnabled = canvas.CanCutSelection;
            paste.IsEnabled = canvas.CanPasteClipboard;
            duplicate.IsEnabled = canvas.CanDuplicateSelection;
            group.IsEnabled = canvas.CanGroupSelection;
            ungroup.IsEnabled = canvas.CanUngroupSelection;
            front.IsEnabled = !canvas.IsReadOnly && canvas.HasSelection;
            back.IsEnabled = !canvas.IsReadOnly && canvas.HasSelection;
            undo.IsEnabled = !canvas.IsReadOnly && canvas.CanUndo;
            redo.IsEnabled = !canvas.IsReadOnly && canvas.CanRedo;
        }

        canvas.CommandStateChanged += Refresh;
        Refresh();
        return bar;
    }

    private static void RegisterTaskNode(DiagramCanvas canvas) => canvas.RegisterNode<TaskNode>(node => new Border
    {
        Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x4A, 0x6B)),
        BorderBrush = new SolidColorBrush(Color.FromRgb(0x4D, 0x9E, 0xFF)),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(6),
        Padding = new Thickness(14, 10),
        Child = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                new TextBlock { Text = node.Title, Foreground = Brushes.White, FontWeight = FontWeight.SemiBold },
                new TextBlock { Text = node.Status, Foreground = Brushes.LightGray, FontSize = 11 },
            },
        },
    });

    private static NodelyDiagram NewDiagram()
    {
        var diagram = new NodelyDiagram();
        diagram.Options.GridSize = 24;
        diagram.Options.Groups.Enabled = true;
        diagram.Options.Links.DefaultTargetMarker = LinkMarker.Arrow;
        return diagram;
    }

    private Control BuildWorkflow()
    {
        var diagram = NewDiagram();
        var start = diagram.Nodes.Add(new NodeModel(new NodelyPoint(120, 230)) { Title = "Start" });
        var build = diagram.Nodes.Add(new TaskNode(new NodelyPoint(420, 160), "Build") { Status = "Running" });
        var test = diagram.Nodes.Add(new TaskNode(new NodelyPoint(690, 160), "Test") { Status = "Queued" });
        var deploy = diagram.Nodes.Add(new TaskNode(new NodelyPoint(420, 330), "Deploy") { Status = "Pending" });

        var startOut = start.AddPort(PortAlignment.Right);
        var buildLink = diagram.Links.Add(new LinkModel(startOut, build.AddPort(PortAlignment.Left))) as LinkModel;
        buildLink!.Segmentable = true;
        buildLink.AddVertex(new NodelyPoint(300, 120));
        diagram.Links.Add(new LinkModel(build.AddPort(PortAlignment.Right), test.AddPort(PortAlignment.Left))).AddLabel("green");
        var deployLink = diagram.Links.Add(new LinkModel(startOut, deploy.AddPort(PortAlignment.Left))) as LinkModel;
        deployLink!.Segmentable = true;
        deployLink.SourceMarker = LinkMarker.Circle;
        diagram.Groups.Group(build, deploy);

        return Editor(diagram);
    }

    private Control BuildStateMachine()
    {
        var diagram = NewDiagram();
        var idle = diagram.Nodes.Add(new NodeModel(new NodelyPoint(0, 0)) { Title = "Idle" });
        var running = diagram.Nodes.Add(new NodeModel(new NodelyPoint(0, 0)) { Title = "Running" });
        var done = diagram.Nodes.Add(new NodeModel(new NodelyPoint(0, 0)) { Title = "Done" });
        var error = diagram.Nodes.Add(new NodeModel(new NodelyPoint(0, 0)) { Title = "Error" });

        diagram.Links.Add(new LinkModel(idle, running)).AddLabel("start");
        diagram.Links.Add(new LinkModel(running, done)).AddLabel("ok");
        diagram.Links.Add(new LinkModel(running, error)).AddLabel("fail");
        diagram.Links.Add(new LinkModel(error, idle)).AddLabel("reset");
        LayeredLayout.Arrange(diagram);

        return Editor(diagram);
    }

    private Control BuildInspector()
    {
        var diagram = NewDiagram();
        var a = diagram.Nodes.Add(new TaskNode(new NodelyPoint(120, 180), "Ingest") { Status = "Done" });
        var b = diagram.Nodes.Add(new TaskNode(new NodelyPoint(430, 180), "Transform") { Status = "Done" });
        var c = diagram.Nodes.Add(new TaskNode(new NodelyPoint(740, 180), "Load") { Status = "Done" });
        diagram.Links.Add(new LinkModel(a.AddPort(PortAlignment.Right), b.AddPort(PortAlignment.Left)));
        diagram.Links.Add(new LinkModel(b.AddPort(PortAlignment.Right), c.AddPort(PortAlignment.Left)));

        return Editor(diagram, readOnly: true);
    }

    private Control BuildExtensibility()
    {
        var diagram = NewDiagram();
        var source = diagram.Nodes.Add(new TaskNode(new NodelyPoint(130, 210), "Source") { Status = "Live" });
        var route = diagram.Nodes.Add(new TaskNode(new NodelyPoint(430, 140), "Route") { Status = "Balanced" });
        var sink = diagram.Nodes.Add(new TaskNode(new NodelyPoint(740, 210), "Sink") { Status = "Healthy" });

        var sourcePort = source.AddPort(new SignalPort(source, PortAlignment.Right, "out"));
        var routeIn = route.AddPort(new SignalPort(route, PortAlignment.Left, "in"));
        var routeOut = route.AddPort(new SignalPort(route, PortAlignment.Right, "out"));
        var sinkPort = sink.AddPort(new SignalPort(sink, PortAlignment.Left, "in"));

        diagram.Links.Add(new FlowLink(sourcePort, routeIn) { Critical = true, Segmentable = true }).AddLabel("custom drawer");
        diagram.Links.Add(new FlowLink(routeOut, sinkPort)).AddLabel("style resolver");
        diagram.Groups.Add(new HighlightGroup(new[] { source, route, sink }, "Pipeline"));

        return Editor(diagram, configureCanvas: canvas =>
        {
            canvas.RegisterPort<SignalPort>(port => new Border
            {
                Width = 14,
                Height = 14,
                CornerRadius = new CornerRadius(7),
                Background = port.Role == "out" ? Brushes.DeepSkyBlue : Brushes.MediumSeaGreen,
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(1),
            });
            canvas.RegisterGroup<HighlightGroup>(group => new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(34, 77, 158, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(77, 158, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
            });
            canvas.RegisterLink<FlowLink>((context, ctx) =>
            {
                ctx.DrawDefault();
                if (((FlowLink)ctx.Link).Critical)
                    context.DrawGeometry(null, new Pen(new SolidColorBrush(Color.FromArgb(110, 255, 208, 90)), 7), ctx.Geometry);
            });
            canvas.RegisterLinkStyle<FlowLink>(link => link.Critical
                ? LinkStyle.Default
                : new LinkStyle { Stroke = Brushes.MediumSeaGreen, DashStyle = DashStyle.Dash, Width = 2.5 });
            canvas.AddLayer(new GuideLayer());
        });
    }

    private Control BuildDatabase()
    {
        var diagram = NewDiagram();

        var customers = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(120, 160), "Customers", "sales"));
        customers.Columns.Add(new DatabaseColumn("CustomerId", "int", isPrimaryKey: true, isNullable: false));
        customers.Columns.Add(new DatabaseColumn("Name", "nvarchar(120)", isNullable: false));
        customers.Columns.Add(new DatabaseColumn("Email", "nvarchar(180)"));

        var orders = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(450, 130), "Orders", "sales"));
        orders.Columns.Add(new DatabaseColumn("OrderId", "int", isPrimaryKey: true, isNullable: false));
        orders.Columns.Add(new DatabaseColumn("CustomerId", "int", isNullable: false) { IsForeignKey = true });
        orders.Columns.Add(new DatabaseColumn("Total", "decimal(12,2)", isNullable: false));
        orders.Columns.Add(new DatabaseColumn("CreatedAt", "datetime2", isNullable: false));

        var summary = diagram.Nodes.Add(new DatabaseViewNode(new NodelyPoint(780, 150), "CustomerOrderSummary", "reporting"));
        summary.Columns.Add(new DatabaseColumn("CustomerId", "int"));
        summary.Columns.Add(new DatabaseColumn("OrderCount", "int"));
        summary.Columns.Add(new DatabaseColumn("TotalSales", "decimal(12,2)"));

        var refresh = diagram.Nodes.Add(new DatabaseProcedureNode(new NodelyPoint(450, 390), "RefreshCustomerSummary", "reporting"));
        refresh.Parameters.Add(new DatabaseParameter("@customerId", "int"));
        refresh.Parameters.Add(new DatabaseParameter("@fromDate", "datetime2"));

        var customersOut = customers.AddPort(new DatabasePortModel(customers, PortAlignment.Right, DatabasePortKind.Relationship, "CustomerId"));
        var ordersIn = orders.AddPort(new DatabasePortModel(orders, PortAlignment.Left, DatabasePortKind.Relationship, "CustomerId"));
        var ordersOut = orders.AddPort(new DatabasePortModel(orders, PortAlignment.Right, DatabasePortKind.Dependency));
        var summaryIn = summary.AddPort(new DatabasePortModel(summary, PortAlignment.Left, DatabasePortKind.Dependency));
        var refreshOut = refresh.AddPort(new DatabasePortModel(refresh, PortAlignment.Top, DatabasePortKind.Output));
        var ordersBottom = orders.AddPort(new DatabasePortModel(orders, PortAlignment.Bottom, DatabasePortKind.Input));

        var relationship = diagram.Links.Add(new DatabaseRelationshipLink(customersOut, ordersIn, RelationshipKind.OneToMany)
        {
            SourceCardinality = "1",
            TargetCardinality = "many",
        });
        relationship.AddLabel("customer orders", 0.5, new NodelyPoint(0, -16));

        var viewDependency = diagram.Links.Add(new DatabaseRelationshipLink(ordersOut, summaryIn, RelationshipKind.Dependency));
        viewDependency.AddLabel("feeds view", 0.5, new NodelyPoint(0, 14));

        var procedureDependency = diagram.Links.Add(new DatabaseRelationshipLink(refreshOut, ordersBottom, RelationshipKind.Dependency));
        procedureDependency.AddLabel("refresh reads", 0.5, new NodelyPoint(0, 14));

        return Editor(diagram, configureCanvas: canvas => canvas.UseDatabaseNodes());
    }

    private Control BuildUml()
    {
        var diagram = NewDiagram();

        var domain = diagram.Nodes.Add(new UmlPackageNode(new NodelyPoint(80, 80), "Sales.Domain"));

        var customer = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(120, 180), "Customer")
        {
            IsAbstract = true,
        });
        customer.Stereotypes.Add("entity");
        customer.Members.Add(new UmlMember("Id", "Guid", UmlVisibility.Public));
        customer.Members.Add(new UmlMember("Name", "string", UmlVisibility.Private));
        var rename = new UmlOperation("Rename", "void");
        rename.Parameters.Add(new UmlParameter("name", "string"));
        customer.Operations.Add(rename);

        var preferred = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(120, 430), "PreferredCustomer"));
        preferred.Members.Add(new UmlMember("DiscountRate", "decimal"));

        var order = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(450, 180), "Order"));
        order.Members.Add(new UmlMember("OrderId", "Guid"));
        order.Members.Add(new UmlMember("Status", "OrderStatus"));
        order.Operations.Add(new UmlOperation("Submit", "void"));

        var status = diagram.Nodes.Add(new UmlEnumNode(new NodelyPoint(780, 170), "OrderStatus"));
        status.Literals.Add("Draft");
        status.Literals.Add("Submitted");
        status.Literals.Add("Cancelled");

        var repository = diagram.Nodes.Add(new UmlInterfaceNode(new NodelyPoint(780, 410), "IOrderRepository"));
        repository.Operations.Add(new UmlOperation("Get", "Order"));
        repository.Operations.Add(new UmlOperation("Save", "void"));

        var sqlRepository = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(450, 430), "SqlOrderRepository"));
        sqlRepository.Stereotypes.Add("adapter");
        sqlRepository.Operations.Add(new UmlOperation("Save", "void"));

        var note = diagram.Nodes.Add(new UmlNoteNode(new NodelyPoint(1060, 180), "Structural UML nodes are plain Nodely models and serialize with the UML registry."));

        diagram.Links.Add(new UmlRelationshipLink(preferred, customer, UmlRelationshipKind.Inheritance)
        {
            Label = "inherits",
        });
        diagram.Links.Add(new UmlRelationshipLink(customer, order, UmlRelationshipKind.Aggregation)
        {
            SourceMultiplicity = "1",
            TargetMultiplicity = "0..*",
            Label = "places",
        });
        diagram.Links.Add(new UmlRelationshipLink(order, status, UmlRelationshipKind.Association)
        {
            Label = "status",
        });
        diagram.Links.Add(new UmlRelationshipLink(sqlRepository, repository, UmlRelationshipKind.Realization)
        {
            Label = "implements",
        });
        diagram.Links.Add(new UmlRelationshipLink(sqlRepository, order, UmlRelationshipKind.Dependency)
        {
            Label = "persists",
        });
        diagram.Links.Add(new UmlRelationshipLink(domain, customer, UmlRelationshipKind.Dependency));
        diagram.Links.Add(new UmlRelationshipLink(note, order, UmlRelationshipKind.Dependency)
        {
            Label = "documents",
        });

        return Editor(diagram, configureCanvas: canvas => canvas.UseUmlNodes());
    }
}
