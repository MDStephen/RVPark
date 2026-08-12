using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
        public string? PaymentType { get; set; }
        [BindProperty]
        public decimal Amount { get; set; }
        [BindProperty]
        public decimal? AmountReceived { get; set; }
        [BindProperty]
        public decimal ChangeOwed { get; set; }
        [BindProperty]
        public string? CheckNumber { get; set; }
        [BindProperty]
        public string? CardLast4 { get; set; }
        [BindProperty]
        public string? CardAuthReference { get; set; }

        public string Message { get; set; } = "";

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var reservation = await _db.Reservations.FindAsync(ReservationId);
            if (reservation == null)
            {
                ModelState.AddModelError("", "Reservation ID not found.");
                return Page();
            }

            var username = User.Identity?.Name;
            var employee = username != null
                ? await _db.Employees.FirstOrDefaultAsync(e => e.username == username)
                : null;
            if (employee == null)
            {
                ModelState.AddModelError("", "You must be logged in as staff to record a payment.");
                return Page();
            }

            if (PaymentType != "Cash" && PaymentType != "Check" && PaymentType != "ManualCard")
            {
                ModelState.AddModelError("", "Please select a payment method.");
                return Page();
            }
            var paymentType = PaymentType!; // narrowed to one of "Cash"/"Check"/"ManualCard" by the check above

            if (paymentType == "Check" && string.IsNullOrWhiteSpace(CheckNumber))
            {
                ModelState.AddModelError("", "Check number is required for check payments.");
                return Page();
            }

            string? cardLast4 = null;
            string? cardAuthReference = null;
            if (paymentType == "ManualCard")
            {
                cardLast4 = (CardLast4 ?? "").Trim();
                cardAuthReference = (CardAuthReference ?? "").Trim();

                if (!Regex.IsMatch(cardLast4, "^[0-9]{4}$"))
                {
                    ModelState.AddModelError("", "Enter exactly the last 4 digits of the card.");
                    return Page();
                }
                if (string.IsNullOrWhiteSpace(cardAuthReference))
                {
                    ModelState.AddModelError("", "A reference/auth number is required for manual card entry.");
                    return Page();
                }
            }

            if (Amount <= 0)
            {
                ModelState.AddModelError("", "Payment amount must be greater than zero.");
                return Page();
            }

            var alreadyPaid = await _db.Payments
                .Where(p => p.ReservationId == reservation.Id)
                .SumAsync(p => p.amount);
            var remainingBalance = reservation.TotalCost - alreadyPaid;

            if (remainingBalance <= 0)
            {
                ModelState.AddModelError("", "This reservation is already paid in full.");
                return Page();
            }
            if (Amount > remainingBalance)
            {
                ModelState.AddModelError("", $"Amount cannot exceed the remaining balance of ${remainingBalance:F2} for this reservation.");
                return Page();
            }

            var checkNumber = (CheckNumber ?? "").Trim();
            if (paymentType == "Cash")
            {
                var amountReceived = AmountReceived ?? 0;
                if (amountReceived < Amount)
                {
                    ModelState.AddModelError("", "Amount received is less than the payment amount.");
                    return Page();
                }
                AmountReceived = amountReceived;
                ChangeOwed = amountReceived - Amount;
            }

            var payment = new asp_net_web_app.Data.Payment
            {
                amount = Amount,
                paidAt = DateTime.UtcNow,
                stripeId = "",
                paymentStatus = "paid",
                ReservationId = reservation.Id,
                PaymentSource = "Manual",
                PaymentMethod = paymentType,
                RecordedByEmployeeId = employee.employeeId,
                CheckNumber = paymentType == "Check" ? checkNumber : null,
                CardLast4 = cardLast4,
                CardAuthReference = cardAuthReference
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            var newRemainingBalance = remainingBalance - Amount;
            if (newRemainingBalance <= 0)
            {
                reservation.Status = "Confirmed";
                await _db.SaveChangesAsync();
            }

            var methodLabel = paymentType switch
            {
                "Cash" => "Cash",
                "Check" => $"Check (#{checkNumber})",
                "ManualCard" => $"Manual card entry (ending {cardLast4})",
                _ => paymentType
            };

            Message = $"{methodLabel} payment of ${Amount:F2} recorded by {employee.firstName} {employee.lastName}. " +
                      $"Remaining balance: ${Math.Max(newRemainingBalance, 0):F2}.";

            // Clear the form for the next entry now that the payment succeeded.
            ReservationId = 0;
            PaymentType = "";
            Amount = 0;
            AmountReceived = 0;
            ChangeOwed = 0;
            CheckNumber = "";
            CardLast4 = "";
            CardAuthReference = "";

            return Page();
        }
    }
}
