using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Domain.Entities;
using DigitalLearningQA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(x => x.User)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task<List<RefreshToken>> GetByUserIdAsync(Guid userId)
    {
        return await _context.RefreshTokens
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ExpiredAt)
            .ToListAsync();
    }

    public async Task AddAsync(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RefreshToken token)
    {
        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(RefreshToken token)
    {
        _context.RefreshTokens.Remove(token);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByUserIdAsync(Guid userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId)
            .ToListAsync();

        if (tokens.Count == 0)
            return;

        _context.RefreshTokens.RemoveRange(tokens);

        await _context.SaveChangesAsync();
    }
    public async Task UpdateRangeAsync(IEnumerable<RefreshToken> tokens)
    {
        _context.RefreshTokens.UpdateRange(tokens);

        await _context.SaveChangesAsync();
    }
}