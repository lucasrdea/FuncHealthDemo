using FirebaseAdmin.Auth;
using FuncHealthDemo.Services;

namespace FuncHealthDemo.Middleware;

public class FirebaseAuthMiddleware
{
    private readonly RequestDelegate _next;

    public FirebaseAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserService userService)
    {
        // Skip authentication for specific paths
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.Contains("/swagger") || 
            path == "/" || 
            path.Contains("/api/users") && context.Request.Method == "POST")
        {
            await _next(context);
            return;
        }

        //// SECURITY: Reject any request with X-User-Id header in production mode
        //if (context.Request.Headers.ContainsKey("X-User-Id"))
        //{
        //    context.Response.StatusCode = 400;
        //    await context.Response.WriteAsJsonAsync(new { error = "X-User-Id header is not allowed in production mode. Use Firebase authentication." });
        //    return;
        //}

        // Get Authorization header
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) || 
            string.IsNullOrWhiteSpace(authHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Authorization header is required" });
            return;
        }

        // Extract Bearer token
        var token = authHeader.ToString();
        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token.Substring("Bearer ".Length).Trim();
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid authorization token" });
            return;
        }

        try
        {
            // Verify Firebase ID token
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);

            // Get Firebase UID from verified token
            string firebaseUid = decodedToken.Uid;

            // Lookup user by Firebase UID
            var user = await userService.GetUserByFbUserIdAsync(firebaseUid);
            if (user == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "User not found in database" });
                return;
            }

            // Store the internal user ID in HttpContext for controllers to access
            context.Items["UserId"] = user.Id;
            context.Items["User"] = user;
            context.Items["FirebaseUid"] = firebaseUid;

            await _next(context);
        }
        catch (FirebaseAuthException ex)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid Firebase token", details = ex.Message });
            return;
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "Authentication error", details = ex.Message });
            return;
        }
    }
}

public static class FirebaseAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseFirebaseAuth(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<FirebaseAuthMiddleware>();
    }
}
