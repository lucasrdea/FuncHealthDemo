using FuncHealthDemo.DB;
using FuncHealthDemo.DTO;
using FuncHealthDemo.Exceptions;
using Microsoft.EntityFrameworkCore;
using TaskStatus = FuncHealthDemo.Enum.TaskStatus;

namespace FuncHealthDemo.Services;

public class TaskService
{
    private readonly DataContext _db;

    public TaskService(DataContext db)
    {
        _db = db;
    }

    public async Task<List<TaskResponseDto>> GetUserTasksAsync(int userId)
    {
        return await _db.Tasks
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                Category = t.Category,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CompletedAt = t.CompletedAt
            })
            .ToListAsync();
    }

    public async Task<TaskResponseDto?> GetTaskByIdAsync(int taskId, int userId)
    {
        return await _db.Tasks
            .Where(t => t.Id == taskId && t.UserId == userId)
            .Select(t => new TaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                Category = t.Category,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CompletedAt = t.CompletedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<TaskResponseDto> CreateTaskAsync(int userId, CreateTaskRequest request)
    {
        // Validate title
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title is required");

        if (request.Title.Length > 200)
            throw new ValidationException("Title must be 200 characters or less");

        // Validate description length
        if (request.Description?.Length > 2000)
            throw new ValidationException("Description must be 2000 characters or less");

        // Validate due date is required
        if (request.DueDate == default(DateTime))
            throw new ValidationException("Due date is required");

        var task = new Entities.Task
        {
            UserId = userId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            Category = request.Category,
            DueDate = request.DueDate,
            Status = TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            Category = task.Category,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CompletedAt = task.CompletedAt
        };
    }

    public async Task<TaskResponseDto> UpdateTaskAsync(int taskId, int userId, UpdateTaskRequest request)
    {
        var task = await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

        if (task == null)
            throw new ValidationException("Task not found or you don't have permission to access it");

        // Update only provided fields
        if (request.Title != null)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ValidationException("Title cannot be empty");

            if (request.Title.Length > 200)
                throw new ValidationException("Title must be 200 characters or less");

            task.Title = request.Title.Trim();
        }

        if (request.Description != null)
        {
            if (request.Description.Length > 2000)
                throw new ValidationException("Description must be 2000 characters or less");

            task.Description = request.Description.Trim();
        }

        if (request.Status.HasValue)
        {
            task.Status = request.Status.Value;

            // Auto-set CompletedAt when status changes to Completed
            if (request.Status.Value == TaskStatus.Completed && task.CompletedAt == null)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            // Clear CompletedAt if status changes from Completed to something else
            else if (request.Status.Value != TaskStatus.Completed && task.CompletedAt != null)
            {
                task.CompletedAt = null;
            }
        }

        if (request.Priority.HasValue)
            task.Priority = request.Priority.Value;

        if (request.Category.HasValue)
            task.Category = request.Category.Value;

        // Validate and update due date (required)
        if (request.DueDate == default(DateTime))
            throw new ValidationException("Due date is required");

        task.DueDate = request.DueDate;

        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            Category = task.Category,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CompletedAt = task.CompletedAt
        };
    }

    public async Task<bool> DeleteTaskAsync(int taskId, int userId)
    {
        var task = await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

        if (task == null)
            return false;

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();

        return true;
    }
}
