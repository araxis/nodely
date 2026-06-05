using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nodely;
using Nodely.Avalonia.Controls;
using Nodely.Avalonia.Api;
using Nodely.Avalonia.Database;
using Nodely.Avalonia.MindMap;
using Nodely.Avalonia.Network;
using Nodely.Avalonia.StateMachine;
using Nodely.Avalonia.Uml;
using Nodely.Avalonia.Workflow;
using Nodely.Models;
using Nodely.Models.Base;
using NodelyPoint = Nodely.Geometry.Point;

namespace Nodely.Demo;

internal sealed class RuntimePropertyInspector : IDisposable
{
    private readonly DiagramCanvas _canvas;
    private readonly NodelyDiagram _diagram;
    private readonly bool _readOnly;
    private readonly Border _panel;
    private bool _disposed;

    public RuntimePropertyInspector(DiagramCanvas canvas, NodelyDiagram diagram, bool readOnly)
    {
        _canvas = canvas;
        _diagram = diagram;
        _readOnly = readOnly;
        _panel = new Border
        {
            Width = 340,
            BorderThickness = new Thickness(1, 0, 0, 0),
        };

        _diagram.SelectionChanged += OnSelectionChanged;
        _canvas.CommandStateChanged += Refresh;
        Refresh();
    }

    public Control View => _panel;

    private bool CanEdit => !_readOnly && !_canvas.IsReadOnly;

