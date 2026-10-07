using MediatR;
using System.Text.Json.Serialization;

namespace DigitalLearningQA.Application.Features.TestRequests.Commands.CompleteTestRequest;

public class CompleteTestRequestCommand : IRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }

    public string Conclusion { get; set; } = string.Empty;
}