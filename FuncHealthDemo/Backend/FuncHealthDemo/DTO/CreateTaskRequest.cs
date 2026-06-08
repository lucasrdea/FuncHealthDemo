using FuncHealthDemo.Enum;

namespace FuncHealthDemo.DTO;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public TaskCategory Category { get; set; } = TaskCategory.Personal;
    public DateTime DueDate { get; set; }
}
