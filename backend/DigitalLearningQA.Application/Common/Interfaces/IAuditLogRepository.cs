using DigitalLearningQA.Domain.Entities;

namespace DigitalLearningQA.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
}