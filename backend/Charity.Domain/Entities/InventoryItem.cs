namespace Charity.Domain.Entities;

public class InventoryItem : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Kind { get; set; } = "صنف ملموس"; // "صنف ملموس" | "خدمة"
    public string Unit { get; set; } = "كيلو";
    public string Type { get; set; } = "أغذية جافة";
    public double Quantity { get; set; } = 0;
    public double MinAlert { get; set; } = 10;
    public string Notes { get; set; } = string.Empty;

    // Tenant Isolation
    public string CharityId { get; set; } = string.Empty;
    public Charity? Charity { get; set; }
}
