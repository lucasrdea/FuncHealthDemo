namespace FuncHealthDemo.DTO
{
    public record RegisterUserRequest(
        string Uid,
        string FirstName,
        string LastName,
        string Email
    );
}
