using DigitalLearningQA.Domain.Entities;

namespace DigitalLearningQA.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);

    Task<User?> GetByIdAsync(Guid id);

    Task AddAsync(User user);

    Task UpdateAsync(User user);

    Task DeleteAsync(User user);

    Task<List<User>> GetAllAsync();
}