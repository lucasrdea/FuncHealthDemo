using FuncHealthDemo.DB;
using FuncHealthDemo.Entities;
using FuncHealthDemo.Enum;
using FuncHealthDemo.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FuncHealthDemo.Services;

public class UserService
{
    private readonly DataContext _db;

    public UserService(DataContext db)
    {
        _db = db;
    }

    public async Task<User?> GetUserByFbUserIdAsync(string fbUserId)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.FbUserId == fbUserId);
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _db.Users.FindAsync(id);
    }

    public async Task<User> CreateUserAsync(string fbUserId, string name, string email, string phoneNumber, DateTime dateOfBirth, UserType type)
    {
        // Check if FbUserId already exists
        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.FbUserId == fbUserId);
        if (existingUser != null)
        {
            throw new ValidationException($"User with Firebase UID '{fbUserId}' already exists.");
        }

        // Check if email already exists
        var existingEmail = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingEmail != null)
        {
            throw new ValidationException($"User with email '{email}' already exists.");
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(fbUserId))
        {
            throw new ValidationException("Firebase User ID is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException("Email is required.");
        }

        var user = new User
        {
            FbUserId = fbUserId,
            Name = name,
            Email = email,
            PhoneNumber = phoneNumber,
            DateOfBirth = dateOfBirth,
            Type = type
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return user;
    }

    public async Task<User> UpdateProfileAsync(int userId, string? phoneNumber, DateTime? dateOfBirth)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            throw new ValidationException("User not found.");
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ValidationException("Phone number must contain exactly 11 digits.");
        }
            // Update phone number if provided
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            // Validate phone number contains exactly 11 digits
            var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length != 11)
            {
                throw new ValidationException("Phone number must contain exactly 11 digits.");
            }

            user.PhoneNumber = phoneNumber;
        }

        // Update date of birth if provided
        if (dateOfBirth.HasValue)
        {
            // Validate date of birth is in the past
            if (dateOfBirth.Value > DateTime.UtcNow)
            {
                throw new ValidationException("Date of birth must be in the past.");
            }

            user.DateOfBirth = dateOfBirth.Value;
        }

        await _db.SaveChangesAsync();
        return user;
    }
}
