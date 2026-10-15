namespace Charity.Application.DTOs.SuspensionReasons;

public class SuspensionReasonDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string CharityId { get; set; } = string.Empty;
    public string CharityName { get; set; } = string.Empty;
    public int LinkedSuspendedBeneficiariesCount { get; set; } = 0;
    public bool CanDelete { get; set; } = true;
}

public class CreateSuspensionReasonDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
