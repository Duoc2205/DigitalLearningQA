using DigitalLearningQA.Domain.Entities;
using MediatR;

namespace DigitalLearningQA.Application.Features.CLOs.Queries.GetAllCLOs;

public class GetAllCLOQuery : IRequest<List<CLO>>
{
}