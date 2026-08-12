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
        public string stripeId { get; set; } = string.Empty;
        public string paymentStatus { get; set; } = string.Empty;

        public int? ReservationId { get; set; }

        // "Stripe" or "Manual" - lets reports split self-checkout from staff-entered payments.
        public string PaymentSource { get; set; } = string.Empty;

        // "Stripe", "Cash", "Check", or "ManualCard".
        public string PaymentMethod { get; set; } = string.Empty;

        // Employees.employeeId of the staff member who recorded a manual payment; null for Stripe.
        public int? RecordedByEmployeeId { get; set; }

        public string? CheckNumber { get; set; }
        public string? CardLast4 { get; set; }
        public string? CardAuthReference { get; set; }
    }
}
