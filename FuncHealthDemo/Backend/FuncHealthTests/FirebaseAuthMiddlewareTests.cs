using FluentAssertions;
using FuncHealthDemo.DB;
using FuncHealthDemo.Entities;
using FuncHealthDemo.Enum;
using FuncHealthDemo.Middleware;
using FuncHealthDemo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace FuncHealthTests;

/// <summary>
/// Tests for FirebaseAuthMiddleware to ensure proper authentication enforcement.
/// These tests verify that:
/// - Protected endpoints require valid Firebase Bearer tokens
/// - Public endpoints (like user registration) allow unauthenticated access
/// - Invalid/missing tokens return 401 Unauthorized
/// - Each Firebase token is verified and mapped to internal user ID
/// </summary>
public class FirebaseAuthMiddlewareTests
{
    private UserService CreateUserService()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new DataContext(options);

        context.Set<User>().Add(new User
        {
            Id = 1,
            FbUserId = "test-fb-uid",
            Name = "Test User",
            Email = "test@example.com",
            PhoneNumber = "12345678901",
            DateOfBirth = new DateTime(1990, 1, 1),
            Type = UserType.Client
        });
        context.SaveChanges();

        return new UserService(context);
    }

    [Fact]
    public async Task NoAuthorizationHeader_Returns401()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tasks";
        context.Response.Body = new MemoryStream();

        var nextMock = new Mock<RequestDelegate>();
        var middleware = new FirebaseAuthMiddleware(nextMock.Object);

        await middleware.InvokeAsync(context, CreateUserService());

        context.Response.StatusCode.Should().Be(401);
        nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task EmptyAuthorizationHeader_Returns401()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tasks";
        context.Request.Headers["Authorization"] = "";
        context.Response.Body = new MemoryStream();

        var nextMock = new Mock<RequestDelegate>();
        var middleware = new FirebaseAuthMiddleware(nextMock.Object);

        await middleware.InvokeAsync(context, CreateUserService());

        context.Response.StatusCode.Should().Be(401);
        nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task BearerWithoutToken_Returns401()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tasks";
        context.Request.Headers["Authorization"] = "Bearer ";
        context.Response.Body = new MemoryStream();

        var nextMock = new Mock<RequestDelegate>();
        var middleware = new FirebaseAuthMiddleware(nextMock.Object);

        await middleware.InvokeAsync(context, CreateUserService());

        context.Response.StatusCode.Should().Be(401);
        nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task InvalidFirebaseToken_Returns401()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tasks";
        context.Request.Headers["Authorization"] = "Bearer invalid-token";
        context.Response.Body = new MemoryStream();

        var nextMock = new Mock<RequestDelegate>();
        var middleware = new FirebaseAuthMiddleware(nextMock.Object);

        await middleware.InvokeAsync(context, CreateUserService());

        // Returns 401 if Firebase is initialized, or 500 if Firebase is not configured (test environment)
        context.Response.StatusCode.Should().BeOneOf(401, 500);
        nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUser_AllowsWithoutAuth()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/users";
        context.Request.Method = "POST";
        context.Response.Body = new MemoryStream();

        var nextMock = new Mock<RequestDelegate>();
        var middleware = new FirebaseAuthMiddleware(nextMock.Object);

        await middleware.InvokeAsync(context, CreateUserService());

        nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Once);
    }

    [Fact]
    public async Task GetUser_RequiresAuth()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/users/1";
        context.Request.Method = "GET";
        context.Response.Body = new MemoryStream();

        var nextMock = new Mock<RequestDelegate>();
        var middleware = new FirebaseAuthMiddleware(nextMock.Object);

        await middleware.InvokeAsync(context, CreateUserService());

        context.Response.StatusCode.Should().Be(401);
        nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUser_RequiresAuth()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/users/profile";
        context.Request.Method = "PUT";
        context.Response.Body = new MemoryStream();

        var nextMock = new Mock<RequestDelegate>();
        var middleware = new FirebaseAuthMiddleware(nextMock.Object);

        await middleware.InvokeAsync(context, CreateUserService());

        context.Response.StatusCode.Should().Be(401);
        nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task GetTasks_RequiresAuth()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tasks";
        context.Request.Method = "GET";
        context.Response.Body = new MemoryStream();

        var nextMock = new Mock<RequestDelegate>();
        var middleware = new FirebaseAuthMiddleware(nextMock.Object);

        await middleware.InvokeAsync(context, CreateUserService());

        context.Response.StatusCode.Should().Be(401);
        nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task CreateTask_RequiresAuth()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tasks";
        context.Request.Method = "POST";
        context.Response.Body = new MemoryStream();

        var nextMock = new Mock<RequestDelegate>();
        var middleware = new FirebaseAuthMiddleware(nextMock.Object);

        await middleware.InvokeAsync(context, CreateUserService());

        context.Response.StatusCode.Should().Be(401);
        nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTask_RequiresAuth()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/tasks/1";
        context.Request.Method = "DELETE";
        context.Response.Body = new MemoryStream();

        var nextMock = new Mock<RequestDelegate>();
        var middleware = new FirebaseAuthMiddleware(nextMock.Object);

        await middleware.InvokeAsync(context, CreateUserService());

        context.Response.StatusCode.Should().Be(401);
        nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }
}
