using FuncHealthDemo.DB;
using FuncHealthDemo.DTO;
using FuncHealthDemo.Entities;
using FuncHealthDemo.Enum;
using FuncHealthDemo.Exceptions;
using FuncHealthDemo.Filters;
using FuncHealthDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace FuncHealthDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseApiController
{
    private readonly DataContext _db;
    private readonly UserService _userService;

    public UsersController(DataContext db, UserService userService)
    {
        _db = db;
        _userService = userService;
    }

    [HttpPost]
    public async Task<ActionResult<User>> Register([FromBody] RegisterUserRequest request)
    {
        try
        {
            var fullName = $"{request.FirstName} {request.LastName}";
            var user = await _userService.CreateUserAsync(
                request.Uid,
                fullName,
                request.Email,
                string.Empty,
                DateTime.UtcNow,
                UserType.Client
            );

            return Ok(user);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [ServiceFilter(typeof(ValidateUserIdFilter))]
    public async Task<ActionResult<User>> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPut("profile")]
    [ServiceFilter(typeof(ValidateUserIdFilter))]
    public async Task<ActionResult<User>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var updatedUser = await _userService.UpdateProfileAsync(userId, dto.PhoneNumber, dto.DateOfBirth);
            return Ok(updatedUser);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}