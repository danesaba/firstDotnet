using TaskManagerAPI.Models;

namespace TaskManagerAPI.Dtos;

/// <summary>
/// Represents the data required from a user to create a new task.
/// Notice there is no 'Id' property.
/// </summary>
public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; } = false;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public DateTime? DueDate { get; set; }
}
