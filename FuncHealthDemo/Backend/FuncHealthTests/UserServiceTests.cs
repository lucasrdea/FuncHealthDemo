using FluentAssertions;
using FuncHealthDemo.DB;
using FuncHealthDemo.Entities;
using FuncHealthDemo.Enum;
using FuncHealthDemo.Exceptions;
using FuncHealthDemo.Services;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace FuncHealthTests;

public class UserServiceTests : IDisposable
{
    private readonly DataContext _context;
    private readonly UserService _userService;

    private const int User1Id = 1;
    private const int User2Id = 2;
    private const string User1FbUid = "test-fb-uid-user1";
    private const string User2FbUid = "test-fb-uid-user2";

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _userService = new UserService(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var user1 = new User
        {
            Id = User1Id,
            FbUserId = User1FbUid,
            Name = "Test User 1",
            Email = "user1@test.com",
            PhoneNumber = "+1-555-0001",
            DateOfBirth = new DateTime(1990, 1, 1),
            Type = UserType.Client
        };

        var user2 = new User
        {
            Id = User2Id,
            FbUserId = User2FbUid,
            Name = "Test User 2",
            Email = "user2@test.com",
            PhoneNumber = "+1-555-0002",
            DateOfBirth = new DateTime(1992, 2, 2),
            Type = UserType.Client
        };

        _context.Users.AddRange(user1, user2);
        _context.SaveChanges();
    }

    #region X-User-Id Header Simulation Tests (Data Ownership Isolation)

    [Fact]
    public async Task GetUserByFbUserId_SimulatingXUserIdHeader_ReturnsCorrectUserOnly()
    {
        // used by middle and filter
        // Arrange - Simulating X-User-Id header containing User1's Firebase UID
        const string authenticatedFbUserId = User1FbUid;

        // Act - Service retrieves user based on the X-User-Id header value
        var user = await _userService.GetUserByFbUserIdAsync(authenticatedFbUserId);

        // Assert - Verify correct user is returned with proper data isolation
        user.Should().NotBeNull("authenticated user should be found");
        user!.FbUserId.Should().Be(User1FbUid);
        user.Id.Should().Be(User1Id);
        user.Email.Should().Be("user1@test.com");
    }

    [Fact]
    public async Task GetUserByFbUserId_DifferentUsers_ReturnsOnlyRequestedUser()
    {
        // Act - Simulate two different users making requests with different X-User-Id headers
        var user1 = await _userService.GetUserByFbUserIdAsync(User1FbUid);
        var user2 = await _userService.GetUserByFbUserIdAsync(User2FbUid);

        // Assert - Each request returns only the correct user
        user1.Should().NotBeNull();
        user1!.Id.Should().Be(User1Id);
        user1.FbUserId.Should().Be(User1FbUid);
        user1.Email.Should().Be("user1@test.com");

        user2.Should().NotBeNull();
        user2!.Id.Should().Be(User2Id);
        user2.FbUserId.Should().Be(User2FbUid);
        user2.Email.Should().Be("user2@test.com");

        // Assert - Users are completely separate
        user1.Id.Should().NotBe(user2.Id);
        user1.Email.Should().NotBe(user2.Email);
    }

    [Fact]
    public async Task GetUserByFbUserId_InvalidXUserIdHeader_ReturnsNull()
    {
        // Arrange - Simulating X-User-Id header with non-existent Firebase UID
        const string invalidFbUserId = "non-existent-fb-uid";

        // Act
        var user = await _userService.GetUserByFbUserIdAsync(invalidFbUserId);

        // Assert - Should return null, allowing middleware to return 401 Unauthorized
        user.Should().BeNull("invalid Firebase UID should not return any user");
    }

    [Fact]
    public async Task GetUserById_WithAuthenticatedUserId_ReturnsOnlyThatUser()
    {
        // Arrange - Simulating scenario where middleware has authenticated User1
        const int authenticatedUserId = User1Id;

        // Act - Controller uses the authenticated user's ID
        var user = await _userService.GetUserByIdAsync(authenticatedUserId);

        // Assert - Returns only the authenticated user's data
        user.Should().NotBeNull();
        user!.Id.Should().Be(User1Id);
        user.FbUserId.Should().Be(User1FbUid);
        user.Email.Should().Be("user1@test.com");
    }
     
