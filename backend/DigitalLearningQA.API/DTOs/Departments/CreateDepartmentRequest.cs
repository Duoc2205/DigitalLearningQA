namespace DigitalLearningQA.API.DTOs.Departments;

public sealed record CreateDepartmentRequest(string Code, string Name, string? Description);
