using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FuncHealthDemo.Filters;

public class ValidateUserIdFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Get the authenticated user ID from HttpContext
        if (!context.HttpContext.Items.TryGetValue("UserId", out var authenticatedUserIdObj) || authenticatedUserIdObj is not int authenticatedUserId)
        {
            context.Result = new UnauthorizedObjectResult(new { error = "User not authenticated." });
            return;
        }

        // Check if the action has a userId parameter (from route or body)
        int? requestedUserId = null;

        // Check route parameters
        if (context.ActionArguments.TryGetValue("userId", out var userIdFromRoute))
        {
            if (userIdFromRoute is int uid)
            {
                requestedUserId = uid;
            }
        }

        // Check if there's a body parameter with UserId property
        if (!requestedUserId.HasValue)
        {
            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg != null)
                {
                    var userIdProperty = arg.GetType().GetProperty("UserId");
                    if (userIdProperty != null && userIdProperty.PropertyType == typeof(int))
                    {
                        requestedUserId = (int?)userIdProperty.GetValue(arg);
                        break;
                    }
                }
            }
        }

        // If we found a userId to validate, check if it matches the authenticated user
        if (requestedUserId.HasValue && requestedUserId.Value != authenticatedUserId)
        {
            context.Result = new ForbidResult();
        }

        base.OnActionExecuting(context);
    }
}
