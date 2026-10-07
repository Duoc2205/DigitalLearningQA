using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

using DigitalLearningQA.Application;
using DigitalLearningQA.Application.Common.Constants;
using DigitalLearningQA.Application.Common.Exceptions;
using DigitalLearningQA.Application.Common.Interfaces;

using DigitalLearningQA.Infrastructure;
using DigitalLearningQA.Infrastructure.Persistence;
using DigitalLearningQA.Infrastructure.Persistence.Seed;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// =========================
// Register Services
// =========================

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });

// =========================
// File Upload Configuration
// =========================

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit =
        FileSettings.MaxFileSize + 1024 * 1024;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize =
        FileSettings.MaxFileSize + 1024 * 1024;
});

// =========================
// Application
// =========================

builder.Services.AddApplication();

// =========================
// Infrastructure
// =========================

builder.Services.AddInfrastructure(builder.Configuration);

// =========================
// Register IApplicationDbContext
// =========================

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

// =========================
// Swagger
// =========================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DigitalLearningQA API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Bearer {JWT Token}",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                },
                Array.Empty<string>()
            }
        });
});

// =========================
// JWT Authentication
// =========================

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = builder.Configuration["Jwt:Issuer"],

                ValidAudience = builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!))
            };
    });

// =========================
// Authorization
// =========================

builder.Services.AddAuthorization();

// Frontend chạy độc lập trong thư mục frontend (Vite mặc định cổng 5173).
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

// =========================
// Debug MediatR Handlers
// =========================

var handlers = Assembly
    .Load("DigitalLearningQA.Application")
    .GetTypes()
    .Where(t => t.Name.EndsWith("Handler"));

Console.WriteLine("===== Registered Handlers =====");

foreach (var handler in handlers)
{
    Console.WriteLine(handler.FullName);
}

Console.WriteLine("===============================");

// =========================
// Build
// =========================

var app = builder.Build();

// =========================
// Global Exception Handler
// =========================

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception =
            context.Features
                .Get<IExceptionHandlerFeature>()?
                .Error;

        var status = exception switch
        {
            RequestValidationException =>
                StatusCodes.Status400BadRequest,

            ResourceNotFoundException =>
                StatusCodes.Status404NotFound,

            ResourceForbiddenException =>
                StatusCodes.Status403Forbidden,

            ConflictException =>
                StatusCodes.Status409Conflict,

            _ =>
                StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            status,
            message = exception?.Message,
            exception = exception?.GetType().FullName,
            stackTrace = exception?.StackTrace,
            innerException = exception?.InnerException?.Message,
            innerStackTrace = exception?.InnerException?.StackTrace
        });
    });
});

// =========================
// Seed Database
// =========================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    // Đồng bộ schema trước khi đọc/ghi dữ liệu, tránh lỗi cột/bảng thiếu khi
    // triển khai phiên bản mới (ví dụ Courses.MajorId).
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

// =========================
// Middleware
// =========================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
