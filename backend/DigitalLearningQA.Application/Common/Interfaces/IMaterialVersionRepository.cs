using DigitalLearningQA.Domain.Entities;

namespace DigitalLearningQA.Application.Common.Interfaces;

public interface IMaterialVersionRepository
{
    Task<List<MaterialVersion>> GetByMaterialIdAsync(Guid materialId);

    Task<MaterialVersion?> GetByIdAsync(Guid id);

    // Lấy số phiên bản lớn nhất hiện có
    Task<int> GetLatestVersionNumberAsync(Guid learningMaterialId);

    Task AddAsync(MaterialVersion version);

    Task CreateVersionAndSetCurrentAsync(MaterialVersion version, LearningMaterial material, CancellationToken cancellationToken = default);

    Task UpdateAsync(MaterialVersion version);

    Task DeleteAsync(MaterialVersion version);
}
