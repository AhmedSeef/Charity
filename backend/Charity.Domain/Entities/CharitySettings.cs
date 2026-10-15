namespace Charity.Domain.Entities;

public class CharitySettings : BaseEntity
{
    public string CharityId { get; set; } = string.Empty;
    public Charity? Charity { get; set; }
    public string CharityName { get; set; } = string.Empty;
    public string Logo { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}
