using System;
using System.ComponentModel.DataAnnotations;

namespace asp_net_web_app.Data
{
    public class Payment
    {
        [Key]
        public int paymentId { get; set; }

        public decimal amount { get; set; }
        public DateTime paidAt { get; set; }
        public string stripeId { get; set; }
        public string paymentStatus { get; set; }
    }
}