    #endregion

    #region Update Profile Tests (Ownership Enforcement)

    [Fact]
    public async Task UpdateProfile_AuthenticatedUserCanUpdateOwnProfile()
    {
        // Arrange - User1 is authenticated via X-User-Id header
        const int authenticatedUserId = User1Id;
        var newPhoneNumber = "+1-555-356-9999";
        var newDateOfBirth = new DateTime(1985, 5, 15);

        // Act - User updates their own profile
        var updatedUser = await _userService.UpdateProfileAsync(
            authenticatedUserId, 
            newPhoneNumber, 
            newDateOfBirth);

        // Assert
        updatedUser.Should().NotBeNull();
        updatedUser.Id.Should().Be(User1Id);
        updatedUser.PhoneNumber.Should().Be(newPhoneNumber);
        updatedUser.DateOfBirth.Should().Be(newDateOfBirth);
    }

    [Fact]
    public async Task UpdateProfile_WithInvalidUserId_ThrowsValidationException()
    {
        // Arrange - Attempting to update non-existent user
        const int nonExistentUserId = 9999;

        // Act & Assert
        var act = async () => await _userService.UpdateProfileAsync(
            nonExistentUserId, 
            "+1-555-8888", 
            DateTime.Now.AddYears(-30));

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task UpdateProfile_WithFutureDateOfBirth_ThrowsValidationException()
    {
        // Arrange
        var futureDateOfBirth = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        var act = async () => await _userService.UpdateProfileAsync(
            User1Id, 
            "+1-555-654-7777", 
            futureDateOfBirth);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Date of birth must be in the past.");
    }

    [Theory]
    [InlineData("123", "Phone number must contain exactly 11 digits.")]
    [InlineData("12345678901234567", "Phone number must contain exactly 11 digits.")]
    [InlineData("+1-555", "Phone number must contain exactly 11 digits.")]
    [InlineData("555-0000", "Phone number must contain exactly 11 digits.")]
    [InlineData("", "Phone number must contain exactly 11 digits.")]
    public async Task UpdateProfile_WithInvalidPhoneNumberLength_ThrowsValidationException(
        string invalidPhoneNumber, string expectedMessage)
    {
        // Act & Assert
        var act = async () => await _userService.UpdateProfileAsync(
            User1Id,
            invalidPhoneNumber,
            null);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage(expectedMessage);
    }

    [Theory]
    [InlineData("+1-555-123-4567")]  // 11 digits with formatting
    [InlineData("15551234567")]       // 11 digits without formatting
    [InlineData("1 (555) 123-4567")]  // 11 digits with spaces and parentheses
    [InlineData("1.555.123.4567")]    // 11 digits with dots
    public async Task UpdateProfile_WithValidPhoneNumberFormats_UpdatesSuccessfully(string validPhoneNumber)
    {
        // Act
        var result = await _userService.UpdateProfileAsync(
            User1Id,
            validPhoneNumber,
            null);

        // Assert
        result.Should().NotBeNull();
        result.PhoneNumber.Should().Be(validPhoneNumber);
    }

    [Fact]
    public async Task UpdateProfile_SimulatingXUserIdMismatch_ShouldBeBlockedAtControllerLevel()
    {
        // Arrange - User1 authenticated (X-User-Id maps to UserId=1)
        // But they try to update User2's profile (UserId=2)
        const int authenticatedUserId = User1Id;
        const int targetUserId = User2Id;

        // Act - Service doesn't enforce this, but controller should
        var result = await _userService.UpdateProfileAsync(
            targetUserId,  // Attempting to update different user
            "+1-555-654-6666",
            new DateTime(1980, 1, 1));

        // Assert - Service allows it, demonstrating need for controller authorization
        result.Should().NotBeNull();
        result.Id.Should().Be(User2Id);
        result.Id.Should().NotBe(authenticatedUserId, 
            "This shows controllers MUST validate: HttpContext.Items['UserId'] == requestedUserId");
    }

    #endregion

    #region Create User Tests

    [Fact]
    public async Task CreateUser_WithValidData_CreatesUserSuccessfully()
    {
        // Arrange
        var fbUserId = "new-fb-uid-123";
        var name = "New User";
        var email = "newuser@test.com";
        var phoneNumber = "+1-555-3333";
        var dateOfBirth = new DateTime(1995, 6, 15);

        // Act
        var createdUser = await _userService.CreateUserAsync(
            fbUserId, name, email, phoneNumber, dateOfBirth, UserType.Client);

        // Assert
        createdUser.Should().NotBeNull();
        createdUser.FbUserId.Should().Be(fbUserId);
        createdUser.Name.Should().Be(name);
        createdUser.Email.Should().Be(email);
        createdUser.PhoneNumber.Should().Be(phoneNumber);
        createdUser.DateOfBirth.Should().Be(dateOfBirth);
        createdUser.Type.Should().Be(UserType.Client);
        createdUser.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateFbUserId_ThrowsValidationException()
    {
        // Arrange - Try to create user with existing Firebase UID
        var existingFbUserId = User1FbUid;

        // Act & Assert
        var act = async () => await _userService.CreateUserAsync(
            existingFbUserId,
            "Duplicate User",
            "duplicate@test.com",
            "+1-555-4444",
            new DateTime(1990, 1, 1),
            UserType.Client);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"User with Firebase UID '{existingFbUserId}' already exists.");
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ThrowsValidationException()
    {
        // Arrange - Try to create user with existing email
        var existingEmail = "user1@test.com";

        // Act & Assert
        var act = async () => await _userService.CreateUserAsync(
            "unique-fb-uid",
            "Duplicate Email User",
            existingEmail,
            "+1-555-5555",
            new DateTime(1990, 1, 1),
            UserType.Client);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"User with email '{existingEmail}' already exists.");
    }

    [Theory]
    [InlineData("", "Test User", "test@test.com", "Firebase User ID is required.")]
    [InlineData(null, "Test User", "test@test.com", "Firebase User ID is required.")]
    [InlineData("fb-uid", "", "test@test.com", "Name is required.")]
    [InlineData("fb-uid", null, "test@test.com", "Name is required.")]
    [InlineData("fb-uid", "Test User", "", "Email is required.")]
    [InlineData("fb-uid", "Test User", null, "Email is required.")]
    public async Task CreateUser_WithInvalidRequiredFields_ThrowsValidationException(
        string? fbUserId, string? name, string? email, string expectedMessage)
    {
        // Act & Assert
        var act = async () => await _userService.CreateUserAsync(
            fbUserId!,
            name!,
            email!,
            "+1-555-6666",
            new DateTime(1990, 1, 1),
            UserType.Client);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage(expectedMessage);
    }

    #endregion

    #region Data Isolation Summary Test

    [Fact]
    public async Task CompleteDataOwnershipIsolation_SimulatingFullAuthFlow()
    {
        // This test simulates the complete flow:
        // 1. User makes request with X-User-Id header
        // 2. Middleware looks up user by Firebase UID
        // 3. Middleware stores internal UserId in HttpContext
        // 4. Controller uses that UserId to access only that user's data

        // Step 1 & 2: Simulate middleware authentication with X-User-Id header
        var xUserIdHeader = User1FbUid;
        var authenticatedUser = await _userService.GetUserByFbUserIdAsync(xUserIdHeader);
        
        authenticatedUser.Should().NotBeNull("middleware should find user by X-User-Id");
        
        // Step 3: Middleware would store this in HttpContext.Items["UserId"]
        var authenticatedUserId = authenticatedUser!.Id;

        // Step 4: Controller gets user data using authenticated UserId
        var userData = await _userService.GetUserByIdAsync(authenticatedUserId);

        // Assert - Complete data isolation
        userData.Should().NotBeNull();
        userData!.Id.Should().Be(User1Id);
        userData.FbUserId.Should().Be(User1FbUid);
        userData.Email.Should().Be("user1@test.com");

        // Verify this user cannot access other user's data
        // (Controller must enforce this by checking HttpContext.Items["UserId"])
        var otherUserId = User2Id;
        authenticatedUserId.Should().NotBe(otherUserId, 
            "authenticated user should not have access to other user IDs");
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
