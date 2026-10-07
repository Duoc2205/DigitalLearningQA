using DigitalLearningQA.Domain.Entities;

namespace DigitalLearningQA.Application.Common.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);

    Task<List<RefreshToken>> GetByUserIdAsync(Guid userId);

    Task AddAsync(RefreshToken token);

    Task UpdateAsync(RefreshToken token);

    Task UpdateRangeAsync(IEnumerable<RefreshToken> tokens);

    Task DeleteAsync(RefreshToken token);

    Task DeleteByUserIdAsync(Guid userId);
    
}