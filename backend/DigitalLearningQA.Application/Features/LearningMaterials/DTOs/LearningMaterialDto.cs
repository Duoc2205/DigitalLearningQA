namespace DigitalLearningQA.Application.Features.LearningMaterials.DTOs;

public class LearningMaterialDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public Guid OwnerId { get; set; }

    public string OwnerName { get; set; } = string.Empty;

    public int MaterialType { get; set; }

    public int Status { get; set; }

    public Guid? CurrentVersionId { get; set; }
}