using DigitalLearningQA.Domain.Entities;

namespace DigitalLearningQA.Application.Common.Interfaces;

public interface ILearningMaterialRepository
{
    Task<List<LearningMaterial>> GetAllAsync();

    Task<LearningMaterial?> GetByIdAsync(Guid id);

    Task AddAsync(LearningMaterial material);

    Task UpdateAsync(LearningMaterial material);

    Task DeleteAsync(LearningMaterial material);
}