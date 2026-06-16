namespace SkyRoute.Application.Common;

public class PricingResult
{
    public decimal BaseFare { get; set; }
    public decimal PricePerPassenger { get; set; }
    public decimal TotalPrice { get; set; }
    public string PricingRule { get; set; } = string.Empty;
}
