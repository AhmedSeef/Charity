using Charity.Application.DTOs.Audit;
namespace Charity.Application.DTOs.SeparationReasons;

public class SeparationReasonDto : AuditedDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string CharityId { get; set; } = string.Empty;
    public string CharityName { get; set; } = string.Empty;
    public int LinkedOperationsCount { get; set; } = 0;
    public bool CanDelete { get; set; } = true;
}

public class CreateSeparationReasonDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
