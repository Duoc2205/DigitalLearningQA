namespace DigitalLearningQA.API.DTOs.Users;

public sealed class CreateUserRequest
{
    public required string Username { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required Guid DepartmentId { get; init; }
    // Chỉ nhận CCD hoặc Lecturer; Admin không thể được tạo qua endpoint này.
    public required string Role { get; init; }
}
