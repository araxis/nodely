namespace Nodely.Avalonia.Workflow;

/// <summary>The kind of work represented by a workflow task node.</summary>
public enum WorkflowTaskType
{
    /// <summary>A general task.</summary>
    Task,

    /// <summary>A person-driven task.</summary>
    User,

    /// <summary>An automated service task.</summary>
    Service,

    /// <summary>A scripted task.</summary>
    Script,

    /// <summary>A review or approval task.</summary>
    Review
}
