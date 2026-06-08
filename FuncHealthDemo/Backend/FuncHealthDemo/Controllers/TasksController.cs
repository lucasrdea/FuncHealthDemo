using FuncHealthDemo.DTO;
using FuncHealthDemo.Exceptions;
using FuncHealthDemo.Filters;
using FuncHealthDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace FuncHealthDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : BaseApiController
{
    private readonly TaskService _taskService;

    public TasksController(TaskService taskService)
    {
        _taskService = taskService;
    }

    // GET: api/tasks - Get all tasks for authenticated user
    [HttpGet]
    [ServiceFilter(typeof(ValidateUserIdFilter))]
    public async Task<ActionResult<List<TaskResponseDto>>> GetMyTasks()
    {
        var userId = GetAuthenticatedUserId();
        var tasks = await _taskService.GetUserTasksAsync(userId);
        return Ok(tasks);
    }

    // GET: api/tasks/{id} - Get specific task (only if it belongs to authenticated user)
    [HttpGet("{id}")]
    [ServiceFilter(typeof(ValidateUserIdFilter))]
    public async Task<ActionResult<TaskResponseDto>> GetTaskById(int id)
    {
        var userId = GetAuthenticatedUserId();
        var task = await _taskService.GetTaskByIdAsync(id, userId);

        if (task == null)
            return NotFound(new { error = "Task not found or you don't have permission to access it" });

        return Ok(task);
    }

    // POST: api/tasks - Create new task
    [HttpPost]
    [ServiceFilter(typeof(ValidateUserIdFilter))]
    public async Task<ActionResult<TaskResponseDto>> CreateTask([FromBody] CreateTaskRequest request)
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var task = await _taskService.CreateTaskAsync(userId, request);
            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT: api/tasks/{id} - Update task (only if it belongs to authenticated user)
    [HttpPut("{id}")]
    [ServiceFilter(typeof(ValidateUserIdFilter))]
    public async Task<ActionResult<TaskResponseDto>> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var task = await _taskService.UpdateTaskAsync(id, userId, request);
            return Ok(task);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // DELETE: api/tasks/{id} - Delete task (only if it belongs to authenticated user)
    [HttpDelete("{id}")]
    [ServiceFilter(typeof(ValidateUserIdFilter))]
    public async Task<ActionResult> DeleteTask(int id)
    {
        var userId = GetAuthenticatedUserId();
        var deleted = await _taskService.DeleteTaskAsync(id, userId);

        if (!deleted)
            return NotFound(new { error = "Task not found or you don't have permission to access it" });

        return NoContent();
    }
}
