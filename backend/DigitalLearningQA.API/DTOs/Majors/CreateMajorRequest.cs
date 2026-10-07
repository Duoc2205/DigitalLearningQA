namespace DigitalLearningQA.API.DTOs.Majors;

public sealed record CreateMajorRequest(Guid DepartmentId, string Code, string Name);
