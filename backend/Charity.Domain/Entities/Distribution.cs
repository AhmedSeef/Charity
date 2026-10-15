namespace Charity.Domain.Entities;

public class Distribution : BaseEntity
{
    public string BeneficiaryId { get; set; } = string.Empty;
    public Beneficiary? Beneficiary { get; set; }

    public string? InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }

    public string ItemName { get; set; } = string.Empty;
    public string Kind { get; set; } = "صنف ملموس"; // "صنف ملموس" | "خدمة" | "إعانة نقدية"
    public double Quantity { get; set; } = 1;
    public string Unit { get; set; } = string.Empty;
    public decimal? Amount { get; set; }

    public string Date { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string Notes { get; set; } = string.Empty;

    // Granting Charity (Stamped upon creation)
    public string CharityId { get; set; } = string.Empty;
    public string CharityName { get; set; } = string.Empty;
    public Charity? Charity { get; set; }
}
