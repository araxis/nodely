using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Nodely;
using Nodely.Algorithms;
using Nodely.Anchors;
using Nodely.Avalonia;
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Database;
using Nodely.Avalonia.MindMap;
using Nodely.Avalonia.Uml;
using Nodely.Avalonia.Workflow;
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
    private RuntimePropertyInspector? _propertyInspector;
    private string? _savedJson;

    public MainWindow()
    {
        Title = "Nodely Gallery";
        Width = 1180;
        Height = 760;

        var scenes = new WrapPanel { Margin = new Thickness(10, 10, 10, 4) };
        AddScene(SceneButton("Workflow", BuildWorkflow));
        AddScene(SceneButton("State machine", BuildStateMachine));
        AddScene(SceneButton("Inspector", BuildInspector));
        AddScene(SceneButton("Extensibility", BuildExtensibility));
        AddScene(SceneButton("Database", BuildDatabase));
        AddScene(SceneButton("UML", BuildUml));
        AddScene(SceneButton("MindMap", BuildMindMap));
        AddScene(new Border { Width = 24 });
        AddScene(ToolButton("Theme", ToggleTheme));
        AddScene(ToolButton("Save", Save));
        AddScene(ToolButton("Load", Load));

        void AddScene(Control control)
        {
            control.Margin = new Thickness(0, 0, 6, 6);
            scenes.Children.Add(control);
        }

        var root = new DockPanel();
        DockPanel.SetDock(scenes, Dock.Top);
        root.Children.Add(scenes);
        root.Children.Add(_host);
        Content = root;

        _host.Content = BuildWorkflow();
    }

    private Button SceneButton(string text, Func<Control> build)
    {
        var button = new Button
        {
            Content = text,
            MinWidth = 72,
            FontSize = 14,
            Padding = new Thickness(12, 6),
        };
        button.Click += (_, _) => _host.Content = build();
        return button;
    }

    private static Button ToolButton(string text, Action onClick)
    {
        var button = new Button
        {
            Content = text,
            MinWidth = 40,
            FontSize = 14,
            Padding = new Thickness(10, 6),
        };
        button.Click += (_, _) => onClick();
        return button;
    }

    private void ToggleTheme()
    {
        _palette = _palette == NodelyPalettes.Dark ? NodelyPalettes.Light : NodelyPalettes.Dark;
        if (_currentCanvas != null)
            _currentCanvas.Palette = _palette;
        _propertyInspector?.Refresh();
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
        _host.Content = Editor(diagram, configureCanvas: canvas => canvas.UseDatabaseNodes().UseMindMapNodes().UseUmlNodes().UseWorkflowNodes());
    }

    private static DiagramSerializationRegistry CreateSerializationRegistry() => DatabaseNodeFactory.CreateRegistry()
        .UseMindMapNodes()
        .UseUmlNodes()
        .UseWorkflowNodes()
        .RegisterNode(TaskNode.ModelKindKey, ns => new TaskNode(ns.Id, new NodelyPoint(ns.X, ns.Y), ns.Title ?? string.Empty))
        .RegisterPort(SignalPort.ModelKindKey, (ps, parent) =>
            new SignalPort(ps.Id, parent, Enum.Parse<PortAlignment>(ps.Alignment)))
        .RegisterLink(FlowLink.ModelKindKey, (ls, source, target) => new FlowLink(ls.Id, source, target));

    private Control Editor(
        NodelyDiagram diagram,
        bool readOnly = false,
        Action<DiagramCanvas>? configureCanvas = null,
        Action<DiagramCanvas, NodelyDiagram>? layoutAction = null)
    {
        _propertyInspector?.Dispose();
        _propertyInspector = null;
        _currentDiagram = diagram;

        var canvas = new DiagramCanvas { Diagram = diagram, Palette = _palette, IsReadOnly = readOnly };
        canvas.AttachedToVisualTree += (_, _) =>
            Dispatcher.UIThread.Post(() => canvas.ZoomToFit(48), DispatcherPriority.Loaded);
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

        var surface = new Grid { Children = { canvas, navigator } };
        var inspector = new RuntimePropertyInspector(canvas, diagram, readOnly);
        _propertyInspector = inspector;
        var body = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,340"),
            Children = { surface, inspector.View },
        };
        Grid.SetColumn(inspector.View, 1);

        var toolbar = BuildEditorToolbar(canvas, diagram, readOnly, layoutAction);
        var editor = new DockPanel();
        DockPanel.SetDock(toolbar, Dock.Top);
        editor.Children.Add(toolbar);
        editor.Children.Add(body);
        return editor;
    }

    private StackPanel BuildEditorToolbar(
        DiagramCanvas canvas,
        NodelyDiagram diagram,
        bool readOnly,
        Action<DiagramCanvas, NodelyDiagram>? layoutAction)
    {
        var bar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8),
        };

        bar.Children.Add(ToolButton("+", canvas.ZoomIn));
        bar.Children.Add(ToolButton("-", canvas.ZoomOut));
        bar.Children.Add(ToolButton("Fit", () => canvas.ZoomToFit()));

        layoutAction ??= (targetCanvas, targetDiagram) =>
        {
            targetCanvas.RunAsUndoableMove(() => LayeredLayout.Arrange(targetDiagram));
            targetCanvas.ZoomToFit();
        };
        var layout = ToolButton("Layout", () => layoutAction(canvas, diagram));
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
        var start = diagram.Nodes.Add(new WorkflowStartNode(new NodelyPoint(100, 250), "Request received"));
        var triage = diagram.Nodes.Add(new WorkflowTaskNode(new NodelyPoint(340, 170), "Triage")
        {
            TaskType = WorkflowTaskType.User,
            Status = WorkflowTaskStatus.Ready,
            Notes = "Assign owner",
        });
        var decision = diagram.Nodes.Add(new WorkflowDecisionNode(new NodelyPoint(610, 170), "Valid request?")
        {
            Condition = "required fields present",
        });
        var service = diagram.Nodes.Add(new WorkflowTaskNode(new NodelyPoint(880, 130), "Provision access")
        {
            TaskType = WorkflowTaskType.Service,
            Status = WorkflowTaskStatus.Running,
        });
        var gateway = diagram.Nodes.Add(new WorkflowGatewayNode(new NodelyPoint(880, 340), "Notify")
        {
            GatewayKind = WorkflowGatewayKind.Parallel,
        });
        var timeout = diagram.Nodes.Add(new WorkflowEventNode(new NodelyPoint(610, 380), "SLA timer")
        {
            EventKind = WorkflowEventKind.Timer,
        });
        var done = diagram.Nodes.Add(new WorkflowEndNode(new NodelyPoint(1140, 210), "Complete"));
        var note = diagram.Nodes.Add(new WorkflowNoteNode(new NodelyPoint(100, 430), "Workflow pack nodes are models/renderers only. Execution stays in the host app."));

        diagram.Links.Add(new WorkflowLink(start, triage, WorkflowLinkKind.Sequence) { Label = "submit" });
        diagram.Links.Add(new WorkflowLink(triage, decision, WorkflowLinkKind.Conditional)
        {
            Label = "check",
            Condition = "valid",
        });
        diagram.Links.Add(new WorkflowLink(decision, service, WorkflowLinkKind.Sequence) { Label = "yes" });
        diagram.Links.Add(new WorkflowLink(decision, timeout, WorkflowLinkKind.Error) { Label = "missing data" });
        diagram.Links.Add(new WorkflowLink(service, gateway, WorkflowLinkKind.Message) { Label = "notify" });
        diagram.Links.Add(new WorkflowLink(gateway, done, WorkflowLinkKind.Sequence) { Label = "finish" });
        diagram.Links.Add(new WorkflowLink(timeout, triage, WorkflowLinkKind.Message) { Label = "retry" });

        return Editor(diagram, configureCanvas: canvas => canvas.UseWorkflowNodes());
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

        var customers = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(90, 150), "Customers", "sales"));
        customers.Columns.Add(new DatabaseColumn("CustomerId", "int", isPrimaryKey: true, isNullable: false));
        customers.Columns.Add(new DatabaseColumn("Name", "nvarchar(120)", isNullable: false));
        customers.Columns.Add(new DatabaseColumn("Email", "nvarchar(180)"));
        customers.Columns.Add(new DatabaseColumn("CreatedAt", "datetime2", isNullable: false));

        var orders = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(420, 120), "Orders", "sales"));
        orders.Columns.Add(new DatabaseColumn("OrderId", "int", isPrimaryKey: true, isNullable: false));
        orders.Columns.Add(new DatabaseColumn("CustomerId", "int", isNullable: false) { IsForeignKey = true });
        orders.Columns.Add(new DatabaseColumn("Status", "nvarchar(24)", isNullable: false));
        orders.Columns.Add(new DatabaseColumn("Total", "decimal(12,2)", isNullable: false));
        orders.Columns.Add(new DatabaseColumn("CreatedAt", "datetime2", isNullable: false));

        var lines = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(760, 150), "OrderLines", "sales"));
        lines.Columns.Add(new DatabaseColumn("OrderLineId", "int", isPrimaryKey: true, isNullable: false));
        lines.Columns.Add(new DatabaseColumn("OrderId", "int", isNullable: false) { IsForeignKey = true });
        lines.Columns.Add(new DatabaseColumn("ProductId", "int", isNullable: false) { IsForeignKey = true });
        lines.Columns.Add(new DatabaseColumn("Quantity", "int", isNullable: false));
        lines.Columns.Add(new DatabaseColumn("UnitPrice", "decimal(12,2)", isNullable: false));

        var products = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(1090, 150), "Products", "catalog"));
        products.Columns.Add(new DatabaseColumn("ProductId", "int", isPrimaryKey: true, isNullable: false));
        products.Columns.Add(new DatabaseColumn("Sku", "nvarchar(48)", isNullable: false));
        products.Columns.Add(new DatabaseColumn("Name", "nvarchar(160)", isNullable: false));
        products.Columns.Add(new DatabaseColumn("IsActive", "bit", isNullable: false));

        var summary = diagram.Nodes.Add(new DatabaseViewNode(new NodelyPoint(760, 470), "CustomerOrderSummary", "reporting"));
        summary.Columns.Add(new DatabaseColumn("CustomerId", "int"));
        summary.Columns.Add(new DatabaseColumn("OrderCount", "int"));
        summary.Columns.Add(new DatabaseColumn("LineCount", "int"));
        summary.Columns.Add(new DatabaseColumn("TotalSales", "decimal(12,2)"));

        var refresh = diagram.Nodes.Add(new DatabaseProcedureNode(new NodelyPoint(420, 500), "RefreshCustomerSummary", "reporting"));
        refresh.Parameters.Add(new DatabaseParameter("@customerId", "int"));
        refresh.Parameters.Add(new DatabaseParameter("@fromDate", "datetime2"));
        refresh.Parameters.Add(new DatabaseParameter("@includeInactive", "bit"));

        var customersOut = customers.AddPort(new DatabasePortModel(customers, PortAlignment.Right, DatabasePortKind.Relationship, "CustomerId"));
        var ordersIn = orders.AddPort(new DatabasePortModel(orders, PortAlignment.Left, DatabasePortKind.Relationship, "CustomerId"));
        var ordersOut = orders.AddPort(new DatabasePortModel(orders, PortAlignment.Right, DatabasePortKind.Relationship, "OrderId"));
        var linesOrderIn = lines.AddPort(new DatabasePortModel(lines, PortAlignment.Left, DatabasePortKind.Relationship, "OrderId"));
        var linesProductOut = lines.AddPort(new DatabasePortModel(lines, PortAlignment.Right, DatabasePortKind.Relationship, "ProductId"));
        var productsIn = products.AddPort(new DatabasePortModel(products, PortAlignment.Left, DatabasePortKind.Relationship, "ProductId"));
        var ordersDependency = orders.AddPort(new DatabasePortModel(orders, PortAlignment.Bottom, DatabasePortKind.Dependency));
        var linesDependency = lines.AddPort(new DatabasePortModel(lines, PortAlignment.Bottom, DatabasePortKind.Dependency));
        var summaryOrdersIn = summary.AddPort(new DatabasePortModel(summary, PortAlignment.Left, DatabasePortKind.Dependency, "OrderCount"));
        var summaryLinesIn = summary.AddPort(new DatabasePortModel(summary, PortAlignment.Right, DatabasePortKind.Dependency, "LineCount"));
        var refreshOut = refresh.AddPort(new DatabasePortModel(refresh, PortAlignment.Right, DatabasePortKind.Output));
        var summaryRefreshIn = summary.AddPort(new DatabasePortModel(summary, PortAlignment.Bottom, DatabasePortKind.Input));

        var relationship = diagram.Links.Add(new DatabaseRelationshipLink(customersOut, ordersIn, RelationshipKind.OneToMany)
        {
            SourceCardinality = "1",
            TargetCardinality = "many",
        });
        relationship.AddLabel("customer orders", 0.5, new NodelyPoint(0, -16));

        var orderLines = diagram.Links.Add(new DatabaseRelationshipLink(ordersOut, linesOrderIn, RelationshipKind.OneToMany)
        {
            SourceCardinality = "1",
            TargetCardinality = "many",
        });
        orderLines.AddLabel("order lines", 0.5, new NodelyPoint(0, -16));

        var productLines = diagram.Links.Add(new DatabaseRelationshipLink(productsIn, linesProductOut, RelationshipKind.OneToMany)
        {
            SourceCardinality = "1",
            TargetCardinality = "many",
        });
        productLines.AddLabel("product lines", 0.5, new NodelyPoint(0, -16));

        var orderSummary = diagram.Links.Add(new DatabaseRelationshipLink(ordersDependency, summaryOrdersIn, RelationshipKind.Dependency));
        orderSummary.AddLabel("feeds view", 0.5, new NodelyPoint(0, 14));

        var lineSummary = diagram.Links.Add(new DatabaseRelationshipLink(linesDependency, summaryLinesIn, RelationshipKind.Dependency));
        lineSummary.AddLabel("line totals", 0.5, new NodelyPoint(0, 14));

        var procedureDependency = diagram.Links.Add(new DatabaseRelationshipLink(refreshOut, summaryRefreshIn, RelationshipKind.Dependency));
        procedureDependency.AddLabel("refreshes", 0.5, new NodelyPoint(0, 14));

        return Editor(diagram, configureCanvas: canvas => canvas.UseDatabaseNodes());
    }

    private Control BuildUml()
    {
        var diagram = NewDiagram();

        var domain = diagram.Nodes.Add(new UmlPackageNode(new NodelyPoint(80, 80), "Sales.Domain"));
        domain.Stereotypes.Add("bounded context");

        var customer = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(130, 210), "Customer")
        {
            IsAbstract = true,
        });
        customer.Stereotypes.Add("entity");
        customer.Members.Add(new UmlMember("Id", "Guid", UmlVisibility.Public));
        customer.Members.Add(new UmlMember("Name", "string", UmlVisibility.Private));
        customer.Members.Add(new UmlMember("CreatedAt", "Instant", UmlVisibility.Protected));
        var rename = new UmlOperation("Rename", "void");
        rename.Parameters.Add(new UmlParameter("name", "string"));
        customer.Operations.Add(rename);

        var preferred = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(130, 540), "PreferredCustomer"));
        preferred.Stereotypes.Add("specialization");
        preferred.Members.Add(new UmlMember("DiscountRate", "decimal", UmlVisibility.Private));
        preferred.Operations.Add(new UmlOperation("ApplyDiscount", "Money"));

        var order = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(560, 210), "Order"));
        order.Stereotypes.Add("aggregate");
        order.Members.Add(new UmlMember("OrderId", "Guid"));
        order.Members.Add(new UmlMember("CustomerId", "Guid", UmlVisibility.Private));
        order.Members.Add(new UmlMember("Status", "OrderStatus"));
        var submit = new UmlOperation("Submit", "void");
        submit.Parameters.Add(new UmlParameter("at", "Instant"));
        order.Operations.Add(submit);

        var status = diagram.Nodes.Add(new UmlEnumNode(new NodelyPoint(990, 220), "OrderStatus"));
        status.Literals.Add("Draft");
        status.Literals.Add("Submitted");
        status.Literals.Add("Cancelled");

        var repository = diagram.Nodes.Add(new UmlInterfaceNode(new NodelyPoint(990, 500), "IOrderRepository"));
        repository.Operations.Add(new UmlOperation("Get", "Order"));
        repository.Operations.Add(new UmlOperation("Save", "void"));

        var sqlRepository = diagram.Nodes.Add(new UmlClassNode(new NodelyPoint(560, 540), "SqlOrderRepository"));
        sqlRepository.Stereotypes.Add("adapter");
        sqlRepository.Members.Add(new UmlMember("_connection", "IDbConnection", UmlVisibility.Private));
        sqlRepository.Operations.Add(new UmlOperation("Get", "Order"));
        sqlRepository.Operations.Add(new UmlOperation("Save", "void"));

        var note = diagram.Nodes.Add(new UmlNoteNode(new NodelyPoint(1370, 250), "Ports attach to UML members, operations, literals, and relationship roles."));

        var packageOut = domain.AddPort(new UmlPortModel(domain, PortAlignment.Bottom, UmlPortKind.Dependency));
        var customerPackage = customer.AddPort(new UmlPortModel(customer, PortAlignment.Top, UmlPortKind.Dependency));
        var customerBase = customer.AddPort(new UmlPortModel(customer, PortAlignment.Bottom, UmlPortKind.Inheritance));
        var preferredBase = preferred.AddPort(new UmlPortModel(preferred, PortAlignment.Top, UmlPortKind.Inheritance));
        var customerOrders = customer.AddPort(new UmlPortModel(customer, PortAlignment.Right, UmlPortKind.Aggregation, "Id"));
        var orderCustomer = order.AddPort(new UmlPortModel(order, PortAlignment.Left, UmlPortKind.Aggregation, "CustomerId"));
        var orderStatus = order.AddPort(new UmlPortModel(order, PortAlignment.Right, UmlPortKind.Association, "Status"));
        var statusLiteral = status.AddPort(new UmlPortModel(status, PortAlignment.Left, UmlPortKind.Association, "Submitted"));
        var sqlRealization = sqlRepository.AddPort(new UmlPortModel(sqlRepository, PortAlignment.Top, UmlPortKind.Realization, "Save"));
        var repoRealization = repository.AddPort(new UmlPortModel(repository, PortAlignment.Bottom, UmlPortKind.Realization, "Save"));
        var sqlDependency = sqlRepository.AddPort(new UmlPortModel(sqlRepository, PortAlignment.Right, UmlPortKind.Dependency, "Get"));
        var orderDependency = order.AddPort(new UmlPortModel(order, PortAlignment.Bottom, UmlPortKind.Dependency, "Submit"));
        var noteDependency = note.AddPort(new UmlPortModel(note, PortAlignment.Left, UmlPortKind.Dependency));
        var orderNote = order.AddPort(new UmlPortModel(order, PortAlignment.Right, UmlPortKind.Dependency, "OrderId"));

        diagram.Links.Add(new UmlRelationshipLink(preferredBase, customerBase, UmlRelationshipKind.Inheritance)
        {
            Label = "inherits",
        });
        diagram.Links.Add(new UmlRelationshipLink(customerOrders, orderCustomer, UmlRelationshipKind.Aggregation)
        {
            SourceMultiplicity = "1",
            TargetMultiplicity = "0..*",
            Label = "places",
        });
        diagram.Links.Add(new UmlRelationshipLink(orderStatus, statusLiteral, UmlRelationshipKind.Association)
        {
            Label = "status",
        });
        diagram.Links.Add(new UmlRelationshipLink(sqlRealization, repoRealization, UmlRelationshipKind.Realization)
        {
            Label = "implements",
        });
        diagram.Links.Add(new UmlRelationshipLink(sqlDependency, orderDependency, UmlRelationshipKind.Dependency)
        {
            Label = "persists",
        });
        diagram.Links.Add(new UmlRelationshipLink(packageOut, customerPackage, UmlRelationshipKind.Dependency)
        {
            Label = "contains",
        });
        diagram.Links.Add(new UmlRelationshipLink(noteDependency, orderNote, UmlRelationshipKind.Dependency)
        {
            Label = "documents",
        });

        return Editor(diagram, configureCanvas: canvas => canvas.UseUmlNodes());
    }

    private Control BuildMindMap()
    {
        var diagram = NewDiagram();

        var root = diagram.Nodes.Add(new MindMapRootNode(new NodelyPoint(0, 0), "Nodely 0.8 planning")
        {
            Notes = "Adoption polish after side packs",
            IconKey = "plan",
            AccentColor = "#4D9EFF",
        });

        var evaluate = diagram.Nodes.Add(new MindMapBranchNode(new NodelyPoint(0, 0), "Evaluate")
        {
            Notes = "Copyable samples and docs paths",
            IconKey = "E",
            AccentColor = "#37A779",
            Side = MindMapTopicSide.Right,
        });
        var runtime = diagram.Nodes.Add(new MindMapBranchNode(new NodelyPoint(0, 0), "Runtime editing")
        {
            Notes = "Inspector updates model metadata",
            IconKey = "R",
            AccentColor = "#D89C35",
            Side = MindMapTopicSide.Right,
        });
        var questions = diagram.Nodes.Add(new MindMapBranchNode(new NodelyPoint(0, 0), "Open questions")
        {
            Notes = "Keep small, decide quickly",
            IconKey = "?",
            AccentColor = "#9779CD",
            Side = MindMapTopicSide.Left,
            Collapsed = true,
        });
        var quality = diagram.Nodes.Add(new MindMapBranchNode(new NodelyPoint(0, 0), "Quality")
        {
            Notes = "Build, test, pack, docs",
            IconKey = "Q",
            AccentColor = "#3A9DAA",
            Side = MindMapTopicSide.Left,
        });

        var quickstart = diagram.Nodes.Add(new MindMapLeafNode(new NodelyPoint(0, 0), "QuickStart"));
        var recipes = diagram.Nodes.Add(new MindMapLeafNode(new NodelyPoint(0, 0), "Recipes"));
        var database = diagram.Nodes.Add(new MindMapLeafNode(new NodelyPoint(0, 0), "Database editor"));
        var uml = diagram.Nodes.Add(new MindMapLeafNode(new NodelyPoint(0, 0), "UML editor"));
        var versioning = diagram.Nodes.Add(new MindMapLeafNode(new NodelyPoint(0, 0), "Version policy"));
        var layout = diagram.Nodes.Add(new MindMapLeafNode(new NodelyPoint(0, 0), "Layout rules"));
        var headless = diagram.Nodes.Add(new MindMapLeafNode(new NodelyPoint(0, 0), "Headless tests"));
        var packageCheck = diagram.Nodes.Add(new MindMapLeafNode(new NodelyPoint(0, 0), "Package check"));

        AddBranch(root, evaluate, PortAlignment.TopRight, PortAlignment.Left, "#37A779", "scope");
        AddBranch(root, runtime, PortAlignment.BottomRight, PortAlignment.Left, "#D89C35", "edit");
        AddBranch(root, questions, PortAlignment.TopLeft, PortAlignment.Right, "#9779CD", "decide");
        AddBranch(root, quality, PortAlignment.BottomLeft, PortAlignment.Right, "#3A9DAA", "verify");

        AddBranch(evaluate, quickstart, PortAlignment.Right, PortAlignment.Left, "#37A779");
        AddBranch(evaluate, recipes, PortAlignment.Right, PortAlignment.Left, "#37A779");
        AddBranch(runtime, database, PortAlignment.Right, PortAlignment.Left, "#D89C35");
        AddBranch(runtime, uml, PortAlignment.Right, PortAlignment.Left, "#D89C35");
        AddBranch(questions, versioning, PortAlignment.Left, PortAlignment.Right, "#9779CD");
        AddBranch(questions, layout, PortAlignment.Left, PortAlignment.Right, "#9779CD");
        AddBranch(quality, headless, PortAlignment.Left, PortAlignment.Right, "#3A9DAA");
        AddBranch(quality, packageCheck, PortAlignment.Left, PortAlignment.Right, "#3A9DAA");

        var runtimeAssociation = runtime.AddPort(new MindMapPortModel(runtime, PortAlignment.Bottom, MindMapPortRole.Association, "runtime"));
        var qualityAssociation = quality.AddPort(new MindMapPortModel(quality, PortAlignment.Top, MindMapPortRole.Association, "quality"));
        diagram.Links.Add(new MindMapLink(runtimeAssociation, qualityAssociation, MindMapLinkKind.Association)
        {
            Label = "verified by",
            AccentColor = "#9779CD",
        });

        MindMapLayout.Arrange(diagram);

        return Editor(
            diagram,
            configureCanvas: canvas => canvas.UseMindMapNodes(),
            layoutAction: (canvas, targetDiagram) =>
            {
                canvas.RunAsUndoableMove(() => MindMapLayout.Arrange(targetDiagram));
                canvas.RefreshVisuals();
                canvas.ZoomToFit();
            });

        void AddBranch(
            MindMapTopicNode source,
            MindMapTopicNode target,
            PortAlignment sourceAlignment,
            PortAlignment targetAlignment,
            string accent,
            string? label = null)
        {
            var sourcePort = source.AddPort(new MindMapPortModel(source, sourceAlignment, MindMapPortRole.Branch, source.Topic));
            var targetPort = target.AddPort(new MindMapPortModel(target, targetAlignment, MindMapPortRole.Branch, target.Topic));
            diagram.Links.Add(new MindMapLink(sourcePort, targetPort, MindMapLinkKind.Branch)
            {
                Label = label,
                AccentColor = accent,
            });
        }
    }
}
