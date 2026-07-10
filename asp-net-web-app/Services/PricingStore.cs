public static class PricingStore
{
    public static decimal BaseRate { get; set; } = 45.00m;
    public static decimal SeasonMult { get; set; } = 1.0m;
    public static decimal LargeSiteMult { get; set; } = 1.0m;
    public static decimal UtilMult { get; set; } = 1.0m;
    public static DateTime UpdatedAt { get; set; } = DateTime.Now;
}