    public void Refresh()
    {
        if (_disposed)
            return;

        var selected = _diagram.GetSelectedModels().ToList();
        _panel.Background = _canvas.Palette.NodeBackground;
        _panel.BorderBrush = _canvas.Palette.NodeBorder;

        var content = new StackPanel
        {
            Spacing = 12,
            Margin = new Thickness(14),
        };

        content.Children.Add(Title("Properties"));
        if (!CanEdit)
            content.Children.Add(Note("Read-only"));

        if (selected.Count == 0)
        {
            content.Children.Add(Note("Select one node or link."));
        }
        else if (selected.Count > 1)
        {
            content.Children.Add(Note(selected.Count.ToString(CultureInfo.InvariantCulture) + " items selected"));
        }
        else
        {
            BuildSelection(content, selected[0]);
        }

        _panel.Child = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = content,
        };
    }

    private void BuildSelection(StackPanel content, SelectableModel model)
    {
        content.Children.Add(Note(model.GetType().Name));
        content.Children.Add(BoolField("Locked", model.Locked, value => model.Locked = value, model));

        switch (model)
        {
            case NodeModel node:
                BuildNode(content, node);
                break;
            case BaseLinkModel link:
                BuildLink(content, link);
                break;
        }
    }

    private void BuildNode(StackPanel content, NodeModel node)
    {
        content.Children.Add(Section("Position"));
        content.Children.Add(NumberField("X", node.Position.X, value => node.SetPosition(value, node.Position.Y), node));
        content.Children.Add(NumberField("Y", node.Position.Y, value => node.SetPosition(node.Position.X, value), node));

        switch (node)
        {
            case TaskNode task:
                BuildTaskNode(content, task);
                break;
            case ApiNodeBase api:
                BuildApiNode(content, api);
                break;
            case DatabaseObjectNode database:
                BuildDatabaseNode(content, database);
                break;
            case UmlNodeBase uml:
                BuildUmlNode(content, uml);
                break;
            case UmlNoteNode note:
                content.Children.Add(Section("UML note"));
                content.Children.Add(TextField("Text", note.Text, value => note.Text = value, note, multiline: true));
                break;
            case MindMapTopicNode mindMap:
                BuildMindMapNode(content, mindMap);
                break;
            case StateMachineNodeBase stateMachine:
                BuildStateMachineNode(content, stateMachine);
                break;
            case StateMachineNoteNode note:
                content.Children.Add(Section("State machine note"));
                content.Children.Add(TextField("Text", note.Text, value => note.Text = value, note, multiline: true));
                content.Children.Add(TextField("Accent", note.AccentColor, value => note.AccentColor = value, note));
                break;
            case NetworkNodeBase network:
                BuildNetworkNode(content, network);
                break;
            case WorkflowNodeBase workflow:
                BuildWorkflowNode(content, workflow);
                break;
            case WorkflowNoteNode note:
                content.Children.Add(Section("Workflow note"));
                content.Children.Add(TextField("Text", note.Text, value => note.Text = value, note, multiline: true));
                break;
            default:
                content.Children.Add(Section("Node"));
                content.Children.Add(TextField("Title", node.Title ?? "", value => node.Title = value, node));
                break;
        }
    }

    private void BuildTaskNode(StackPanel content, TaskNode task)
    {
        content.Children.Add(Section("Task"));
        content.Children.Add(TextField("Title", task.Title ?? "", value => task.Title = value, task));
        content.Children.Add(TextField("Status", task.Status, value => task.Status = value, task));
    }

    private void BuildDatabaseNode(StackPanel content, DatabaseObjectNode node)
    {
        content.Children.Add(Section("Database object"));
        content.Children.Add(TextField("Name", node.ObjectName, value => node.ObjectName = value, node));
        content.Children.Add(TextField("Schema", node.Schema, value => node.Schema = value, node));

        switch (node)
        {
            case DatabaseTableNode table:
                BuildDatabaseColumns(content, table, table.Columns, "Columns");
                break;
            case DatabaseViewNode view:
                BuildDatabaseColumns(content, view, view.Columns, "Projected columns");
                break;
            case DatabaseProcedureNode procedure:
                BuildDatabaseParameters(content, procedure);
                break;
        }
    }

    private void BuildDatabaseColumns(
        StackPanel content,
        NodeModel owner,
        ObservableCollection<DatabaseColumn> columns,
        string label)
    {
        content.Children.Add(Section(label));
        foreach (var column in columns.ToList())
        {
            var row = Row();
            row.Children.Add(TextField("Name", column.Name, value => column.Name = value, owner));
            row.Children.Add(TextField("Type", column.DataType, value => column.DataType = value, owner));
            row.Children.Add(BoolField("Primary key", column.IsPrimaryKey, value => column.IsPrimaryKey = value, owner));
            row.Children.Add(BoolField("Foreign key", column.IsForeignKey, value => column.IsForeignKey = value, owner));
            row.Children.Add(BoolField("Nullable", column.IsNullable, value => column.IsNullable = value, owner));
            row.Children.Add(ActionButton("Remove", () => RemoveItem(owner, columns, column)));
            content.Children.Add(row);
        }

        content.Children.Add(ActionButton("Add column", () =>
            AddItem(owner, columns, new DatabaseColumn("Column" + (columns.Count + 1), "nvarchar(50)"))));
    }

    private void BuildDatabaseParameters(StackPanel content, DatabaseProcedureNode procedure)
    {
        content.Children.Add(Section("Parameters"));
        foreach (var parameter in procedure.Parameters.ToList())
        {
            var row = Row();
            row.Children.Add(TextField("Name", parameter.Name, value => parameter.Name = value, procedure));
            row.Children.Add(TextField("Type", parameter.DataType, value => parameter.DataType = value, procedure));
            row.Children.Add(TextField("Direction", parameter.Direction, value => parameter.Direction = value, procedure));
            row.Children.Add(ActionButton("Remove", () => RemoveItem(procedure, procedure.Parameters, parameter)));
            content.Children.Add(row);
        }

        content.Children.Add(ActionButton("Add parameter", () =>
            AddItem(procedure, procedure.Parameters, new DatabaseParameter("@parameter" + (procedure.Parameters.Count + 1), "int"))));
    }

    private void BuildUmlNode(StackPanel content, UmlNodeBase node)
    {
        content.Children.Add(Section("UML element"));
        content.Children.Add(TextField("Name", node.Name, value => node.Name = value, node));
        content.Children.Add(TextField(
            "Stereotypes",
            string.Join(", ", node.Stereotypes),
            value => ReplaceValues(node.Stereotypes, value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)),
            node));

        switch (node)
        {
            case UmlClassNode cls:
                content.Children.Add(BoolField("Abstract", cls.IsAbstract, value => cls.IsAbstract = value, cls));
                content.Children.Add(BoolField("Static", cls.IsStatic, value => cls.IsStatic = value, cls));
                BuildUmlMembers(content, cls, cls.Members);
                BuildUmlOperations(content, cls, cls.Operations);
                break;
            case UmlInterfaceNode iface:
                BuildUmlOperations(content, iface, iface.Operations);
                break;
            case UmlEnumNode enumNode:
                content.Children.Add(Section("Literals"));
                content.Children.Add(TextField(
                    "Values",
                    string.Join(Environment.NewLine, enumNode.Literals),
                    value => ReplaceValues(enumNode.Literals, SplitLines(value)),
                    enumNode,
                    multiline: true));
                break;
        }
    }

    private void BuildUmlMembers(StackPanel content, NodeModel owner, ObservableCollection<UmlMember> members)
    {
        content.Children.Add(Section("Members"));
        foreach (var member in members.ToList())
        {
            var row = Row();
            row.Children.Add(EnumField("Visibility", member.Visibility, value => member.Visibility = value, owner));
            row.Children.Add(TextField("Name", member.Name, value => member.Name = value, owner));
            row.Children.Add(TextField("Type", member.Type, value => member.Type = value, owner));
            row.Children.Add(BoolField("Static", member.IsStatic, value => member.IsStatic = value, owner));
            row.Children.Add(BoolField("Abstract", member.IsAbstract, value => member.IsAbstract = value, owner));
            row.Children.Add(ActionButton("Remove", () => RemoveItem(owner, members, member)));
            content.Children.Add(row);
        }

        content.Children.Add(ActionButton("Add member", () =>
            AddItem(owner, members, new UmlMember("Member" + (members.Count + 1), "string"))));
    }

    private void BuildUmlOperations(StackPanel content, NodeModel owner, ObservableCollection<UmlOperation> operations)
    {
        content.Children.Add(Section("Operations"));
        foreach (var operation in operations.ToList())
        {
            var row = Row();
            row.Children.Add(EnumField("Visibility", operation.Visibility, value => operation.Visibility = value, owner));
            row.Children.Add(TextField("Name", operation.Name, value => operation.Name = value, owner));
            row.Children.Add(TextField("Returns", operation.ReturnType, value => operation.ReturnType = value, owner));
            row.Children.Add(TextField("Parameters", FormatParameters(operation), value => ReplaceParameters(operation, value), owner));
            row.Children.Add(BoolField("Static", operation.IsStatic, value => operation.IsStatic = value, owner));
            row.Children.Add(BoolField("Abstract", operation.IsAbstract, value => operation.IsAbstract = value, owner));
            row.Children.Add(ActionButton("Remove", () => RemoveItem(owner, operations, operation)));
            content.Children.Add(row);
        }

        content.Children.Add(ActionButton("Add operation", () =>
            AddItem(owner, operations, new UmlOperation("Operation" + (operations.Count + 1)))));
    }

    private void BuildWorkflowNode(StackPanel content, WorkflowNodeBase node)
    {
        content.Children.Add(Section("Workflow node"));
        content.Children.Add(TextField("Label", node.Label, value => node.Label = value, node));
        content.Children.Add(TextField("Notes", node.Notes, value => node.Notes = value, node, multiline: true));

        switch (node)
        {
            case WorkflowTaskNode task:
                content.Children.Add(EnumField("Task type", task.TaskType, value => task.TaskType = value, task));
                content.Children.Add(EnumField("Status", task.Status, value => task.Status = value, task));
                break;
            case WorkflowDecisionNode decision:
                content.Children.Add(TextField("Condition", decision.Condition, value => decision.Condition = value, decision));
                break;
            case WorkflowGatewayNode gateway:
                content.Children.Add(EnumField("Gateway", gateway.GatewayKind, value => gateway.GatewayKind = value, gateway));
                break;
            case WorkflowEventNode eventNode:
                content.Children.Add(EnumField("Event", eventNode.EventKind, value => eventNode.EventKind = value, eventNode));
                break;
        }
    }

    private void BuildMindMapNode(StackPanel content, MindMapTopicNode node)
    {
        content.Children.Add(Section("Mind map topic"));
        content.Children.Add(TextField("Topic", node.Topic, value => node.Topic = value, node));
        content.Children.Add(TextField("Notes", node.Notes ?? "", value => node.Notes = NormalizeOptional(value), node, multiline: true));
        content.Children.Add(TextField("Accent", node.AccentColor, value => node.AccentColor = value, node));
        content.Children.Add(TextField("Icon", node.IconKey ?? "", value => node.IconKey = NormalizeOptional(value), node));
        content.Children.Add(BoolField("Collapsed", node.Collapsed, value =>
        {
            node.Collapsed = value;
            MindMapLayout.ApplyCollapseState(_diagram);
        }, node));
        content.Children.Add(EnumField("Side", node.Side, value => node.Side = value, node));
    }

    private void BuildStateMachineNode(StackPanel content, StateMachineNodeBase node)
    {
        content.Children.Add(Section("State machine node"));
        content.Children.Add(TextField("Name", node.Name, value => node.Name = value, node));
        content.Children.Add(TextField("Description", node.Description ?? "", value => node.Description = NormalizeOptional(value), node, multiline: true));
        content.Children.Add(TextField("Accent", node.AccentColor, value => node.AccentColor = value, node));

        if (node is StateMachineStateNode state)
        {
            content.Children.Add(Section("Actions"));
            content.Children.Add(TextField("Entry", state.EntryAction ?? "", value => state.EntryAction = NormalizeOptional(value), state));
            content.Children.Add(TextField("Exit", state.ExitAction ?? "", value => state.ExitAction = NormalizeOptional(value), state));
        }
    }

    private void BuildNetworkNode(StackPanel content, NetworkNodeBase node)
    {
        content.Children.Add(Section("Network node"));
        content.Children.Add(TextField("Name", node.Name, value => node.Name = value, node));
        content.Children.Add(TextField("Address", node.Address ?? "", value => node.Address = NormalizeOptional(value), node));
        content.Children.Add(EnumField("Status", node.Status, value => node.Status = value, node));
        content.Children.Add(TextField("Role", node.Role, value => node.Role = value, node));
        content.Children.Add(TextField("Zone", node.Zone ?? "", value => node.Zone = NormalizeOptional(value), node));
        content.Children.Add(TextField("Notes", node.Notes ?? "", value => node.Notes = NormalizeOptional(value), node, multiline: true));
        content.Children.Add(TextField("Accent", node.AccentColor, value => node.AccentColor = value, node));
        content.Children.Add(TextField("Icon", node.IconKey ?? "", value => node.IconKey = NormalizeOptional(value), node));

        if (node is NetworkSwitchNode switchNode)
        {
            content.Children.Add(Section("Switch ports"));
            content.Children.Add(NumberField("Total", switchNode.PortCount, value => switchNode.PortCount = Math.Max(4, (int)Math.Round(value)), switchNode));
            content.Children.Add(NumberField("Active", switchNode.ActivePorts, value => switchNode.ActivePorts = Math.Max(0, (int)Math.Round(value)), switchNode));
        }
    }

    private void BuildApiNode(StackPanel content, ApiNodeBase node)
    {
        content.Children.Add(Section("API node"));
        content.Children.Add(TextField("Name", node.Name, value => node.Name = value, node));
        content.Children.Add(TextField("Version", node.Version ?? "", value => node.Version = NormalizeOptional(value), node));
        content.Children.Add(EnumField("Status", node.Status, value => node.Status = value, node));
        content.Children.Add(TextField("Summary", node.Summary ?? "", value => node.Summary = NormalizeOptional(value), node, multiline: true));
        content.Children.Add(TextField("Accent", node.AccentColor, value => node.AccentColor = value, node));
        content.Children.Add(TextField("Icon", node.IconKey ?? "", value => node.IconKey = NormalizeOptional(value), node));

        switch (node)
        {
            case ApiServiceNode service:
                content.Children.Add(Section("Service"));
                content.Children.Add(TextField("Base URL", service.BaseUrl ?? "", value => service.BaseUrl = NormalizeOptional(value), service));
                content.Children.Add(TextField("Owner", service.Owner ?? "", value => service.Owner = NormalizeOptional(value), service));
                break;
            case ApiEndpointNode endpoint:
                content.Children.Add(Section("Endpoint"));
                content.Children.Add(EnumField("Method", endpoint.Method, value => endpoint.Method = value, endpoint));
                content.Children.Add(TextField("Route", endpoint.Route, value => endpoint.Route = value, endpoint));
                content.Children.Add(TextField("Request", endpoint.RequestType ?? "", value => endpoint.RequestType = NormalizeOptional(value), endpoint));
                content.Children.Add(TextField("Response", endpoint.ResponseType ?? "", value => endpoint.ResponseType = NormalizeOptional(value), endpoint));
                break;
            case ApiOperationNode operation:
                content.Children.Add(Section("Operation"));
                content.Children.Add(TextField("Input", operation.Input ?? "", value => operation.Input = NormalizeOptional(value), operation));
                content.Children.Add(TextField("Output", operation.Output ?? "", value => operation.Output = NormalizeOptional(value), operation));
                content.Children.Add(BoolField("Read only", operation.SideEffectFree, value => operation.SideEffectFree = value, operation));
                break;
            case ApiClientNode client:
                content.Children.Add(Section("Client"));
                content.Children.Add(TextField("Platform", client.Platform ?? "", value => client.Platform = NormalizeOptional(value), client));
                break;
            case ApiGatewayNode gateway:
                content.Children.Add(Section("Gateway"));
                content.Children.Add(TextField("Host", gateway.Host ?? "", value => gateway.Host = NormalizeOptional(value), gateway));
                break;
            case ApiAuthNode auth:
                content.Children.Add(Section("Auth"));
                content.Children.Add(TextField("Scheme", auth.Scheme ?? "", value => auth.Scheme = NormalizeOptional(value), auth));
                content.Children.Add(TextField("Scopes", auth.Scopes ?? "", value => auth.Scopes = NormalizeOptional(value), auth));
                break;
            case ApiContractNode contract:
                content.Children.Add(Section("Contract"));
                content.Children.Add(TextField("Fields", string.Join("; ", contract.Fields.Select(field => $"{field.Name}:{field.Type}")), value =>
                {
                    contract.Fields.Clear();
                    foreach (var part in value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        var pieces = part.Split(':', 2, StringSplitOptions.TrimEntries);
                        contract.Fields.Add(new ApiContractField(pieces[0], pieces.Length > 1 ? pieces[1] : "string"));
                    }
                }, contract, multiline: true));
                break;
        }
    }

    private void BuildLink(StackPanel content, BaseLinkModel link)
    {
        content.Children.Add(Section("Link"));
        content.Children.Add(BoolField("Segmentable", link.Segmentable, value => link.Segmentable = value, link));

        if (link is LinkModel standard)
        {
            content.Children.Add(NumberField("Width", standard.Width, value => standard.Width = Math.Max(0.5, value), standard));
            content.Children.Add(TextField("Color", standard.Color ?? "", value => standard.Color = NormalizeOptional(value), standard));
        }

        switch (link)
        {
            case ApiLink api:
                content.Children.Add(Section("API link"));
                content.Children.Add(EnumField("Kind", api.Kind, value => api.Kind = value, api));
                content.Children.Add(TextField("Label", api.Label ?? "", value => api.Label = NormalizeOptional(value), api));
                content.Children.Add(TextField("Protocol", api.Protocol ?? "", value => api.Protocol = NormalizeOptional(value), api));
                content.Children.Add(TextField("Payload", api.Payload ?? "", value => api.Payload = NormalizeOptional(value), api));
                content.Children.Add(EnumField("Status", api.Status, value => api.Status = value, api));
                content.Children.Add(TextField("Accent", api.AccentColor ?? "", value => api.AccentColor = NormalizeOptional(value), api));
                break;
            case DatabaseRelationshipLink relationship:
                content.Children.Add(Section("Database relationship"));
                content.Children.Add(EnumField("Kind", relationship.Kind, value => relationship.Kind = value, relationship));
                content.Children.Add(TextField("Label", FirstLabel(relationship), value => SetFirstLabel(relationship, value), relationship));
                content.Children.Add(TextField("Source", relationship.SourceCardinality ?? "", value => relationship.SourceCardinality = NormalizeOptional(value), relationship));
                content.Children.Add(TextField("Target", relationship.TargetCardinality ?? "", value => relationship.TargetCardinality = NormalizeOptional(value), relationship));
                break;
            case UmlRelationshipLink relationship:
                content.Children.Add(Section("UML relationship"));
                content.Children.Add(EnumField("Kind", relationship.Kind, value => relationship.Kind = value, relationship));
                content.Children.Add(TextField("Label", relationship.Label ?? "", value => relationship.Label = NormalizeOptional(value), relationship));
                content.Children.Add(TextField("Source", relationship.SourceMultiplicity ?? "", value => relationship.SourceMultiplicity = NormalizeOptional(value), relationship));
                content.Children.Add(TextField("Target", relationship.TargetMultiplicity ?? "", value => relationship.TargetMultiplicity = NormalizeOptional(value), relationship));
                break;
            case WorkflowLink workflow:
                content.Children.Add(Section("Workflow link"));
                content.Children.Add(EnumField("Kind", workflow.Kind, value => workflow.Kind = value, workflow));
                content.Children.Add(TextField("Label", workflow.Label ?? "", value => workflow.Label = NormalizeOptional(value), workflow));
                content.Children.Add(TextField("Condition", workflow.Condition ?? "", value => workflow.Condition = NormalizeOptional(value), workflow));
                break;
            case MindMapLink mindMap:
                content.Children.Add(Section("Mind map link"));
                content.Children.Add(EnumField("Kind", mindMap.Kind, value => mindMap.Kind = value, mindMap));
                content.Children.Add(TextField("Label", mindMap.Label ?? "", value => mindMap.Label = NormalizeOptional(value), mindMap));
                content.Children.Add(TextField("Accent", mindMap.AccentColor ?? "", value => mindMap.AccentColor = NormalizeOptional(value), mindMap));
                break;
            case StateMachineTransitionLink transition:
                content.Children.Add(Section("State machine transition"));
                content.Children.Add(EnumField("Kind", transition.Kind, value => transition.Kind = value, transition));
                content.Children.Add(TextField("Trigger", transition.Trigger ?? "", value => transition.Trigger = NormalizeOptional(value), transition));
                content.Children.Add(TextField("Guard", transition.Guard ?? "", value => transition.Guard = NormalizeOptional(value), transition));
                content.Children.Add(TextField("Action", transition.Action ?? "", value => transition.Action = NormalizeOptional(value), transition));
                content.Children.Add(NumberField("Priority", transition.Priority, value => transition.Priority = Math.Max(0, (int)Math.Round(value)), transition));
                content.Children.Add(TextField("Accent", transition.AccentColor ?? "", value => transition.AccentColor = NormalizeOptional(value), transition));
                break;
            case NetworkLink network:
                content.Children.Add(Section("Network link"));
                content.Children.Add(EnumField("Kind", network.Kind, value => network.Kind = value, network));
                content.Children.Add(TextField("Label", network.Label ?? "", value => network.Label = NormalizeOptional(value), network));
                content.Children.Add(TextField("Protocol", network.Protocol ?? "", value => network.Protocol = NormalizeOptional(value), network));
                content.Children.Add(TextField("Bandwidth", network.Bandwidth ?? "", value => network.Bandwidth = NormalizeOptional(value), network));
                content.Children.Add(TextField("Latency", network.Latency ?? "", value => network.Latency = NormalizeOptional(value), network));
                content.Children.Add(EnumField("Status", network.Status, value => network.Status = value, network));
                content.Children.Add(EnumField("Direction", network.Direction, value => network.Direction = value, network));
                content.Children.Add(TextField("Accent", network.AccentColor ?? "", value => network.AccentColor = NormalizeOptional(value), network));
                break;
            case FlowLink flow:
                content.Children.Add(Section("Flow"));
                content.Children.Add(BoolField("Critical", flow.Critical, value => flow.Critical = value, flow));
                content.Children.Add(TextField("Label", FirstLabel(flow), value => SetFirstLabel(flow, value), flow));
                break;
            default:
                content.Children.Add(TextField("Label", FirstLabel(link), value => SetFirstLabel(link, value), link));
                break;
        }
    }

    private Control TextField(string label, string value, Action<string> set, Model model, bool multiline = false)
    {
        var box = new TextBox
        {
            Text = value,
            IsEnabled = CanEdit,
            AcceptsReturn = multiline,
            TextWrapping = multiline ? TextWrapping.Wrap : TextWrapping.NoWrap,
            MinHeight = multiline ? 78 : 0,
        };
        var original = value;
        var committed = false;

        void Commit()
        {
            if (committed)
                return;

            var next = box.Text ?? "";
            if (string.Equals(next, original, StringComparison.Ordinal))
                return;

            committed = true;
            Apply(model, () => set(next), () => set(original));
        }

        box.LostFocus += (_, _) => Commit();
        if (!multiline)
        {
            box.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Enter)
                    Commit();
            };
        }

        return Field(label, box);
    }

    private Control NumberField(string label, double value, Action<double> set, Model model)
    {
        var text = value.ToString("0.###", CultureInfo.InvariantCulture);
        var box = new TextBox
        {
            Text = text,
            IsEnabled = CanEdit,
        };
        var original = value;
        var committed = false;

        void Commit()
        {
            if (committed)
                return;

            if (!double.TryParse(box.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var next))
                return;

            if (Math.Abs(next - original) < 0.001)
                return;

            committed = true;
            Apply(model, () => set(next), () => set(original));
        }

        box.LostFocus += (_, _) => Commit();
        box.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
                Commit();
        };

        return Field(label, box);
    }

    private Control BoolField(string label, bool value, Action<bool> set, Model model)
    {
        var box = new CheckBox
        {
            Content = label,
            IsChecked = value,
            IsEnabled = CanEdit,
            Foreground = _canvas.Palette.NodeText,
        };
        var original = value;
        box.Click += (_, _) =>
        {
            var next = box.IsChecked == true;
            if (next != original)
                Apply(model, () => set(next), () => set(original));
        };

        return box;
    }

    private Control EnumField<TEnum>(string label, TEnum value, Action<TEnum> set, Model model)
        where TEnum : struct, Enum
    {
        var combo = new ComboBox
        {
            ItemsSource = Enum.GetValues<TEnum>(),
            SelectedItem = value,
            IsEnabled = CanEdit,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        var original = value;
        combo.SelectionChanged += (_, _) =>
        {
            if (combo.SelectedItem is TEnum next && !next.Equals(original))
                Apply(model, () => set(next), () => set(original));
        };

        return Field(label, combo);
    }

    private Button ActionButton(string text, Action onClick)
    {
        var button = new Button
        {
            Content = text,
            IsEnabled = CanEdit,
            HorizontalAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(10, 4),
        };
        button.Click += (_, _) => onClick();
        return button;
    }

    private void Apply(Model model, Action apply, Action undo)
    {
        if (!CanEdit)
            return;

        _canvas.RunAsUndoableEdit(
            () =>
            {
                apply();
                RefreshModel(model);
            },
            () =>
            {
                undo();
                RefreshModel(model);
            });
        Refresh();
    }

    private void AddItem<T>(Model owner, ObservableCollection<T> collection, T item)
        => Apply(owner, () => collection.Add(item), () => collection.Remove(item));

    private void RemoveItem<T>(Model owner, ObservableCollection<T> collection, T item)
    {
        var index = collection.IndexOf(item);
        Apply(
            owner,
            () => collection.Remove(item),
            () =>
            {
                if (!collection.Contains(item))
                    collection.Insert(Math.Clamp(index, 0, collection.Count), item);
            });
    }

    private static void RefreshModel(Model model)
    {
        switch (model)
        {
            case NodeModel node:
                node.ReinitializePorts();
                node.RefreshAll();
                node.RefreshLinks();
                foreach (var link in node.PortLinks.Distinct())
                    link.Refresh();
                break;
            case BaseLinkModel link:
                link.Refresh();
                break;
            default:
                model.Refresh();
                break;
        }
    }

    private static string FirstLabel(BaseLinkModel link) => link.Labels.FirstOrDefault()?.Content ?? "";

    private static void SetFirstLabel(BaseLinkModel link, string text)
    {
        var value = NormalizeOptional(text);
        if (value == null)
        {
            link.Labels.Clear();
            link.Refresh();
            return;
        }

        if (link.Labels.Count == 0)
            link.AddLabel(value, 0.5, new NodelyPoint(0, -16));
        else
            link.Labels[0].Content = value;

        link.Refresh();
    }

    private static string FormatParameters(UmlOperation operation) => string.Join(
        ", ",
        operation.Parameters.Select(parameter =>
        {
            var type = string.IsNullOrWhiteSpace(parameter.Type) ? "object" : parameter.Type;
            var text = parameter.Name + ":" + type;
            return string.IsNullOrWhiteSpace(parameter.DefaultValue) ? text : text + "=" + parameter.DefaultValue;
        }));

    private static void ReplaceParameters(UmlOperation operation, string text)
    {
        operation.Parameters.Clear();
        foreach (var segment in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var assignment = segment.Split('=', 2, StringSplitOptions.TrimEntries);
            var parts = assignment[0].Split(':', 2, StringSplitOptions.TrimEntries);
            var name = parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]) ? parts[0] : "parameter";
            var type = parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1] : "object";
            operation.Parameters.Add(new UmlParameter(name, type)
            {
                DefaultValue = assignment.Length > 1 ? NormalizeOptional(assignment[1]) : null,
            });
        }
    }

    private static void ReplaceValues(ObservableCollection<string> collection, string[] values)
    {
        collection.Clear();
        foreach (var value in values)
            if (!string.IsNullOrWhiteSpace(value))
                collection.Add(value.Trim());
    }

    private static string[] SplitLines(string value) => value
        .Replace("\r\n", "\n", StringComparison.Ordinal)
        .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private TextBlock Title(string text) => new()
    {
        Text = text,
        Foreground = _canvas.Palette.NodeText,
        FontSize = 17,
        FontWeight = FontWeight.SemiBold,
    };

    private TextBlock Section(string text) => new()
    {
        Text = text,
        Foreground = _canvas.Palette.NodeText,
        FontWeight = FontWeight.SemiBold,
        Margin = new Thickness(0, 4, 0, 0),
    };

    private TextBlock Note(string text) => new()
    {
        Text = text,
        Foreground = _canvas.Palette.LinkStroke,
        TextWrapping = TextWrapping.Wrap,
    };

    private Control Field(string label, Control control)
    {
        var stack = new StackPanel { Spacing = 4 };
        stack.Children.Add(new TextBlock
        {
            Text = label,
            Foreground = _canvas.Palette.LinkStroke,
            FontSize = 12,
        });
        stack.Children.Add(control);
        return stack;
    }

    private static StackPanel Row() => new()
    {
        Spacing = 8,
        Margin = new Thickness(0, 0, 0, 4),
    };

    private void OnSelectionChanged(SelectableModel _) => Refresh();

    public void Dispose()
    {
        if (_disposed)
            return;

        _diagram.SelectionChanged -= OnSelectionChanged;
        _canvas.CommandStateChanged -= Refresh;
        _disposed = true;
    }
}
