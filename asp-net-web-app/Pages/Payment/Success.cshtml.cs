using asp_net_web_app.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asp_net_web_app.Pages.Payment;

public class SuccessModel : PageModel
{
    private readonly IPaymentService _payment;

    public SuccessModel(IPaymentService payment)
    {
        _payment = payment;
    }

    public bool PaymentConfirmed { get; set; }
    public int? ReservationId { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string? session_id)
    {
        if (string.IsNullOrWhiteSpace(session_id))
        {
            ErrorMessage = "Missing payment session.";
            return Page();
        }

        try
        {
            PaymentConfirmed = await _payment.CompletePaymentAsync(session_id);
            if (!PaymentConfirmed)
            {
                ErrorMessage = "Payment was not completed. Please contact the park office.";
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Unable to verify payment. Please contact the park office with your confirmation number.";
        }

        return Page();
    }
}
