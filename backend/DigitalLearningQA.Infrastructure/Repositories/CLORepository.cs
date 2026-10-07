using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Domain.Entities;
using DigitalLearningQA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.Infrastructure.Repositories;

public class CLORepository : ICLORepository
{
    private readonly ApplicationDbContext _context;

    public CLORepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CLO>> GetAllAsync()
    {
        return await _context.CLOs
            .Include(x => x.Course)
            .OrderBy(x => x.Course.Name)
            .ThenBy(x => x.Code)
            .ToListAsync();
    }

    public async Task<List<CLO>> GetByCourseIdAsync(Guid courseId)
    {
        return await _context.CLOs
            .Where(x => x.CourseId == courseId)
            .OrderBy(x => x.Code)
            .ToListAsync();
    }

    public async Task<CLO?> GetByIdAsync(Guid id)
    {
        return await _context.CLOs
            .Include(x => x.Course)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsAsync(
        Guid courseId,
        string code)
    {
        return await _context.CLOs.AnyAsync(x =>
            x.CourseId == courseId &&
            x.Code == code);
    }

    public async Task AddAsync(CLO clo)
    {
        await _context.CLOs.AddAsync(clo);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CLO clo)
    {
        _context.CLOs.Update(clo);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(CLO clo)
    {
        _context.CLOs.Remove(clo);
        await _context.SaveChangesAsync();
    }
}