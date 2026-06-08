using FuncHealthDemo.Enum;

namespace FuncHealthDemo.Entities;

public class User
{
    public int Id { get; set; }
    public string FbUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public UserType Type { get; set; }
}
