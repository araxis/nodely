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
using Nodely.Avalonia.Api;
using Nodely.Avalonia.Database;
using Nodely.Avalonia.Designer;
using Nodely.Avalonia.MindMap;
using Nodely.Avalonia.Network;
using Nodely.Avalonia.StateMachine;
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
    private DiagramDesignerShell? _currentDesigner;
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
        AddScene(SceneButton("Architecture", BuildArchitecture));
        AddScene(SceneButton("Database", BuildDatabase));
        AddScene(SceneButton("UML", BuildUml));
        AddScene(SceneButton("MindMap", BuildMindMap));
        AddScene(SceneButton("Network", BuildNetwork));
        AddScene(SceneButton("API", BuildApi));
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
        if (_currentDesigner != null)
            _currentDesigner.Palette = _palette;
        else if (_currentCanvas != null)
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
        _host.Content = Editor(diagram, configureCanvas: UseAllSidePackages);
    }

    private static DiagramSerializationRegistry CreateSerializationRegistry() => ApiNodeFactory.CreateRegistry()
        .UseDatabaseNodes()
        .UseMindMapNodes()
        .UseNetworkNodes()
        .UseStateMachineNodes()
        .UseUmlNodes()
        .UseWorkflowNodes()
        .RegisterNode(TaskNode.ModelKindKey, ns => new TaskNode(ns.Id, new NodelyPoint(ns.X, ns.Y), ns.Title ?? string.Empty))
        .RegisterPort(SignalPort.ModelKindKey, (ps, parent) =>
            new SignalPort(ps.Id, parent, Enum.Parse<PortAlignment>(ps.Alignment)))
        .RegisterLink(FlowLink.ModelKindKey, (ls, source, target) => new FlowLink(ls.Id, source, target));

    private static void UseAllSidePackages(DiagramCanvas canvas)
    {
        canvas
            .UseApiNodes()
            .UseDatabaseNodes()
            .UseMindMapNodes()
            .UseNetworkNodes()
            .UseStateMachineNodes()
            .UseUmlNodes()
            .UseWorkflowNodes();
    }

    private Control Editor(
        NodelyDiagram diagram,
        bool readOnly = false,
        Action<DiagramCanvas>? configureCanvas = null,
        Action<DiagramCanvas, NodelyDiagram>? layoutAction = null)
    {
        _currentDesigner?.Dispose();
        _currentDesigner = null;
        _currentDiagram = diagram;

        layoutAction ??= (targetCanvas, targetDiagram) =>
        {
            targetCanvas.RunAsUndoableMove(() => LayeredLayout.Arrange(targetDiagram));
            targetCanvas.ZoomToFit();
        };

        var designer = new DiagramDesignerShell(diagram, new DiagramDesignerOptions
        {
            Palette = _palette,
            IsReadOnly = readOnly,
            PropertyRegistry = CreateDesignerPropertyRegistry(),
            ToolboxSections = CreateToolboxSections(),
            ShowToolbox = !readOnly,
            ConfigureCanvas = canvas =>
            {
                RegisterTaskNode(canvas);
                configureCanvas?.Invoke(canvas);
            },
            LayoutAction = (canvas, targetDiagram) =>
            {
                if (targetDiagram is NodelyDiagram nodelyDiagram)
                    layoutAction(canvas, nodelyDiagram);
            },
        });

        _currentDesigner = designer;
        _currentCanvas = designer.Canvas;
        return designer;
    }

    private static DiagramPropertyRegistry CreateDesignerPropertyRegistry() => DiagramPropertyRegistry.CreateDefault()
        .Register<TaskNode>(
            DiagramProperty.Text<TaskNode>("Status", node => node.Status, (node, value) => node.Status = value ?? string.Empty, "Task"))
        .Register<FlowLink>(
            DiagramProperty.Boolean<FlowLink>("Critical", link => link.Critical, (link, value) => link.Critical = value, "Flow"))
        .Register<ApiNodeBase>(
            DiagramProperty.Text<ApiNodeBase>("Name", node => node.Name, (node, value) => node.Name = value ?? string.Empty, "API node"),
            DiagramProperty.Text<ApiNodeBase>("Version", node => node.Version ?? string.Empty, (node, value) => node.Version = NormalizeOptional(value), "API node"),
            DiagramProperty.Enum<ApiNodeBase, ApiEndpointStatus>("Status", node => node.Status, (node, value) => node.Status = value, "API node"),
            DiagramProperty.Text<ApiNodeBase>("Summary", node => node.Summary ?? string.Empty, (node, value) => node.Summary = NormalizeOptional(value), "API node", multiline: true),
            DiagramProperty.Color<ApiNodeBase>("Accent", node => node.AccentColor, (node, value) => node.AccentColor = value ?? string.Empty, "API node"),
            DiagramProperty.Text<ApiNodeBase>("Icon", node => node.IconKey ?? string.Empty, (node, value) => node.IconKey = NormalizeOptional(value), "API node"))
        .Register<ApiServiceNode>(
            DiagramProperty.Text<ApiServiceNode>("Base URL", node => node.BaseUrl ?? string.Empty, (node, value) => node.BaseUrl = NormalizeOptional(value), "Service"),
            DiagramProperty.Text<ApiServiceNode>("Owner", node => node.Owner ?? string.Empty, (node, value) => node.Owner = NormalizeOptional(value), "Service"))
        .Register<ApiEndpointNode>(
            DiagramProperty.Enum<ApiEndpointNode, ApiEndpointMethod>("Method", node => node.Method, (node, value) => node.Method = value, "Endpoint"),
            DiagramProperty.Text<ApiEndpointNode>("Route", node => node.Route, (node, value) => node.Route = value ?? string.Empty, "Endpoint"),
            DiagramProperty.Text<ApiEndpointNode>("Request", node => node.RequestType ?? string.Empty, (node, value) => node.RequestType = NormalizeOptional(value), "Endpoint"),
            DiagramProperty.Text<ApiEndpointNode>("Response", node => node.ResponseType ?? string.Empty, (node, value) => node.ResponseType = NormalizeOptional(value), "Endpoint"))
        .Register<ApiOperationNode>(
            DiagramProperty.Text<ApiOperationNode>("Input", node => node.Input ?? string.Empty, (node, value) => node.Input = NormalizeOptional(value), "Operation"),
            DiagramProperty.Text<ApiOperationNode>("Output", node => node.Output ?? string.Empty, (node, value) => node.Output = NormalizeOptional(value), "Operation"),
            DiagramProperty.Boolean<ApiOperationNode>("Read only", node => node.SideEffectFree, (node, value) => node.SideEffectFree = value, "Operation"))
        .Register<ApiClientNode>(
            DiagramProperty.Text<ApiClientNode>("Platform", node => node.Platform ?? string.Empty, (node, value) => node.Platform = NormalizeOptional(value), "Client"))
        .Register<ApiGatewayNode>(
            DiagramProperty.Text<ApiGatewayNode>("Host", node => node.Host ?? string.Empty, (node, value) => node.Host = NormalizeOptional(value), "Gateway"))
        .Register<ApiAuthNode>(
            DiagramProperty.Text<ApiAuthNode>("Scheme", node => node.Scheme ?? string.Empty, (node, value) => node.Scheme = NormalizeOptional(value), "Auth"),
            DiagramProperty.Text<ApiAuthNode>("Scopes", node => node.Scopes ?? string.Empty, (node, value) => node.Scopes = NormalizeOptional(value), "Auth"))
        .Register<ApiContractNode>(
            DiagramProperty.Collection<ApiContractNode, ApiContractField>(
                "Fields",
                node => node.Fields,
                node => new ApiContractField("field" + (node.Fields.Count + 1), "string"),
                field => field.Name + ": " + field.Type,
                "Contract",
                "Add field"))
        .Register<DatabaseObjectNode>(
            DiagramProperty.Text<DatabaseObjectNode>("Name", node => node.ObjectName, (node, value) => node.ObjectName = value ?? string.Empty, "Database object"),
            DiagramProperty.Text<DatabaseObjectNode>("Schema", node => node.Schema, (node, value) => node.Schema = value ?? string.Empty, "Database object"))
        .Register<DatabaseTableNode>(
            DiagramProperty.Collection<DatabaseTableNode, DatabaseColumn>(
                "Columns",
                node => node.Columns,
                node => new DatabaseColumn("Column" + (node.Columns.Count + 1), "nvarchar(50)"),
                FormatColumn,
                "Columns",
                "Add column"))
        .Register<DatabaseViewNode>(
            DiagramProperty.Collection<DatabaseViewNode, DatabaseColumn>(
                "Columns",
                node => node.Columns,
                node => new DatabaseColumn("Column" + (node.Columns.Count + 1), "nvarchar(50)"),
                FormatColumn,
                "Columns",
                "Add column"))
        .Register<DatabaseProcedureNode>(
            DiagramProperty.Collection<DatabaseProcedureNode, DatabaseParameter>(
                "Parameters",
                node => node.Parameters,
                node => new DatabaseParameter("@parameter" + (node.Parameters.Count + 1), "int"),
                parameter => parameter.Name + ": " + parameter.DataType,
                "Parameters",
                "Add parameter"))
        .Register<UmlNodeBase>(
            DiagramProperty.Text<UmlNodeBase>("Name", node => node.Name, (node, value) => node.Name = value ?? string.Empty, "UML element"),
            DiagramProperty.Text<UmlNodeBase>("Stereotypes", node => string.Join(", ", node.Stereotypes), (node, value) => ReplaceValues(node.Stereotypes, SplitComma(value)), "UML element"))
        .Register<UmlClassNode>(
            DiagramProperty.Boolean<UmlClassNode>("Abstract", node => node.IsAbstract, (node, value) => node.IsAbstract = value, "Class"),
            DiagramProperty.Boolean<UmlClassNode>("Static", node => node.IsStatic, (node, value) => node.IsStatic = value, "Class"),
            DiagramProperty.Collection<UmlClassNode, UmlMember>("Members", node => node.Members, node => new UmlMember("Member" + (node.Members.Count + 1), "string"), member => member.Name + ": " + member.Type, "Members", "Add member"),
            DiagramProperty.Collection<UmlClassNode, UmlOperation>("Operations", node => node.Operations, node => new UmlOperation("Operation" + (node.Operations.Count + 1)), operation => operation.Name + "()", "Operations", "Add operation"))
        .Register<UmlInterfaceNode>(
            DiagramProperty.Collection<UmlInterfaceNode, UmlOperation>("Operations", node => node.Operations, node => new UmlOperation("Operation" + (node.Operations.Count + 1)), operation => operation.Name + "()", "Operations", "Add operation"))
        .Register<UmlEnumNode>(
            DiagramProperty.Text<UmlEnumNode>("Literals", node => string.Join(Environment.NewLine, node.Literals), (node, value) => ReplaceValues(node.Literals, SplitLines(value)), "Enum", multiline: true))
        .Register<UmlNoteNode>(
            DiagramProperty.Text<UmlNoteNode>("Text", node => node.Text, (node, value) => node.Text = value ?? string.Empty, "Note", multiline: true))
        .Register<WorkflowNodeBase>(
            DiagramProperty.Text<WorkflowNodeBase>("Label", node => node.Label, (node, value) => node.Label = value ?? string.Empty, "Workflow node"),
            DiagramProperty.Text<WorkflowNodeBase>("Notes", node => node.Notes, (node, value) => node.Notes = value ?? string.Empty, "Workflow node", multiline: true))
        .Register<WorkflowTaskNode>(
            DiagramProperty.Enum<WorkflowTaskNode, WorkflowTaskType>("Task type", node => node.TaskType, (node, value) => node.TaskType = value, "Task"),
            DiagramProperty.Enum<WorkflowTaskNode, WorkflowTaskStatus>("Status", node => node.Status, (node, value) => node.Status = value, "Task"))
        .Register<WorkflowDecisionNode>(
            DiagramProperty.Text<WorkflowDecisionNode>("Condition", node => node.Condition, (node, value) => node.Condition = value ?? string.Empty, "Decision"))
        .Register<WorkflowGatewayNode>(
            DiagramProperty.Enum<WorkflowGatewayNode, WorkflowGatewayKind>("Gateway", node => node.GatewayKind, (node, value) => node.GatewayKind = value, "Gateway"))
        .Register<WorkflowEventNode>(
            DiagramProperty.Enum<WorkflowEventNode, WorkflowEventKind>("Event", node => node.EventKind, (node, value) => node.EventKind = value, "Event"))
        .Register<WorkflowNoteNode>(
            DiagramProperty.Text<WorkflowNoteNode>("Text", node => node.Text, (node, value) => node.Text = value ?? string.Empty, "Note", multiline: true))
        .Register<MindMapTopicNode>(
            DiagramProperty.Text<MindMapTopicNode>("Topic", node => node.Topic, (node, value) => node.Topic = value ?? string.Empty, "Mind map topic"),
            DiagramProperty.Text<MindMapTopicNode>("Notes", node => node.Notes ?? string.Empty, (node, value) => node.Notes = NormalizeOptional(value), "Mind map topic", multiline: true),
            DiagramProperty.Color<MindMapTopicNode>("Accent", node => node.AccentColor, (node, value) => node.AccentColor = value ?? string.Empty, "Mind map topic"),
            DiagramProperty.Text<MindMapTopicNode>("Icon", node => node.IconKey ?? string.Empty, (node, value) => node.IconKey = NormalizeOptional(value), "Mind map topic"),
            DiagramProperty.Boolean<MindMapTopicNode>("Collapsed", node => node.Collapsed, (node, value) => node.Collapsed = value, "Mind map topic"),
            DiagramProperty.Enum<MindMapTopicNode, MindMapTopicSide>("Side", node => node.Side, (node, value) => node.Side = value, "Mind map topic"))
        .Register<StateMachineNodeBase>(
            DiagramProperty.Text<StateMachineNodeBase>("Name", node => node.Name, (node, value) => node.Name = value ?? string.Empty, "State machine node"),
            DiagramProperty.Text<StateMachineNodeBase>("Description", node => node.Description ?? string.Empty, (node, value) => node.Description = NormalizeOptional(value), "State machine node", multiline: true),
            DiagramProperty.Color<StateMachineNodeBase>("Accent", node => node.AccentColor, (node, value) => node.AccentColor = value ?? string.Empty, "State machine node"))
        .Register<StateMachineStateNode>(
            DiagramProperty.Text<StateMachineStateNode>("Entry", node => node.EntryAction ?? string.Empty, (node, value) => node.EntryAction = NormalizeOptional(value), "Actions"),
            DiagramProperty.Text<StateMachineStateNode>("Exit", node => node.ExitAction ?? string.Empty, (node, value) => node.ExitAction = NormalizeOptional(value), "Actions"))
        .Register<StateMachineNoteNode>(
            DiagramProperty.Text<StateMachineNoteNode>("Text", node => node.Text, (node, value) => node.Text = value ?? string.Empty, "Note", multiline: true),
            DiagramProperty.Color<StateMachineNoteNode>("Accent", node => node.AccentColor, (node, value) => node.AccentColor = value ?? string.Empty, "Note"))
        .Register<NetworkNodeBase>(
            DiagramProperty.Text<NetworkNodeBase>("Name", node => node.Name, (node, value) => node.Name = value ?? string.Empty, "Network node"),
            DiagramProperty.Text<NetworkNodeBase>("Address", node => node.Address ?? string.Empty, (node, value) => node.Address = NormalizeOptional(value), "Network node"),
            DiagramProperty.Enum<NetworkNodeBase, NetworkStatus>("Status", node => node.Status, (node, value) => node.Status = value, "Network node"),
            DiagramProperty.Text<NetworkNodeBase>("Role", node => node.Role, (node, value) => node.Role = value ?? string.Empty, "Network node"),
            DiagramProperty.Text<NetworkNodeBase>("Zone", node => node.Zone ?? string.Empty, (node, value) => node.Zone = NormalizeOptional(value), "Network node"),
            DiagramProperty.Text<NetworkNodeBase>("Notes", node => node.Notes ?? string.Empty, (node, value) => node.Notes = NormalizeOptional(value), "Network node", multiline: true),
            DiagramProperty.Color<NetworkNodeBase>("Accent", node => node.AccentColor, (node, value) => node.AccentColor = value ?? string.Empty, "Network node"),
            DiagramProperty.Text<NetworkNodeBase>("Icon", node => node.IconKey ?? string.Empty, (node, value) => node.IconKey = NormalizeOptional(value), "Network node"))
        .Register<NetworkSwitchNode>(
            DiagramProperty.Number<NetworkSwitchNode>("Total", node => node.PortCount, (node, value) => node.PortCount = Math.Max(4, (int)Math.Round(value)), "Switch ports"),
            DiagramProperty.Number<NetworkSwitchNode>("Active", node => node.ActivePorts, (node, value) => node.ActivePorts = Math.Max(0, (int)Math.Round(value)), "Switch ports"))
        .Register<ApiLink>(
            DiagramProperty.Enum<ApiLink, ApiLinkKind>("Kind", link => link.Kind, (link, value) => link.Kind = value, "API link"),
            DiagramProperty.Text<ApiLink>("Label", link => link.Label ?? string.Empty, (link, value) => link.Label = NormalizeOptional(value), "API link"),
            DiagramProperty.Text<ApiLink>("Protocol", link => link.Protocol ?? string.Empty, (link, value) => link.Protocol = NormalizeOptional(value), "API link"),
            DiagramProperty.Text<ApiLink>("Payload", link => link.Payload ?? string.Empty, (link, value) => link.Payload = NormalizeOptional(value), "API link"),
            DiagramProperty.Enum<ApiLink, ApiEndpointStatus>("Status", link => link.Status, (link, value) => link.Status = value, "API link"),
            DiagramProperty.Color<ApiLink>("Accent", link => link.AccentColor ?? string.Empty, (link, value) => link.AccentColor = NormalizeOptional(value), "API link"))
        .Register<DatabaseRelationshipLink>(
            DiagramProperty.Enum<DatabaseRelationshipLink, RelationshipKind>("Kind", link => link.Kind, (link, value) => link.Kind = value, "Database relationship"),
            DiagramProperty.Text<DatabaseRelationshipLink>("Source", link => link.SourceCardinality ?? string.Empty, (link, value) => link.SourceCardinality = NormalizeOptional(value), "Database relationship"),
            DiagramProperty.Text<DatabaseRelationshipLink>("Target", link => link.TargetCardinality ?? string.Empty, (link, value) => link.TargetCardinality = NormalizeOptional(value), "Database relationship"))
        .Register<UmlRelationshipLink>(
            DiagramProperty.Enum<UmlRelationshipLink, UmlRelationshipKind>("Kind", link => link.Kind, (link, value) => link.Kind = value, "UML relationship"),
            DiagramProperty.Text<UmlRelationshipLink>("Label", link => link.Label ?? string.Empty, (link, value) => link.Label = NormalizeOptional(value), "UML relationship"),
            DiagramProperty.Text<UmlRelationshipLink>("Source", link => link.SourceMultiplicity ?? string.Empty, (link, value) => link.SourceMultiplicity = NormalizeOptional(value), "UML relationship"),
            DiagramProperty.Text<UmlRelationshipLink>("Target", link => link.TargetMultiplicity ?? string.Empty, (link, value) => link.TargetMultiplicity = NormalizeOptional(value), "UML relationship"))
        .Register<WorkflowLink>(
            DiagramProperty.Enum<WorkflowLink, WorkflowLinkKind>("Kind", link => link.Kind, (link, value) => link.Kind = value, "Workflow link"),
            DiagramProperty.Text<WorkflowLink>("Label", link => link.Label ?? string.Empty, (link, value) => link.Label = NormalizeOptional(value), "Workflow link"),
            DiagramProperty.Text<WorkflowLink>("Condition", link => link.Condition ?? string.Empty, (link, value) => link.Condition = NormalizeOptional(value), "Workflow link"))
        .Register<MindMapLink>(
            DiagramProperty.Enum<MindMapLink, MindMapLinkKind>("Kind", link => link.Kind, (link, value) => link.Kind = value, "Mind map link"),
            DiagramProperty.Text<MindMapLink>("Label", link => link.Label ?? string.Empty, (link, value) => link.Label = NormalizeOptional(value), "Mind map link"),
            DiagramProperty.Color<MindMapLink>("Accent", link => link.AccentColor ?? string.Empty, (link, value) => link.AccentColor = NormalizeOptional(value), "Mind map link"))
        .Register<StateMachineTransitionLink>(
            DiagramProperty.Enum<StateMachineTransitionLink, StateMachineTransitionKind>("Kind", link => link.Kind, (link, value) => link.Kind = value, "State machine transition"),
            DiagramProperty.Text<StateMachineTransitionLink>("Trigger", link => link.Trigger ?? string.Empty, (link, value) => link.Trigger = NormalizeOptional(value), "State machine transition"),
            DiagramProperty.Text<StateMachineTransitionLink>("Guard", link => link.Guard ?? string.Empty, (link, value) => link.Guard = NormalizeOptional(value), "State machine transition"),
            DiagramProperty.Text<StateMachineTransitionLink>("Action", link => link.Action ?? string.Empty, (link, value) => link.Action = NormalizeOptional(value), "State machine transition"),
            DiagramProperty.Number<StateMachineTransitionLink>("Priority", link => link.Priority, (link, value) => link.Priority = Math.Max(0, (int)Math.Round(value)), "State machine transition"),
            DiagramProperty.Color<StateMachineTransitionLink>("Accent", link => link.AccentColor ?? string.Empty, (link, value) => link.AccentColor = NormalizeOptional(value), "State machine transition"))
        .Register<NetworkLink>(
            DiagramProperty.Enum<NetworkLink, NetworkLinkKind>("Kind", link => link.Kind, (link, value) => link.Kind = value, "Network link"),
            DiagramProperty.Text<NetworkLink>("Label", link => link.Label ?? string.Empty, (link, value) => link.Label = NormalizeOptional(value), "Network link"),
            DiagramProperty.Text<NetworkLink>("Protocol", link => link.Protocol ?? string.Empty, (link, value) => link.Protocol = NormalizeOptional(value), "Network link"),
            DiagramProperty.Text<NetworkLink>("Bandwidth", link => link.Bandwidth ?? string.Empty, (link, value) => link.Bandwidth = NormalizeOptional(value), "Network link"),
            DiagramProperty.Text<NetworkLink>("Latency", link => link.Latency ?? string.Empty, (link, value) => link.Latency = NormalizeOptional(value), "Network link"),
            DiagramProperty.Enum<NetworkLink, NetworkStatus>("Status", link => link.Status, (link, value) => link.Status = value, "Network link"),
            DiagramProperty.Enum<NetworkLink, NetworkLinkDirection>("Direction", link => link.Direction, (link, value) => link.Direction = value, "Network link"),
            DiagramProperty.Color<NetworkLink>("Accent", link => link.AccentColor ?? string.Empty, (link, value) => link.AccentColor = NormalizeOptional(value), "Network link"));

    private static IEnumerable<DesignerToolboxSection> CreateToolboxSections() => new[]
    {
        new DesignerToolboxSection("Core", new[]
        {
            new DesignerToolboxItem("Task", point => new TaskNode(point, "Task") { Status = "Pending" })
            {
                Detail = "Sample custom node",
                Accent = new SolidColorBrush(Color.FromRgb(0x4D, 0x9E, 0xFF)),
            },
        }),
        new DesignerToolboxSection("Side packages", new[]
        {
            new DesignerToolboxItem("API endpoint", point =>
            {
                var node = new ApiEndpointNode(point, "/resource", ApiEndpointMethod.Get);
                node.AddPort(new ApiPortModel(node, PortAlignment.Left, ApiPortRole.Request, "in"));
                node.AddPort(new ApiPortModel(node, PortAlignment.Right, ApiPortRole.Response, "out"));
                return node;
            })
            {
                Detail = "Endpoint card",
                Accent = new SolidColorBrush(Color.FromRgb(0x2D, 0x7D, 0xE0)),
            },
            new DesignerToolboxItem("Database table", point =>
            {
                var node = new DatabaseTableNode(point, "Table", "dbo");
                node.Columns.Add(new DatabaseColumn("Id", "int", isPrimaryKey: true, isNullable: false));
                node.AddPort(new DatabasePortModel(node, PortAlignment.Right, DatabasePortKind.Relationship, "Id"));
                return node;
            })
            {
                Detail = "Rows and relationship port",
                Accent = new SolidColorBrush(Color.FromRgb(0x37, 0x8A, 0x63)),
            },
            new DesignerToolboxItem("Workflow task", point => new WorkflowTaskNode(point, "Task"))
            {
                Detail = "Process step",
                Accent = new SolidColorBrush(Color.FromRgb(0xC6, 0x85, 0x21)),
            },
            new DesignerToolboxItem("State", point => new StateMachineStateNode(point, "State"))
            {
                Detail = "Lifecycle state",
                Accent = new SolidColorBrush(Color.FromRgb(0x8B, 0x68, 0xB8)),
            },
            new DesignerToolboxItem("Network router", point => new NetworkRouterNode(point, "Router"))
            {
                Detail = "Topology device",
                Accent = new SolidColorBrush(Color.FromRgb(0x54, 0x9B, 0xC5)),
            },
            new DesignerToolboxItem("UML class", point => new UmlClassNode(point, "Class"))
            {
                Detail = "Structural type",
                Accent = new SolidColorBrush(Color.FromRgb(0x7C, 0x8A, 0x9A)),
            },
            new DesignerToolboxItem("Mind map topic", point => new MindMapBranchNode(point, "Topic"))
            {
                Detail = "Planning branch",
                Accent = new SolidColorBrush(Color.FromRgb(0xD4, 0x6A, 0x6A)),
            },
        }),
    };

    private static string FormatColumn(DatabaseColumn column)
    {
        var flags = new List<string>();
        if (column.IsPrimaryKey)
            flags.Add("PK");
        if (column.IsForeignKey)
            flags.Add("FK");
        if (!column.IsNullable)
            flags.Add("required");

        var suffix = flags.Count == 0 ? string.Empty : " (" + string.Join(", ", flags) + ")";
        return column.Name + ": " + column.DataType + suffix;
    }

    private static void ReplaceValues(IList<string> collection, IEnumerable<string> values)
    {
        collection.Clear();
        foreach (var value in values)
            if (!string.IsNullOrWhiteSpace(value))
                collection.Add(value.Trim());
    }

    private static string[] SplitComma(string? value) => (value ?? string.Empty)
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string[] SplitLines(string? value) => (value ?? string.Empty)
        .Replace("\r\n", "\n", StringComparison.Ordinal)
        .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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

    private Control BuildArchitecture()
    {
        var diagram = NewDiagram();

        var client = diagram.Nodes.Add(new ApiClientNode(new NodelyPoint(0, 0), "Storefront")
        {
            Platform = "desktop and web",
            Summary = "Customer order entry",
            Version = "v2",
        });
        var gateway = diagram.Nodes.Add(new ApiGatewayNode(new NodelyPoint(0, 0), "Public gateway")
        {
            Host = "api.contoso.test",
            Summary = "Routes public traffic",
        });
        var service = diagram.Nodes.Add(new ApiServiceNode(new NodelyPoint(0, 0), "Orders service")
        {
            BaseUrl = "orders.internal",
            Owner = "Commerce",
            Version = "v1",
            Summary = "Coordinates order creation",
        });
        var endpoint = diagram.Nodes.Add(new ApiEndpointNode(new NodelyPoint(0, 0), "/orders", ApiEndpointMethod.Post)
        {
            RequestType = "CreateOrderRequest",
            ResponseType = "OrderDto",
            Status = ApiEndpointStatus.Preview,
            Summary = "Creates an order",
        });
        var request = diagram.Nodes.Add(new ApiContractNode(new NodelyPoint(0, 0), "CreateOrderRequest"));
        request.Fields.Add(new ApiContractField("customerId", "string", required: true));
        request.Fields.Add(new ApiContractField("items", "OrderItem[]", required: true));
        request.Fields.Add(new ApiContractField("couponCode", "string"));

        var orders = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(0, 0), "Orders", "sales"));
        orders.Columns.Add(new DatabaseColumn("OrderId", "uuid", isPrimaryKey: true, isNullable: false));
        orders.Columns.Add(new DatabaseColumn("CustomerId", "uuid", isNullable: false));
        orders.Columns.Add(new DatabaseColumn("Status", "text", isNullable: false));
        var lines = diagram.Nodes.Add(new DatabaseTableNode(new NodelyPoint(0, 0), "OrderLines", "sales"));
        lines.Columns.Add(new DatabaseColumn("OrderLineId", "uuid", isPrimaryKey: true, isNullable: false));
        lines.Columns.Add(new DatabaseColumn("OrderId", "uuid", isNullable: false) { IsForeignKey = true });
        lines.Columns.Add(new DatabaseColumn("Sku", "text", isNullable: false));

        var internet = diagram.Nodes.Add(new NetworkCloudNode(new NodelyPoint(0, 0), "Internet")
        {
            Address = "0.0.0.0/0",
            Zone = "external",
        });
        var firewall = diagram.Nodes.Add(new NetworkFirewallNode(new NodelyPoint(0, 0), "Policy edge")
        {
            Address = "10.0.0.1",
            Zone = "edge",
            Status = NetworkStatus.Online,
        });
        var apiHost = diagram.Nodes.Add(new NetworkServiceNode(new NodelyPoint(0, 0), "Orders API")
        {
            Address = "orders.internal",
            Zone = "app",
            Status = NetworkStatus.Online,
        });
        var databaseHost = diagram.Nodes.Add(new NetworkServerNode(new NodelyPoint(0, 0), "Data host")
        {
            Address = "10.0.3.12",
            Zone = "data",
            Status = NetworkStatus.Online,
        });

        var start = diagram.Nodes.Add(new WorkflowStartNode(new NodelyPoint(0, 0), "Order received"));
        var validate = diagram.Nodes.Add(new WorkflowTaskNode(new NodelyPoint(0, 0), "Validate request")
        {
            TaskType = WorkflowTaskType.User,
            Status = WorkflowTaskStatus.Ready,
        });
        var callApi = diagram.Nodes.Add(new WorkflowTaskNode(new NodelyPoint(0, 0), "Call Orders API")
        {
            TaskType = WorkflowTaskType.Service,
            Status = WorkflowTaskStatus.Running,
        });
        var done = diagram.Nodes.Add(new WorkflowEndNode(new NodelyPoint(0, 0), "Order accepted"));

        var clientOut = client.AddPort(new ApiPortModel(client, PortAlignment.Right, ApiPortRole.Request, "request"));
        var gatewayIn = gateway.AddPort(new ApiPortModel(gateway, PortAlignment.Left, ApiPortRole.Request, "public"));
        var gatewayOut = gateway.AddPort(new ApiPortModel(gateway, PortAlignment.Right, ApiPortRole.Request, "route"));
        var serviceIn = service.AddPort(new ApiPortModel(service, PortAlignment.Left, ApiPortRole.Request, "in"));
        var serviceOut = service.AddPort(new ApiPortModel(service, PortAlignment.Right, ApiPortRole.Dependency, "handler"));
        var endpointIn = endpoint.AddPort(new ApiPortModel(endpoint, PortAlignment.Left, ApiPortRole.Request, "POST"));
        var endpointContract = endpoint.AddPort(new ApiPortModel(endpoint, PortAlignment.Right, ApiPortRole.Dependency, "body"));
        var contractIn = request.AddPort(new ApiPortModel(request, PortAlignment.Left, ApiPortRole.Dependency, "schema"));

        diagram.Links.Add(new ApiLink(clientOut, gatewayIn, ApiLinkKind.Request) { Label = "submit", Protocol = "HTTPS" });
        diagram.Links.Add(new ApiLink(gatewayOut, serviceIn, ApiLinkKind.Request) { Label = "route", Protocol = "HTTP" });
        diagram.Links.Add(new ApiLink(serviceOut, endpointIn, ApiLinkKind.Request) { Label = "create", Protocol = "POST" });
        diagram.Links.Add(new ApiLink(endpointContract, contractIn, ApiLinkKind.DependsOn) { Label = "body", Payload = "request" });

        var ordersOut = orders.AddPort(new DatabasePortModel(orders, PortAlignment.Right, DatabasePortKind.Relationship, "OrderId"));
        var linesIn = lines.AddPort(new DatabasePortModel(lines, PortAlignment.Left, DatabasePortKind.Relationship, "OrderId"));
        diagram.Links.Add(new DatabaseRelationshipLink(ordersOut, linesIn, RelationshipKind.OneToMany)
        {
            SourceCardinality = "1",
            TargetCardinality = "many",
        }).AddLabel("order lines", 0.5, new NodelyPoint(0, -16));
        diagram.Links.Add(new DatabaseRelationshipLink(service, orders, RelationshipKind.Dependency)).AddLabel("persists", 0.5, new NodelyPoint(0, 16));

        var internetOut = internet.AddPort(new NetworkPortModel(internet, PortAlignment.Right, NetworkPortRole.Wan, "public"));
        var firewallIn = firewall.AddPort(new NetworkPortModel(firewall, PortAlignment.Left, NetworkPortRole.Wan, "wan"));
        var firewallOut = firewall.AddPort(new NetworkPortModel(firewall, PortAlignment.Right, NetworkPortRole.Service, "https"));
        var apiIn = apiHost.AddPort(new NetworkPortModel(apiHost, PortAlignment.Left, NetworkPortRole.Service, "443"));
        var dbIn = databaseHost.AddPort(new NetworkPortModel(databaseHost, PortAlignment.Left, NetworkPortRole.Service, "5432"));
        diagram.Links.Add(new NetworkLink(internetOut, firewallIn, NetworkLinkKind.Fiber) { Label = "public", Protocol = "TLS" });
        diagram.Links.Add(new NetworkLink(firewallOut, apiIn, NetworkLinkKind.VpnTunnel) { Label = "edge route", Protocol = "HTTPS" });
        diagram.Links.Add(new NetworkLink(apiIn, dbIn, NetworkLinkKind.Dependency) { Label = "queries", Protocol = "SQL" });
        diagram.Links.Add(new NetworkLink(apiHost, service, NetworkLinkKind.Dependency) { Label = "hosts", Protocol = "HTTP" });

        diagram.Links.Add(new WorkflowLink(start, validate, WorkflowLinkKind.Sequence) { Label = "start" });
        diagram.Links.Add(new WorkflowLink(validate, callApi, WorkflowLinkKind.Conditional) { Label = "valid", Condition = "has items" });
        diagram.Links.Add(new WorkflowLink(callApi, endpoint, WorkflowLinkKind.Message) { Label = "invokes" });
        diagram.Links.Add(new WorkflowLink(callApi, done, WorkflowLinkKind.Sequence) { Label = "accepted" });

        Arrange();

        return Editor(
            diagram,
            configureCanvas: UseAllSidePackages,
            layoutAction: (canvas, _) =>
            {
                canvas.RunAsUndoableMove(Arrange);
                canvas.RefreshVisuals();
                canvas.ZoomToFit();
            });

        void Arrange()
        {
            internet.SetPosition(70, 70);
            firewall.SetPosition(340, 50);
            apiHost.SetPosition(620, 50);
            databaseHost.SetPosition(910, 50);

            client.SetPosition(70, 245);
            gateway.SetPosition(340, 230);
            service.SetPosition(620, 225);
            endpoint.SetPosition(920, 230);
            request.SetPosition(1220, 220);

            orders.SetPosition(610, 520);
            lines.SetPosition(920, 520);

            start.SetPosition(70, 720);
            validate.SetPosition(340, 690);
            callApi.SetPosition(620, 690);
            done.SetPosition(920, 720);
        }
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
        var initial = diagram.Nodes.Add(new StateMachineInitialNode(new NodelyPoint(80, 260), "Start"));
        var idle = diagram.Nodes.Add(new StateMachineStateNode(new NodelyPoint(250, 190), "Idle")
        {
            Description = "Waiting for a request",
            EntryAction = "show ready",
            ExitAction = "clear ready",
            AccentColor = "#37A779",
        });
        var choice = diagram.Nodes.Add(new StateMachineChoiceNode(new NodelyPoint(560, 230), "Route")
        {
            Description = "Validate request",
            AccentColor = "#8B68B8",
        });
        var running = diagram.Nodes.Add(new StateMachineStateNode(new NodelyPoint(820, 150), "Running")
        {
            Description = "Processing work",
            EntryAction = "start timer",
            ExitAction = "stop timer",
            AccentColor = "#4D9EFF",
        });
        var delayed = diagram.Nodes.Add(new StateMachineStateNode(new NodelyPoint(820, 430), "Delayed")
        {
            Description = "Waiting for retry",
            EntryAction = "schedule retry",
            AccentColor = "#D18B30",
        });
        var failed = diagram.Nodes.Add(new StateMachineStateNode(new NodelyPoint(1120, 420), "Failed")
        {
            Description = "Terminal error",
            EntryAction = "record failure",
            AccentColor = "#C45552",
        });
        var done = diagram.Nodes.Add(new StateMachineFinalNode(new NodelyPoint(1160, 190), "Done"));
        var note = diagram.Nodes.Add(new StateMachineNoteNode(new NodelyPoint(250, 520),
            "StateMachine nodes expose runtime-editable names, actions, transition guards, priorities, ports, and accent colors."));

        var initialOut = initial.AddPort(new StateMachinePortModel(initial, PortAlignment.Right, StateMachinePortRole.Exit, "start"));
        var idleIn = idle.AddPort(new StateMachinePortModel(idle, PortAlignment.Left, StateMachinePortRole.Entry, "entry"));
        var idleOut = idle.AddPort(new StateMachinePortModel(idle, PortAlignment.Right, StateMachinePortRole.Exit, "submit"));
        var choiceIn = choice.AddPort(new StateMachinePortModel(choice, PortAlignment.Left, StateMachinePortRole.Entry, "request"));
        var choiceOk = choice.AddPort(new StateMachinePortModel(choice, PortAlignment.Right, StateMachinePortRole.Exit, "ok"));
        var choiceDelay = choice.AddPort(new StateMachinePortModel(choice, PortAlignment.Bottom, StateMachinePortRole.Exit, "delay"));
        var runningIn = running.AddPort(new StateMachinePortModel(running, PortAlignment.Left, StateMachinePortRole.Entry, "start"));
        var runningOut = running.AddPort(new StateMachinePortModel(running, PortAlignment.Right, StateMachinePortRole.Exit, "complete"));
        var delayedIn = delayed.AddPort(new StateMachinePortModel(delayed, PortAlignment.Left, StateMachinePortRole.Entry, "wait"));
        var delayedOut = delayed.AddPort(new StateMachinePortModel(delayed, PortAlignment.Right, StateMachinePortRole.Exit, "retry"));
        var failedIn = failed.AddPort(new StateMachinePortModel(failed, PortAlignment.Left, StateMachinePortRole.Entry, "fail"));
        var doneIn = done.AddPort(new StateMachinePortModel(done, PortAlignment.Left, StateMachinePortRole.Entry, "finish"));

        diagram.Links.Add(new StateMachineTransitionLink(initialOut, idleIn)
        {
            Trigger = "created",
            Action = "initialize",
            Priority = 1,
        });
        diagram.Links.Add(new StateMachineTransitionLink(idleOut, choiceIn, StateMachineTransitionKind.Choice)
        {
            Trigger = "submit",
            Guard = "has payload",
            Action = "validate",
            Priority = 2,
        });
        diagram.Links.Add(new StateMachineTransitionLink(choiceOk, runningIn)
        {
            Trigger = "accepted",
            Guard = "quota available",
            Priority = 3,
        });
        diagram.Links.Add(new StateMachineTransitionLink(choiceDelay, delayedIn, StateMachineTransitionKind.Timeout)
        {
            Trigger = "defer",
            Guard = "quota full",
            Action = "schedule",
            AccentColor = "#D18B30",
        });
        diagram.Links.Add(new StateMachineTransitionLink(runningOut, doneIn)
        {
            Trigger = "completed",
            Action = "publish result",
        });
        diagram.Links.Add(new StateMachineTransitionLink(running, running, StateMachineTransitionKind.Self)
        {
            Trigger = "progress",
            Guard = "more work",
            Action = "continue",
        });
        diagram.Links.Add(new StateMachineTransitionLink(delayedOut, idleIn, StateMachineTransitionKind.Timeout)
        {
            Trigger = "retry",
            Guard = "window open",
        });
        diagram.Links.Add(new StateMachineTransitionLink(delayedOut, failedIn, StateMachineTransitionKind.Error)
        {
            Trigger = "retry exhausted",
            Action = "notify owner",
            AccentColor = "#C45552",
        });

        StateMachineLayout.Arrange(diagram);
        note.SetPosition(250, 520);

        return Editor(
            diagram,
            configureCanvas: canvas => canvas.UseStateMachineNodes(),
            layoutAction: (canvas, targetDiagram) =>
            {
                canvas.RunAsUndoableMove(() => StateMachineLayout.Arrange(targetDiagram));
                canvas.RefreshVisuals();
                canvas.ZoomToFit();
            });
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

    private Control BuildNetwork()
    {
        var diagram = NewDiagram();

        var internet = diagram.Nodes.Add(new NetworkCloudNode(new NodelyPoint(0, 0), "Internet")
        {
            Address = "0.0.0.0/0",
            Zone = "external",
            Notes = "Public traffic source",
            AccentColor = "#4D9EFF",
        });
        var admin = diagram.Nodes.Add(new NetworkClientNode(new NodelyPoint(0, 0), "Admin laptop")
        {
            Address = "10.9.0.24",
            Zone = "users",
            Status = NetworkStatus.Online,
        });
        var router = diagram.Nodes.Add(new NetworkRouterNode(new NodelyPoint(0, 0), "Edge router")
        {
            Address = "203.0.113.10",
            Zone = "edge",
            Notes = "Dual WAN edge",
        });
        var firewall = diagram.Nodes.Add(new NetworkFirewallNode(new NodelyPoint(0, 0), "Policy gateway")
        {
            Address = "10.0.0.1",
            Zone = "edge",
            Status = NetworkStatus.Maintenance,
            Notes = "Change window active",
        });
        var balancer = diagram.Nodes.Add(new NetworkLoadBalancerNode(new NodelyPoint(0, 0), "Public LB")
        {
            Address = "10.0.1.10",
            Zone = "dmz",
        });
        var switchNode = diagram.Nodes.Add(new NetworkSwitchNode(new NodelyPoint(0, 0), "Core switch")
        {
            Address = "10.0.2.2",
            Zone = "core",
            PortCount = 24,
            ActivePorts = 19,
        });
        var appZone = diagram.Nodes.Add(new NetworkZoneNode(new NodelyPoint(0, 0), "App subnet")
        {
            Address = "10.0.2.0/24",
            Zone = "prod",
            Status = NetworkStatus.Online,
        });
        var api = diagram.Nodes.Add(new NetworkServiceNode(new NodelyPoint(0, 0), "Orders API")
        {
            Address = "orders.internal",
            Zone = "prod",
            Status = NetworkStatus.Online,
        });
        var worker = diagram.Nodes.Add(new NetworkServerNode(new NodelyPoint(0, 0), "Worker host")
        {
            Address = "10.0.2.42",
            Zone = "prod",
            Status = NetworkStatus.Warning,
            Notes = "High latency",
        });
        var database = diagram.Nodes.Add(new NetworkServerNode(new NodelyPoint(0, 0), "Database host")
        {
            Address = "10.0.3.12",
            Zone = "data",
            Status = NetworkStatus.Online,
        });

        var internetWan = internet.AddPort(new NetworkPortModel(internet, PortAlignment.Right, NetworkPortRole.Wan, "internet"));
        var routerWan = router.AddPort(new NetworkPortModel(router, PortAlignment.Left, NetworkPortRole.Wan, "wan0"));
        var routerLan = router.AddPort(new NetworkPortModel(router, PortAlignment.Right, NetworkPortRole.Lan, "lan0"));
        var firewallWan = firewall.AddPort(new NetworkPortModel(firewall, PortAlignment.Left, NetworkPortRole.Wan, "outside"));
        var firewallLan = firewall.AddPort(new NetworkPortModel(firewall, PortAlignment.Right, NetworkPortRole.Lan, "inside"));
        var balancerIn = balancer.AddPort(new NetworkPortModel(balancer, PortAlignment.Left, NetworkPortRole.Service, "https"));
        var balancerOut = balancer.AddPort(new NetworkPortModel(balancer, PortAlignment.Right, NetworkPortRole.Service, "pool"));
        var switchIn = switchNode.AddPort(new NetworkPortModel(switchNode, PortAlignment.Left, NetworkPortRole.Uplink, "uplink", index: 0));
        var switchApi = switchNode.AddPort(new NetworkPortModel(switchNode, PortAlignment.Right, NetworkPortRole.Downlink, "api", index: 4));
        var switchWorker = switchNode.AddPort(new NetworkPortModel(switchNode, PortAlignment.Right, NetworkPortRole.Downlink, "worker", index: 5));
        var apiPort = api.AddPort(new NetworkPortModel(api, PortAlignment.Left, NetworkPortRole.Service, "443"));
        var workerPort = worker.AddPort(new NetworkPortModel(worker, PortAlignment.Left, NetworkPortRole.Service, "jobs"));
        var databasePort = database.AddPort(new NetworkPortModel(database, PortAlignment.Left, NetworkPortRole.Service, "5432"));
        var adminPort = admin.AddPort(new NetworkPortModel(admin, PortAlignment.Right, NetworkPortRole.Client, "vpn"));

        diagram.Links.Add(new NetworkLink(internetWan, routerWan, NetworkLinkKind.Fiber)
        {
            Label = "primary",
            Protocol = "BGP",
            Bandwidth = "10Gbps",
            Latency = "3ms",
            Direction = NetworkLinkDirection.Bidirectional,
        });
        diagram.Links.Add(new NetworkLink(routerLan, firewallWan, NetworkLinkKind.Ethernet)
        {
            Label = "edge",
            Bandwidth = "10Gbps",
        });
        diagram.Links.Add(new NetworkLink(firewallLan, balancerIn, NetworkLinkKind.VpnTunnel)
        {
            Label = "dmz tunnel",
            Protocol = "IPsec",
            Status = NetworkStatus.Warning,
            AccentColor = "#8B68B8",
        });
        diagram.Links.Add(new NetworkLink(balancerOut, switchIn, NetworkLinkKind.Ethernet)
        {
            Label = "app uplink",
            Bandwidth = "10Gbps",
        });
        diagram.Links.Add(new NetworkLink(switchApi, apiPort, NetworkLinkKind.Ethernet)
        {
            Label = "orders",
            Protocol = "HTTPS",
            Bandwidth = "1Gbps",
        });
        diagram.Links.Add(new NetworkLink(switchWorker, workerPort, NetworkLinkKind.Wireless)
        {
            Label = "telemetry",
            Protocol = "MQTT",
            Latency = "18ms",
            Status = NetworkStatus.Warning,
        });
        diagram.Links.Add(new NetworkLink(apiPort, databasePort, NetworkLinkKind.Dependency)
        {
            Label = "queries",
            Protocol = "SQL",
            Direction = NetworkLinkDirection.SourceToTarget,
        });
        diagram.Links.Add(new NetworkLink(adminPort, firewallWan, NetworkLinkKind.Blocked)
        {
            Label = "blocked admin",
            Protocol = "SSH",
            Status = NetworkStatus.Blocked,
        });

        NetworkLayout.Arrange(diagram);

        return Editor(
            diagram,
            configureCanvas: canvas => canvas.UseNetworkNodes(),
            layoutAction: (canvas, targetDiagram) =>
            {
                canvas.RunAsUndoableMove(() => NetworkLayout.Arrange(targetDiagram));
                canvas.RefreshVisuals();
                canvas.ZoomToFit();
            });
    }

    private Control BuildApi()
    {
        var diagram = NewDiagram();

        var client = diagram.Nodes.Add(new ApiClientNode(new NodelyPoint(0, 0), "Partner portal")
        {
            Platform = "web app",
            Summary = "External ordering client",
            Version = "v2",
        });
        var gateway = diagram.Nodes.Add(new ApiGatewayNode(new NodelyPoint(0, 0), "Public gateway")
        {
            Host = "api.example.test",
            Summary = "Routes public traffic",
            Version = "edge",
        });
        var auth = diagram.Nodes.Add(new ApiAuthNode(new NodelyPoint(0, 0), "Orders policy")
        {
            Scheme = "OAuth2",
            Scopes = "orders:read orders:write",
            Status = ApiEndpointStatus.Internal,
        });
        var group = diagram.Nodes.Add(new ApiGroupNode(new NodelyPoint(0, 0), "Orders API")
        {
            Summary = "Public order workflow boundary",
            Version = "v1",
        });
        var service = diagram.Nodes.Add(new ApiServiceNode(new NodelyPoint(0, 0), "Orders service")
        {
            BaseUrl = "orders.internal",
            Owner = "Commerce",
            Version = "v1",
            Summary = "Owns order creation and lookup",
        });
        var getOrder = diagram.Nodes.Add(new ApiEndpointNode(new NodelyPoint(0, 0), "/orders/{id}", ApiEndpointMethod.Get)
        {
            ResponseType = "OrderDto",
            Status = ApiEndpointStatus.Stable,
            Version = "v1",
            Summary = "Fetches a single order",
        });
        var createOrder = diagram.Nodes.Add(new ApiEndpointNode(new NodelyPoint(0, 0), "/orders", ApiEndpointMethod.Post)
        {
            RequestType = "CreateOrderRequest",
            ResponseType = "OrderDto",
            Status = ApiEndpointStatus.Preview,
            Version = "v1",
            Summary = "Creates a draft order",
        });
        var createOperation = diagram.Nodes.Add(new ApiOperationNode(new NodelyPoint(0, 0), "Create order")
        {
            Input = "CreateOrderRequest",
            Output = "OrderDto",
            SideEffectFree = false,
            Summary = "Validates, prices, and persists",
        });
        var publishOperation = diagram.Nodes.Add(new ApiOperationNode(new NodelyPoint(0, 0), "Publish event")
        {
            Input = "OrderDto",
            Output = "OrderCreated",
            SideEffectFree = false,
            Status = ApiEndpointStatus.Internal,
        });
        var createRequest = diagram.Nodes.Add(new ApiContractNode(new NodelyPoint(0, 0), "CreateOrderRequest")
        {
            Version = "v1",
        });
        createRequest.Fields.Add(new ApiContractField("customerId", "string", required: true));
        createRequest.Fields.Add(new ApiContractField("items", "OrderItem[]", required: true));
        createRequest.Fields.Add(new ApiContractField("couponCode", "string"));

        var orderDto = diagram.Nodes.Add(new ApiContractNode(new NodelyPoint(0, 0), "OrderDto")
        {
            Version = "v1",
        });
        orderDto.Fields.Add(new ApiContractField("id", "string", required: true));
        orderDto.Fields.Add(new ApiContractField("status", "string", required: true));
        orderDto.Fields.Add(new ApiContractField("total", "decimal", required: true));

        var orderEvent = diagram.Nodes.Add(new ApiContractNode(new NodelyPoint(0, 0), "OrderCreated")
        {
            Version = "v1",
            Status = ApiEndpointStatus.Internal,
        });
        orderEvent.Fields.Add(new ApiContractField("orderId", "string", required: true));
        orderEvent.Fields.Add(new ApiContractField("occurredAt", "date-time", required: true));

        var clientOut = client.AddPort(new ApiPortModel(client, PortAlignment.Right, ApiPortRole.Request, "request"));
        var gatewayIn = gateway.AddPort(new ApiPortModel(gateway, PortAlignment.Left, ApiPortRole.Request, "public"));
        var gatewayAuth = gateway.AddPort(new ApiPortModel(gateway, PortAlignment.Bottom, ApiPortRole.Auth, "auth"));
        var gatewayOut = gateway.AddPort(new ApiPortModel(gateway, PortAlignment.Right, ApiPortRole.Request, "route"));
        var authPort = auth.AddPort(new ApiPortModel(auth, PortAlignment.Left, ApiPortRole.Auth, "policy"));
        var serviceIn = service.AddPort(new ApiPortModel(service, PortAlignment.Left, ApiPortRole.Request, "in"));
        var serviceOut = service.AddPort(new ApiPortModel(service, PortAlignment.Right, ApiPortRole.Dependency, "ops"));
        var getIn = getOrder.AddPort(new ApiPortModel(getOrder, PortAlignment.Left, ApiPortRole.Request, "GET"));
        var getOut = getOrder.AddPort(new ApiPortModel(getOrder, PortAlignment.Right, ApiPortRole.Response, "200"));
        var postIn = createOrder.AddPort(new ApiPortModel(createOrder, PortAlignment.Left, ApiPortRole.Request, "POST"));
        var postOut = createOrder.AddPort(new ApiPortModel(createOrder, PortAlignment.Right, ApiPortRole.Response, "201"));
        var postAuth = createOrder.AddPort(new ApiPortModel(createOrder, PortAlignment.Bottom, ApiPortRole.Auth, "scope"));
        var opIn = createOperation.AddPort(new ApiPortModel(createOperation, PortAlignment.Left, ApiPortRole.Dependency, "input"));
        var opOut = createOperation.AddPort(new ApiPortModel(createOperation, PortAlignment.Right, ApiPortRole.Event, "event"));
        var publishIn = publishOperation.AddPort(new ApiPortModel(publishOperation, PortAlignment.Left, ApiPortRole.Event, "event"));
        var requestPort = createRequest.AddPort(new ApiPortModel(createRequest, PortAlignment.Left, ApiPortRole.Dependency, "request"));
        var responsePort = orderDto.AddPort(new ApiPortModel(orderDto, PortAlignment.Left, ApiPortRole.Response, "response"));
        var eventPort = orderEvent.AddPort(new ApiPortModel(orderEvent, PortAlignment.Left, ApiPortRole.Event, "event"));

        diagram.Links.Add(new ApiLink(clientOut, gatewayIn, ApiLinkKind.Request)
        {
            Label = "public",
            Protocol = "HTTPS",
            Payload = "JSON",
        });
        diagram.Links.Add(new ApiLink(authPort, gatewayAuth, ApiLinkKind.Secures)
        {
            Label = "token check",
            Protocol = "OAuth2",
            Status = ApiEndpointStatus.Internal,
        });
        diagram.Links.Add(new ApiLink(gatewayOut, serviceIn, ApiLinkKind.Request)
        {
            Label = "route",
            Protocol = "HTTPS",
        });
        diagram.Links.Add(new ApiLink(serviceOut, getIn, ApiLinkKind.Request)
        {
            Label = "read",
            Protocol = "GET",
        });
        diagram.Links.Add(new ApiLink(serviceOut, postIn, ApiLinkKind.Request)
        {
            Label = "create",
            Protocol = "POST",
            Status = ApiEndpointStatus.Preview,
        });
        diagram.Links.Add(new ApiLink(authPort, postAuth, ApiLinkKind.Secures)
        {
            Label = "orders:write",
            Status = ApiEndpointStatus.Internal,
        });
        diagram.Links.Add(new ApiLink(postIn, requestPort, ApiLinkKind.DependsOn)
        {
            Label = "body",
            Payload = "CreateOrderRequest",
        });
        diagram.Links.Add(new ApiLink(getOut, responsePort, ApiLinkKind.Response)
        {
            Label = "200",
            Payload = "OrderDto",
        });
        diagram.Links.Add(new ApiLink(postOut, responsePort, ApiLinkKind.Response)
        {
            Label = "201",
            Payload = "OrderDto",
        });
        diagram.Links.Add(new ApiLink(postOut, opIn, ApiLinkKind.DependsOn)
        {
            Label = "handler",
            Payload = "Create order",
        });
        diagram.Links.Add(new ApiLink(opOut, publishIn, ApiLinkKind.Publishes)
        {
            Label = "publish",
            Protocol = "event",
        });
        diagram.Links.Add(new ApiLink(publishIn, eventPort, ApiLinkKind.Publishes)
        {
            Label = "OrderCreated",
            Payload = "event",
            Status = ApiEndpointStatus.Internal,
        });

        ApiLayout.Arrange(diagram);

        return Editor(
            diagram,
            configureCanvas: canvas => canvas.UseApiNodes(),
            layoutAction: (canvas, targetDiagram) =>
            {
                canvas.RunAsUndoableMove(() => ApiLayout.Arrange(targetDiagram));
                canvas.RefreshVisuals();
                canvas.ZoomToFit();
            });
    }
}
