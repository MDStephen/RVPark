using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using asp_net_web_app.Data;

namespace asp_net_web_app.Pages.Payment
{
    public class AlternativePaymentModel : PageModel
    {
        private readonly DatabaseWrapper _db;

        public AlternativePaymentModel(DatabaseWrapper db)
        {
            _db = db;
        }
        // Bound properties from the form
        [BindProperty]
        public int ReservationId { get; set; }
        [BindProperty]
        public string PaymentType { get; set; } = "";
        [BindProperty]
        public decimal AmountDue { get; set; }
        [BindProperty]
        public decimal AmountReceived { get; set; }
        [BindProperty]
        public decimal ChangeOwed { get; set; }
        [BindProperty]
        public string CheckNumber { get; set; } = "";
        public string Message { get; set; } = "";
        public void OnGet()
        {

        }
        public IActionResult OnPost()
        {
            // Validate reservation exists
            var reservation = _db.Reservations.Find(ReservationId);
            if (reservation == null)
            {
                ModelState.AddModelError("", "Reservation ID not found.");
                return Page();
            }
            if (string.IsNullOrWhiteSpace(PaymentType))
            {
                ModelState.AddModelError("", "Please select a payment method.");
                return Page();
            }
            // Handle Check Payments
            if (PaymentType == "Check")
            {
                if (string.IsNullOrWhiteSpace(CheckNumber))
                {
                    ModelState.AddModelError("", "Check number is required for check payments.");
                    return Page();
                }

                CreatePaymentRecord(reservation, reservation.TotalCost, CheckNumber);

                Message = $"Check payment recorded. Check #: {CheckNumber}";
                return Page();
            }
            // Handle Cash Payments
            if (PaymentType == "Cash")
            {
                if (AmountDue <= 0)
                {
                    ModelState.AddModelError("", "Amount due must be greater than zero.");
                    return Page();
                }
                if (AmountReceived <= 0)
                {
                    ModelState.AddModelError("", "Amount received must be greater than zero.");
                    return Page();
                }
                ChangeOwed = AmountReceived - AmountDue;
                if (ChangeOwed < 0)
                {
                    ModelState.AddModelError("", "Amount received is less than amount due.");
                    return Page();
                }
                CreatePaymentRecord(reservation, AmountDue);
                Message = $"Cash payment recorded. Change owed: ${ChangeOwed:F2}";
                return Page();
            }
            // Handle Manual Credit Card Entry
            if (PaymentType == "ManualCard")
            {
                // Redirect to secure manual card entry page
                return RedirectToPage("Payment/Success");   //
            }
            ModelState.AddModelError("", "Unknown payment type.");
            return Page();
        }
        private Payment CreatePaymentRecord(Reservations reservations, decimal amount, string paymentID, string status = "Paid")
        {
            var payment = new Payment
            {
                amount = amount,
                paidAt = DateTime.Now,
                stripeId = stripeId,
                paymentStatus = status,
                ReservationId = reservation.Id
            };

            _db.Payments.Add(payment);

            ///reservation.Status = "Paid";   // sshould we have a paid status?
            _db.SaveChanges();

            return payment;
        }
    }
}