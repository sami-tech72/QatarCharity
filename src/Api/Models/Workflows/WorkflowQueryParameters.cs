namespace Api.Models.Workflows;

public class WorkflowQueryParameters
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }
}
