using DigitalLearningQA.Application.Common.Constants;
using DigitalLearningQA.Application.Common.Exceptions;
using DigitalLearningQA.Application.Common.Helpers;
using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Features.MaterialVersions.Commands;
using DigitalLearningQA.Domain.Entities;
using DigitalLearningQA.Domain.Enums;
using MediatR;

namespace DigitalLearningQA.Application.Features.MaterialVersions.Handlers;

public class UploadMaterialVersionCommandHandler : IRequestHandler<UploadMaterialVersionCommand, Guid>
{
    private readonly IMaterialVersionRepository _repository;
    private readonly ILearningMaterialRepository _materialRepository;
    private readonly IFileStorageService _storage;

    public UploadMaterialVersionCommandHandler(
        IMaterialVersionRepository repository,
        ILearningMaterialRepository materialRepository,
        IFileStorageService storage)
    {
        _repository = repository;
        _materialRepository = materialRepository;
        _storage = storage;
    }

    public async Task<Guid> Handle(UploadMaterialVersionCommand request, CancellationToken cancellationToken)
    {
        var material = await _materialRepository.GetByIdAsync(request.LearningMaterialId);
        if (material is null)
            throw new ResourceNotFoundException("Không tìm thấy học liệu.");

        if (material.OwnerId != request.UploadedBy)
            throw new ResourceForbiddenException("Bạn không có quyền tải phiên bản cho học liệu này.");

        if (request.FileSize <= 0 || request.FileSize > FileSettings.MaxFileSize)
            throw new RequestValidationException("Dung lượng tệp vượt quá giới hạn cho phép.");

        var extension = Path.GetExtension(request.FileName);
        if (!FileHelper.IsAllowedExtension(extension))
            throw new RequestValidationException("Định dạng tệp chưa được hỗ trợ.");

        if (!await FileHelper.HasValidFileSignatureAsync(request.FileStream, extension, cancellationToken))
            throw new RequestValidationException("Nội dung tệp không khớp với phần mở rộng tệp.");

        var versionNumber = await _repository.GetLatestVersionNumberAsync(request.LearningMaterialId) + 1;
        var folder = FileHelper.GetFolder(extension);
        string? filePath = null;

        try
        {
            filePath = await _storage.SaveFileAsync(request.FileStream, request.FileName, folder, cancellationToken);
            var version = new MaterialVersion
            {
                LearningMaterialId = request.LearningMaterialId,
                VersionNumber = versionNumber,
                FileName = request.FileName,
                FilePath = filePath,
                MimeType = FileHelper.GetMimeType(extension),
                FileSize = request.FileSize,
                UploadedBy = request.UploadedBy,
                UploadDate = DateTime.UtcNow,
                Status = MaterialStatus.Draft,
                Remark = request.Remark
            };

            await _repository.CreateVersionAndSetCurrentAsync(version, material, cancellationToken);
            return version.Id;
        }
        catch (ConflictException)
        {
            if (filePath is not null)
                await _storage.DeleteFileAsync(filePath);
            throw;
        }
        catch
        {
            if (filePath is not null)
                await _storage.DeleteFileAsync(filePath);
            throw;
        }
    }
}
