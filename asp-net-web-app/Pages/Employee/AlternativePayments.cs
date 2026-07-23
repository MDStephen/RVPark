using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using asp_net_web_app.Data;


namespace ReservationService.Pages.Employee
{
    public class AlternativePaymentsModel : PageModel
    {
        private readonly DatabaseWrapper _db;

        public AlternativePaymentsModel(DatabaseWrapper db)
        {
            _db = db;
        }

        [BindProperty]
        public string PaymentMethod { get; set; }

        [BindProperty]
        public decimal Amount { get; set; }

        public string ConfirmationMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(PaymentMethod) || Amount <= 0)
            {
                ConfirmationMessage = "Please select a payment method and enter a valid amount.";
                return Page();
            }

            var transaction = new Transaction
            {
                Amount = Amount,
                PaymentMethod = PaymentMethod,
                Notes = Notes,
                CreatedAt = DateTime.UtcNow,
                Status = "Completed",
                Source = "EmployeeManualEntry"
            };

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();

            ConfirmationMessage = $"Recorded {PaymentMethod} payment of ${Amount:F2}.";
            return Page();
        }
    }
}
