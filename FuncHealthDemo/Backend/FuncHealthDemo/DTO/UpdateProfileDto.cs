namespace FuncHealthDemo.DTO
{
    public record UpdateProfileDto(
        string? PhoneNumber,
        DateTime? DateOfBirth
    );
}
