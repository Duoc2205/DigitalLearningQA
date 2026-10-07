namespace DigitalLearningQA.API.DTOs.LearningMaterials;

public sealed record UpdateLearningMaterialRequest(Guid CourseId, string Title, string? Description, int MaterialType);
