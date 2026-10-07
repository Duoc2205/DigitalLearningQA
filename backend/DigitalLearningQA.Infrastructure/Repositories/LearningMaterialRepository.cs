using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Domain.Entities;
using DigitalLearningQA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.Infrastructure.Repositories;

public class LearningMaterialRepository : ILearningMaterialRepository
{
    private readonly ApplicationDbContext _context;

    public LearningMaterialRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LearningMaterial>> GetAllAsync()
    {
        return await _context.LearningMaterials
            .Include(x => x.Course)
            .Include(x => x.Owner)
            .Include(x => x.CurrentVersion)
            .ToListAsync();
    }

    public async Task<LearningMaterial?> GetByIdAsync(Guid id)
    {
        return await _context.LearningMaterials
            .Include(x => x.Course)
            .Include(x => x.Owner)
            .Include(x => x.Versions)
            .Include(x => x.CurrentVersion)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(LearningMaterial material)
    {
        Console.WriteLine("========== ADD ==========");
        Console.WriteLine(material.Code);

        await _context.LearningMaterials.AddAsync(material);

        Console.WriteLine("Before Save:");
        Console.WriteLine(_context.ChangeTracker.DebugView.ShortView);

        await _context.SaveChangesAsync();

        Console.WriteLine("After Save");
        Console.WriteLine(material.Id);
    }

    public async Task UpdateAsync(LearningMaterial material)
    {
        _context.LearningMaterials.Update(material);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(LearningMaterial material)
    {
        _context.LearningMaterials.Remove(material);
        await _context.SaveChangesAsync();
    }
}