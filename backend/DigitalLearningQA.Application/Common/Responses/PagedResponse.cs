namespace DigitalLearningQA.Application.Common.Responses;

public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; }
        = Enumerable.Empty<T>();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }

    public bool Success { get; set; } = true;

    public string Message { get; set; } = "Success";
}