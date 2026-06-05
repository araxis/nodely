namespace Nodely.Avalonia.Workflow;

/// <summary>The status of a workflow task node.</summary>
public enum WorkflowTaskStatus
{
    /// <summary>The task is being drafted.</summary>
    Draft,

    /// <summary>The task is ready to run.</summary>
    Ready,

    /// <summary>The task is running.</summary>
    Running,

    /// <summary>The task is blocked.</summary>
    Blocked,

    /// <summary>The task completed successfully.</summary>
    Complete,

    /// <summary>The task failed.</summary>
    Failed
}
