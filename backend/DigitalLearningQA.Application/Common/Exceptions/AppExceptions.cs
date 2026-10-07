namespace DigitalLearningQA.Application.Common.Exceptions;

public sealed class RequestValidationException(string message) : Exception(message);
public sealed class ResourceNotFoundException(string message) : Exception(message);
public sealed class ResourceForbiddenException(string message) : Exception(message);
public sealed class ConflictException(string message) : Exception(message);
