using System;
namespace TaskManagerAPI.Models;

/// <summary>
/// Represents the priority level of a task.
/// </summary>
public enum PriorityLevel
{
    Low,
    Medium,
    High
}

/// <summary>
/// Represents a single task item in the database.
/// </summary>
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; } = false;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public DateTime? DueDate { get; set; }
}
