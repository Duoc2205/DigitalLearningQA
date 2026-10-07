using DigitalLearningQA.Domain.Entities;

namespace DigitalLearningQA.Application.Common.Interfaces;

public interface ICourseRepository
{
    Task<List<Course>> GetAllAsync();

    Task<Course?> GetByIdAsync(Guid id);

    Task AddAsync(Course course);

    Task UpdateAsync(Course course);

    Task DeleteAsync(Course course);
}