using FuncHealthDemo.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FuncHealthDemo.Controllers;

public class BaseApiController : ControllerBase
{
    protected int GetAuthenticatedUserId()
    {
        if (HttpContext.Items.TryGetValue("UserId", out var userId) && userId is int id)
        {
            return id;
        }

        throw new UnauthorizedAccessException("User not authenticated");
    }

    protected User GetAuthenticatedUser()
    {
        if (HttpContext.Items.TryGetValue("User", out var user) && user is User u)
        {
            return u;
        }

        throw new UnauthorizedAccessException("User not authenticated");
    }

    protected int? TryGetAuthenticatedUserId()
    {
        if (HttpContext.Items.TryGetValue("UserId", out var userId) && userId is int id)
        {
            return id;
        }

        return null;
    }
}
