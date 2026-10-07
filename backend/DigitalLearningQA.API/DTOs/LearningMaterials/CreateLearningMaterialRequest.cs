namespace DigitalLearningQA.API.DTOs.LearningMaterials;

public sealed record CreateLearningMaterialRequest(
    string Code, Guid CourseId, string Title, string? Description, int MaterialType);
