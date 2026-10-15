namespace Charity.Domain.Entities;

public class Donation : BaseEntity
{
    public string DonorName { get; set; } = string.Empty;
    public string DonorPhone { get; set; } = string.Empty;
    public string DonationKind { get; set; } = "نقدي"; // "نقدي" | "عيني" | "خدمة"
    public decimal? Amount { get; set; }
    public string CashCategory { get; set; } = "زكاة مال";
    public string PaymentMethod { get; set; } = "نقداً بالخزينة";

    // In-Kind or Service Linkage
    public string? InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public string? ItemName { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal? EstimatedValue { get; set; }

    public string Date { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string Notes { get; set; } = string.Empty;

    // Tenant Isolation
    public string CharityId { get; set; } = string.Empty;
    public Charity? Charity { get; set; }
}
