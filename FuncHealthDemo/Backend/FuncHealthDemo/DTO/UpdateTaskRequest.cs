using FuncHealthDemo.Enum;
using TaskStatus = FuncHealthDemo.Enum.TaskStatus;

namespace FuncHealthDemo.DTO;

public class UpdateTaskRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public TaskCategory? Category { get; set; }
    public DateTime DueDate { get; set; }
}
