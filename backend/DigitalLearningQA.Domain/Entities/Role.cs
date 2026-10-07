using DigitalLearningQA.Domain.Common;

namespace DigitalLearningQA.Domain.Entities;

public class Role : BaseEntity
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}   