using DigitalLearningQA.Domain.Entities;

namespace DigitalLearningQA.Application.Common.Interfaces;

public interface ICLORepository
{
    Task<List<CLO>> GetAllAsync();

    Task<List<CLO>> GetByCourseIdAsync(Guid courseId);

    Task<CLO?> GetByIdAsync(Guid id);

    Task<bool> ExistsAsync(
        Guid courseId,
        string code);

    Task AddAsync(CLO clo);

    Task UpdateAsync(CLO clo);

    Task DeleteAsync(CLO clo);
}