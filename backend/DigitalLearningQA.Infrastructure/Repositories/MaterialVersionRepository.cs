using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Common.Exceptions;
using DigitalLearningQA.Domain.Entities;
using DigitalLearningQA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.Infrastructure.Repositories;

public class MaterialVersionRepository : IMaterialVersionRepository
{
    private readonly ApplicationDbContext _context;

    public MaterialVersionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MaterialVersion>> GetByMaterialIdAsync(Guid materialId)
    {
        return await _context.MaterialVersions
            .Include(x => x.UploadedUser)
            .Where(x => x.LearningMaterialId == materialId)
            .OrderByDescending(x => x.VersionNumber)
            .ToListAsync();
    }

    public async Task<MaterialVersion?> GetByIdAsync(Guid id)
    {
        return await _context.MaterialVersions
            .Include(x => x.UploadedUser)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    // Lấy version lớn nhất hiện có
    public async Task<int> GetLatestVersionNumberAsync(Guid learningMaterialId)
    {
        return await _context.MaterialVersions
            .Where(x => x.LearningMaterialId == learningMaterialId)
            .Select(x => (int?)x.VersionNumber)
            .MaxAsync() ?? 0;
    }

    public async Task AddAsync(MaterialVersion version)
    {
        await _context.MaterialVersions.AddAsync(version);
        await _context.SaveChangesAsync();
    }

    public async Task CreateVersionAndSetCurrentAsync(MaterialVersion version, LearningMaterial material, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _context.MaterialVersions.AddAsync(version, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            material.CurrentVersionId = version.Id;
            _context.LearningMaterials.Update(material);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConflictException("Another version was uploaded at the same time. Please try again.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateAsync(MaterialVersion version)
    {
        _context.MaterialVersions.Update(version);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(MaterialVersion version)
    {
        _context.MaterialVersions.Remove(version);
        await _context.SaveChangesAsync();
    }
}
