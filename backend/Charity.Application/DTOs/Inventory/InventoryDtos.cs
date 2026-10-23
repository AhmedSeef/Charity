using Charity.Application.DTOs.Audit;
namespace Charity.Application.DTOs.Inventory;

public class InventoryItemDto : AuditedDto
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Kind { get; set; } = "صنف ملموس";
    public string Unit { get; set; } = "كيلو";
    public string Type { get; set; } = "أغذية جافة";
    public double Quantity { get; set; }
    public double MinAlert { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string CharityId { get; set; } = string.Empty;
}

public class CreateInventoryItemDto
{
    public string? Code { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Kind { get; set; } = "صنف ملموس";
    public string Unit { get; set; } = "كيلو";
    public string Type { get; set; } = "أغذية جافة";
    public double Quantity { get; set; } = 0;
    public double MinAlert { get; set; } = 10;
    public string Notes { get; set; } = string.Empty;
}

public class UpdateInventoryItemDto
{
    public string Code { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Kind { get; set; } = "صنف ملموس";
    public string Unit { get; set; } = "كيلو";
    public string Type { get; set; } = "أغذية جافة";
    // Omitted quantity means catalog metadata update; stock changes use adjust-stock.
    public double? Quantity { get; set; }
    public double MinAlert { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class AdjustStockDto
{
    public double NewQuantity { get; set; }
    public string Reason { get; set; } = "جرد مخزني دوري";
}
