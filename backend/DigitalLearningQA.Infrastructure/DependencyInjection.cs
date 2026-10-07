using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Infrastructure.Persistence;
using DigitalLearningQA.Infrastructure.Repositories;
using DigitalLearningQA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalLearningQA.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"));
        });

        // Repository
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ILearningMaterialRepository, LearningMaterialRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IMaterialVersionRepository, MaterialVersionRepository>();
        services.AddScoped<ICLORepository, CLORepository>();

        // Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IContentExtractionService, ContentExtractionService>();
        services.AddHttpClient<ISemanticAssessmentService, OpenAiSemanticAssessmentService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(90);
        });
        return services;
    }
}
