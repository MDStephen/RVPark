using System;
using System.ComponentModel.DataAnnotations;

namespace asp_net_web_app.Data
{
    // Singleton at the application layer — enforce single-row usage
    // in whatever service/repository reads/writes this table.
    public class Pricing
    {
        [Key]
        public int pricingId { get; set; }

        public decimal baseNightlyRate { get; set; }
        public decimal seasonMultiplier { get; set; }
        public decimal largeSiteMultiplier { get; set; }
        public decimal utilityMultiplier { get; set; }
        public DateTime lastUpdated { get; set; }
        public decimal cancellationFee { get; set; }
        public decimal earlyCheckInFee { get; set; }
        public decimal lateCheckOutFee { get; set; }
        public decimal specialEventMultiplier { get; set; }
    }
}
